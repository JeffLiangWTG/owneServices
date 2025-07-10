using CargoWise.Application;
using Enterprise.Integration.Licensing;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class ProductRegistrationHelper
	{
		public ProductRegistrationHelper()
		{
			this.registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
		}

		readonly IProductRegistrationKey registrationKey;
		public string GetClientId()
		{
			var clientId = registrationKey.SystemId;
			return clientId;
		}

		public string GetSystemType()
		{
			return registrationKey.DatabaseType;
		}

		public string GetRegistrationKeyPassword()
		{
			var password = registrationKey.Password;
			return password;
		}
	}
}
