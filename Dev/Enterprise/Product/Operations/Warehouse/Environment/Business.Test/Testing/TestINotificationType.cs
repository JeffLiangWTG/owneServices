using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class TestINotificationType : INotificationSubscriberType
	{
		public TestINotificationType(string message, string name)
		{
			this.message = message;
			this.name = name;
		}

		public string Message { get { return message; } }
		public string Name { get { return name; } }
		public string GetDisplayMessage(string additionalInfo) { return Message + additionalInfo; }

		readonly string message;
		readonly string name;

		bool INotificationType.IsFatal
		{
			get { return false; }
		}

		int INotificationType.Severity
		{
			get { return 0; }
		}

		string INotificationType.EnumValueName
		{
			get { return "None"; }
		}
	}
}
