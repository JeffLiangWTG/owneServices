using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ChargeCodesLinkedToTaxFrameworkConfigurationCollection))]
	sealed class ChargeCodesLinkedToTaxFrameworkConfigurationTest : ActiveBusinessObjectCollectionTestCase<ChargeCodesLinkedToTaxFrameworkConfigurationCollection>
	{
		protected override ChargeCodesLinkedToTaxFrameworkConfigurationCollection GetCollectionToTest()
		{
			return new ChargeCodesLinkedToTaxFrameworkConfigurationCollection(Factory.New<AccTaxOverrideGroup>());
		}

		public void TestModuleIDAttribute()
		{
			var attributes = (ModuleIDAttribute[])GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true);
			AssertEquals(ModuleIDs.AccChargeCode, attributes[0].ModuleIdentifier);
		}

		public void TestChargeCodesLinkedToTaxFrameworkConfigurationCollection()
		{
			AccTaxOverrideGroup overrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			var query = new ZQuery(AccTaxOverrideGroupChargeCodePivotSchema.ACP_AX_TaxOverrideGroup, overrideGroup.PK);
			var result = Factory.Load<AccTaxOverrideGroupChargeCodePivot>(query);

			AssertEquals("No pivot item yet", 0, result.Length);

			var collection = new ChargeCodesLinkedToTaxFrameworkConfigurationCollection(overrideGroup);
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			collection.Add(chargeCode1);

			result = Factory.Load<AccTaxOverrideGroupChargeCodePivot>(query);
			AssertEquals("1 item added to the collection", 1, result.Length);
			Assert("Pivot record present for chargeCode1", result.Any(x => x.ACP_AC_ChargeCode == chargeCode1.PK));

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			collection.Add(chargeCode2);

			result = Factory.Load<AccTaxOverrideGroupChargeCodePivot>(query);
			AssertEquals("2 items added to the collection", 2, result.Length);
			Assert("Pivot record present for chargeCode1", result.Any(x => x.ACP_AC_ChargeCode == chargeCode1.PK));
			Assert("Pivot record present for chargeCode2", result.Any(x => x.ACP_AC_ChargeCode == chargeCode2.PK));

			collection.RemoveFromRelationship(chargeCode2);

			result = Factory.Load<AccTaxOverrideGroupChargeCodePivot>(query);
			AssertEquals("1 item removed, so 1 item remaining in the collection", 1, result.Length);
			Assert("Pivot record for chargeCode2 removed from database", result.Any(x => x.ACP_AC_ChargeCode == chargeCode1.PK));

			var collection1 = new ChargeCodesLinkedToTaxFrameworkConfigurationCollection(overrideGroup);
			AssertEquals("Pivot item loaded into a new collection", 1, collection1.Count);
			AssertEquals(chargeCode1.PK, collection1[0].PK);
		}
	}
}
