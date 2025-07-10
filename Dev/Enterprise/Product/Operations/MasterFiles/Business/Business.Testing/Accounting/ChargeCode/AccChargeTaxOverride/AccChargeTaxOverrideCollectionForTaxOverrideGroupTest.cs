using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeTaxOverrideCollection))]
	sealed class AccChargeTaxOverrideCollectionForTaxOverrideGroupTest : AccChargeTaxOverrideCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			return new AccChargeTaxOverrideCollection(taxOverrideGroup);
		}
	}
}
