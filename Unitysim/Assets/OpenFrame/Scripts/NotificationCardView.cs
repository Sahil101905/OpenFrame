using UnityEngine;
using UnityEngine.UI;

namespace OpenFrame.Simulator
{
    public sealed class NotificationCardView : MonoBehaviour
    {
        [SerializeField] private Text sourceApplicationText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageBodyText;
        [SerializeField] private Text timestampText;

        public void Configure(Text source, Text title, Text message, Text timestamp)
        {
            sourceApplicationText = source;
            titleText = title;
            messageBodyText = message;
            timestampText = timestamp;
        }

        public void Display(NotificationData notification)
        {
            sourceApplicationText.text = notification.sourceApplication;
            titleText.text = notification.title;
            messageBodyText.text = notification.messageBody;
            timestampText.text = notification.timestamp;
            gameObject.SetActive(true);
        }

        public void Dismiss()
        {
            gameObject.SetActive(false);
        }
    }
}
