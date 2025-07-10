using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconEntryOriginalChargeLookupsTest : TestCaseWithFactory
	{
		public void TestCY_SelectedRateTypeList()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			var charge = originalEntry.OriginalCharges.AddNew();
			var lookup = charge.Lookups;
			AssertNotNull(lookup.CY_SelectedRateTypeList);
		}

		public void TestCY_CodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, RefCusCodeListTypes.Codes.AccountingClassFeeCode);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				"124", "Pecan Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				"125", "Christmas Tree Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			var charge = originalEntry.OriginalCharges.AddNew();
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode("124"));
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode("125"));
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode("DTY"));
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode("ARS"));
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode("MPC"));

			reconDeclaration.Invoices.AddNew();
			var invoiceLine = reconDeclaration.InvoiceLines.AddNew();
			charge = invoiceLine.ReconOriginalCharges.AddNew();
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode("124"));
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode("125"));
		}
	}
}
