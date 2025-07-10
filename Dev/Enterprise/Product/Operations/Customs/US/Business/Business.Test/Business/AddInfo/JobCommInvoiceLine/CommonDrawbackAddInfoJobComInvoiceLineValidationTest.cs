using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	class CommonDrawbackAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckUS_ImportEntryNo()
		{
			MakeImportLine();
			InvoiceLine.US_ImportEntryNo = "ABC1234567";
			AssertHasMessageErrorContaining(InvoiceLine.US_ImportEntryNoInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ImportEntryLength);
			InvoiceLine.US_ImportEntryNo = "ABC12345678";
			AssertNoWarningContaining(InvoiceLine.US_ImportEntryNoInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ImportEntryLength);
			InvoiceLine.US_ImportEntryNo = "";
			AssertNoWarningContaining(InvoiceLine.US_ImportEntryNoInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ImportEntryLength);
		}

		public void TestCheckUS_ExportTariff()
		{
			var invalidTariffCode = "11112288";
			var validTariffCode = "11112299";
			MakeExportLine();
			InvoiceLine.US_ExportTariff = invalidTariffCode;
			AssertNull("Tarriff should not exist", new Universal.TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, invalidTariffCode, ZDateTime.Today));
			AssertHasMessageErrorContaining(InvoiceLine.US_ExportTariffInfo, ListValidation.InvalidCodeMessageError);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, validTariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			scheduleB.ZZ1_Description = "SCHEDULB ITEM";
			helper.CreateTariffUOM(scheduleB, "CU1", "KG");
			InvoiceLine.US_ExportTariff = "";
			InvoiceLine.US_ExportTariff = validTariffCode;
			AssertNoMessageErrorContaining(InvoiceLine.US_ExportTariffInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_PetroleumClaimInd = true;
			InvoiceLine.US_ExportTariff = "111111";
			AssertHasMessageErrorContaining(InvoiceLine.US_ExportTariffInfo, CommonDrawbackJobComInvoiceLineValidation.PetroliumTariffValidation);
			InvoiceLine.US_ExportTariff = "11111111";
			AssertNoMessageErrorContaining(InvoiceLine.US_ExportTariffInfo, CommonDrawbackJobComInvoiceLineValidation.PetroliumTariffValidation);
			declaration.US_PetroleumClaimInd = false;
			InvoiceLine.US_ExportTariff = "111111";
			AssertNoMessageErrorContaining(InvoiceLine.US_ExportTariffInfo, CommonDrawbackJobComInvoiceLineValidation.PetroliumTariffValidation);
		}

		public void TestCheckCheckUS_DRWExportAction()
		{
			MakeExportLine();
			InvoiceLine.US_DRWExportAction = "";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportAction();
			AssertNoMessageErrors(InvoiceLine.US_DRWExportActionInfo);
			InvoiceLine.US_DRWExportAction = "X";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportActionInfo, ListValidation.InvalidCodeMessageError);
			CodeDescriptionPairList list = InvoiceLine.Lookups.DRWExportActionList;
			foreach (CodeDescriptionPair pair in list)
			{
				InvoiceLine.US_DRWExportAction = pair.Code;
				AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportActionInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_DRWExportDate()
		{
			MakeExportLine();
			InvoiceLine.US_DRWExportDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_DRWExportDateGreaterThanClaimDate()
		{
			MakeExportLine();
			declaration.US_EstimatedEntryDate = ZDateTime.Now;
			InvoiceLine.US_DRWExportDate = ZDateTime.Now.AddYears(-4);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanClaimDate);
			InvoiceLine.US_DRWExportDate = ZDateTime.Now.AddYears(-2);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanClaimDate);
		}

		public void TestDifferenceBetweenImportAndExport()
		{
			MakeExportLine();
			InvoiceLine.US_DRWEntryDate = ZDateTime.Now.AddYears(-4);
			InvoiceLine.US_DRWExportDate = ZDateTime.Now;
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanImportDate);
			InvoiceLine.US_DRWEntryDate = ZDateTime.Now.AddYears(-1);
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportDate();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanImportDate);
		}

		public void TestDifferenceBetweenImportAndExportWhenTFTEA()
		{
			MakeExportLine();
			declaration.US_EntryType = declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			InvoiceLine.US_DRWEntryDate = ZDateTime.Now.AddYears(-4);
			InvoiceLine.US_DRWExportDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanImportDateWhenTFTEA);
			InvoiceLine.US_DRWEntryDate = ZDateTime.Now.AddYears(-6);
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportDate();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanImportDateWhenTFTEA);
			InvoiceLine.US_DRWEntryDate = ZDateTime.Now.AddYears(-1);
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportDate();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanImportDateWhenTFTEA);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
		}

		protected JobDeclaration declaration;

		JobComInvoiceLine invoiceLine;
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var invoice = declaration.Invoices.AddNew();
					invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}

		protected void MakeImportLine()
		{
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWIsForExportSection = false;
		}

		protected void MakeExportLine()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.US_DRWIsForExportSection = true;
		}
	}
}
