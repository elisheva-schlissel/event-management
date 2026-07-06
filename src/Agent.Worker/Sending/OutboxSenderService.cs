using System.Text;
using Agent.Worker.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Agent.Worker.Sending;

/// <summary>
/// רץ ברקע ברציפות: מושך הודעות Pending מה-Outbox ומשדר לשרת המרכזי.
/// כישלון (שרת/DB נפולים) → לא מסמן כ-Sent, מגדיל Attempts וקובע NextAttemptAt
/// עם exponential backoff. כך אף אירוע לא נאבד (§9).
/// </summary>
public sealed class OutboxSenderService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ServerOptions _server;
    private readonly ILogger<OutboxSenderService> _logger;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    public OutboxSenderService(
        IServiceProvider services,
        IHttpClientFactory httpFactory,
        ServerOptions server,
        ILogger<OutboxSenderService> logger)
    {
        _services = services;
        _httpFactory = httpFactory;
        _server = server;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בעיבוד ה-Outbox");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        var now = DateTime.UtcNow;

        var pending = await db.Messages
            .Where(m => m.Status == OutboxStatus.Pending
                        && (m.NextAttemptAt == null || m.NextAttemptAt <= now))
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        var client = _httpFactory.CreateClient("server");

        foreach (var msg in pending)
        {
            try
            {
                using var content = new StringContent(msg.PayloadJson, Encoding.UTF8, "application/json");
                content.Headers.Add(AgentKeyHeader, _server.AgentKey);

                var response = await client.PostAsync("/api/events/ingest", content, ct);
                if (response.IsSuccessStatusCode)
                {
                    msg.Status = OutboxStatus.Sent;
                    msg.SentAt = DateTime.UtcNow;
                    _logger.LogInformation("אירוע {Key} שודר לשרת בהצלחה", msg.IdempotencyKey);
                }
                else
                {
                    RecordFailure(msg, $"HTTP {(int)response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // שרת לא זמין → נשאר Pending, ננסה שוב מאוחר יותר.
                RecordFailure(msg, ex.Message);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private void RecordFailure(OutboxMessage msg, string error)
    {
        msg.Attempts++;
        msg.LastError = error;
        // backoff: 5s, 10s, 20s ... עד תקרה של 5 דקות.
        var delaySeconds = Math.Min(300, 5 * Math.Pow(2, Math.Min(msg.Attempts - 1, 6)));
        msg.NextAttemptAt = DateTime.UtcNow.AddSeconds(delaySeconds);
        _logger.LogWarning("שידור אירוע {Key} נכשל (ניסיון {Attempts}): {Error}. ניסיון הבא בעוד {Delay}s",
            msg.IdempotencyKey, msg.Attempts, error, delaySeconds);
    }

    public const string AgentKeyHeader = "X-Agent-Key";
}

/// <summary>הגדרות היעד (השרת המרכזי).</summary>
public sealed class ServerOptions
{
    public const string SectionName = "Server";
    public string BaseUrl { get; set; } = "http://localhost:5125";
    public string AgentKey { get; set; } = string.Empty;
}
