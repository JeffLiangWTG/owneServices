using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public static class MessageHostedServiceRequirement
	{
		public static ZString CheckUYCompanyHasCertificate()
		{
			var companyHasCertificate = CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Uruguay, PasswordTypesList.Codes.UTB, PasswordStatusList.Codes.Valid);
			return companyHasCertificate ? string.Empty : (NoResString)"There is no Valid Certificate in Uruguay.";
		}
	}
}
