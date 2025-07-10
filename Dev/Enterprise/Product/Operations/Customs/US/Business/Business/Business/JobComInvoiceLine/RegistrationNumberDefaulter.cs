using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	class RegistrationNumberDefaulter
	{
		public void DefaultDDTCRegistrationNumber(ZPropertyInfo registrationNumberInfo, ZString licenseType, USOrganisation usUSPPI)
		{
			if (!licenseType.IsEmpty)
			{
				var organisation = usUSPPI.Organisation;
				if (registrationNumberInfo.Value.IsEmpty && USAESLicenseCode.IsDDTCDataRequired(licenseType) && organisation != null)
				{
					registrationNumberInfo.Value = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.DDTCRegistrationNumber);
				}
			}
		}

		public void DefaultDDTCRegistrationNumber(ZPropertyInfo registrationNumberInfo, OrgHeader importer)
		{
			if (registrationNumberInfo.Value.IsEmpty && importer != null)
			{
				registrationNumberInfo.Value = importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.DDTCRegistrationNumber);
			}
		}
	}
}
