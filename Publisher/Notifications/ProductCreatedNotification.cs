using Publisher.Notifications.Common;

namespace Publisher.Notifications
{
    public sealed record ProductCreatedNotification(int Id, string Name, string Description) : INotification
    {
    }
}
