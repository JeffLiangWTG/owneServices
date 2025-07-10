using CargoWise.Application;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class ProductRegistrationHelperTest : TestCase
	{
		public void TestGetClientId()
		{
			var clientId = new ProductRegistrationHelper().GetClientId();
			AssertEquals(ObjectFactory.Get<IProductRegistration>().Key.SystemId, clientId);

			var password = new ProductRegistrationHelper().GetRegistrationKeyPassword();
			AssertEquals(ObjectFactory.Get<IProductRegistration>().Key.Password, password);
		}
	}
}
