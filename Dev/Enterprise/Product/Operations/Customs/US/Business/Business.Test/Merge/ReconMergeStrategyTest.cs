using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconMergeStrategyTest : TestCaseWithFactory
	{
		public void TestOrigDutyRateDesc()
		{
			var tariff_99034525 = Factory.New<USCTariff>();
			tariff_99034525.UE_Tariff = "99034525";
			tariff_99034525.UE_DateFrom = new ZDateTime(2016, 02, 09);
			tariff_99034525.UE_DateTo = new ZDateTime(2099, 02, 06);
			tariff_99034525.UE_DutyComputationCode = "7";
			tariff_99034525.UE_Column1RateAdValorem = 0.25m;

			var tariff_99034526 = Factory.New<USCTariff>();
			tariff_99034526.UE_Tariff = "99034526";
			tariff_99034526.UE_DateFrom = new ZDateTime(2016, 02, 09);
			tariff_99034526.UE_DateTo = new ZDateTime(2099, 02, 06);
			tariff_99034526.UE_DutyComputationCode = "7";
			tariff_99034526.UE_Column1RateAdValorem = 0.26m;

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration1.US_EnableENS = true;
			declaration1.US_BondProducerAccNo = "12";
			declaration1.Invoices.AddNew();
			var invoiceLine = declaration1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_LinePrice = 3406.00m;
			invoiceLine.US_SupTariff = "99034525";
			invoiceLine.US_98GoodsValue = 1852.00m;

			declaration1.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry1 = declaration1.CustomsEntryHeaders[0];
			var reconInnerDec = Factory.New<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(reconInnerDec);
			var reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry1.EntryNumber;
			AssertEquals("CH_CH_OrigEntry is updated", ensEntry1.PK, reconOriginalEntry.CH_CH_OriginalEntry);

			var invoice = reconOriginalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var recinvoiceLine = invoice.JobComInvoiceLines.AddNew();
			recinvoiceLine.JI_Tariff = "3201.90.1000";
			recinvoiceLine.JI_CustomsQuantity = 50m;
			recinvoiceLine.JI_LinePrice = 3000m;
			recinvoiceLine.US_SupTariff = "99034525";
			recinvoiceLine.US_Duty = 125m;
			recinvoiceLine.US_R_OrigEntryLineNo = "1";
			recinvoiceLine.US_R_OrigTariff = recinvoiceLine.JI_Tariff;
			recinvoiceLine.US_R_OrigSupTariff = recinvoiceLine.US_SupTariff;
			recinvoiceLine.US_R_OrigCV = 1000m;
			recinvoiceLine.US_R_OrigDuty = 17m;
			recinvoiceLine.InvoiceHeader.ReconOriginalEntry.US_R_DutyRateDate = invoiceLine.EffectiveDateForDutyRate;
			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Documents).DoMerge(false);
			var reconChangeLine = reconDeclaration.ChangedLines.Cast<ReconChangedLine>().FirstOrDefault(x => x.US_OrigTariff == "3201901000");
			AssertEquals("1.5%", reconChangeLine.US_OrigDutyRateDesc);
			AssertEquals("1.5%", reconChangeLine.DutyRateDesc);
			AssertEquals("1.5%", reconChangeLine.OrigDutyRateDesc);

			var reconSupLine = reconDeclaration.ChangedLines.Cast<ReconChangedLine>().FirstOrDefault(x => x.US_OrigTariff == "99034525");
			AssertEquals("25%", reconSupLine.US_OrigDutyRateDesc);
			AssertEquals("25%", reconSupLine.DutyRateDesc);
			AssertEquals("25%", reconSupLine.OrigDutyRateDesc);

			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Messaging).DoMerge(false);
			reconChangeLine = reconDeclaration.ChangedLines.Cast<ReconChangedLine>().FirstOrDefault(x => x.US_OrigTariff == "3201901000");
			reconSupLine = reconDeclaration.ChangedLines.Cast<ReconChangedLine>().FirstOrDefault(x => x.US_OrigTariff == "99034525");
			AssertEquals("", reconChangeLine.US_OrigDutyRateDesc);
			AssertEquals("25%", reconSupLine.US_OrigDutyRateDesc);

			recinvoiceLine.US_R_OrigTariff = "9102.11.1010";
			recinvoiceLine.US_R_OrigSupTariff = "99034526";
			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Documents).DoMerge(false);
			reconChangeLine = reconDeclaration.ChangedLines.Cast<ReconChangedLine>().FirstOrDefault(x => x.US_OrigTariff == "9102111010");
			AssertEquals("", reconChangeLine.US_OrigDutyRateDesc);
			AssertEquals("4.17%", reconChangeLine.DutyRateDesc);
			AssertEquals("1.70%", reconChangeLine.OrigDutyRateDesc);
		}

		public void TestCorrectEntryLineIsPicked()
		{
			var tariff_99034525 = Factory.New<USCTariff>();
			tariff_99034525.UE_Tariff = "99034525";
			tariff_99034525.UE_DateFrom = new ZDateTime(2016, 02, 09);
			tariff_99034525.UE_DateTo = new ZDateTime(2099, 02, 06);
			tariff_99034525.UE_DutyComputationCode = "7";
			tariff_99034525.UE_Column1RateAdValorem = 0.25m;

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration1.US_EnableENS = true;
			declaration1.US_BondProducerAccNo = "12";
			declaration1.Invoices.AddNew();
			var invoiceLine = declaration1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99034525";
			invoiceLine.US_SupTariff = "99034525";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_LinePrice = 3406.00m;
			invoiceLine.US_98GoodsValue = 1852.00m;

			declaration1.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var ensEntry = declaration1.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(2, ensEntry.MergedLines.Count);
			var supEntryLine = invoiceLine.GetMatchingSupEntryLine(invoiceLine.US_SupTariff);
			supEntryLine.US_DutyRateDesc += "SUP";
			var entryLine = invoiceLine.CusEntryLine;
			entryLine.US_DutyRateDesc += "NONSUP";
			Factory.Save();

			var reconInnerDec = Factory.New<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(reconInnerDec);
			var reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry.EntryNumber;
			reconOriginalEntry.US_R_DutyRateDate = invoiceLine.EffectiveDateForDutyRate;
			AssertEquals("CH_CH_OrigEntry is updated", ensEntry.PK, reconOriginalEntry.CH_CH_OriginalEntry);

			var invoice = reconOriginalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var recinvoiceLine = invoice.JobComInvoiceLines.AddNew();
			recinvoiceLine.US_R_OrigEntryLineNo = "1";
			recinvoiceLine.US_R_OrigTariff = "99034525";
			recinvoiceLine.JI_Tariff = "99034525";
			recinvoiceLine.US_R_OrigSupTariff = "99034525";
			recinvoiceLine.US_SupTariff = "99034525";
			recinvoiceLine.US_R_OrigSPI = "A";
			recinvoiceLine.US_SPI = "B";
			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Documents).DoMerge(false);
			AssertEquals(2, reconDeclaration.ChangedLines.Count);
			var reconChangeLine = reconDeclaration.ChangedLines.Cast<ReconChangedLine>().FirstOrDefault(x => x.OrigDutyRateDesc == "25%SUP");
			AssertNotNull(reconChangeLine);
			AssertEquals("25%SUP", reconChangeLine.US_OrigDutyRateDesc);
			AssertEquals("25%SUP", reconChangeLine.DutyRateDesc);
			reconChangeLine = reconDeclaration.ChangedLines.Cast<ReconChangedLine>().FirstOrDefault(x => x.OrigDutyRateDesc == "25%NONSUP");
			AssertNotNull(reconChangeLine);
			AssertEquals("25%NONSUP", reconChangeLine.US_OrigDutyRateDesc);
			AssertEquals("25%NONSUP", reconChangeLine.DutyRateDesc);
		}

		public void TestGetKeyIncludingSecondaryLines()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;

			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 27000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9802.00.8015";
			invoiceLine1.JI_LinePrice = 3000m;
			SetReconOriginalValues(invoiceLine1);

			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "8421.39.4000";
			invoiceLine2.JI_CustomsQuantity = 100m;
			invoiceLine2.JI_LinePrice = 7000m;
			SetReconOriginalValues(invoiceLine2);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "9802.00.8015";
			invoiceLine3.JI_LinePrice = 3000m;
			SetReconOriginalValues(invoiceLine3);

			var invoiceLine4 = invoiceLine3.AddSecondaryInvoiceLine();
			invoiceLine4.JI_Tariff = "8421.39.4000";
			invoiceLine4.JI_CustomsQuantity = 100m;
			invoiceLine4.JI_LinePrice = 7000m;
			SetReconOriginalValues(invoiceLine4);

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "8421.39.4000";
			invoiceLine5.JI_CustomsQuantity = 100m;
			invoiceLine5.JI_LinePrice = 7000m;
			SetReconOriginalValues(invoiceLine5);

			var strategy = new ReconMergeStrategy(reconDeclaration);
			AssertEquals("InvoiceLine1 and InvoiceLine3 should be merged together", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine3));
			AssertNotEquals("InvoiceLine1 and InvoiceLine2 should NOT be merged together", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));
			AssertNotEquals("InvoiceLine4 and InvoiceLine5 should NOT be merged together", strategy.GetKeyForLine(invoiceLine4), strategy.GetKeyForLine(invoiceLine5));

			invoiceLine4.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.DairyFee, "20");
			invoiceLine4.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DairyFee, "15");

			strategy = new ReconMergeStrategy(reconDeclaration);
			AssertNotEquals("InvoiceLine1 and InvoiceLine3 should NOT be merged together because the fee on secondary line is different.", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine3));
			AssertNotEquals("InvoiceLine1 and InvoiceLine2 should NOT be merged together", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));
			AssertNotEquals("InvoiceLine4 and InvoiceLine5 should NOT be merged together", strategy.GetKeyForLine(invoiceLine4), strategy.GetKeyForLine(invoiceLine5));

			reconDeclaration.US_R_Waive = true;
			strategy = new ReconMergeStrategy(reconDeclaration);
			AssertNotEquals("InvoiceLine1 and InvoiceLine3 should not be merged together, because line 3 has decrease and line 1 not.", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine3));
			AssertNotEquals("InvoiceLine1 and InvoiceLine2 should NOT be merged together", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));
			AssertNotEquals("InvoiceLine4 and InvoiceLine5 should NOT be merged together", strategy.GetKeyForLine(invoiceLine4), strategy.GetKeyForLine(invoiceLine5));
		}

		public void TestGetKeyForLine()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 1);
			originalEntry.US_R_ReleaseDate = new ZDateTime(2018, 1, 1);
			
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_Column1RateAdValorem = .65m;
			tariff.UE_Column1RateOther = .89m;
			tariff.UE_Column1RateSpecific = .23m;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "00000000";
			invoiceLine.JI_Tariff = "000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_R_OrigFirstUQ = "NO";
			invoiceLine.US_R_OrigSecondUQ = "KG";
			invoiceLine.US_R_OrigThirdUQ = "M3";
			invoiceLine.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.US_SPI = "XX";

			ReconMergeStrategy strategy = new ReconMergeStrategy(reconDec);
			MergeKey mergeKey = strategy.GetKeyForLine(invoiceLine);

			AssertEquals(true, mergeKey.Contains(new ZString("00000000")));
			AssertEquals(true, mergeKey.Contains(new ZString("000000")));

			AssertEquals(true, mergeKey.Contains(new ZString("NZ")));
			AssertEquals(true, mergeKey.Contains(new ZString("NO")));
			AssertEquals(true, mergeKey.Contains(new ZString("KG")));
			AssertEquals(true, mergeKey.Contains(new ZString("M3")));
			AssertEquals(true, mergeKey.Contains(new ZString("XX")));
			AssertEquals(true, mergeKey.Contains(new ZString("2018")));
			AssertEquals(2, mergeKey.Keys.Where(x => x is ZBool y && !y).Count());
			Assert(!mergeKey.Contains(new ZString(SPICompleteList.MoreCodes.NotApplicable)));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				originalEntry.US_NAFTAReconIndicator = true;
				strategy = new ReconMergeStrategy(reconDec);
				mergeKey = strategy.GetKeyForLine(invoiceLine);
				AssertEquals(1, mergeKey.Keys.Where(x => x is ZBool y && !y).Count());
				AssertEquals(1, mergeKey.Keys.Where(x => x is ZBool y && y).Count());
			}
		}

		public void TestGetKeyForLineACE()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 1);
			originalEntry.US_R_ReleaseDate = new ZDateTime(2018, 1, 1);

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_Column1RateAdValorem = .65m;
			tariff.UE_Column1RateOther = .89m;
			tariff.UE_Column1RateSpecific = .23m;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "00000000";
			invoiceLine.JI_Tariff = "000000";
			invoiceLine.US_R_OrigOverrideDuty = true;
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_R_OrigFirstUQ = "NO";
			invoiceLine.US_R_OrigSecondUQ = "KG";
			invoiceLine.US_R_OrigThirdUQ = "M3";
			invoiceLine.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.US_SPI = "XX";
			invoiceLine.US_R_HTSChanged4ValueInd = true;

			ReconMergeStrategy strategy = new ReconMergeStrategy(reconDec);
			MergeKey mergeKey = strategy.GetKeyForLine(invoiceLine);

			AssertEquals(true, mergeKey.Contains(new ZString("00000000")));
			AssertEquals(true, mergeKey.Contains(new ZString("000000")));

			AssertEquals(true, mergeKey.Contains(new ZString("NZ")));
			AssertEquals(true, mergeKey.Contains(new ZString("NO")));
			AssertEquals(true, mergeKey.Contains(new ZString("KG")));
			AssertEquals(true, mergeKey.Contains(new ZString("M3")));
			AssertEquals(true, mergeKey.Contains(new ZString("XX")));
			AssertEquals(true, mergeKey.Contains(new ZString("HC")));
			AssertEquals(true, mergeKey.Contains(new ZString("2018")));
			Assert(!mergeKey.Contains(new ZString(SPICompleteList.MoreCodes.NotApplicable)));
		}

		public void TestGetTariffRequiredFeeCode()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6205202067";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeClassCode = "056";
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			var invoiceLine = originalEntry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "6205202067";
			invoiceLine.US_R_OrigCV = 15m;
			invoiceLine.JI_LinePrice = 16m;
			invoiceLine.US_R_OrigCottonFeeExempt = YesNoDefaultList.Codes.No;

			new ReconChangedLinesMerger(reconDec).DoMerge(false);
			AssertEquals(1, reconDec.ChangedLines.Count);
			var mergedLine = (IReconEntryLine)reconDec.ChangedLines[0];
			var cottonFee = mergedLine.Fees.FirstOrDefault(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.Cotton);
			AssertNotNull(cottonFee);
			AssertEquals(0m, cottonFee.OriginalFee);
			AssertEquals(0m, cottonFee.EstimatedReconciliationFee);
		}

		[TestDate(2018, 10, 24)]
		public void TestGetKeyIncludingCottonFeeMandatory()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;

			var originalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry1.US_R_CottonFeeMandatory = false;
			var invoice1 = originalEntry1.Invoice;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_InvoiceNumber = "INVOICE1";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9802.00.8015";
			invoiceLine1.JI_LinePrice = 3000m;
			SetReconOriginalValues(invoiceLine1);

			var originalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry2.US_R_CottonFeeMandatory = false;
			var invoice2 = originalEntry2.Invoice;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_InvoiceNumber = "INVOICE2";
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9802.00.8015";
			invoiceLine2.JI_LinePrice = 3000m;
			SetReconOriginalValues(invoiceLine2);

			var strategy = new ReconMergeStrategy(reconDeclaration);
			AssertEquals("InvoiceLine1 and InvoiceLine2 should be merged together", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

			originalEntry1.US_R_CottonFeeMandatory = true;
			strategy = new ReconMergeStrategy(reconDeclaration);
			AssertNotEquals("InvoiceLine1 and InvoiceLine2 should NOT be merged together", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));

			originalEntry1.US_R_CottonFeeMandatory = false;
			var originalCharge = invoiceLine1.ReconOriginalCharges.AddNew();
			originalCharge.CY_Code = Core.Constants.USCustoms.FeeCodes.Honey;
			originalCharge.CY_IsOverridden = true;
			originalCharge.CY_Amount = 10m;
			AssertNotEquals("InvoiceLine1 and InvoiceLine2 should NOT be merged together", strategy.GetKeyForLine(invoiceLine1), strategy.GetKeyForLine(invoiceLine2));
		}

		[TestDate(2020, 01, 17)]
		public void TestMergeCombinedLines()
		{
			#region Setup Tariffs

			var tariff_99038817 = Factory.New<USCTariff>();
			tariff_99038817.UE_Tariff = "99038817";
			tariff_99038817.UE_DateFrom = new ZDateTime(2018, 08, 23);
			tariff_99038817.UE_DateTo = new ZDateTime(2020, 09, 20);

			var tariff_99034525 = Factory.New<USCTariff>();
			tariff_99034525.UE_Tariff = "99034525";
			tariff_99034525.UE_DateFrom = new ZDateTime(2019, 02, 09);
			tariff_99034525.UE_DateTo = new ZDateTime(2020, 02, 06);
			tariff_99034525.UE_DutyComputationCode = "7";
			tariff_99034525.UE_Column1RateAdValorem = 0.3m;

			var tariff_8541406015 = Factory.New<USCTariff>();
			tariff_8541406015.UE_Tariff = "8541406015";
			tariff_8541406015.UE_DateFrom = new ZDateTime(2018, 07, 01);
			tariff_8541406015.UE_DateTo = new ZDateTime(2020, 12, 31);

			Factory.Save();

			#endregion

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			var invoiceLine1 = originalEntry.Invoice.InvoiceLines.AddNew();
			invoiceLine1.US_R_OrigSupTariff = tariff_99038817.UE_Tariff;
			invoiceLine1.US_SupTariff = tariff_99038817.UE_Tariff;
			var invoiceLine2 = originalEntry.Invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_R_OrigTariff = tariff_8541406015.UE_Tariff;
			invoiceLine2.US_R_OrigSupTariff = tariff_99034525.UE_Tariff;
			invoiceLine2.JI_Tariff = tariff_8541406015.UE_Tariff;
			invoiceLine2.US_SupTariff = tariff_99034525.UE_Tariff;
			invoiceLine2.US_R_OrigCV = 5000m;
			invoiceLine2.JI_LinePrice = 5500m;

			new ReconChangedLinesMerger(reconDec).DoMerge(false);
			AssertEquals("Empty tariff should be excluded", 3, reconDec.ChangedLines.Count);
		}

		[TestDate(2020, 01, 20)]
		public void TestCustomsValueInMergedLines()
		{
			#region Setup Tariffs

			var tariff_99038803 = Factory.New<USCTariff>();
			tariff_99038803.UE_Tariff = "99038803";
			tariff_99038803.UE_DateFrom = new ZDateTime(2019, 05, 10);
			tariff_99038803.UE_DateTo = new ZDateTime(2020, 12, 31);
			tariff_99038803.UE_DutyComputationCode = "7";
			tariff_99038803.UE_Column1RateAdValorem = 0.25m;

			var tariff_8537109120 = Factory.New<USCTariff>();
			tariff_8537109120.UE_Tariff = "8537109120";
			tariff_8537109120.UE_DateFrom = new ZDateTime(2016, 07, 01);
			tariff_8537109120.UE_DateTo = new ZDateTime(2020, 12, 31);
			tariff_8537109120.UE_DutyComputationCode = "7";
			tariff_8537109120.UE_Column1RateAdValorem = 0.027m;

			Factory.Save();

			#endregion

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			var invoiceLine = originalEntry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = tariff_8537109120.UE_Tariff;
			invoiceLine.US_R_OrigSupTariff = tariff_99038803.UE_Tariff;
			invoiceLine.JI_Tariff = tariff_8537109120.UE_Tariff;
			invoiceLine.US_SupTariff = tariff_99038803.UE_Tariff;
			invoiceLine.US_R_OrigCV = 10000m;
			invoiceLine.JI_LinePrice = 10500m;

			reconDec.ChangedLines.RemoveAndDeleteAll();
			new ReconChangedLinesMerger(reconDec, ReconMergeContext.Documents).DoMerge(false);
			AssertEquals(2, reconDec.ChangedLines.Count);
			AssertEquals("Customs value change on tariff 9903 should be zero", 0m, reconDec.ChangedLines[0].CustomsValueChange);
			AssertEquals("Customs value change on tariff 8537 should be 500", 500m, reconDec.ChangedLines[1].CustomsValueChange);

			reconDec.ChangedLines.RemoveAndDeleteAll();
			new ReconChangedLinesMerger(reconDec, ReconMergeContext.Messaging).DoMerge(false);
			AssertEquals(2, reconDec.ChangedLines.Count);
			AssertEquals("Customs value change on tariff 9903 should be zero", 0m, reconDec.ChangedLines[0].CustomsValueChange);
			AssertEquals("Customs value change on tariff 8537 should be 500", 500m, reconDec.ChangedLines[1].CustomsValueChange);
		}

		[TestDate(2019, 12, 1)]
		public void TestPayableMPFChangeOnSingleEntry()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			originalEntry.US_R_CalcOrigDuty = true;

			var invoice = originalEntry.Invoice;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigTariff = "6206.90.0040";
			invoiceLine.US_R_OrigCV = 2000.00m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6206.90.0040";
			invoiceLine2.US_UC_NKCountryOfExport = "NZ";
			invoiceLine2.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine2.JI_LinePrice = 4000m;
			invoiceLine2.US_R_OrigTariff = "6206.90.0040";
			invoiceLine2.US_R_OrigCV = 2000.00m;

			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();

			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Documents).DoMerge(false);

			AssertEquals(2, reconDeclaration.ChangedLines.Count);
			AssertEquals("Original MPF for line1", 6.93m, reconDeclaration.ChangedLines[0].US_OrigMPF);
			AssertEquals("Original MPF for line2", 6.93m, reconDeclaration.ChangedLines[1].US_OrigMPF);

			AssertEquals("Recon MPF for line1", 3.46m, reconDeclaration.ChangedLines[0].US_MPF);
			AssertEquals("Recon MPF for line2", 13.86m, reconDeclaration.ChangedLines[1].US_MPF);
			AssertEquals(-7.71m, reconDeclaration.ChangedLines[0].MPFChange);
			AssertEquals(7.71m, reconDeclaration.ChangedLines[1].MPFChange);
			AssertEquals(reconDeclaration.TotalFeeDifference, reconDeclaration.ChangedLines.Sum(x => ((ReconChangedLine)x).MPFChange));

			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Messaging).DoMerge(false);
			AssertEquals(2, reconDeclaration.ChangedLines.Count);
			AssertEquals("MPFChange is used on document only", 0m, reconDeclaration.ChangedLines[0].MPFChange);
			AssertEquals("MPFChange is used on document only", 0m, reconDeclaration.ChangedLines[1].MPFChange);
		}

		[TestDate(2020, 11, 11)]
		public void TestCalculatePayableMPFChangeCrossMultipleEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			var reconDeclaration = new ReconDeclaration(declaration);
			declaration.AllocateEntryNumber("12345678");

			var originalEntryOne = reconDeclaration.OriginalEntries.AddNew();
			originalEntryOne.CH_OrigEntryReference = "33648180583";
			originalEntryOne.US_R_DutyRateDate = ZDateTime.Today;
			originalEntryOne.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntryOne.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			originalEntryOne.US_R_CalcOrigDuty = true;
			var invoiceOne = originalEntryOne.Invoice;
			var invoiceLineOneForEntryOne = invoiceOne.JobComInvoiceLines.AddNew();
			invoiceLineOneForEntryOne.US_R_OrigTariff = "3004909290";
			invoiceLineOneForEntryOne.JI_Tariff = "3004909290";
			invoiceLineOneForEntryOne.US_R_OrigCV = 250000m;
			invoiceLineOneForEntryOne.JI_LinePrice = 200000m;
			invoiceLineOneForEntryOne.US_UC_NKCountryOfOrigin = "IE";
			var invoiceLineTwoForEntryOne = invoiceOne.JobComInvoiceLines.AddNew();
			invoiceLineTwoForEntryOne.US_R_OrigTariff = "2844400028";
			invoiceLineTwoForEntryOne.JI_Tariff = "2844400028";
			invoiceLineTwoForEntryOne.US_R_OrigCV = 10000m;
			invoiceLineTwoForEntryOne.JI_LinePrice = 10000m;
			invoiceLineTwoForEntryOne.US_UC_NKCountryOfOrigin = "CA";
			invoiceLineTwoForEntryOne.US_R_OrigSPI = "CA";
			invoiceLineTwoForEntryOne.US_SPI = "N/A";

			var originalEntryTwo = reconDeclaration.OriginalEntries.AddNew();
			originalEntryTwo.CH_OrigEntryReference = "33648180584";
			originalEntryTwo.US_R_DutyRateDate = ZDateTime.Today;
			originalEntryTwo.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntryTwo.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			originalEntryTwo.US_R_CalcOrigDuty = true;
			var invoiceTwo = originalEntryTwo.Invoice;
			var invoiceLineOneForEntryTwo = invoiceTwo.JobComInvoiceLines.AddNew();
			invoiceLineOneForEntryTwo.US_R_OrigTariff = "3004909290";
			invoiceLineOneForEntryTwo.JI_Tariff = "3004909290";
			invoiceLineOneForEntryTwo.US_R_OrigCV = 11000m;
			invoiceLineOneForEntryTwo.JI_LinePrice = 12000m;
			invoiceLineOneForEntryTwo.US_UC_NKCountryOfOrigin = "IE";
			invoiceLineOneForEntryTwo.US_R_OrigSPI = "N/A";
			invoiceLineOneForEntryTwo.US_SPI = "N/A";
			var invoiceLineTwoForEntryTwo = invoiceTwo.JobComInvoiceLines.AddNew();
			invoiceLineTwoForEntryTwo.US_R_OrigTariff = "3920992000";
			invoiceLineTwoForEntryTwo.JI_Tariff = "3920992000";
			invoiceLineTwoForEntryTwo.US_R_OrigCV = 5000m;
			invoiceLineTwoForEntryTwo.JI_LinePrice = 5000m;
			invoiceLineTwoForEntryTwo.US_UC_NKCountryOfOrigin = "CA";
			invoiceLineTwoForEntryTwo.US_R_OrigSPI = "N/A";
			invoiceLineTwoForEntryTwo.US_SPI = "S";

			var originalEntryThree = reconDeclaration.OriginalEntries.AddNew();
			originalEntryThree.CH_OrigEntryReference = "33648180585";
			originalEntryThree.US_R_DutyRateDate = ZDateTime.Today;
			originalEntryThree.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntryThree.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			originalEntryThree.US_R_CalcOrigDuty = true;
			var invoiceThree = originalEntryThree.Invoice;
			var invoiceLineOneForEntryThree = invoiceThree.JobComInvoiceLines.AddNew();
			invoiceLineOneForEntryThree.US_R_OrigTariff = "3004909290";
			invoiceLineOneForEntryThree.JI_Tariff = "3004909290";
			invoiceLineOneForEntryThree.US_R_OrigCV = 50000m;
			invoiceLineOneForEntryThree.JI_LinePrice = 55000m;
			invoiceLineOneForEntryThree.US_UC_NKCountryOfOrigin = "IE";
			var invoiceLineTwoForEntryThree = invoiceThree.JobComInvoiceLines.AddNew();
			invoiceLineTwoForEntryThree.US_R_OrigTariff = "2844400028";
			invoiceLineTwoForEntryThree.JI_Tariff = "2844400028";
			invoiceLineTwoForEntryThree.US_R_OrigCV = 130000m;
			invoiceLineTwoForEntryThree.JI_LinePrice = 140000m;
			invoiceLineTwoForEntryThree.US_UC_NKCountryOfOrigin = "CA";

			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();

			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Documents).DoMerge(false);
			AssertEquals(4, reconDeclaration.ChangedLines.Count);
			AssertEquals(-18.11m, reconDeclaration.ChangedLines[0].MPFChange);
			AssertEquals(23.71m, reconDeclaration.ChangedLines[1].MPFChange);
			AssertEquals(-17.32m, reconDeclaration.ChangedLines[2].MPFChange);
			AssertEquals(-2.13m, reconDeclaration.ChangedLines[3].MPFChange);
			AssertEquals(reconDeclaration.TotalFeeDifference, reconDeclaration.ChangedLines.Sum(x => ((ReconChangedLine)x).MPFChange));

			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Messaging).DoMerge(false);
			AssertEquals(4, reconDeclaration.ChangedLines.Count);
			AssertEquals("MPFChange is used on document only", 0m, reconDeclaration.ChangedLines[0].MPFChange);
			AssertEquals("MPFChange is used on document only", 0m, reconDeclaration.ChangedLines[1].MPFChange);
			AssertEquals("MPFChange is used on document only", 0m, reconDeclaration.ChangedLines[2].MPFChange);
			AssertEquals("MPFChange is used on document only", 0m, reconDeclaration.ChangedLines[3].MPFChange);
		}

		public void TestUS_NAFTAReconIndicator()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
				reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

				var originalEntry = reconDec.OriginalEntries.AddNew();
				originalEntry.US_R_DutyRateDate = ZDateTime.Today;
				originalEntry.US_NAFTAReconIndicator = true;
				var invoiceLine = originalEntry.Invoice.InvoiceLines.AddNew();
				invoiceLine.US_R_OrigTariff = "12345678";
				invoiceLine.JI_CustomsQuantity = 1;
				invoiceLine.US_R_OrigFirstQty = 2;
				var originalEntry1 = reconDec.OriginalEntries.AddNew();
				originalEntry1.US_R_DutyRateDate = ZDateTime.Today;
				originalEntry1.US_NAFTAReconIndicator = false;
				var invoiceLine1 = originalEntry1.Invoice.InvoiceLines.AddNew();
				invoiceLine1.US_R_OrigTariff = "98765432";
				invoiceLine1.JI_CustomsQuantity = 1;
				invoiceLine1.US_R_OrigFirstQty = 2;

				new ReconChangedLinesMerger(reconDec).DoMerge(false);
				AssertEquals(2, reconDec.ChangedLines.Count);
				var mergedLine1 = reconDec.ChangedLines.Cast<IReconEntryLine>().First(x => x.OriginalHTS == "12345678");
				Assert(mergedLine1.IsNAFTARecon);
				var mergedLine2 = reconDec.ChangedLines.Cast<IReconEntryLine>().First(x => x.OriginalHTS == "98765432");
				Assert(!mergedLine2.IsNAFTARecon);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		void SetReconOriginalValues(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigCV = invoiceLine.JI_CustomsValue >= invoiceLine.US_98GoodsValue ? (ZDecimal)(invoiceLine.JI_CustomsValue - invoiceLine.US_98GoodsValue) : invoiceLine.JI_CustomsValue;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;
			invoiceLine.US_R_OrigSPI = invoiceLine.US_SPI;
			invoiceLine.US_R_OrigSecondQty = invoiceLine.JI_CustomsSecondQuantity;
		}
	}
}
