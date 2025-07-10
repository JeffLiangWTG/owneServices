using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryHeaderChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckC1_ChargeType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var charge1 = entry.Charges.AddNew("A00");
			var charge2 = entry.Charges.AddNew();
			charge2.C1_IsLandedCostOnly = true;
			charge2.C1_ChargeType = "B00";

			CombineAssertions(() =>
			{
				var charge3 = entry.Charges.AddNew();
				charge3.C1_ChargeType = "A00";
				AssertHasErrorContaining("A00 is duplicate there should be an error.", charge3.C1_ChargeTypeInfo, "is not unique in this list");

				charge3.C1_ChargeType = "B00";
				AssertNoErrorContaining("Can have LandedCostOnly charges with the same type", charge3.C1_ChargeTypeInfo, "is not unique in this list");

				charge3.C1_ChargeType = "C00";
				AssertNoErrorContaining("C00: no existed C00 Charges so no error", charge3.C1_ChargeTypeInfo, "is not unique in this list");
			});
		}
	}
}
