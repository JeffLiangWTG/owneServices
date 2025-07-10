using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DebtorToSelectFromForPrinting))]
	sealed class DebtorToSelectFromForPrintingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();

			return new DebtorToSelectFromForPrinting(debtor);
		}

		#region Properties

		public void TestDebtor()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			DebtorToSelectFromForPrinting debtorOrg = new DebtorToSelectFromForPrinting(header);
			AssertEquals("Debtor should be the same as Header", header.PK, debtorOrg.Debtor.PK);
		}

		public void TestOH_Calc_PrintDebtor()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			DebtorToSelectFromForPrinting debtorOrg = new DebtorToSelectFromForPrinting(header);
			debtorOrg.OH_Calc_PrintDebtor = ZBool.True;
			AssertEquals(ZBool.True, debtorOrg.OH_Calc_PrintDebtor);

			debtorOrg.OH_Calc_PrintDebtor = ZBool.False;
			AssertEquals(ZBool.False, debtorOrg.OH_Calc_PrintDebtor);
		}

		#endregion

	}
}
