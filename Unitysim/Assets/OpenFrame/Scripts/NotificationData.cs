using System;

namespace OpenFrame.Simulator
{
    [Serializable]
    public sealed class NotificationData
    {
        public string sourceApplication;
        public string title;
        public string messageBody;
        public string timestamp;
    }

    [Serializable]
    public sealed class NotificationCollection
    {
        public NotificationData[] notifications;
    }
}
