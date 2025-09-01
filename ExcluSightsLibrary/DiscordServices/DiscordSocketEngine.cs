using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace ExcluSightsLibrary.DiscordServices
{
    public class DiscordSocketEngine : ISocketEngine
    {
        private readonly DiscordSocketClient _client;
        private readonly string _token;
        private readonly ILogger<DiscordSocketEngine> _log;

        private int _started;
        private readonly SemaphoreSlim _gate = new(1, 1);

        // barrier for the first complete backfill
        private TaskCompletionSource<bool> _initialBackfillTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private volatile bool _backfillStarted;   // avoid re-running on reconnect
        private volatile bool _backfillCompleted; // keeps initial guild state

        public DiscordSocketEngine(string botToken, string connStr, ILogger<DiscordSocketEngine> log)
        {
            _token = botToken!;
            _log = log;

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = false,
                LogLevel = LogSeverity.Info
            });

            _client.Log += msg => { _log.LogInformation("[Discord] {Msg}", msg.ToString()); return Task.CompletedTask; };

            _client.Ready += async () =>
            {
                _log.LogInformation("Discord Ready as {User}", _client.CurrentUser);

                if (_backfillCompleted || _backfillStarted)
                {
                    _log.LogInformation("Initial backfill already handled (started={Started}, completed={Completed}).",
                        _backfillStarted, _backfillCompleted);
                    return;
                }

                _backfillStarted = true;

                try
                {
                    // kick off downloads for all guilds
                    var downloads = _client.Guilds.Select(g =>
                    {
                        _log.LogInformation("Starting member download for Guild: {Name} ({Id})", g.Name, g.Id);
                        return g.DownloadUsersAsync();
                    }).ToArray();

                    // wait for them to finish
                    await Task.WhenAll(downloads);

                    // mark complete
                    _backfillCompleted = true;
                    _initialBackfillTcs.TrySetResult(true);
                    _log.LogInformation("Initial member backfill completed for {Count} guild(s).", _client.Guilds.Count);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Initial member backfill failed.");
                    _initialBackfillTcs.TrySetException(ex);
                }
            };
        }

        public async Task EnsureStartedAsync()
        {
            if (Interlocked.CompareExchange(ref _started, 1, 0) == 1)
                return;

            await _gate.WaitAsync();
            try
            {
                if (_client.LoginState != LoginState.LoggedIn)
                {
                    await _client.LoginAsync(TokenType.Bot, _token);
                    await _client.StartAsync();
                }
            }
            finally
            {
                _gate.Release();
            }
        }
        public async Task<bool> WaitForInitialBackfillAsync(TimeSpan timeout)
        {
            // return if already filled
            if (_backfillCompleted) return true;

            using var cts = new CancellationTokenSource(timeout);
            try
            {
                await _initialBackfillTcs.Task.WaitAsync(cts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("InitialBackfill timed out after {Timeout}. Proceeding without full backfill.", timeout);
                return false;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "WaitForInitialBackfillAsync failed.");
                return false;
            }
        }
    }
}
