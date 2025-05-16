using System.ComponentModel.DataAnnotations.Schema;
using CRM.EventStore.Domain.Common.Entities;
using CRM.EventStore.Domain.Entities.Permissions.Enums;

namespace CRM.EventStore.Domain.Entities.Permissions;

[Table(nameof(Permission))]
public class Permission : Entity
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActionType ActionType { get; set; }
    public string AllowedRoles { get; set; }

    public Permission(
        int order,
        string title,
        string section,
        string? description,
        ActionType actionType,
        string allowedRoles)
    {
        Order = order;
        Title = title;
        Section = section;
        Description = description;
        ActionType = actionType;
        AllowedRoles = allowedRoles;
    }

    public void Update(
        int order,
        string title,
        string section,
        string? description,
        ActionType actionType,
        string allowedRoles)
    {
        Order = order;
        Title = title;
        Section = section;
        Description = description;
        ActionType = actionType;
        AllowedRoles = allowedRoles;
    }
}

