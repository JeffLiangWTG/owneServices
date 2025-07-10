using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class NZPortMessageValidationStrategyTest : TestCaseWithFactory
	{
		public void TestRegister_RegisterValidationsInAFactory()
		{
			NZPortMessageValidationStrategy.RegisterForFactory(Factory);

			var hasValidation = Factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLading), typeof(NZPortMessageShipmentValidation));
			AssertEquals("ReleaseOrderShipmentValidation is registered in a factory", true, hasValidation);

			hasValidation = Factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLadingContainer), typeof(NZPortMessageContainerValidation));
			AssertEquals("NZReleaseOrderContainerValidation is registered in a factory", true, hasValidation);
		}

		public void TestUnregister_UnregisterValidationsFromAFactory()
		{
			Factory.Validation.MainGroup.RegisterValidationType<BillOfLading, NZPortMessageShipmentValidation>();

			NZPortMessageValidationStrategy.UnregisterForFactory(Factory);

			var hasValidation = Factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLading), typeof(NZPortMessageShipmentValidation));
			AssertEquals("ReleaseOrderShipmentValidation is registered in a factory", false, hasValidation);

			hasValidation = Factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLadingContainer), typeof(NZPortMessageContainerValidation));
			AssertEquals("NZReleaseOrderContainerValidation is registered in a factory", false, hasValidation);
		}

		public void TestIsRegistered_ReturnTrueIfTheValidationIsRegisteredInAFactory()
		{
			AssertEquals("IsRegisteredInFactory", false, NZPortMessageValidationStrategy.IsRegisteredInFactory(Factory));

			Factory.Validation.MainGroup.RegisterValidationType<BillOfLading, NZPortMessageShipmentValidation>();

			AssertEquals("IsRegisteredInFactory", true, NZPortMessageValidationStrategy.IsRegisteredInFactory(Factory));
		}
	}
}
