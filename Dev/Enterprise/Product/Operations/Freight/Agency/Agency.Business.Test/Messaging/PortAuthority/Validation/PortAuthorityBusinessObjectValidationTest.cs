using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class PortAuthorityBusinessObjectValidationTest : TestCaseWithFactory
	{
		public void TestRegistration()
		{
			AssertEquals(false, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));

			PortAuthorityBusinessObjectValidation.RegisterForFactory(Factory);
			AssertEquals(true, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));

			PortAuthorityBusinessObjectValidation.RegisterForFactory(Factory);
			AssertEquals(true, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));

			PortAuthorityBusinessObjectValidation.UnregisterForFactory(Factory);
			AssertEquals(false, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));
		}
	}
}
