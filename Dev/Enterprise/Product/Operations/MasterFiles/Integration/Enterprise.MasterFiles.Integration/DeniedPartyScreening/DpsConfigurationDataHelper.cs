using CargoWise.Application;
using CargoWise.Services.Common;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Integration
{
	public static class DpsConfigurationDataHelper
	{
		public static ServiceRequestConfigurationData NewServiceConfig()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;
			return new ServiceRequestConfigurationData()
			{
				UserName = EnvProxy.Instance.CurrentUser.LoginName,
				LicenceCodeEnterpriseAndDatabase = enterpriseCode + "-" + serverCode,
				LicenceCode = enterpriseCode + "-" + EnvProxy.Instance.CurrentCompany.Code + "-" + serverCode
			};
		}
	}
}
