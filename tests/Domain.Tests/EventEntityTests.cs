using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Xunit;

namespace Domain.Tests;

/// <summary>
/// בדיקות ל-aggregate root Event — אכיפת מעברים דרך ההתנהגות, ורישום היסטוריה
/// עם משתמש וחותמת זמן (§5 באפיון).
/// </summary>
public class EventEntityTests
{
    private static readonly DateTime T0 = new(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc);

    private static Event NewEvent() =>
        Event.Create("דלת פרוצה", "התקבל דיווח", "בניין A", Guid.NewGuid(), "idem-1", T0);

    [Fact]
    public void Create_SetsStatusNew_AndRecordsInitialHistory()
    {
        var e = NewEvent();

        Assert.Equal(EventStatus.New, e.Status);
        Assert.Single(e.StatusHistory);
        var first = e.StatusHistory[0];
        Assert.Null(first.FromStatus);
        Assert.Equal(EventStatus.New, first.ToStatus);
        Assert.Null(first.ChangedByUserId);
        Assert.Equal(T0, first.ChangedAt);
    }

    [Fact]
    public void Create_WithEmptyTitle_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Event.Create("  ", "d", null, Guid.NewGuid(), "idem", T0));
    }

    [Fact]
    public void ChangeStatus_LegalTransition_UpdatesStatusAndAppendsHistoryWithUserAndTimestamp()
    {
        var e = NewEvent();
        var dispatcher = Guid.NewGuid();
        var at = T0.AddMinutes(5);

        e.ChangeStatus(EventStatus.Assigned, dispatcher, at, "הוקצה");

        Assert.Equal(EventStatus.Assigned, e.Status);
        Assert.Equal(at, e.UpdatedAt);
        Assert.Equal(2, e.StatusHistory.Count);
        var last = e.StatusHistory[^1];
        Assert.Equal(EventStatus.New, last.FromStatus);
        Assert.Equal(EventStatus.Assigned, last.ToStatus);
        Assert.Equal(dispatcher, last.ChangedByUserId);
        Assert.Equal(at, last.ChangedAt);
        Assert.Equal("הוקצה", last.Note);
    }

    [Fact]
    public void ChangeStatus_IllegalTransition_ThrowsAndKeepsStateUnchanged()
    {
        var e = NewEvent();

        var ex = Assert.Throws<InvalidEventTransitionException>(() =>
            e.ChangeStatus(EventStatus.Completed, Guid.NewGuid(), T0.AddMinutes(1)));

        Assert.Equal(EventStatus.New, ex.From);
        Assert.Equal(EventStatus.Completed, ex.To);
        // המצב לא השתנה ולא נוספה היסטוריה
        Assert.Equal(EventStatus.New, e.Status);
        Assert.Single(e.StatusHistory);
    }

    [Fact]
    public void FullLifecycle_NewToCompleted_TracksEveryTransition()
    {
        var e = NewEvent();
        var user = Guid.NewGuid();

        e.ChangeStatus(EventStatus.Assigned, user, T0.AddMinutes(1));
        e.ChangeStatus(EventStatus.InProgress, user, T0.AddMinutes(2));
        e.ChangeStatus(EventStatus.WaitingForInfo, user, T0.AddMinutes(3));
        e.ChangeStatus(EventStatus.InProgress, user, T0.AddMinutes(4));
        e.ChangeStatus(EventStatus.Completed, user, T0.AddMinutes(5));

        Assert.Equal(EventStatus.Completed, e.Status);
        Assert.Equal(6, e.StatusHistory.Count); // יצירה + 5 מעברים
    }

    [Fact]
    public void ChangeStatus_FromTerminalState_Throws()
    {
        var e = NewEvent();
        e.ChangeStatus(EventStatus.Cancelled, Guid.NewGuid(), T0.AddMinutes(1));

        Assert.Throws<InvalidEventTransitionException>(() =>
            e.ChangeStatus(EventStatus.Assigned, Guid.NewGuid(), T0.AddMinutes(2)));
    }

    [Fact]
    public void AssignTo_SetsTechnicianAndMovesToAssigned()
    {
        var e = NewEvent();
        var tech = Guid.NewGuid();
        var dispatcher = Guid.NewGuid();

        e.AssignTo(tech, dispatcher, T0.AddMinutes(1));

        Assert.Equal(EventStatus.Assigned, e.Status);
        Assert.Equal(tech, e.AssignedTechnicianId);
    }
}
