using Domain.Enums;
using Domain.StateMachine;
using Xunit;

namespace Domain.Tests;

/// <summary>
/// בדיקות לטבלת המעברים עצמה — ללא תלות ב-Entity או ב-DB.
/// מוודאות ש"לא ניתן להגיע לכל מצב מכל מצב" (§5 באפיון).
/// </summary>
public class EventStateMachineTests
{
    [Theory]
    [InlineData(EventStatus.New, EventStatus.Assigned)]
    [InlineData(EventStatus.New, EventStatus.Cancelled)]
    [InlineData(EventStatus.Assigned, EventStatus.InProgress)]
    [InlineData(EventStatus.Assigned, EventStatus.Cancelled)]
    [InlineData(EventStatus.InProgress, EventStatus.WaitingForInfo)]
    [InlineData(EventStatus.InProgress, EventStatus.Completed)]
    [InlineData(EventStatus.InProgress, EventStatus.Cancelled)]
    [InlineData(EventStatus.WaitingForInfo, EventStatus.InProgress)]
    [InlineData(EventStatus.WaitingForInfo, EventStatus.Cancelled)]
    public void CanTransition_AllowedTransitions_ReturnsTrue(EventStatus from, EventStatus to)
    {
        Assert.True(EventStateMachine.CanTransition(from, to));
    }

    [Theory]
    [InlineData(EventStatus.New, EventStatus.InProgress)]      // דילוג על Assigned
    [InlineData(EventStatus.New, EventStatus.Completed)]       // דילוג ישיר לסיום
    [InlineData(EventStatus.Assigned, EventStatus.Completed)]  // דילוג על InProgress
    [InlineData(EventStatus.Assigned, EventStatus.New)]        // חזרה אחורה
    [InlineData(EventStatus.Completed, EventStatus.InProgress)]// יציאה ממצב סופי
    [InlineData(EventStatus.Cancelled, EventStatus.Assigned)]  // יציאה ממצב סופי
    [InlineData(EventStatus.Completed, EventStatus.Cancelled)] // בין שני מצבים סופיים
    public void CanTransition_IllegalTransitions_ReturnsFalse(EventStatus from, EventStatus to)
    {
        Assert.False(EventStateMachine.CanTransition(from, to));
    }

    [Theory]
    [InlineData(EventStatus.Completed)]
    [InlineData(EventStatus.Cancelled)]
    public void IsTerminal_FinalStates_ReturnsTrue(EventStatus status)
    {
        Assert.True(EventStateMachine.IsTerminal(status));
        Assert.Empty(EventStateMachine.AllowedTargets(status));
    }

    [Theory]
    [InlineData(EventStatus.New)]
    [InlineData(EventStatus.Assigned)]
    [InlineData(EventStatus.InProgress)]
    [InlineData(EventStatus.WaitingForInfo)]
    public void IsTerminal_NonFinalStates_ReturnsFalse(EventStatus status)
    {
        Assert.False(EventStateMachine.IsTerminal(status));
    }

    [Fact]
    public void AllowedTargets_FromNew_ReturnsAssignedAndCancelledOnly()
    {
        var targets = EventStateMachine.AllowedTargets(EventStatus.New);
        Assert.Equal(2, targets.Count);
        Assert.Contains(EventStatus.Assigned, targets);
        Assert.Contains(EventStatus.Cancelled, targets);
    }
}
