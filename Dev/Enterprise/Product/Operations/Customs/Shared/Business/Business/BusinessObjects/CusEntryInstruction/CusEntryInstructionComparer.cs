using CargoWise.ComponentModel;

namespace Enterprise.Customs.Business
{
	public class CusEntryInstructionComparer : ICusEntryInstructionComparer<CusEntryInstruction>
	{
		public CusEntryInstructionComparer()
		{
		}

		public int Compare(CusEntryInstruction x, CusEntryInstruction y) => CompareCore(x, y);

		protected virtual int CompareCore(CusEntryInstruction x, CusEntryInstruction y)
		{
			var result = x.CEI_Style.CompareTo(y.CEI_Style);
			if (result == 0)
			{
				result = x.CEI_SubStyle.CompareTo(y.CEI_SubStyle);
			}
			if (result == 0)
			{
				result = x.CEI_Description.CompareTo(y.CEI_Description);
			}
			return result;
		}

		public INotificationType GetUniquenessNotificationSeverity() => GetUniquenessNotificationSeverityCore();
		protected virtual INotificationType GetUniquenessNotificationSeverityCore() => NotificationType.Warning;
	}
}
