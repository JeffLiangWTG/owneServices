using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCustomsSGAXmlCredentialConfigurationHandler : GlbExternalPasswordConfigurationHandler
	{
		public SGCustomsSGAXmlCredentialConfigurationHandler(LoggingInformation logger) : base(logger) { }

		protected override bool SupportsPasswordChanging => true;

		protected override ZString GetGP_PasswordType(Group groupData)
		{
			return groupData.Type == GlbExternalPassword_SGA.SGCustomsAccount ? new ZString(PasswordTypesList.Codes.SGA) : base.GetGP_PasswordType(groupData);
		}
	}
}
