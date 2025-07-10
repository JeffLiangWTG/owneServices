using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeTaxOverrideCollection))]
	sealed class AccChargeTaxOverrideCollectionForChargeCodeTest : AccChargeTaxOverrideCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			return new AccChargeTaxOverrideCollection(chargeCode);
		}
	}
}
