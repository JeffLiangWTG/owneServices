using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestDutyAndFeeAnalysisGathering()
		{
			Helper.SetupReferenceData();
			var declaration = Helper.CreateDeclaration();
			var invoiceLine1 = declaration.InvoiceLines[0];
			var invoiceLine2 = declaration.InvoiceLines[1];
			var invoiceLine3 = declaration.InvoiceLines[2];
			var invoiceLine4 = declaration.InvoiceLines[3];
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CombineAssertions(() =>
			{
				var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				AssertEquals("ensEntry.TotalDutyAmount", 29800m, ensEntry.TotalDutyAmount);
				AssertEquals("ensEntry.MPFAmountForEntry", 508.70m, ensEntry.MPFAmountForEntry);
				AssertEquals("ensEntry.Charges.Count", 1, ensEntry.Charges.Count);
				AssertEquals("ensEntry.499 Customs", 508.70m, ensEntry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

				var entryLines = ensEntry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber).ToArray();
				AssertEquals("entryLines.Length", 4, entryLines.Length);
				var entryLine1 = entryLines[0];
				AssertData(entryLine1, 13000m, 692.80m, 169.57m, 2);
				AssertData("entryLine1 1", entryLine1.Fees[0], Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 13000m);
				AssertData("entryLine1 2", entryLine1.Fees[1], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);
				var entryLine2 = entryLines[1];
				AssertData(entryLine2, 8400m, 692.80m, 169.57m, 2);
				AssertData("entryLine2 1", entryLine2.Fees[0], Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 8400m);
				AssertData("entryLine2 2", entryLine2.Fees[1], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);

				var entryLine3 = entryLines[2];
				AssertData(entryLine3, 0m, 0, 0m, 0);
				var entryLine4 = entryLines[3];
				AssertData(entryLine4, 8400m, 692.80m, 169.56m, 2);
				AssertData("entryLine4 1", entryLine4.Fees[0], Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 8400m);
				AssertData("entryLine4 2", entryLine4.Fees[1], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);

				AssertData(invoiceLine1, 13000m, 169.57m, 0m, 0m, "AU", 13000m, 127.18m, 1);
				AssertData("invoiceLine1 1", invoiceLine1.FeeCusCodes[0], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);
				AssertData(invoiceLine2, 8400m, 169.57, 0m, 0m, "CA", 8400m, 127.18m, 1);
				AssertData("invoiceLine2 1", invoiceLine2.FeeCusCodes[0], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, "AU", 13000m, 127.17m, 0);
				AssertData(invoiceLine4, 8400m, 169.56m, 8400m, 508.70m, ZString.Empty, 8400m, 127.17m, 1);
				AssertData("invoiceLine4 1", invoiceLine4.FeeCusCodes[0], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);
			});
		}

		public void TestCalculateActualDuty()
		{
			CombineAssertions(() =>
			{
				Helper.SetupReferenceData();
				var declaration = Helper.CreateDeclaration();
				var invoiceLine1 = declaration.InvoiceLines[0];
				var invoiceLine2 = declaration.InvoiceLines[1];
				var invoiceLine3 = declaration.InvoiceLines[2];
				var invoiceLine4 = declaration.InvoiceLines[3];

				new EntryDutyCalculator(declaration).CalculateActualDuty();
				AssertEquals(0, declaration.CustomsEntryHeaders.Count);
				AssertData(invoiceLine1, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine2, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine4, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);

				var entry = Helper.CreateEntry(declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4);
				var entryLine1 = invoiceLine1.CusEntryLine;
				var entryLine2 = invoiceLine2.CusEntryLine;
				var entryLine3 = invoiceLine3.CusEntryLine;
				var entryLine4 = invoiceLine4.CusEntryLine;

				declaration.US_NoDutyCalc = true;
				new EntryDutyCalculator(declaration).CalculateActualDuty();
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				AssertData(entry, 0m, 0m, 0);
				AssertEquals(4, entry.MergedLines.Count);
				AssertData(entryLine1, 0m, 0m, 0m, 0);
				AssertData(entryLine2, 0m, 0m, 0m, 0);
				AssertData(entryLine3, 0m, 0m, 0m, 0);
				AssertData(entryLine4, 0m, 0m, 0m, 0);
				AssertData(invoiceLine1, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine2, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine4, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);

				declaration.US_NoDutyCalc = false;
				new EntryDutyCalculator(declaration).CalculateActualDuty();
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				AssertData(entry, 29800m, 508.70m, 1);
				AssertEquals(4, entry.MergedLines.Count);
				AssertEquals("entry.499 Customs", 508.70m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertData(entryLine1, 13000m, 692.80m, 169.57m, 2);
				AssertData("entryLine1 1", entryLine1.Fees[0], Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 13000m);
				AssertData("entryLine1 2", entryLine1.Fees[1], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);
				AssertData(entryLine2, 8400m, 692.80m, 169.57m, 2);
				AssertData("entryLine2 1", entryLine2.Fees[0], Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 8400m);
				AssertData("entryLine2 2", entryLine2.Fees[1], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);
				AssertData(entryLine3, 0m, 0m, 0m, 0);
				AssertData(entryLine4, 8400m, 692.80m, 169.56m, 2);
				AssertData("entryLine4 1", entryLine4.Fees[0], Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 8400m);
				AssertData("entryLine4 2", entryLine4.Fees[1], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);

				AssertData(invoiceLine1, 13000m, 169.57m, 0m, 0m, ZString.Empty, 0m, 0m, 1);
				AssertData("invoiceLine1 1", invoiceLine1.FeeCusCodes[0], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);
				AssertData(invoiceLine2, 8400m, 169.57m, 0m, 0m, ZString.Empty, 0m, 0m, 1);
				AssertData("invoiceLine2 1", invoiceLine2.FeeCusCodes[0], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine4, 8400m, 169.56m, 0m, 0m, ZString.Empty, 0m, 0m, 1);
				AssertData("invoiceLine4 1", invoiceLine4.FeeCusCodes[0], Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 692.80m);

				declaration = Helper.CreateDeclaration();
				invoiceLine1 = declaration.InvoiceLines[0];
				invoiceLine2 = declaration.InvoiceLines[1];
				invoiceLine3 = declaration.InvoiceLines[2];
				invoiceLine4 = declaration.InvoiceLines[3];
				entry = Helper.CreateEntry(declaration, CusEntryHeaderMessageTypeList.Codes.CargoRelease, invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4);
				entryLine1 = invoiceLine1.CusEntryLine;
				entryLine2 = invoiceLine2.CusEntryLine;
				entryLine3 = invoiceLine3.CusEntryLine;
				entryLine4 = invoiceLine4.CusEntryLine;
				new EntryDutyCalculator(declaration).CalculateActualDuty();
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				AssertData(entry, 0m, 0m, 0);
				AssertEquals(4, entry.MergedLines.Count);
				AssertData(entryLine1, 0m, 0m, 0m, 0);
				AssertData(entryLine2, 0m, 0m, 0m, 0);
				AssertData(entryLine3, 0m, 0m, 0m, 0);
				AssertData(entryLine4, 0m, 0m, 0m, 0);
				AssertData(invoiceLine1, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine2, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine4, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
			});
		}

		public void TestCalculateDutyReportingDuty()
		{
			CombineAssertions(() =>
			{
				Helper.SetupReferenceData();
				var declaration = Helper.CreateDeclaration();
				var invoiceLine1 = declaration.InvoiceLines[0];
				var invoiceLine2 = declaration.InvoiceLines[1];
				var invoiceLine3 = declaration.InvoiceLines[2];
				var invoiceLine4 = declaration.InvoiceLines[3];

				new EntryDutyCalculator(declaration).CalculateDutyReportingDuty();
				AssertEquals(0, declaration.CustomsEntryHeaders.Count);
				AssertData(invoiceLine1, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine2, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine4, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);

				var entry = Helper.CreateEntry(declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4);
				var entryLine1 = invoiceLine1.CusEntryLine;
				var entryLine2 = invoiceLine2.CusEntryLine;
				var entryLine3 = invoiceLine3.CusEntryLine;
				var entryLine4 = invoiceLine4.CusEntryLine;

				declaration.US_NoDutyCalc = true;
				new EntryDutyCalculator(declaration).CalculateDutyReportingDuty();
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				AssertData(entry, 0m, 0m, 0);
				AssertEquals(4, entry.MergedLines.Count);
				AssertData(entryLine1, 0m, 0m, 0m, 0);
				AssertData(entryLine2, 0m, 0m, 0m, 0);
				AssertData(entryLine3, 0m, 0m, 0m, 0);
				AssertData(entryLine4, 0m, 0m, 0m, 0);
				AssertData(invoiceLine1, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine2, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine4, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);

				declaration.US_NoDutyCalc = false;
				new EntryDutyCalculator(declaration).CalculateDutyReportingDuty();
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				AssertData(entry, 0m, 0m, 0);
				AssertEquals(4, entry.MergedLines.Count);
				AssertData(entryLine1, 0m, 0m, 0m, 0);
				AssertData(entryLine2, 0m, 0m, 0m, 0);
				AssertData(entryLine3, 0m, 0m, 0m, 0);
				AssertData(entryLine4, 0m, 0m, 0m, 0);
				AssertData(invoiceLine1, 0m, 0m, 0m, 0m, "AU", 13000m, 127.18m, 0);
				AssertData(invoiceLine2, 0m, 0m, 0m, 0m, "CA", 8400m, 127.18m, 0);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, "AU", 13000m, 127.17m, 0);
				AssertData(invoiceLine4, 0m, 0m, 8400, 508.70m, ZString.Empty, 8400m, 127.17m, 0);

				declaration = Helper.CreateDeclaration();
				invoiceLine1 = declaration.InvoiceLines[0];
				invoiceLine2 = declaration.InvoiceLines[1];
				invoiceLine3 = declaration.InvoiceLines[2];
				invoiceLine4 = declaration.InvoiceLines[3];
				entry = Helper.CreateEntry(declaration, CusEntryHeaderMessageTypeList.Codes.CargoRelease, invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4);
				entryLine1 = invoiceLine1.CusEntryLine;
				entryLine2 = invoiceLine2.CusEntryLine;
				entryLine3 = invoiceLine3.CusEntryLine;
				entryLine4 = invoiceLine4.CusEntryLine;
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				AssertData(entry, 0m, 0m, 0);
				AssertEquals(4, entry.MergedLines.Count);
				AssertData(entryLine1, 0m, 0m, 0m, 0);
				AssertData(entryLine2, 0m, 0m, 0m, 0);
				AssertData(entryLine3, 0m, 0m, 0m, 0);
				AssertData(entryLine4, 0m, 0m, 0m, 0);
				AssertData(invoiceLine1, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine2, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine3, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
				AssertData(invoiceLine4, 0m, 0m, 0m, 0m, ZString.Empty, 0m, 0m, 0);
			});
		}

		void AssertData(string message, FeeCusCodeData fee, ZString code, ZDecimal amount)
		{
			AssertEquals(message + " fee.CY_Code", code, fee.CY_Code);
			AssertEquals(message + " fee.CY_FeeAmount", amount, fee.CY_FeeAmount);
		}

		void AssertData(string message, CusEntryLineFee fee, ZString type, ZDecimal amount)
		{
			AssertEquals(message + " fee.CF_ChargeType", type, fee.CF_ChargeType);
			AssertEquals(message + " fee.CF_ChargeAmount", amount, fee.CF_ChargeAmount);
		}

		void AssertData(CusEntryHeader entry, ZDecimal duty, ZDecimal mpfAmout, int chargesCount)
		{
			AssertEquals("entry.TotalDutyAmount", duty, entry.TotalDutyAmount);
			AssertEquals("entry.MPFAmountForEntry", mpfAmout, entry.MPFAmountForEntry);
			AssertEquals("entry.Charges.Count", chargesCount, entry.Charges.Count);
		}

		void AssertData(CusEntryLine entryLine, ZDecimal dutyAmount, ZDecimal mpfAmount, ZDecimal payableMPFAmount, int feesCount)
		{
			var lineNumber = entryLine.CL_LineNumber;
			AssertEquals(lineNumber + " entryLine.DutyAmount", dutyAmount, entryLine.DutyAmount);
			AssertEquals(lineNumber + " entryLine.MPFAmount", mpfAmount, entryLine.MPFAmount);
			AssertEquals(lineNumber + " entryLine.PayableMPFAmount", payableMPFAmount, entryLine.PayableMPFAmount);
			AssertEquals(lineNumber + " entryLine.Fees.Count", feesCount, entryLine.Fees.Count);
		}

		void AssertData(JobComInvoiceLine invoiceLine, ZDecimal dutyAmount, ZDecimal payableMPFAmount, ZDecimal ftaDutyAmount, ZDecimal ftaPayableMPFAmount, ZString ftaSPI, ZDecimal nonFTADutyAmount, ZDecimal nonFTAPayableMPFAmount, int feesCount)
		{
			var lineNumber = invoiceLine.JI_LineNo;
			AssertEquals(lineNumber + " invoiceLine.US_Duty", dutyAmount, invoiceLine.US_Duty);
			AssertEquals(lineNumber + " invoiceLine.US_PayableMPF", payableMPFAmount, invoiceLine.US_PayableMPF);
			AssertEquals(lineNumber + " invoiceLine.US_FTADuty", ftaDutyAmount, invoiceLine.US_FTADuty);
			AssertEquals(lineNumber + " invoiceLine.US_FTAPayableMPF", ftaPayableMPFAmount, invoiceLine.US_FTAPayableMPF);
			AssertEquals(lineNumber + " invoiceLine.US_FTASPI", ftaSPI, invoiceLine.US_FTASPI);
			AssertEquals(lineNumber + " invoiceLine.US_NonFTADuty", nonFTADutyAmount, invoiceLine.US_NonFTADuty);
			AssertEquals(lineNumber + " invoiceLine.US_NonFTAPayableMPF", nonFTAPayableMPFAmount, invoiceLine.US_NonFTAPayableMPF);
			AssertEquals(lineNumber + " invoiceLine.FeeCusCodes.Count", feesCount, invoiceLine.FeeCusCodes.Count);
		}

		EntryDutyCalculatorTestDataHelper Helper => helper ?? (helper = new EntryDutyCalculatorTestDataHelper(Factory));
		EntryDutyCalculatorTestDataHelper helper;
	}

	public class EntryDutyCalculatorTestDataHelper : UniversalReferenceTestDataHelper
	{
		public EntryDutyCalculatorTestDataHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusEntryHeader CreateEntry(JobDeclaration declaration, ZString messageType, params JobComInvoiceLine[] invoiceLines)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = messageType;
			foreach (var invoiceLine in invoiceLines)
			{
				var entryLine = entry.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				entryLine.CL_LineNumber = invoiceLine.JI_LineNo;
				entryLine.CL_AdValoremTariff = invoiceLine.JI_Tariff.Left(CusEntryLine.Schema.CL_AdValoremTariffMaxLength);
				entryLine.MergeInvoiceLine(invoiceLine);
			}
			return entry;
		}

		public const string tariffCode1 = "3920992123";
		public const string tariffCode2 = "3824994812";
		public JobDeclaration CreateDeclaration()
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 600000m;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariffCode1;
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_LinePrice = 200000m;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine1.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariffCode2;
			invoiceLine2.JI_CustomsQuantity = 100m;
			invoiceLine2.JI_LinePrice = 200000m;
			invoiceLine2.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Cambodia;
			invoiceLine2.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XC;
			invoiceLine2.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = tariffCode1;
			invoiceLine3.JI_CustomsQuantity = 100m;
			invoiceLine3.JI_LinePrice = 200000m;
			invoiceLine3.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine3.US_SPI = SpecialProgramList.Codes.AU;
			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = tariffCode2;
			invoiceLine4.JI_CustomsQuantity = 100m;
			invoiceLine4.JI_LinePrice = 200000m;
			invoiceLine4.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Cambodia;
			invoiceLine4.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Eritrea;
			invoiceLine4.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			return declaration;
		}

		public void SetupReferenceData()
		{
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			var tariff1 = SetupTariff(tariffCode1, "BINDERS,OF HYDROCARBS,PETR", 0.065m, 0.25m, "AU");
			var tariff1DutyRate = SetupDutyRate(tariff1, "5", Core.Constants.CountryCodes.KoreaSouth, 0.012m);
			var tariff2 = SetupTariff(tariffCode2, "PLAST,FLAT,FILM/STRIP,OTHE", 0.042m, 0.25m, "CA");

			CreateRefCusTaxOrFeeType("AVL", "ad valorem");
			CreateTaxOrFee("499", 0.3464m, "US", 26.22m, 508.70m, "AVL", new ZDateTime(ZDateTime.Today.Year, 1, 1), new ZDateTime(2079, 6, 6), "Merchandise Processing Fee");
		}

		public USCTariffDutyRate SetupDutyRate(USCTariff tariff, ZString dutyElement, ZString countryCode, ZDecimal adValoremSpecialRate)
		{
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_DutyElement = dutyElement;
			dutyRate.UD_ISOCountryCode = countryCode;
			dutyRate.UD_AdValoremSpecialRate = adValoremSpecialRate;
			return dutyRate;
		}

		public USCTariff SetupTariff(ZString tariffCode, ZString description, ZDecimal adValoremRate1, ZDecimal adValoremRate2, ZString spi)
		{
			var tariff = factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_DateFrom = ZDateTime.Today.AddMonths(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_ShortDescription = description;
			tariff.UE_Column1RateAdValorem = adValoremRate1;
			tariff.UE_Column2RateAdValorem = adValoremRate2;
			tariff.UE_SPICode = spi;
			tariff.UE_DutyComputationCode = "7";
			tariff.UE_NumberOfReportingUnits = 1;
			tariff.UE_Unit1 = Core.Constants.Weight.Kilograms;
			return tariff;
		}
	}
}
