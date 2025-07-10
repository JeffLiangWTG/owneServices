using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationTaxRateFileImportLineCollection))]
	sealed class OrganisationTaxRateFileImportLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrganisationTaxRateFileImportLineCollection>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		protected override OrganisationTaxRateFileImportLineCollection GetCollectionToTest() => new OrganisationTaxRateFileImportLineCollection(Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new OrganisationTaxRateFileImportLine(Factory);
	}
}
