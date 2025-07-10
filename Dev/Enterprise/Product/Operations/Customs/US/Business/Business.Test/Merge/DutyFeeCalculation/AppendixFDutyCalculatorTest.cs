using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AppendixFDutyCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2007, 3, 7)]
		public void TestCalculation0()
		{
			var tariff = LoadMostRecentTariff("0101100010");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			RunaMock(ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero);
		}

		[TestDate(2007, 3, 7)]
		public void TestCalculation1()
		{
			var tariff = LoadMostRecentTariff("1210200020");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			mock.Setup(m => m.Quantity1).Returns(10m);
			mock.Setup(m => m.UQ1).Returns("KG");
			RunaMock(.132m, "KG", ZDecimal.Zero, 1.32m);
		}

		[TestDate(2007, 3, 7)]
		public void TestCalculation2()
		{
			var tariff = LoadMostRecentTariff("8524513040");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			mock.Setup(m => m.Quantity1).Returns(1m);
			mock.Setup(m => m.UQ1).Returns("NO");
			mock.Setup(m => m.Quantity2).Returns(5m);
			mock.Setup(m => m.UQ2).Returns("KG");
			RunaMock(.048m, "KG", ZDecimal.Zero, 0.24m);
		}

		[TestDate(2007, 3, 7)]
		public void TestCalculation3()
		{
			var tariff = LoadMostRecentTariff("2206003000");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			mock.Setup(m => m.Quantity1).Returns(5m);    // Litres
			mock.Setup(m => m.UQ1).Returns("L");
			mock.Setup(m => m.Quantity2).Returns(.15m);  // PFL
			mock.Setup(m => m.UQ2).Returns("PFL");
			RunaMock(0m, ZString.Empty, ZDecimal.Zero, 0.19m);
		}

		[TestDate(2007, 3, 7)]
		public void TestCalculation4()
		{
			var tariff = LoadMostRecentTariff("2942000500");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			mock.Setup(m => m.Quantity1).Returns(5m);    // Kilograms
			mock.Setup(m => m.UQ1).Returns("KG");
			RunaMock(0m, "KG", 6.5m, 3.25m);
		}

		[TestDate(2007, 3, 7)]
		public void TestCalculation5()
		{
			var tariff = LoadMostRecentTariff("6103101000");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			mock.Setup(m => m.Quantity1).Returns(2m); // NO
			mock.Setup(m => m.UQ1).Returns("NO");
			mock.Setup(m => m.Quantity2).Returns(3m); // KG
			mock.Setup(m => m.UQ2).Returns("KG");
			RunaMock(.388m, "KG", 10m, 6.16m);
		}

		[TestDate(2007, 3, 7)]
		public void TestCalculation6()
		{
			var tariff = LoadMostRecentTariff("9106902000");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			mock.Setup(m => m.Quantity1).Returns(2m); // NO
			mock.Setup(m => m.UQ1).Returns("NO");
			mock.Setup(m => m.Quantity2).Returns(3m); // JWL
			mock.Setup(m => m.UQ2).Returns("JWL");
			RunaMock(0m, ZString.Empty, 5.6m, 3.58m);
		}

		[TestDate(2007, 3, 7)]
		public void TestCalculation7()
		{
			var tariff = LoadMostRecentTariff("3506990000");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			RunaMock(0m, "", 2.1m, 1.05m);
		}

		//[ExpectExceptionMessage(typeof(InvalidOperationException), "Should be agregated separately")]
		public void TestCalculation9DerivedDuty()
		{
			var tariff = LoadMostRecentTariff("6204223060");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			RunaMock(0m, "", 0m, 0m);
		}

		public void TestVExcluded()
		{
			var tariff = LoadMostRecentTariff("1701120500");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			mock.Setup(m => m.Quantity1).Returns(100m); // KG
			mock.Setup(m => m.UQ1).Returns("KG");
			mock.Setup(m => m.Quantity2).Returns(30m); // Degrees
			mock.Setup(m => m.UQ1).Returns("DG");
			mock.Setup(m => m.SpecialProgramsIndicatorSecondary).Returns(SecondarySpecProgIndicatorList.Codes.V);
			mock.Setup(m => m.IsSetVLine).Returns(true);
			RunaMock(0m, "", 0m, 0m);
		}

		[TestDate(2008, 9, 11)]
		public void TestAppendixFDutyCalculatorForPrinting()
		{
			CusEntryLine line = Factory.NewWithValidTestData<CusEntryLine>();
			line.CL_AdValoremTariff = "7326908587";

			CusEntryLine parentLine = Factory.NewWithValidTestData<CusEntryLine>();
			parentLine.CL_AdValoremTariff = "98130030";
			parentLine.CL_CustomsValue = 24055m;

			var tibLine = new DutyDataProxy(line);
			tibLine.CustomsValue += parentLine.CustomsValue.Amount;

			calculator = new AppendixFDutyCalculator(tibLine, Factory);

			IDutyResult dutyResult = calculator.DutyResult;
			AssertEquals("Total duty calculation amount", 697.6m, dutyResult.TotalAmount.Amount);
		}

		[TestDate(2008, 9, 11)]
		public void TestAppendixFDutyCalculatorForPrintingValueOnChildLine()
		{
			CusEntryLine line = Factory.NewWithValidTestData<CusEntryLine>();
			line.CL_AdValoremTariff = "7326908587";
			line.CL_CustomsValue = 24055m;

			CusEntryLine parentLine = Factory.NewWithValidTestData<CusEntryLine>();
			parentLine.CL_AdValoremTariff = "98130030";

			var tibLine = new DutyDataProxy(line);
			tibLine.CustomsValue += parentLine.CustomsValue.Amount;
			calculator = new AppendixFDutyCalculator(tibLine, Factory);

			IDutyResult dutyResult = calculator.DutyResult;
			AssertEquals("Total duty calculation amount should calculate if customs value on parent or child", 697.6m, dutyResult.TotalAmount.Amount);
		}

		public void TestCalculationK()
		{
			var mock = new Mock<IDutyData> { CallBase = true };
			mock.Setup(m => m.DateForDutyCalculation).Returns(new ZDate(2007, 2, 19));
			mock.Setup(m => m.SpecialProgramsIndicatorCountry).Returns("");
			mock.Setup(m => m.SpecialProgramsIndicatorPrimary).Returns("");
			mock.Setup(m => m.SpecialProgramsIndicatorSecondary).Returns("");
			mock.Setup(m => m.CountryOfOrigin).Returns("");
			mock.Setup(m => m.CustomsValue).Returns(50m);
			mock.Setup(m => m.IsSetVLine).Returns(false);
			mock.Setup(m => m.IsSetXLine).Returns(false);
			calculator = new AppendixFDutyCalculator(mock.Object, Factory);

			// TODO: Fix this test. Tariff appears to always return 'null'.
			var tariff = LoadMostRecentTariff("1701120500");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			mock.Setup(m => m.Quantity1).Returns(100m); // KG
			mock.SetupSequence(m => m.UQ1).Returns("KG").Returns("DG");
			mock.Setup(m => m.Quantity2).Returns(30m); // Degrees

			IDutyResult dutyResult = calculator.DutyResult;

			AssertNotNull("Tariff may not be empty", mock.Object.ImportTariff);
			AssertEquals("Per Unit Amount", 0.03143854m, dutyResult.PerUnitAmount.Amount);
			AssertEquals("Per Unit UQ", "KG", dutyResult.PerUnitUQ);
			AssertEquals("Percent of Value", 0m, dutyResult.PercentOfValue);
			AssertEquals("Total Amount", 3.14m, dutyResult.TotalAmount.Amount);
		}

		public void TestCalculationX()
		{
			var tariff = LoadMostRecentTariff("1702900500");
			mock.Setup(m => m.ImportTariff).Returns(tariff);
			RunaMock(0m, "", 0m, 0m);
		}

		public void TestCalculationForACE_XVLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1902.19.4000";
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_LinePrice = 5000m;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "0712.31.1000";
			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.JI_LinePrice = 1300m;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "2002.90.8020";
			invoiceLine3.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.JI_LinePrice = 1300m;

			var invoiceLine4 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine4.JI_Tariff = "1902.19.4000";
			invoiceLine4.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine4.JI_LinePrice = 2400m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("MergedLines", 4, entry.MergedLines.Count);

			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			var entryLine3 = entry.MergedLines[2];
			var entryLine4 = entry.MergedLines[3];

			AssertEquals("parent X line duty", 320m, entryLine1.RandomLine.US_Duty);
			AssertEquals("V line duty", 0m, entryLine2.RandomLine.US_Duty);
			AssertEquals("V line duty", 0m, entryLine3.RandomLine.US_Duty);
			AssertEquals("V line duty", 0m, entryLine4.RandomLine.US_Duty);
		}

		[TestDate(2021, 08, 31)]
		public void TestFactoryCachedValueUsedInCalculator()
		{
			#region Setup Tariffs

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "3603009020";
			tariff.UE_Unit1 = ABIUnitOfMeasureList.Codes.Number;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.002;
			tariff.UE_DateFrom = ZDateTime.Today.AddMonths(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_LinePrice = 100m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine = declaration.FormalEntry.AllEntryLines[0];
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedEntryLine = newFactory.Load<CusEntryLine>(entryLine.PK);
			USCTariff tariffCached;
			Assert("Cached Value Not Found In Factory", !newFactory.TryGetValueFromCacheOnly("360300902031-Aug-21", out tariffCached));
			AssertNull(tariffCached);

			var calculator = new AppendixFDutyCalculator(loadedEntryLine, newFactory);
			var result = calculator.DutyResult;
			Assert("Cached Value Found In Factory", newFactory.TryGetValueFromCacheOnly("360300902031-Aug-21", out tariffCached));
			AssertNotNull(tariffCached);
			AssertEquals(0.2m, result.TotalAmount.Amount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mock = new Mock<IDutyData>();
			mock.Setup(m => m.DateForDutyCalculation).Returns(new ZDate(2007, 2, 19));
			mock.Setup(m => m.SpecialProgramsIndicatorCountry).Returns("");
			mock.Setup(m => m.SpecialProgramsIndicatorPrimary).Returns("");
			mock.Setup(m => m.SpecialProgramsIndicatorSecondary).Returns("");
			mock.Setup(m => m.CountryOfOrigin).Returns("");
			mock.Setup(m => m.CustomsValue).Returns(50m);
			mock.Setup(m => m.IsSetVLine).Returns(false);
			mock.Setup(m => m.IsSetXLine).Returns(false);
			calculator = new AppendixFDutyCalculator(mock.Object, Factory);
		}

		Mock<IDutyData> mock;
		AppendixFDutyCalculator calculator;

		void RunaMock(ZDecimal perUnitAmount, ZString perUnitUQ, ZDecimal percentOfValue, ZDecimal totalAmount)
		{
			IDutyResult dutyResult = calculator.DutyResult;

			AssertNotNull("Tariff may not be empty", mock.Object.ImportTariff);
			AssertEquals("Per Unit Amount", perUnitAmount, dutyResult.PerUnitAmount.Amount);
			AssertEquals("Per Unit UQ", perUnitUQ, dutyResult.PerUnitUQ);
			AssertEquals("Percent of Value", percentOfValue, dutyResult.PercentOfValue);
			AssertEquals("Total Amount", totalAmount, dutyResult.TotalAmount.Amount);
		}

		USCTariff LoadMostRecentTariff(ZString tariffCode) => new USCTariff.Loader(Factory).LoadBestMatchOrMostRecent(tariffCode, new ZDate(2007, 2, 19));
	}
}
