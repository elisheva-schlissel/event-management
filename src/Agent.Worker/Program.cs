using System.Text.Json;
using Agent.Worker.Ingestion;
using Agent.Worker.Outbox;
using Agent.Worker.Security;
using Agent.Worker.Sending;
using Contracts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Outbox מקומי (SQLite) ---
builder.Services.AddDbContext<OutboxDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Outbox") ?? "Data Source=agent-outbox.db"));

// --- אימות מקורות (HMAC) ---
var sourceAuth = builder.Configuration.GetSection(SourceAuthOptions.SectionName).Get<SourceAuthOptions>()
                 ?? new SourceAuthOptions();
builder.Services.AddSingleton(sourceAuth);
builder.Services.AddSingleton<SourceAuthenticator>();

// --- יעד השרת המרכזי + שולח רקע ---
var server = builder.Configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>() ?? new ServerOptions();
builder.Services.AddSingleton(server);
builder.Services.AddHttpClient("server", c => c.BaseAddress = new Uri(server.BaseUrl));
builder.Services.AddHostedService<OutboxSenderService>();

var app = builder.Build();

// יצירת ה-DB המקומי אם לא קיים.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

/// <summary>
/// Webhook נכנס: מקור חיצוני מדווח על אירוע.
/// מאמת HMAC → כותב ל-Outbox (שידור לשרת מתבצע אסינכרונית ב-BackgroundService).
/// </summary>
app.MapPost("/ingest", async (HttpRequest http, SourceAuthenticator auth, SourceAuthOptions opts, OutboxDbContext db) =>
{
    using var reader = new StreamReader(http.Body);
    var rawBody = await reader.ReadToEndAsync();

    var apiKey = http.Headers["X-Api-Key"].ToString();
    var timestamp = http.Headers["X-Timestamp"].ToString();
    var signature = http.Headers["X-Signature"].ToString();

    if (!auth.IsValid(apiKey, timestamp, signature, rawBody, out var error))
        return Results.Json(new { message = $"אימות מקור נכשל: {error}" }, statusCode: 401);

    SourceEventPayload? payload;
    try
    {
        payload = JsonSerializer.Deserialize<SourceEventPayload>(rawBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    catch
    {
        return Results.BadRequest(new { message = "גוף הבקשה אינו JSON תקין" });
    }

    if (payload is null || string.IsNullOrWhiteSpace(payload.Title))
        return Results.BadRequest(new { message = "שדה title הוא חובה" });

    // ה-Agent מייצר idempotencyKey יציב פעם אחת — משמש למניעת כפילות בשידורים חוזרים.
    var request = new IngestEventRequest
    {
        IdempotencyKey = Guid.NewGuid().ToString(),
        Title = payload.Title,
        Description = payload.Description ?? string.Empty,
        Location = payload.Location,
        SourceId = opts.SourceId,
        Priority = payload.Priority ?? 1
    };

    db.Messages.Add(new OutboxMessage
    {
        IdempotencyKey = request.IdempotencyKey,
        PayloadJson = JsonSerializer.Serialize(request),
        CreatedAt = DateTime.UtcNow
    });
    await db.SaveChangesAsync();

    // 202: התקבל ונשמר ב-Outbox; השידור לשרת יקרה אסינכרונית.
    return Results.Accepted(value: new { accepted = true, idempotencyKey = request.IdempotencyKey });
});

app.Run();
