using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DutyFeeDateCalculatorTest : TestCaseWithFactory
	{
		public void TestGetDateForADD_CVD()
		{
			declaration.US_EntryDate = new ZDateTime(2009, 9, 11);
			AssertEquals(new ZDate(2009, 9, 11), invoiceLine.DateForADD_CVD);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 9, 18);
			AssertEquals(new ZDate(2009, 9, 11), invoiceLine.DateForADD_CVD);

			declaration.US_EstimatedEntryDate = new ZDateTime(2009, 9, 17);
			AssertEquals(new ZDate(2009, 9, 17), invoiceLine.DateForADD_CVD);

			declaration.US_ITDate = new ZDateTime(2009, 9, 14);
			AssertEquals(new ZDate(2009, 9, 17), invoiceLine.DateForADD_CVD);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2009, 9, 16);
			AssertEquals(new ZDate(2009, 9, 17), invoiceLine.DateForADD_CVD);

			declaration.US_EstimatedEntryDate = ZDateTime.Empty;
			AssertEquals("from release date", new ZDate(2009, 9, 16), invoiceLine.DateForADD_CVD);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_PresentationDate = new ZDateTime(2009, 9, 15);
			AssertEquals("Presentation date should not affect ADD/CVD", new ZDate(2009, 9, 16), invoiceLine.DateForADD_CVD);
		}

		public void TestGetDutyFeeDateForHMF()
		{
			AssertEquals(ZDateTime.Today, new DutyFeeDateCalculator().GetDutyFeeDateForHMF(declaration));

			declaration.US_EntryDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1).Date, new DutyFeeDateCalculator().GetDutyFeeDateForHMF(declaration));

			declaration.JE_DateOfArrival = ZDateTime.Empty;
			declaration.US_ITDate = ZDateTime.BrettsBirthday.AddDays(2);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(2).Date, new DutyFeeDateCalculator().GetDutyFeeDateForHMF(declaration));

			declaration.JE_DateOfArrival = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday.Date, new DutyFeeDateCalculator().GetDutyFeeDateForHMF(declaration));
		}

		[TestDate(2007, 1, 31)]
		public void TestGetDutyFeeDateForMPF()
		{
			//for border shipments
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			AssertEquals("IsBorder movement", true, declaration.IsBorderMovement);

			AssertEquals(ZDate.Today, new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			ZDate tomorrow = ZDate.Today.AddDays(1);
			declaration.US_EntryDate = tomorrow;
			AssertEquals(tomorrow, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 1, 2);
			AssertEquals(new ZDate(2007, 1, 2), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2007, 1, 3);
			AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			declaration.US_PresentationDate = new ZDateTime(2007, 1, 5);
			AssertEquals("Presentation Date should not affect the calculation for duty date", new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 4);
			AssertEquals("CS00241661: Estimated entry date takes precedence", new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			//for non-border shipments
			declaration.US_ITDate = new ZDateTime(2007, 1, 6);
			SetUpChildrenAndDataForDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			AssertEquals("IsBorder movement", false, declaration.IsBorderMovement);

			AssertEquals(ZDate.Today, new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			declaration.US_EntryDate = tomorrow;
			AssertEquals(tomorrow, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 1, 2);
			AssertEquals(new ZDate(2007, 1, 2), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2007, 1, 4);
			AssertEquals(new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			declaration.US_PresentationDate = new ZDateTime(2007, 1, 5);
			AssertEquals("Presentation Date should not affect calculation for duty date", new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 3);
			AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			declaration.US_ITDate = new ZDateTime(2007, 1, 6);
			AssertEquals("IT Date should not affect calculation for MPF", new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			entry.US_MPFCalcDate = new ZDate(2007, 2, 4);
			AssertEquals(new ZDate(2007, 2, 4), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			using (declaration.ReCalculateMPFAndDutyDate())
			{
				AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));
			}

			declaration.US_ImmediateDelivery = true;
			AssertEquals(new ZDate(2007, 1, 31), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals(new ZDate(2007, 2, 4), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));

			using (declaration.ReCalculateMPFAndDutyDate())
			{
				AssertEquals(new ZDate(2007, 1, 31), new DutyFeeDateCalculator().GetDutyFeeDateForMPF(declaration));
			}
		}

		public void TestGetDutyFeeDateForReconciliationMPFCalculation()
		{
			var reconWrappedDeclaration = Factory.New<JobDeclaration>();
			reconWrappedDeclaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			reconWrappedDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			invoice = reconWrappedDeclaration.Invoices.AddNew();
			invoiceLine = reconWrappedDeclaration.InvoiceLines.AddNew();
			reconWrappedDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = reconWrappedDeclaration.CustomsEntryHeaders[0];

			var reconDeclaration = new ReconDeclaration(reconWrappedDeclaration);
			var reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + "123456";
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.Today.AddDays(-2);
			AssertEquals("Recon Date for MPF calculation", ZDateTime.Today.AddDays(-2), reconOriginalEntry.US_R_DateForMPFCalc);

			declaration.US_EntryDate = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var ensEntry2 = declaration.CustomsEntryHeaders[0];

			var reconOriginalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry2.CH_OrigEntryReference = "XJ5" + ensEntry2.EntryNumber;
			AssertEquals("Recon Date for MPF calculation", ZDateTime.Today.AddDays(1), reconOriginalEntry2.US_R_DateForMPFCalc);
		}

		public void TestGetDutyFeeDateForQuotaAndWithdrawalEntry()
		{
			//for border shipments
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			AssertEquals("IsQuota", true, invoiceLine.IsQuota);
			AssertEquals("IsBorder movement", true, declaration.IsBorderMovement);

			AssertEquals(ZDate.Today, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			ZDate tomorrow = ZDate.Today.AddDays(1);
			declaration.US_EntryDate = tomorrow;
			AssertEquals(tomorrow, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 1, 2);
			AssertEquals(new ZDate(2007, 1, 2), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2007, 1, 3);
			AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;

			declaration.US_EnableCRL = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2007, 1, 3);
			AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PresentationDate = new ZDateTime(2007, 1, 5);
			AssertEquals("Presentation Date should not affect the calculation for duty date", new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 4);
			AssertEquals(new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			//for non-border shipments
			declaration.US_ITDate = new ZDateTime(2007, 1, 6);
			SetUpChildrenAndDataForDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;

			AssertEquals(ZDate.Today, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_EntryDate = tomorrow;
			AssertEquals(tomorrow, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 1, 2);
			AssertEquals(new ZDate(2007, 1, 2), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2007, 1, 4);
			AssertEquals(new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 3);
			AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PresentationDate = new ZDateTime(2007, 1, 5);
			AssertEquals("Presentation Date should not affect the calculation for duty date", new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_ITDate = new ZDateTime(2007, 1, 6);
			AssertEquals("IT Date should not affect calculation for withdrawal Entries", new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));
		}

		public void TestGetDutyFeeDateForNonQuotaNonWithdrawalBorderEntry()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			AssertEquals("IsBorder movement", true, declaration.IsBorderMovement);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("IsQuota", false, invoiceLine.IsQuota);

			AssertEquals(ZDate.Today, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			ZDate tomorrow = ZDate.Today.AddDays(1);
			declaration.US_EntryDate = tomorrow;
			AssertEquals(tomorrow, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 1, 2);
			AssertEquals(new ZDate(2007, 1, 2), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2007, 1, 3);
			AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PresentationDate = new ZDateTime(2007, 1, 5);
			AssertEquals("Presentation Date should not affect the calculation for duty date", new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 4);
			AssertEquals(new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_ITDate = new ZDateTime(2007, 1, 6);
			AssertEquals("IT Date for normal entries", new ZDate(2007, 1, 6), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));
		}

		public void TestGetDutyFeeDateForNonQuotaNonWithdrawalNonBorderEntry()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			AssertEquals("IsBorder movement", false, declaration.IsBorderMovement);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("IsQuota", false, invoiceLine.IsQuota);

			AssertEquals(ZDate.Today, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			ZDate tomorrow = ZDate.Today.AddDays(1);
			declaration.US_EntryDate = tomorrow;
			AssertEquals(tomorrow, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 1, 2);
			AssertEquals(new ZDate(2007, 1, 2), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2007, 1, 4);
			AssertEquals(new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 3);
			AssertEquals(new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PresentationDate = new ZDateTime(2007, 1, 5);
			AssertEquals("Presentation Date should not affect the calculation for duty date", new ZDate(2007, 1, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_ITDate = new ZDateTime(2007, 1, 6);
			AssertEquals("IT Date for normal entries", new ZDate(2007, 1, 6), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));
		}

		public void TestCalculateForNonMergedInvoiceLine()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(ZDate.Today, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			ZDate tomorrow = ZDate.Today.AddDays(1);
			declaration.US_EntryDate = tomorrow;
			AssertEquals(tomorrow, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 1, 2);
			AssertEquals(new ZDate(2007, 1, 2), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 4);
			AssertEquals(new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_PresentationDate = new ZDateTime(2007, 1, 5);
			AssertEquals("Presentation Date should not affect the calculation for duty date", new ZDate(2007, 1, 4), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EstimatedEntryDate = ZDateTime.Empty;
			AssertEquals(tomorrow, new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.US_ITDate = new ZDateTime(2007, 1, 6);
			AssertEquals("IT Date for normal entries", new ZDate(2007, 1, 6), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));
		}

		public void TestGetDutyFeeDateReCalculate()
		{
			SetUpChildrenAndDataForDeclaration();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 10, 3);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			AssertEquals(true, declaration.IsExWarehouse);
			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			mainSender.OnSave += Factory.Save;

			AssertEquals(false, mainSender.SendMessage());
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(new ZDate(2018, 10, 3), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));
			AssertEquals(new ZDate(2018, 10, 3), entry.US_DutyCalcDate);
			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 10, 4);
			AssertEquals(false, mainSender.SendMessage());
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(new ZDate(2018, 10, 4), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));
			AssertEquals(new ZDate(2018, 10, 4), entry.US_DutyCalcDate);
		}

		[TestDate(2024, 7, 1)]
		public void TestGetDutyFeeDateForImmediateDelivery()
		{
			SetUpChildrenAndDataForDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_ImmediateDelivery = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals(new ZDate(2024, 7, 1), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			var summary = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			summary.US_DutyCalcDate = new ZDate(2024, 6, 30);
			summary.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals(new ZDate(2024, 6, 30), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			AssertEquals(new ZDate(2024, 6, 30), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));

			using (declaration.ReCalculateMPFAndDutyDate())
			{
				AssertEquals(new ZDate(2024, 7, 1), new DutyFeeDateCalculator().GetDutyFeeDate(declaration));
			}
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entry;

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			SetUpChildrenAndDataForDeclaration();
		}

		void SetUpChildrenAndDataForDeclaration()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
		}
	}
}
