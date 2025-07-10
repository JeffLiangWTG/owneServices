using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusBrokerStaffValidation
	{
		public CusBrokerStaffValidation(CusBrokerStaff parent)
		{
			this.parent = parent;
		}

		readonly CusBrokerStaff parent;

		public void ValidateAll()
		{
			ValidateBrokerStaffCode();
			ValidateMailbox();
		}

		public void ValidateBrokerStaffCode()
		{
			var targetInfo = parent.BrokerStaffCodeInfo;
			targetInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}

		public void ValidateMailbox()
		{
			var targetInfo = parent.MailboxInfo;
			targetInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}
	}
}
