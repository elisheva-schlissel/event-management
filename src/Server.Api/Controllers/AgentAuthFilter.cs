using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Api.Controllers;

/// <summary>
/// אימות פנימי של קריאות ה-Agent לשרת דרך כותרת X-Agent-Key.
/// זו הפרדה מכוונת מאימות המשתמשים (JWT): ה-Agent הוא שירות/מכונה, לא משתמש אנושי
/// עם login (ראה §18.ה בתוכנית). המפתח נטען מ-configuration ("AgentAuth:Key").
/// </summary>
public sealed class AgentAuthFilter : IAsyncActionFilter
{
    public const string HeaderName = "X-Agent-Key";
    private readonly string _expectedKey;

    public AgentAuthFilter(IConfiguration config)
    {
        _expectedKey = config["AgentAuth:Key"] ?? string.Empty;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrEmpty(_expectedKey) || provided != _expectedKey)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Agent key חסר או שגוי" });
            return;
        }

        await next();
    }
}
