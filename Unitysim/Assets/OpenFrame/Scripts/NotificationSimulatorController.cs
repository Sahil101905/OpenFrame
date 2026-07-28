using UnityEngine;

namespace OpenFrame.Simulator
{
    public sealed class NotificationSimulatorController : MonoBehaviour
    {
        [SerializeField] private TextAsset notificationsJson;
        [SerializeField] private NotificationCardView notificationCard;

        private NotificationData[] notifications = new NotificationData[0];
        private int nextNotificationIndex;

        public void Configure(TextAsset json, NotificationCardView card)
        {
            notificationsJson = json;
            notificationCard = card;
        }

        private void Awake()
        {
            LoadNotifications();
            notificationCard.Dismiss();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                DisplayNextNotification();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                notificationCard.Dismiss();
            }
        }

        public void DisplayNextNotification()
        {
            if (notifications.Length == 0)
            {
                return;
            }

            notificationCard.Display(notifications[nextNotificationIndex]);
            nextNotificationIndex = (nextNotificationIndex + 1) % notifications.Length;
        }

        private void LoadNotifications()
        {
            if (notificationsJson == null)
            {
                Debug.LogError("OpenFrame mock notification JSON is not assigned.");
                return;
            }

            NotificationCollection collection =
                JsonUtility.FromJson<NotificationCollection>(notificationsJson.text);

            if (collection == null || collection.notifications == null)
            {
                Debug.LogError("OpenFrame mock notification JSON is invalid.");
                return;
            }

            notifications = collection.notifications;
        }
    }
}
