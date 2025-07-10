using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class ReleaseOrderMessageValidationStrategyTest : TestCaseWithFactory
	{
		public void TestRegister_RegisterValidationsInAFactory()
		{
			NZReleaseOrderMessageValidationStrategy.RegisterForFactory(Factory);

			var hasValidation = Factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLading), typeof(NZReleaseOrderShipmentValidation));
			AssertEquals("ReleaseOrderShipmentValidation is registered in a factory", true, hasValidation);

			hasValidation = Factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLadingContainer), typeof(NZReleaseOrderContainerValidation));
			AssertEquals("NZReleaseOrderContainerValidation is registered in a factory", true, hasValidation);
		}

		public void TestUnregister_UnregisterValidationsFromAFactory()
		{
			Factory.Validation.MainGroup.RegisterValidationType<BillOfLading, NZReleaseOrderShipmentValidation>();

			NZReleaseOrderMessageValidationStrategy.UnregisterForFactory(Factory);

			var hasValidation = Factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLading), typeof(NZReleaseOrderShipmentValidation));
			AssertEquals("ReleaseOrderShipmentValidation is registered in a factory", false, hasValidation);

			hasValidation = Factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLadingContainer), typeof(NZReleaseOrderContainerValidation));
			AssertEquals("NZReleaseOrderContainerValidation is registered in a factory", false, hasValidation);
		}

		public void TestIsRegistered_ReturnTrueIfTheValidationIsRegisteredInAFactory()
		{
			AssertEquals("IsRegisteredInFactory", false, NZReleaseOrderMessageValidationStrategy.IsRegisteredInFactory(Factory));

			Factory.Validation.MainGroup.RegisterValidationType<BillOfLading, NZReleaseOrderShipmentValidation>();

			AssertEquals("IsRegisteredInFactory", true, NZReleaseOrderMessageValidationStrategy.IsRegisteredInFactory(Factory));
		}
	}
}
