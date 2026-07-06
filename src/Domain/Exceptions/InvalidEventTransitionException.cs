using Domain.Enums;

namespace Domain.Exceptions;

/// <summary>
/// נזרקת כאשר מנסים לבצע מעבר סטטוס לא חוקי על אירוע.
/// זהו המנגנון שאוכף "לא ניתן להגיע לכל מצב מכל מצב" (§6 בתוכנית).
/// </summary>
public sealed class InvalidEventTransitionException : Exception
{
    public EventStatus From { get; }
    public EventStatus To { get; }

    public InvalidEventTransitionException(EventStatus from, EventStatus to)
        : base($"מעבר סטטוס לא חוקי: לא ניתן לעבור מ-{from} ל-{to}.")
    {
        From = from;
        To = to;
    }
}
