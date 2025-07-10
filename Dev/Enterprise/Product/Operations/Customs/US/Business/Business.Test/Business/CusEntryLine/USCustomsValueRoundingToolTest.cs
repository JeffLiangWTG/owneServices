using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCustomsValueRoundingToolTest : TestCaseWithFactory
	{
		public void TestRoundForFTZWhenCustomsValueIsLessThanOne()
		{
			AssertCustomsVlaueOfFTZDeclaration(0.3m, 0.3m, 1m, 1m);
			AssertCustomsVlaueOfFTZDeclaration(0.3m, 0.6m, 1m, 1m);
			AssertCustomsVlaueOfFTZDeclaration(0.6m, 0.6m, 1m, 1m);
			AssertCustomsVlaueOfFTZDeclaration(0.3m, 1.1m, 1m, 1m);
			AssertCustomsVlaueOfFTZDeclaration(0.6m, 1.1m, 1m, 1m);
			AssertCustomsVlaueOfFTZDeclaration(1.1m, 1.1m, 1m, 1m);
			AssertCustomsVlaueOfFTZDeclaration(0.4m, 1.4m, 1m, 1m);
			AssertCustomsVlaueOfFTZDeclaration(0.4m, 1.8m, 1m, 2m);
			AssertCustomsVlaueOfFTZDeclaration(0.8m, 1.6m, 1m, 2m);
		}

		void AssertCustomsVlaueOfFTZDeclaration(ZDecimal customsValue1, ZDecimal customsValue2, ZDecimal expectedCustomsValue1, ZDecimal expectedCustomsValue2)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = customsValue1;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = customsValue2;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(expectedCustomsValue1, invoiceLine1.CusEntryLine.CL_CustomsValue);
			AssertEquals(expectedCustomsValue2, invoiceLine2.CusEntryLine.CL_CustomsValue);
		}

		public void TestRoundForXVVLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068";
			invoiceLine1.US_98GoodsValue = 2544.12m;
			invoiceLine1.JI_LinePrice = 2465.32m;
			invoiceLine1.US_SetInd = "X";
			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.US_98GoodsValue = 104.52m;
			invoiceLine2.JI_LinePrice = 100.51m;
			var invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine3.US_98GoodsValue = 204.52m;
			invoiceLine3.JI_LinePrice = 200.52m;
			invoiceLine3.JI_ParentID = invoiceLine2.PK;
			var invoiceLine4 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine4.US_98GoodsValue = 204.52m;
			invoiceLine4.JI_LinePrice = 10.52m;
			invoiceLine4.JI_ParentID = invoiceLine2.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(((ZDecimal)(100.51m + 200.52m + 10.52m)).Round(0), invoiceLine1.CusEntryLine.CL_CustomsValue);
			AssertEquals(100m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals(201m, invoiceLine3.CusEntryLine.CL_CustomsValue);
			AssertEquals(11m, invoiceLine4.CusEntryLine.CL_CustomsValue);
		}

		[TestDate(2018, 10, 9)]
		public void TestSumUpXLinesFromVLinesWithProvTariff()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			testJob.US_IsHMFApplicable = "Y";
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test98191112Tariff.UE_Tariff;
			invoiceLine1.JI_Tariff = "6211.33.9010";
			invoiceLine1.US_SetInd = "X";
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SetInd = "V";
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test98191112Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = "6211.33.9010";
			invoiceLine2.JI_LinePrice = 5000m;
			var invoiceLine3 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.US_SetInd = "V";
			invoiceLine3.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038001Tariff.UE_Tariff;
			invoiceLine3.JI_Tariff = "6505.00.8090";
			invoiceLine3.JI_LinePrice = 5000m;
			testJob.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			testJob.Factory.Save();
			var entryHeader = testJob.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(entryHeader);
			var entryLine1 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == testHelper.Chapter98TestingHelper.Test98191112Tariff.UE_Tariff && x.US_CL_ParentLine.IsEmpty);
			AssertEquals("The CustomsValue should be 10000m(5000 + 5000)", 0m, entryLine1.CL_CustomsValue);
			var entryLine2 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "6211339010" && x.US_CL_ParentLine == entryLine1.PK);
			AssertEquals("CustomsValue should be on Prov/Prog line", 10000m, entryLine2.CL_CustomsValue);
			var hmfFee = invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertNotNull(hmfFee);
			AssertEquals("HMF should be 12.5(0.125% * 10000m)", 12.5m, ZDecimal.Parse(hmfFee.CY_Data));
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entryHeader, true, true, UpdateActionCode.Add);
			var messageText = messageBuilder.PopulateMessage().EM_FormattedMessageText;
			Assert(messageText.Contains(@"40  001X                  0000000000     0000000000    N                        
5098191112   0000000000 0000000000                                              
506211339010 0000000000 0000010000                                              
6250100001250                                                                   
40  002V                  0000000000     0000000000    N                        
5098191112   0000000000 0000000000                                              
506211339010 0000000000 0000005000                                              
40  003V                  0000000000     0000000000    N                        
5099038001   0000000000 0000000000                                              
506505008090 0000000000 0000005000                                              
8950100000001250                                                                "));
		}

		public void TestRoundWhenOneOfLinesHasAValueRoundedDownToZero()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 0.01m;
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 0.41m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(0m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(1m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine2.JI_LinePrice = 0m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(0m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(0m, invoiceLine2.CusEntryLine.CL_CustomsValue);
		}

		public void TestDeterministicSort()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var tariff6306229030 = Factory.New<USCTariff>();
			tariff6306229030.UE_Tariff = "6306229030";
			tariff6306229030.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff6306229030.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff6306229030.UE_DutyComputationCode = "7";
			tariff6306229030.UE_Column1RateAdValorem = 0.088m;
			var tariff3926909995 = Factory.New<USCTariff>();
			tariff3926909995.UE_Tariff = "3926909995";
			tariff3926909995.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3926909995.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff3926909995.UE_DutyComputationCode = "7";
			tariff3926909995.UE_Column1RateAdValorem = 0.053m;
			var tariff9817005000 = Factory.New<USCTariff>();
			tariff9817005000.UE_Tariff = "9817005000";
			tariff9817005000.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff9817005000.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff9817005000.UE_DutyComputationCode = "7";
			tariff9817005000.UE_Column1RateAdValorem = 0.0m;
			var tariff9817005001 = Factory.New<USCTariff>();
			tariff9817005001.UE_Tariff = "9817005001";
			tariff9817005001.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff9817005001.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff9817005001.UE_DutyComputationCode = "7";
			tariff9817005001.UE_Column1RateAdValorem = 0.0m;
			var tariff9403200018 = Factory.New<USCTariff>();
			tariff9403200018.UE_Tariff = "9403200018";
			tariff9403200018.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff9403200018.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff9403200018.UE_DutyComputationCode = "7";
			tariff9403200018.UE_Column1RateAdValorem = 0.0m;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 31943.00m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6306.22.9030";
			invoiceLine.JI_LinePrice = 21055.50m;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3926.90.9995";
			invoiceLine2.US_SupTariff = "9817.00.5000";
			invoiceLine2.JI_LinePrice = 5287.50m;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3926.90.9995";
			invoiceLine3.US_SupTariff = "9817.00.5001";
			invoiceLine3.JI_LinePrice = 1522.50m;
			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "9403.20.0018";
			invoiceLine4.JI_LinePrice = 4077.50m;
			AssertNotNull(invoiceLine.ImportTariff);
			AssertNotNull(invoiceLine2.ImportTariff);
			AssertNotNull(invoiceLine2.ImportSupTariff);
			AssertNotNull(invoiceLine3.ImportTariff);
			AssertNotNull(invoiceLine3.ImportSupTariff);
			AssertNotNull(invoiceLine4.ImportTariff);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(87.07m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
			invoiceLine4.JI_LineNo = 3;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(87.06m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
		}

		public void TestRoundWhenOneShouldBeRoundedDown()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			for (int index = 0; index < 11; index++)
			{
				JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 221.55m;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2437m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			foreach (CusEntryLine entryLine in declaration.ActiveEntryHeaders.EntrySummaryEntry.MergedLines)
			{
				Assert(entryLine.CL_CustomsValue == 221m || entryLine.CL_CustomsValue == 222m);
			}
		}

		public void TestRoundWithParentLineWithZeroValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 186074.45m;
			var linePrices = new ZDecimal[] { 68.10m, 27580.50m, 306.45m, 34050.00m, 176.80m, 13923.00m, 12928.50m, 563.55m, 19558.50m, 19558.50m, 132.60m, 10276.50m, 13260.00m, 541.45m, 13591.50m, 19558.50m };
			foreach (ZDecimal linePrice in linePrices)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = linePrice;
				invoiceLine.US_SupTariff = "9819.11.12";
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(186074m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
		}

		public void TestRoundWithZeroValueDoesNotResultInException()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 186074.45m;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SupTariff = "9819.11.12";
			AssertNoExceptionThrown(() => declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		}

		public void TestRoundForEachInvoice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 2544.45m;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2000.20m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 544.25m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice2.JZ_InvoiceAmount = 2465.65m;
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 500.51m;
			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 500.54m;
			var invoiceLine5 = invoice2.InvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 1464.60;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice3.JZ_InvoiceAmount = 218.60m;
			var invoiceLine6 = invoice3.InvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 100.10m;
			var invoiceLine7 = invoice3.InvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 118.50m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("This line is truncated to round it correctly", 500m, invoiceLine3.CusEntryLine.CL_CustomsValue);
			AssertEquals("This line is rounded up as it should be", 119m, invoiceLine7.CusEntryLine.CL_CustomsValue);
			AssertEquals(5229m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
		}

		public void TestRoundPer7501Line()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068";
			invoiceLine1.US_98GoodsValue = 2544.64m;
			invoiceLine1.JI_LinePrice = 2465.12m;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 218.68m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryLine line1 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			CusEntryLine line2 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(2545m, line1.CL_CustomsValue);
			AssertEquals(2465m, line2.CL_CustomsValue);
			AssertEquals(218m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestRoundPer7501Line2()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068";
			invoiceLine1.US_98GoodsValue = 2544.12m;
			invoiceLine1.JI_LinePrice = 2465.64m;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 218.68m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryLine line1 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			CusEntryLine line2 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(2544m, line1.CL_CustomsValue);
			AssertEquals(2466m, line2.CL_CustomsValue);
			AssertEquals(218m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestRoundPer7501Line3()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068"; // 0%
			invoiceLine1.JI_Tariff = "3201.90.1000"; // 5%
			invoiceLine1.US_98GoodsValue = 2544.50m;
			invoiceLine1.JI_LinePrice = 2465.50m;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 218.68m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryLine line1 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			CusEntryLine line2 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(2544m, line1.CL_CustomsValue);
			AssertEquals(2466m, line2.CL_CustomsValue);
			AssertEquals(219m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestRoundPer7501Line4()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068";
			invoiceLine1.US_98GoodsValue = 2544.90m;
			invoiceLine1.JI_LinePrice = 2465.90m;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 218.68m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryLine line1 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			CusEntryLine line2 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(2545m, line1.CL_CustomsValue);
			AssertEquals(2466m, line2.CL_CustomsValue);
			AssertEquals(218m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestRoundPer7501Line5()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068";
			invoiceLine1.US_98GoodsValue = 2544.12m;
			invoiceLine1.JI_LinePrice = 2465.32m;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 218.06m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryLine line1 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			CusEntryLine line2 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(2544m, line1.CL_CustomsValue);
			AssertEquals(2466m, line2.CL_CustomsValue);
			AssertEquals(218m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestRoundWhenOneShouldBeRoundedUp()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 22.35m;
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 22.45m;
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 22.49m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(67m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(22m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(22m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals(23m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		[TestDate(2009, 1, 1)]
		public void TestRoundUpWhenDutyRateMatters()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 22.35m;
			invoiceLine.JI_Tariff = "2903460010"; //0.037
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 22.35m;
			invoiceLine2.JI_Tariff = "7326190010"; // 0.029
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 22.35m;
			invoiceLine3.JI_Tariff = "8411999081"; // 0.025
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(67m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(23m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(22m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals(22m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		[TestDate(2009, 1, 1)]
		public void TestRoundDownWhenDutyRateMatters()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 221.55m;
			invoiceLine.JI_Tariff = "2903460010"; //0.037
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 221.55m;
			invoiceLine2.JI_Tariff = "7326190010"; // 0.029
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 221.55m;
			invoiceLine3.JI_Tariff = "8411999081"; // 0.025
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(665m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(222m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(222m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals(221m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestIgnoreFiguresThatDidNotAffectRoundingIssue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 221.01m;
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 221.50m;
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 221.51m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(664m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(221m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(221m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals(222m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestIgnoreFiguresThatDidNotAffectRoundingIssue2()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 221.99m;
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 221.48m;
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 221.49m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(665m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(222m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(221m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals(222m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestRoundEvenWhenThereIsNoGap()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 221.01m;
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 221.49m;
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 221.50m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(664m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(221m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(221m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals(222m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestAdjustFDAValue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000.50m;
			FDA fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAValue = 10000.50m;
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200.51m;
			FDA fda2 = invoiceLine2.FDAs.AddNew();
			fda2.US_FDAValue = 200.51m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(10000m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals("Should not exceed entry lines' CV", 10000m, fda.US_FDAValue);
			AssertEquals(201m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals("Should not exceed entry lines' CV", 201m, fda2.US_FDAValue);
		}

		public void TestRoundForXAndV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = "01";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CS00174232";
			invoice.JZ_InvoiceAmount = 8757.60m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.US_UC_NKCountryOfExport = "CH";
			invoice.US_UC_NKCountryOfOrigin = "CH";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew("OFT", 150m, "USD");
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6211.43.0020";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "DOZ";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_DestinationState = "IL";
			invoiceLine.JI_Weight = 83m;
			invoiceLine.JI_WeightUQ = "KG";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6211.43.0020";
			invoiceLine2.JI_CustomsQuantity = 82m;
			invoiceLine2.JI_CustomsUnitQty = "DOZ";
			invoiceLine2.JI_CustomsSecondQuantity = 82m;
			invoiceLine2.JI_CustomsSecondUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 7409.52m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6505.00.2060";
			invoiceLine3.JI_CustomsQuantity = 82m;
			invoiceLine3.JI_CustomsUnitQty = "DOZ";
			invoiceLine3.JI_CustomsSecondQuantity = 15m;
			invoiceLine3.JI_CustomsSecondUnitQty = "KG";
			invoiceLine3.JI_LinePrice = 1348.08m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("InvoiceLine1 Customs Value", 8758m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals("InvoiceLine2 Customs Value", 7410m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals("InvoiceLine3 Customs Value", 1348m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueOfXLineWith98GoodsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CS00174232";
			invoice.JZ_InvoiceAmount = 8757.60m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.US_UC_NKCountryOfExport = "CH";
			invoice.US_UC_NKCountryOfOrigin = "CH";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew("OFT", 150m, "USD");
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Tariff = "6211.43.0020";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "DOZ";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_DestinationState = "IL";
			invoiceLine.JI_Weight = 83m;
			invoiceLine.JI_WeightUQ = "KG";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802008068";
			invoiceLine2.US_98GoodsValue = 50000m;
			invoiceLine2.JI_Tariff = "6211.43.0020";
			invoiceLine2.JI_CustomsQuantity = 82m;
			invoiceLine2.JI_CustomsUnitQty = "DOZ";
			invoiceLine2.JI_CustomsSecondQuantity = 82m;
			invoiceLine2.JI_CustomsSecondUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 7409.52m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "9802008068";
			invoiceLine3.US_98GoodsValue = 25000m;
			invoiceLine3.JI_Tariff = "6505.00.2060";
			invoiceLine3.JI_CustomsQuantity = 82m;
			invoiceLine3.JI_CustomsUnitQty = "DOZ";
			invoiceLine3.JI_CustomsSecondQuantity = 15m;
			invoiceLine3.JI_CustomsSecondUnitQty = "KG";
			invoiceLine3.JI_LinePrice = 1348.08m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var x98Line = invoiceLine.GetEntryLineFor("ENS", true);
			var xNormalLine = invoiceLine.GetEntryLineFor("ENS", false);
			AssertEquals(75000m, x98Line.CL_CustomsValue);
			AssertEquals(8758m, xNormalLine.CL_CustomsValue);
		}

		public void TestCS00178399()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CS00174232";
			invoice.JZ_InvoiceAmount = 40138.60m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.US_UC_NKCountryOfExport = "CH";
			invoice.US_UC_NKCountryOfOrigin = "CH";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew("OFT", 150m, "USD");
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 3312.00m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3481.30m;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 2980.80m;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 124.40m;
			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 8064.00m;
			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 9360.00m;
			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 0m;
			invoiceLine7.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLineV = invoice.JobComInvoiceLines.AddNew();
			invoiceLineV.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLineV.JI_LinePrice = 10843.40m;
			var invoiceLineV2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLineV2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLineV2.JI_LinePrice = 1972.70m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(40139m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue);
			AssertEquals(3312m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(3481m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals(2981m, invoiceLine3.CusEntryLine.CL_CustomsValue);
			AssertEquals(125m, invoiceLine4.CusEntryLine.CL_CustomsValue);
			AssertEquals(8064m, invoiceLine5.CusEntryLine.CL_CustomsValue);
			AssertEquals(9360m, invoiceLine6.CusEntryLine.CL_CustomsValue);
			AssertEquals(10843m, invoiceLineV.CusEntryLine.CL_CustomsValue);
			AssertEquals(1973m, invoiceLineV2.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueOfXLineWhenXIsNotSupLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0901.21.0045";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0901.21.0045";
			invoiceLine2.JI_LinePrice = 17713m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "9904.04.38";
			invoiceLine3.JI_Tariff = "0402.21.5000";
			invoiceLine3.JI_LinePrice = 30159m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("X line Customs Value", 47872m, invoiceLine.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueOfXLineWhen98_99AreMixed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9904.04.38";
			invoiceLine.JI_Tariff = "0901.21.0045";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9904.04.38";
			invoiceLine2.JI_Tariff = "0901.21.0045";
			invoiceLine2.JI_LinePrice = 7409.52m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "9802008068";
			invoiceLine3.US_98GoodsValue = 25000m;
			invoiceLine3.JI_Tariff = "0402.21.5000";
			invoiceLine3.JI_LinePrice = 1348m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var xSupLine = invoiceLine.GetEntryLineFor("ENS", true);
			AssertEquals("X Sup line Customs Value", 0m, xSupLine.CL_CustomsValue);
			AssertEquals("X normal line Customs Value", 1348m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals("V line Customs Value", 7410m, invoiceLine2.GetEntryLineFor("ENS", true).CL_CustomsValue);
			AssertEquals("V line Customs Value", 0m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals("V line Customs Value", 1348m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueOfXLineWith98XLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Tariff = "0901.21.0045";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802008068";
			invoiceLine2.US_98GoodsValue = 25000m;
			invoiceLine2.JI_Tariff = "0901.21.0045";
			invoiceLine2.JI_LinePrice = 7409.52m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "9904.04.38";
			invoiceLine3.JI_Tariff = "0402.21.5000";
			invoiceLine3.JI_LinePrice = 1348m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var xSupLine = invoiceLine.GetEntryLineFor("ENS", true);
			AssertEquals("X Sup line Customs Value", 25000m, xSupLine.CL_CustomsValue);
			AssertEquals("X normal line Customs Value", 8758m, invoiceLine.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueOfXLineWith99XLineWithValueDeclaredAtParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9904.04.38";
			invoiceLine.JI_Tariff = "0901.21.0045";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9904.04.38";
			invoiceLine2.JI_Tariff = "0901.21.0045";
			invoiceLine2.JI_LinePrice = 7409.52m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "9904.04.38";
			invoiceLine3.JI_Tariff = "0402.21.5000";
			invoiceLine3.JI_LinePrice = 1348m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var xSupLine = invoiceLine.GetEntryLineFor("ENS", true);
			AssertEquals("X Sup line Customs Value", 0m, xSupLine.CL_CustomsValue);
			AssertEquals("X normal line Customs Value", 0m, invoiceLine.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueOfXLineWith99XLineWithValueDeclaredAtSecondary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0901.21.0045";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0901.21.0045";
			invoiceLine2.JI_LinePrice = 7409.52m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			invoiceLine3.JI_Tariff = "0402.21.5000";
			invoiceLine3.JI_LinePrice = 1348m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("X normal line Customs Value", 8758m, invoiceLine.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueOfXLineWith99XLineWithValueDeclaredAtSecondary2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			invoiceLine.JI_Tariff = "0901.21.0045";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			invoiceLine2.JI_Tariff = "0901.21.0045";
			invoiceLine2.JI_LinePrice = 7409.52m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0402.21.5000";
			invoiceLine3.JI_LinePrice = 1348m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("X normal line Customs Value", 8758m, invoiceLine.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueOfXLineForACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			invoiceLine.JI_Tariff = "0901.21.0045";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			invoiceLine2.JI_Tariff = "0901.21.0045";
			invoiceLine2.JI_LinePrice = 7409m;
			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0402.21.5000";
			invoiceLine3.JI_LinePrice = 1348m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("X normal line Customs Value", 0m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals("First V line Customs Value", 7409m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals("Second V line Customs Value", 1348m, invoiceLine3.CusEntryLine.CL_CustomsValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
