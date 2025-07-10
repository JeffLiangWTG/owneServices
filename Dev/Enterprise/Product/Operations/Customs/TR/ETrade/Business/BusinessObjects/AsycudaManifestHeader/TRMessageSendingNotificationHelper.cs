using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class TRMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public TRMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		public override ZString GetNotifications()
		{
			var result = new ZStringBuilder();

			if (GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty)
			{
				result.AppendLine(ASYCUDA.Business.ValidationConstants.MissingEmailAddress);
			}

			var password = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			if (password == null)
			{
				result.AppendLine(Res.GetString("DC59ADF6-FDAD-4C5A-A971-50D1C895E32A", "Please edit your staff record to add the broker in the Brokerage tab."));
			}
			else if (password.GP_PasswordStatus == PasswordStatusList.Codes.Invalid)
			{
				result.AppendLine(Res.GetString("8755D46B-15D4-46C9-841A-0EB23F4C5DDD", "Your customs credentials is marked as invalid, please update your customs credentials."));
			}

			return result.ToString();
		}
	}
}
