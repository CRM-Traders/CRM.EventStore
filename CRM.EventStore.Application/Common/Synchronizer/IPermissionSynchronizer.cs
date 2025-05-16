namespace CRM.EventStore.Application.Common.Synchronizer;

public interface IPermissionSynchronizer
{
    Task SynchronizePermissionsAsync(CancellationToken cancellationToken = default);
}