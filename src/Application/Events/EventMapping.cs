using Contracts;
using Domain.Entities;

namespace Application.Events;

/// <summary>מיפוי בין ה-Domain entity ל-DTO של הלקוח.</summary>
public static class EventMapping
{
    public static EventDto ToDto(this Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        Location = e.Location,
        Status = e.Status.ToString(),
        Priority = e.Priority.ToString(),
        AssignedTechnicianId = e.AssignedTechnicianId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
