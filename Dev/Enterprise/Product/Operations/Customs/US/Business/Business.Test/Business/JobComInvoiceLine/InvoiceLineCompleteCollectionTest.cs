using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	sealed class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestUseJobDutyRateWhenRefreshingTariff()
		{
			//reference file 
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000000";
			tariff1.UE_DateFrom = new ZDateTime(2009, 1, 1);
			tariff1.UE_DateTo = new ZDateTime(2009, 6, 30);
			tariff1.UE_Unit1 = "KG";

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000000";
			tariff2.UE_DateFrom = new ZDateTime(2009, 7, 1);
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_Unit1 = "KG";
			tariff2.UE_Unit2 = "L";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 1);//this becomes duty date

			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("2nd UQ should be empty", ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);

			declaration.RefreshTariff();

			//should stay the same.
			AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("2nd UQ should be empty", ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);
		}

		public void TestClearCalculateException()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.TariffCalculateExceptionMessage = "Calculate Error";

			AssertEquals("Calculate Error", invoiceLine.TariffCalculateExceptionMessage);

			declaration.InvoiceLines.ClearCalculateException();

			Assert("Calculation exception cleared", invoiceLine.TariffCalculateExceptionMessage.IsEmpty);
		}

		public void TestTypedIndexer()
		{
			InvoiceLineCompleteCollection collection = new InvoiceLineCompleteCollection(Declaration);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestRefreshTariff()
		{
			CreateTestTariffIfNotExists("39393939", new ZDateTime(2009, 01, 01), new ZDateTime(2099, 12, 31), "", "", "", "TEST TARIFF");
			CreateTestTariffIfNotExists("45454545", new ZDateTime(2009, 01, 01), new ZDateTime(2099, 12, 31), ABIUnitOfMeasureList.Codes.MetricTon, "", "", "TARIFF UQ TO BE REMOVED");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "39393939";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "NO";
			invoiceLine1.JI_LinePrice = 243055m;
			AssertEquals("Customs UQ currently blank", "", invoiceLine1.JI_CustomsUnitQty);

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "45454545";
			invoiceLine2.JI_InvoiceQuantity = 50m;
			invoiceLine2.JI_InvoiceUQ = "T";
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.JI_CustomsQuantity = 50m;
			AssertEquals("Customs UQ currently Ton", "T", invoiceLine2.JI_CustomsUnitQty);
			AssertEquals("Customs Qty", 50m, invoiceLine2.JI_CustomsQuantity);

			UpdateTestTariff("39393939", new ZDateTime(2009, 01, 01), new ZDateTime(2099, 12, 31), ABIUnitOfMeasureList.Codes.Kilograms, AESUnitOfMeasureList.Codes.Liters, "", "OTHER MIXTURES");
			UpdateTestTariff("45454545", new ZDateTime(2009, 01, 01), new ZDateTime(2099, 12, 31), "", "", "", "TARIFF UQ REMOVED");

			declaration.RefreshTariff();

			AssertEquals("Line 1 Customs UQ should be updated", "KG", invoiceLine1.JI_CustomsUnitQty);
			AssertEquals("Line 1 Customs second UQ should be updated", "L", invoiceLine1.JI_CustomsSecondUnitQty);
			AssertEquals("Line 1 Invoice Qty shold not be affected", 1m, invoiceLine1.JI_InvoiceQuantity);
			AssertEquals("Line 1 Invoice price shold not be affected", 243055m, invoiceLine1.JI_LinePrice);

			AssertEquals("Line 2 Customs UQ should be updated", "", invoiceLine2.JI_CustomsUnitQty);
			AssertEquals("Line 2 Customs Qty should be cleared out as tariff no longer has UQ", 0m, invoiceLine2.JI_CustomsQuantity);
			AssertEquals("Line 2 Invoice Qty shold not be affected", 50m, invoiceLine2.JI_InvoiceQuantity);
			AssertEquals("Line 2 Invoice price shold not be affected", 500m, invoiceLine2.JI_LinePrice);
		}

		public void TestRefreshTariffAddingSecondaryLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9102111010";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "NO";
			var secondaryLine = invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.IsSecondaryTariffLine).ToList();
			secondaryLine.ForEach(x => x.Delete());
			invoiceLine1.JI_CustomsUnitQty = "";
			Factory.Save();
			declaration.RefreshTariff();
			Assert(invoice.JobComInvoiceLines.Count > 1);
		}

		public void TestReconInvoiceLineDefault()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice1 = originalEntry1.Invoice;
			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Grams;

			ReconOriginalEntryHeader originalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice2 = originalEntry2.Invoice;
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Hectograms;

			AssertEquals(2, reconDeclaration.FilteredInvoiceLines.Count);

			reconDeclaration.SelectedOriginalEntry = originalEntry1.CH_PK;
			AssertEquals(1, reconDeclaration.FilteredInvoiceLines.Count);
			AssertEquals(invoiceLine1.PK, reconDeclaration.FilteredInvoiceLines[0].PK);
			JobComInvoiceLine invoiceLine3 = reconDeclaration.FilteredInvoiceLines.AddNew();
			AssertEquals(invoice1.PK, invoiceLine3.JI_JZ);
			AssertEquals(originalEntry1.CH_PK, invoiceLine3.US_CH_ReconEntry);
			AssertEquals(Core.Constants.Weight.Grams, invoiceLine3.JI_WeightUQ);
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilotonnes;

			reconDeclaration.SelectedOriginalEntry = originalEntry2.CH_PK;
			AssertEquals(1, reconDeclaration.FilteredInvoiceLines.Count);
			AssertEquals(invoiceLine2.PK, reconDeclaration.FilteredInvoiceLines[0].PK);
			JobComInvoiceLine invoiceLine4 = reconDeclaration.FilteredInvoiceLines.AddNew();
			AssertEquals(invoice2.PK, invoiceLine4.JI_JZ);
			AssertEquals(originalEntry2.CH_PK, invoiceLine4.US_CH_ReconEntry);
			AssertEquals(Core.Constants.Weight.Hectograms, invoiceLine4.JI_WeightUQ);

			JobComInvoiceHeader invoice3 = originalEntry2.Invoice;
			invoiceLine4.JI_JZ = invoice3.PK;

			JobComInvoiceLine invoiceLine5 = reconDeclaration.FilteredInvoiceLines.AddNew();
			AssertEquals(invoice3.PK, invoiceLine5.JI_JZ);
			AssertEquals(originalEntry2.CH_PK, invoiceLine5.US_CH_ReconEntry);
			AssertEquals(Core.Constants.Weight.Hectograms, invoiceLine5.JI_WeightUQ);

			reconDeclaration.SelectedOriginalEntry = originalEntry1.CH_PK;
			JobComInvoiceLine invoiceLine6 = reconDeclaration.FilteredInvoiceLines.AddNew();
			AssertEquals(invoice1.PK, invoiceLine6.JI_JZ);
			AssertEquals(originalEntry1.CH_PK, invoiceLine6.US_CH_ReconEntry);
			AssertEquals(Core.Constants.Weight.Kilotonnes, invoiceLine6.JI_WeightUQ);
		}

		public void TestReconIssues()
		{
			var declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "USCHI";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var list = new List<ReconIssues>(declaration.InvoiceLines.ReconIssues);
			AssertEquals(0, list.Count);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "Z!Z2Z";
			part.RelatedOrganisations.AddOwner(declarationImporter);
			part.RelatedOrganisations.AddSupplier(declarationSupplier);

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "Z1Z!";
			classification.CC_TariffNum = "30000000";

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			pivot.CD_ReconIssue = ReconIssueCodeList.Codes.Class9802Recon;
			Factory.Save();

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			list = new List<ReconIssues>(declaration.InvoiceLines.ReconIssues);
			AssertEquals(1, list.Count);
			AssertEquals(ReconIssues.CL | ReconIssues._98, list[0]);
		}

		public void TestSaveNewProductsorActivateInactiveOnes()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ZZZ11Z@";
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "ADDRESS 1";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PRODUCT";
			product.RelatedOrganisations.AddOwner(org);
			product.RelatedOrganisations.AddSupplier(org);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6203434030";

			var relatedPivot = pivot.Children.AddNew();
			relatedPivot.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedPivot.CI_TariffNum = "6203434040";

			product.OP_IsActive = false;

			Factory.Save();

			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_OH_Importer = org.PK;
			testDec.JE_OH_Supplier = org.PK;
			var header = testDec.Invoices.AddNew();
			header.JZ_InvoiceNumber = "6";

			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "6203434030";
			line1.JI_PartNo = product.OP_PartNum;
			line1.JI_Description = "666";
			line1.JI_InvoiceQuantity = 1.0m;
			line1.JI_LinePrice = 1.0m;
			line1.JI_InvoiceUQ = "KG";

			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "6203434040";
			line2.JI_PartNo = product.OP_PartNum;
			line2.JI_Description = "777";
			line2.JI_InvoiceQuantity = 2.0m;
			line2.JI_LinePrice = 2.0m;
			line2.JI_InvoiceUQ = "KG";

			testDec.SaveNewProductsorActivateInactiveOnes();
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			Assert(product.OP_IsActive);
			AssertEquals("product.OP_Desc", "666", product.OP_Desc);
			AssertEquals("product.OP_StockKeepingUnit", "KG", product.OP_StockKeepingUnit);
			AssertEquals("pivot.IsDeleted", true, pivot.IsDeleted);
			AssertEquals("relatedPivot.IsDeleted", true, relatedPivot.IsDeleted);
			AssertEquals("product.PivotsForBinding.Count", 1, product.PivotsForBinding.Count);
			pivot = product.PivotsForBinding[0];
			AssertEquals("pivot.CI_ChildType", ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
			AssertEquals("pivot.CI_TariffNum", "6203434030", pivot.CI_TariffNum);
			AssertEquals(2, testDec.InvoiceLines.Count);
			AssertCollectionContains(line1, testDec.InvoiceLines);
			AssertEquals("line1.JI_Tariff", "6203434030", line1.JI_Tariff);
			AssertEquals("line1.JI_PartNo", product.OP_PartNum, line1.JI_PartNo);
			AssertEquals("line1.JI_OP", product.PK, line1.JI_OP);
			AssertEquals("line1.JI_Description", "666", line1.JI_Description);
			AssertEquals("line1.JI_InvoiceQuantity", 1.0m, line1.JI_InvoiceQuantity);
			AssertEquals("line1.JI_LinePrice", 1.0m, line1.JI_LinePrice);
			AssertEquals("line1.JI_InvoiceUQ", "KG", line1.JI_InvoiceUQ);
			AssertCollectionContains(line2, testDec.InvoiceLines);
			AssertEquals("line2.JI_Tariff", "6203434030", line2.JI_Tariff);
			AssertEquals("line2.JI_PartNo", product.OP_PartNum, line2.JI_PartNo);
			AssertEquals("line2.JI_OP", product.PK, line2.JI_OP);
			AssertEquals("line2.JI_Description", "777", line2.JI_Description);
			AssertEquals("line2.JI_InvoiceQuantity", 2.0m, line2.JI_InvoiceQuantity);
			AssertEquals("line2.JI_LinePrice", 2.0m, line2.JI_LinePrice);
			AssertEquals("line2.JI_InvoiceUQ", "KG", line2.JI_InvoiceUQ);
		}

		public void TestSetDefaultDrawbackClaimsValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine.US_DRWDeclaredVFD", 0m, invoiceLine.US_DRWDeclaredVFD);
			AssertEquals("invoiceLine.US_DRWDeclaredTax", 0m, invoiceLine.US_DRWDeclaredTax);
			AssertEquals("invoiceLine.US_DRWDeclaredHMF", 0m, invoiceLine.US_DRWDeclaredHMF);
			AssertEquals("invoiceLine.US_DRWDeclaredMPF", 0m, invoiceLine.US_DRWDeclaredMPF);
			AssertEquals("invoiceLine.US_DRWDeclaredOtherFees", 0m, invoiceLine.US_DRWDeclaredOtherFees);
			AssertEquals("invoiceLine.US_DRWDutyRate_New", 0m, invoiceLine.US_DRWDutyRate_New);
			AssertEquals("invoiceLine.US_DRWWeightedRatio", 0m, invoiceLine.US_DRWWeightedRatio);
			AssertEquals("invoiceLine.US_DRWMPFWeightedRatio", 0m, invoiceLine.US_DRWMPFWeightedRatio);
			AssertEquals("invoiceLine.US_DRWLineDuty", 0m, invoiceLine.US_DRWLineDuty);
		}

		public void TestGetInvoiceLinesWithTariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLines = declaration.InvoiceLines;

			var lines = invoiceLines.GetInvoiceLinesWithTariffType("NOT EXIST");
			AssertEquals(0, lines.Count());

			var invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.US_TariffType = TariffTypeList.Codes.HTS;
			var invoiceLine2 = invoiceLines.AddNew();
			invoiceLine2.US_TariffType = TariffTypeList.Codes.HTS;
			var invoiceLine3 = invoiceLines.AddNew();
			invoiceLine3.US_TariffType = TariffTypeList.Codes.ScheduleB;

			lines = invoiceLines.GetInvoiceLinesWithTariffType(TariffTypeList.Codes.HTS);
			AssertEquals(2, lines.Count());

			lines = invoiceLines.GetInvoiceLinesWithTariffType(TariffTypeList.Codes.ScheduleB);
			AssertEquals(1, lines.Count());
		}

		public void TestFetchValidationForMultipleTariffs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");  // TariffType
			Factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038801", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			for (var i = 0; i <= 100; i++)
			{
				var tariffNumber = "1234567" + i.ToString().PadLeft(3, '0');
				USCTariffTest.CreateTariff(Factory, tariffNumber);
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = tariffNumber;
			}

			Factory.ResetDatabaseLoadCount();
			declaration.InvoiceLines.FetchStrategy.FetchForValidate();
			AssertEquals(0, Factory.ActiveFetchHintsForTable(TariffAttributeViewSchema.Constants.TableName));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

		protected override BaseJobDeclaration GetMeANewJobDeclaration() => Factory.New<JobDeclaration>();

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		USCTariff CreateTestTariffIfNotExists(ZString tariffCode, ZDateTime dateFrom, ZDateTime dateTo, ZString unit1, ZString unit2, ZString unit3, ZString shortDescription)
		{
			ZQuery query = new ZQuery(USCTariffSchema.UE_Tariff, tariffCode);
			query.AddToFilter(USCTariffSchema.UE_DateFrom, dateFrom);
			query.AddToFilter(USCTariffSchema.UE_DateTo, dateTo);
			USCTariff tariff = Factory.LoadTop1<USCTariff>(query);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = tariffCode;
				tariff.UE_DateFrom = dateFrom;
				tariff.UE_DateTo = dateTo;
				tariff.UE_Unit1 = unit1;
				tariff.UE_Unit2 = unit2;
				tariff.UE_Unit3 = unit3;
				tariff.UE_ShortDescription = shortDescription;
			}

			return tariff;
		}

		void UpdateTestTariff(ZString tariffCode, ZDateTime dateFrom, ZDateTime dateTo, ZString unit1, ZString unit2, ZString unit3, ZString shortDescription)
		{
			ZQuery query = new ZQuery(USCTariffSchema.UE_Tariff, tariffCode);
			query.AddToFilter(USCTariffSchema.UE_DateFrom, dateFrom);
			query.AddToFilter(USCTariffSchema.UE_DateTo, dateTo);
			USCTariff tariff = Factory.LoadTop1<USCTariff>(query);
			if (tariff != null)
			{
				tariff.UE_Unit1 = unit1;
				tariff.UE_Unit2 = unit2;
				tariff.UE_Unit3 = unit3;
				tariff.UE_ShortDescription = shortDescription;
			}
		}
	}
}
