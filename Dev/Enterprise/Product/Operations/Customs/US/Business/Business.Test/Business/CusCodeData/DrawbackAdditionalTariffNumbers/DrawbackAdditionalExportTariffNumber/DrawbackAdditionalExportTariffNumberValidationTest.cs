using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DrawbackAdditionalExportTariffNumberValidationTest : TestCaseWithFactory
	{
		public void TestCheckCY_Code()
		{
			DrawbackAdditionalExportTariffNumber bizObj = Factory.New<DrawbackAdditionalExportTariffNumber>();
			bizObj.RunPreSaveValidation();
			AssertNoErrors(bizObj.CY_CodeInfo);
		}

		public void TestCheckCY_Data()
		{
			const string invalidTariffCode = "111122XX";
			const string validTariffCode = "111122YY";
			var drawbackAdditionalExportTariffNumber = InvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew();
			drawbackAdditionalExportTariffNumber.CY_Data = invalidTariffCode;
			AssertNull("Tarriff should not exist", new Universal.TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, invalidTariffCode, ZDateTime.Today));
			AssertHasMessageErrorContaining(drawbackAdditionalExportTariffNumber.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, validTariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			scheduleB.ZZ1_Description = "SCHEDB ITEM";
			helper.CreateTariffUOM(scheduleB, "CU1", "KG");
			drawbackAdditionalExportTariffNumber.CY_Data = "";
			drawbackAdditionalExportTariffNumber.CY_Data = validTariffCode;
			AssertNoMessageErrorContaining(drawbackAdditionalExportTariffNumber.CY_DataInfo, ListValidation.InvalidCodeMessageError);
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
	}
}
