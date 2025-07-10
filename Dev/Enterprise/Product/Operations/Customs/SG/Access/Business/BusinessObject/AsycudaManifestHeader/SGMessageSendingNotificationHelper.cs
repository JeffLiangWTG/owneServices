using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.Access.Business
{
	public class SGMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public SGMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			var additionalNotification = base.GetExtraMessageSendingNotificationCore();
			if (additionalNotification.IsEmpty)
			{
				if (SGStaffWrapper == null || SGStaffWrapper.AccessPassword.GP_UserID.IsEmpty)
				{
					additionalNotification = ValidationConstants.AccessCredentialsNotSetUp;
				}
				else
				{
					var accessPasswordStatus = SGStaffWrapper.AccessPassword.GP_PasswordStatus;
					if (accessPasswordStatus != Core.Constants.PasswordOK)
					{
						additionalNotification = ValidationConstants.InvalidSGAccessCredentials(accessPasswordStatus);
					}
				}
			}

			return additionalNotification;
		}

		SGGlbStaffWrapper SGStaffWrapper => SGGlbStaffWrapper.Get(header.Factory.Load<GlbStaff>(Env.CurrentUser.PK));
	}
}
