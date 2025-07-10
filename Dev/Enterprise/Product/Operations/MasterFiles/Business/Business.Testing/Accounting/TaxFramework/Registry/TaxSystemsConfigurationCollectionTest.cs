using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxSystemsConfigurationCollection))]
	sealed class TaxSystemsConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TaxSystemsConfigurationCollection>
	{
		public void TestDuplicateTaxCode()
		{
			var collection = new TaxSystemsConfigurationCollection();
			var taxConfiguration1 = TestObjectCreator.CreateTaxSystem("ABC");
			collection.Add(taxConfiguration1);
			collection.RunPreSaveValidation();

			var expectedMessage = "This Code is already in use. Please use a different Code.";
			AssertNoError(taxConfiguration1.CodeInfo, expectedMessage);

			var taxConfiguration2 = TestObjectCreator.CreateTaxSystem("ABC");
			collection.Add(taxConfiguration2);
			collection.RunPreSaveValidation();

			AssertHasError(taxConfiguration2.CodeInfo, expectedMessage);

			taxConfiguration2.Code = "DEF";
			collection.RunPreSaveValidation();

			AssertNoError(taxConfiguration2.CodeInfo, expectedMessage);
		}

		#region Implementation

		protected override TaxSystemsConfigurationCollection GetCollectionToTest() => new TaxSystemsConfigurationCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TaxSystemsConfiguration();

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
		#endregion
	}
}
