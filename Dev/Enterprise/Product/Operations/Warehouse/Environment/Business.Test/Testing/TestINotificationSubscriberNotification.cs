using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class TestINotificationSubscriberNotification : INotificationSubscriberNotification
	{
		public TestINotificationSubscriberNotification(string displayMessage)
			: this("Additional Info", false, displayMessage, "", false, new TestINotificationType(displayMessage, "TestINotificationType"))
		{
		}

		public TestINotificationSubscriberNotification(string displayMessage, INotificationSubscriberType iNotificationType)
			: this("Additional Info", false, displayMessage, "", false, iNotificationType)
		{
		}

		public TestINotificationSubscriberNotification(string additionalInfo, bool allowBlankDisplayMessage, string displayMessage, string multiLineDisplayMessage, bool shouldBeDisplayedOnBatchProcessor, INotificationSubscriberType type)
		{
			this.additionalInfo = additionalInfo;
			this.allowBlankDisplayMessage = allowBlankDisplayMessage;
			this.message = displayMessage;
			this.multiLineDisplayMessage = multiLineDisplayMessage;
			this.shouldBeDisplayedOnBatchProcessor = shouldBeDisplayedOnBatchProcessor;
			this.type = type;
		}

		public string AdditionalInfo { get { return additionalInfo; } }
		public bool AllowBlankDisplayMessage { get { return allowBlankDisplayMessage; } }
		public string Message { get { return message; } }
		public string MultiLineDisplayMessage { get { return multiLineDisplayMessage; } }
		public bool ShouldBeDisplayedOnBatchProcessor { get { return shouldBeDisplayedOnBatchProcessor; } }
		public INotificationSubscriberType Type { get { return type; } }

		readonly string additionalInfo;
		readonly bool allowBlankDisplayMessage;
		string message;
		readonly string multiLineDisplayMessage;
		readonly bool shouldBeDisplayedOnBatchProcessor;
		readonly INotificationSubscriberType type;

		#region INotification Members

		INotificationType INotification.Type
		{
			get { return Type; }
		}

		INotification INotification.ReplaceMessage(string message)
		{
			TestINotificationSubscriberNotification result = (TestINotificationSubscriberNotification)MemberwiseClone();
			result.message = message;
			return result;
		}

		#endregion
	}
}
