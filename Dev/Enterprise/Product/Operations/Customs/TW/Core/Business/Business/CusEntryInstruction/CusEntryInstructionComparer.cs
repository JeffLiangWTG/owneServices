using CargoWise.ComponentModel;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryInstructionComparer : Customs.Business.CusEntryInstructionComparer
	{
		public CusEntryInstructionComparer()
		{
		}

		protected override INotificationType GetUniquenessNotificationSeverityCore() => NotificationType.Information;
	}
}
