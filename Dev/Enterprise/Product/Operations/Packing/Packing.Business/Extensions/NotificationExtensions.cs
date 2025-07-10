using CargoWise.ComponentModel;

namespace Enterprise.Packing.Business
{
	public static class NotificationExtensions
	{
		public static void AddInformation(this INotifications notify, string message)
		{
			notify.Add(new InformationNotification(), message);
		}
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class InformationNotification : INotificationType
	{
		public string EnumValueName
		{
			get { return Res.GetString("1e5c991e-fc24-4c03-bf4f-437204ad20fc", "Information"); }
		}

		public bool IsFatal
		{
			get { return false; }
		}

		public int Severity
		{
			get { return 0; }
		}
	}
}

