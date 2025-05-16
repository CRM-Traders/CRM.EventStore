using CRM.EventStore.Domain.Entities.StoredEvents;
using CRM.EventStore.Persistence.Databases.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.EventStore.Persistence.Databases.Configurations;

public class EventConfiguration : BaseEntityTypeConfiguration<Event>
{
    public override void Configure(EntityTypeBuilder<Event> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(Event));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(e => e.ServiceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.AggregateId)
            .IsRequired();

        builder.Property(e => e.AggregateType)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(e => e.OccurredOn)
            .IsRequired();

        builder.Property(e => e.Content)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb");

        builder.Property(e => e.ProcessedAt)
            .IsRequired();

        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.ServiceName);
        builder.HasIndex(e => e.AggregateId);
        builder.HasIndex(e => e.OccurredOn);
    }
}
