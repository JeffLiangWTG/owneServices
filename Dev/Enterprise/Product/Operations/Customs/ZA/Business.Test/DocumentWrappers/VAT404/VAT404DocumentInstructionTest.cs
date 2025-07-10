using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VAT404DocumentInstruction))]
	sealed class VAT404DocumentInstructionTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2016, 9, 25)]
		public void TestDefaultDate()
		{
			VAT404TestHelper.SetupPayInfo(Factory);
			var tester = new VAT404DocumentInstruction(Factory);
			AssertEquals(new ZDateTime(2016, 09, 01), tester.StartDate);
			AssertEquals(new ZDateTime(2016, 09, 25), tester.EndDate);
		}

		[TestDate(2016, 9, 25)]
		public void TestSearching()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "000001";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "000002";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "000003";

			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_OH_Importer = org1.PK;
			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_OH_Importer = org2.PK;
			var dec3 = Factory.NewWithValidTestData<JobDeclaration>();
			dec3.JE_OH_Importer = org3.PK;

			var entry1 = dec1.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "LRN0001";
			var entry2 = dec2.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "LRN0002";
			var entry3 = dec3.ActiveEntryHeaders.AddNew();
			entry3.CH_BGMReference = "LRN0003";
			var entry4 = dec3.ActiveEntryHeaders.AddNew();
			entry4.CH_BGMReference = "LRN0004";

			VAT404TestHelper.AddPayInfo(entry1, "DTY", 1m, "RPT1", new ZDateTime(2016, 09, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry1, "VAT", -2m, "RPT3", new ZDateTime(2016, 09, 25), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry1, "VAT", 3m, "", new ZDateTime(2016, 09, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry1, "VAT", 4m, "", new ZDateTime(2016, 09, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry1, "VAT", 0m, "RPT2", new ZDateTime(2016, 09, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry1, "VAT", 5m, "RPT2", new ZDateTime(2016, 02, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry1, "VAT", 6m, "RPT3", new ZDateTime(2016, 09, 05), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry1, "VAT", 7m, "RPT4", new ZDateTime(2016, 09, 26), new ZDateTime(2016, 09, 01));

			VAT404TestHelper.AddPayInfo(entry3, "DTY", 11m, "RPT1", new ZDateTime(2016, 09, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry3, "VAT", -12m, "RPT3", new ZDateTime(2016, 09, 25), new ZDateTime(2016, 09, 02));
			VAT404TestHelper.AddPayInfo(entry3, "VAT", 13m, "", new ZDateTime(2016, 09, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry3, "VAT", 14m, "", new ZDateTime(2016, 09, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry3, "VAT", 00m, "RPT2", new ZDateTime(2016, 09, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry3, "VAT", 15m, "RPT2", new ZDateTime(2016, 02, 01), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry4, "VAT", 16m, "RPT3", new ZDateTime(2016, 09, 02), new ZDateTime(2016, 09, 01));
			VAT404TestHelper.AddPayInfo(entry4, "VAT", 17m, "RPT4", new ZDateTime(2016, 09, 26), new ZDateTime(2016, 09, 01));

			Factory.Save();

			var tester = new VAT404DocumentInstruction(Factory);

			CombineAssertions(() =>
			{
				tester.PerformSearch();
				AssertEquals(2, tester.VAT404Documents.Count);
				AssertEquals("000001", tester.VAT404Documents[0].Importer.OH_Code);
				AssertEquals("000003", tester.VAT404Documents[1].Importer.OH_Code);
				AssertEquals(2, tester.VAT404Documents[0].ProofOfPayments.Count);
				AssertEquals(2, tester.VAT404Documents[1].ProofOfPayments.Count);
				AssertEquals(6m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);
				AssertEquals(-2m, tester.VAT404Documents[0].ProofOfPayments[1].C9_PaymentAmount);
				AssertEquals(16m, tester.VAT404Documents[1].ProofOfPayments[0].C9_PaymentAmount);
				AssertEquals(-12m, tester.VAT404Documents[1].ProofOfPayments[1].C9_PaymentAmount);
			});

			tester.EndDate = new ZDateTime(2016, 09, 24);
			tester.PerformSearch();
			AssertEquals(2, tester.VAT404Documents.Count);
			AssertEquals("000001", tester.VAT404Documents[0].Importer.OH_Code);
			AssertEquals("000003", tester.VAT404Documents[1].Importer.OH_Code);
			AssertEquals(1, tester.VAT404Documents[0].ProofOfPayments.Count);
			AssertEquals(1, tester.VAT404Documents[1].ProofOfPayments.Count);
			AssertEquals(6m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);
			AssertEquals(16m, tester.VAT404Documents[1].ProofOfPayments[0].C9_PaymentAmount);

			tester.EndDate = new ZDateTime(2016, 09, 28);
			tester.ImporterForFilter.AddNew().ImporterPK = org2.PK;
			tester.PerformSearch();
			AssertEquals(0, tester.VAT404Documents.Count);

			tester.EndDate = new ZDateTime(2016, 09, 28);
			tester.ImporterForFilter.AddNew().ImporterPK = org1.PK;
			tester.PerformSearch();
			AssertEquals(1, tester.VAT404Documents.Count);
			AssertEquals("000001", tester.VAT404Documents[0].Importer.OH_Code);
			AssertEquals(3, tester.VAT404Documents[0].ProofOfPayments.Count);
			AssertEquals(6m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);
			AssertEquals(-2m, tester.VAT404Documents[0].ProofOfPayments[1].C9_PaymentAmount);
			AssertEquals(7m, tester.VAT404Documents[0].ProofOfPayments[2].C9_PaymentAmount);

			tester.StartDate = new ZDateTime(2016, 09, 06);
			tester.ImporterForFilter.RemoveAll();
			tester.ImporterForFilter.AddNew().ImporterPK = org3.PK;
			tester.PerformSearch();
			AssertEquals(1, tester.VAT404Documents.Count);
			AssertEquals("000003", tester.VAT404Documents[0].Importer.OH_Code);
			AssertEquals(2, tester.VAT404Documents[0].ProofOfPayments.Count);
			AssertEquals(-12m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);
			AssertEquals(17m, tester.VAT404Documents[0].ProofOfPayments[1].C9_PaymentAmount);

			tester.LocalReferenceNumbersForFilter.RemoveAll();
			tester.LocalReferenceNumbersForFilter.AddNew().LocalReferenceNumber = "LRN0004";
			tester.PerformSearch();
			AssertEquals(1, tester.VAT404Documents.Count);
			AssertEquals("000003", tester.VAT404Documents[0].Importer.OH_Code);
			AssertEquals(1, tester.VAT404Documents[0].ProofOfPayments.Count);
			AssertEquals(17m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);

			tester.LocalReferenceNumbersForFilter.AddNew().LocalReferenceNumber = "LRN0003";
			tester.PerformSearch();
			AssertEquals(1, tester.VAT404Documents.Count);
			AssertEquals("000003", tester.VAT404Documents[0].Importer.OH_Code);
			AssertEquals(2, tester.VAT404Documents[0].ProofOfPayments.Count);
			AssertEquals(-12m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);
			AssertEquals(17m, tester.VAT404Documents[0].ProofOfPayments[1].C9_PaymentAmount);

			tester.ReceiptNumbersForFilter.AddNew().ReceiptNumber = "RPT3";
			tester.PerformSearch();
			AssertEquals(1, tester.VAT404Documents.Count);
			AssertEquals("000003", tester.VAT404Documents[0].Importer.OH_Code);
			AssertEquals(1, tester.VAT404Documents[0].ProofOfPayments.Count);
			AssertEquals(-12m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);

			tester.ReceiptNumbersForFilter.AddNew().ReceiptNumber = "XXX";
			tester.PerformSearch();
			AssertEquals(1, tester.VAT404Documents.Count);
			AssertEquals("000003", tester.VAT404Documents[0].Importer.OH_Code);
			AssertEquals(1, tester.VAT404Documents[0].ProofOfPayments.Count);
			AssertEquals(-12m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);

			tester.ReceiptNumbersForFilter.AddNew().ReceiptNumber = "RPT4";
			tester.PerformSearch();
			AssertEquals(1, tester.VAT404Documents.Count);
			AssertEquals("000003", tester.VAT404Documents[0].Importer.OH_Code);
			AssertEquals(2, tester.VAT404Documents[0].ProofOfPayments.Count);
			AssertEquals(-12m, tester.VAT404Documents[0].ProofOfPayments[0].C9_PaymentAmount);
			AssertEquals(17m, tester.VAT404Documents[0].ProofOfPayments[1].C9_PaymentAmount);
		}

		[TestDate(2016, 9, 25)]
		public void TestCheckDates()
		{
			var tester = new VAT404DocumentInstruction(Factory);
			AssertNoErrors(tester.StartDateInfo);
			AssertNoMessageErrors(tester.StartDateInfo);
			AssertNoWarnings(tester.StartDateInfo);
			AssertNoErrors(tester.EndDateInfo);
			AssertNoMessageErrors(tester.EndDateInfo);
			AssertNoWarnings(tester.EndDateInfo);

			tester.StartDate = ZDateTime.Empty;
			AssertHasErrorContaining(tester.StartDateInfo, "Please enter a Transaction Period Start Date.");

			tester.StartDate = new ZDateTime(2016, 9, 26);
			AssertNoErrors(tester.StartDateInfo);
			AssertHasMessageErrorContaining(tester.StartDateInfo, "Period Start Date shouldn't be greater than Today");

			tester.StartDate = new ZDateTime(2016, 9, 20);
			AssertNoErrors(tester.StartDateInfo);
			AssertNoMessageErrors(tester.StartDateInfo);
			AssertNoWarnings(tester.StartDateInfo);

			tester.EndDate = ZDateTime.Empty;
			AssertHasErrorContaining(tester.EndDateInfo, "Please enter a Transaction Period End Date.");

			tester.EndDate = new ZDateTime(2016, 9, 26);
			AssertNoErrors(tester.EndDateInfo);
			AssertHasWarningContaining(tester.EndDateInfo, "Period End Date shouldn't be greater than Today");

			tester.EndDate = new ZDateTime(2016, 9, 20);
			AssertNoErrors(tester.EndDateInfo);
			AssertNoMessageErrors(tester.EndDateInfo);
			AssertNoWarnings(tester.EndDateInfo);

			tester.EndDate = new ZDateTime(2016, 9, 19);
			AssertNoErrors(tester.EndDateInfo);
			AssertHasMessageErrorContaining(tester.EndDateInfo, "Period End Date shouldn't be before the Start Date");
		}
	}
}
