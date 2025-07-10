using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class SanitizedISFDocAddressWrapper : SanitizedACEOceanManifestJobDocAddressWrapper, IISFDocAddress
	{
		public SanitizedISFDocAddressWrapper(ISFDocAddress docAddress)
			: base(docAddress)
		{ }

		internal protected new ISFDocAddress DocAddress
		{
			get { return (ISFDocAddress)base.DocAddress; }
		}

		public ZString E2_Contact
		{
			get { return GetSanitizedString(DocAddress.E2_Contact); }
		}

		public ZString E2_SocialSecurityNumber
		{
			get { return DocAddress.E2_SocialSecurityNumber; }
		}

		public ZDateTime E2_SocialSecurityNumberDateOfBirth
		{
			get { return DocAddress.E2_SocialSecurityNumberDateOfBirth; }
		}
	}
}
