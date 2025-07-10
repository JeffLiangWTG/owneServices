using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCustomsNTPXmlCredentialConfigurationHandler : GlbExternalPasswordConfigurationHandler
	{
		public SGCustomsNTPXmlCredentialConfigurationHandler(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override void ProcessCredential(GlbExternalPassword externalPassword, Group group, Credential credential, ZString statusReason)
		{
			if (group.StatusSpecified)
			{
				base.ProcessCredential(externalPassword, group, credential, statusReason);
				var ntpPassword = externalPassword as GlbExternalPassword_SGNTP;
				if (ntpPassword != null && group.Status == PasswordStatusList.Codes.Valid)
				{
					if (credential.Password != null)
					{
						ntpPassword.CurrentDecryptedPassword = GetDecrypted(credential.Password);
						NotifyPasswordStatusUpdate(ntpPassword, statusReason);
					}
					ntpPassword.GP_NextPassword = ZString.Empty;
				}
			}
		}
	}
}
