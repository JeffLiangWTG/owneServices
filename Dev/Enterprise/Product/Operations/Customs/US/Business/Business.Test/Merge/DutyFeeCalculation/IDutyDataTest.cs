using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IDutyDataExtensionMethodTest : TestCaseWithFactory
	{
		public void TestIDutyDataExtensionMethodForCombinedLines()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 1000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntryHeader = testJob.ActiveEntryHeaders.EntrySummaryEntry;
			var entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			var entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			var entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);

			Assert(entryline1.IsCombinedLine());
			Assert(entryline2.IsCombinedLine());
			Assert(entryline3.IsCombinedLine());
			Assert(invoiceLine1.IsCombinedLine());
			Assert(invoiceLine1.IsCombinedLine());

			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);
			Assert(entryline1.IsCombinedLine());
			Assert(entryline2.IsCombinedLine());
			Assert(entryline3.IsCombinedLine());

			Assert(!entryline1.IsNormalTariffLine());
			Assert(!entryline2.IsNormalTariffLine());
			Assert(entryline3.IsNormalTariffLine());

			var invoiceLine3 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038501Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entryline4 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine3.US_SupTariff);
			Assert(entryline4.IsCombinedLine());
			Assert(!entryline4.IsNormalTariffLine());
		}

		public void TestIEntryLineOrInvoiceLineDutyDataExtensionMethodForCombinedLines()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 1000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntryHeader = testJob.ActiveEntryHeaders.EntrySummaryEntry;
			var entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			var entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			var entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);

			var combinedLines = new EntryLineIEntryLineOrInvoiceLineDutyData(entryline1).GetCombineLines();
			AssertEquals(combinedLines.Count(), 3);
			Assert(combinedLines.Any(x => x.Tariff == entryline1.CL_AdValoremTariff));
			Assert(combinedLines.Any(x => x.Tariff == entryline2.CL_AdValoremTariff));
			Assert(combinedLines.Any(x => x.Tariff == entryline3.CL_AdValoremTariff));

			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entryline1 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine1.US_SupTariff);
			entryline2 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.US_SupTariff);
			entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);

			combinedLines = new EntryLineIEntryLineOrInvoiceLineDutyData(entryline1).GetCombineLines();
			AssertEquals(combinedLines.Count(), 3);
			Assert(combinedLines.Any(x => x.Tariff == entryline1.CL_AdValoremTariff));
			Assert(combinedLines.Any(x => x.Tariff == entryline2.CL_AdValoremTariff));
			Assert(combinedLines.Any(x => x.Tariff == entryline3.CL_AdValoremTariff));

			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			entryline3 = ensEntryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == invoiceLine2.JI_Tariff);
			Assert(new EntryLineIEntryLineOrInvoiceLineDutyData(entryline3).HasBothSupTariffAndNormalTariff());
		}

		public void TestGetCombinedDutyDataListForDDPCalculation()
		{
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038801", "7", 0.25m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "0", 0m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030123", "0", 0m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8424201000", "7", 0.029m, "X");

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8424201000";
			invoiceLine.US_SupTariff = "99038801";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var dutyDataList = invoiceLine.GetCombinedDutyDataListForDDPCalculation(new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>(), new Dictionary<JobComInvoiceLine, CustomsValues>(), true);
			AssertEquals(2, dutyDataList.Count);
			AssertType<DDPDisbursementSupDutyData>(dutyDataList[0]);
			AssertType<DDPDisbursementLineDutyData>(dutyDataList[1]);

			invoiceLine.SupFormattedAdditionalTariff1 = "99030123";
			dutyDataList = invoiceLine.GetCombinedDutyDataListForDDPCalculation(new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>(), new Dictionary<JobComInvoiceLine, CustomsValues>(), true);
			AssertEquals(3, dutyDataList.Count);
			AssertType<DDPDisbursementSupAdditionalDutyData>(dutyDataList[0]);
			AssertType<DDPDisbursementSupDutyData>(dutyDataList[1]);
			AssertType<DDPDisbursementLineDutyData>(dutyDataList[2]);

			invoiceLine.SupFormattedAdditionalTariff2 = "99038802";
			dutyDataList = invoiceLine.GetCombinedDutyDataListForDDPCalculation(new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>(), new Dictionary<JobComInvoiceLine, CustomsValues>(), true);
			AssertEquals(4, dutyDataList.Count);
			AssertType<DDPDisbursementSupAdditionalDutyData>(dutyDataList[0]);
			AssertType<DDPDisbursementSupAdditionalDutyData>(dutyDataList[1]);
			AssertType<DDPDisbursementSupDutyData>(dutyDataList[2]);
			AssertType<DDPDisbursementLineDutyData>(dutyDataList[3]);
		}

		public void TestIsCombinedLineForAdditionalSupTariffWithParentChildRelationship()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.SupFormattedAdditionalTariff1 = "9903.01.10";
			parentLine.SupTariffFormatted = "9903.01.25";
			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_ParentID = parentLine.PK;
			childLine.JI_FormattedTariff = "8529.90.7300";
			childLine.SupTariffFormatted = "9903.01.24";
			childLine.SupFormattedAdditionalTariff1 = "9903.01.05";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.FormalEntry;
			var entryLine = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99030125");
			Assert("IsCombinedLine() should return true for tariff 99030125", entryLine.IsCombinedLine());
		}
	}
}
