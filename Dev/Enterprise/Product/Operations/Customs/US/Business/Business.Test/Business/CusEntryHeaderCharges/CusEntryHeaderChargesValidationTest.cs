using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryHeaderChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryHeaderCharges()
		{
			CusEntryHeaderCharges parent = Factory.New<CusEntryHeaderCharges>();
			AssertEquals(parent.Validation.EntryHeaderCharges, parent);
		}

		public void TestCheckC1_ChargeType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			var charge1 = entry.Charges.AddNew("AAA", 10m);
			var charge2 = entry.Charges.AddNew();
			charge2.C1_ChargeType = "AAA";
			AssertHasMessageError(charge2.C1_ChargeTypeInfo, CusEntryHeaderChargesValidation.CodeCannotBeDuplicated);
			charge2.C1_ChargeType = "BBB";
			AssertNoMessageError(charge2.C1_ChargeTypeInfo, CusEntryHeaderChargesValidation.CodeCannotBeDuplicated);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			charge2.C1_ChargeType = "AAA";
			AssertNoMessageError(charge2.C1_ChargeTypeInfo, CusEntryHeaderChargesValidation.CodeCannotBeDuplicated);
		}
	}
}
