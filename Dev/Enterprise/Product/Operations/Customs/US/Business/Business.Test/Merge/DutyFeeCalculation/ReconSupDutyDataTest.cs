using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconSupDutyDataTest : TestCaseWithFactory
	{
		[TestDate(2008, 1, 3)]
		public void Test9820CaribbeanBasin()
		{
			invoiceLine.US_SupTariff = "98201103";
			invoiceLine.JI_Tariff = "6102.20.00 10";//15.9%
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsSecondQuantity = 2500m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;

			invoiceLine.US_R_OrigSupTariff = "98201103";
			invoiceLine.US_R_OrigTariff = "6102200010";//15.9%
			invoiceLine.US_R_OrigCV = 15000m;
			invoiceLine.US_R_OrigSecondQty = 2600m;
			invoiceLine.US_R_OrigCottonFeeExempt = YesNoDefaultList.Codes.Yes;

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals("No MPF", 0m, originalEntry.ReconMPF);
			AssertEquals("No Cotton", 0m, originalEntry.ReconCotton);

			AssertEquals("No MPF", 0m, originalEntry.OriginalMPF);
			AssertEquals("No Cotton", 0m, originalEntry.OriginalCotton);
		}

		public void TestCalculateException()
		{
			invoiceLine.TariffCalculateExceptionMessage = "Test error1";
			var dutyData = new ReconSupDutyData(invoiceLine, ZDateTime.BrettsBirthday);
			AssertEquals("Test error1", ((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException);

			((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException = "Test error2";
			AssertEquals("Test error2", invoiceLine.TariffCalculateExceptionMessage);
		}

		public void TestSecondaryLines()
		{
			invoiceLine.JI_Tariff = "8457.20.0010";
			invoiceLine.US_SupTariff = "9802.00.8068";
			invoiceLine.US_98GoodsValue = 319m;
			invoiceLine.JI_LinePrice = 1356m;

			var dutyData = new ReconSupDutyData(invoiceLine, ZDateTime.BrettsBirthday);
			Assert(!((IDutyData)dutyData).IsSecondaryTariffLine);
			AssertEquals(1, ((IFeeCalculationDataProvider)dutyData).SecondaryLines.Count());

			invoiceLine.JI_Tariff = "9102.11.1010";
			AssertEquals("PreCondition:SecondaryTariffLines are added for watch", 3, invoiceLine.SecondaryTariffLines.Count());
			var dutyData2 = new ReconSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);

			Assert(!((IDutyData)dutyData2).IsSecondaryTariffLine);
			AssertEquals("include supData for the first child line", 7, ((IFeeCalculationDataProvider)dutyData2).SecondaryLines.Count());

			var firstChildLine = invoiceLine.SecondaryTariffLines.ElementAt(0);
			var dutyData3 = new ReconSupDutyData(firstChildLine, originalEntry.US_R_DateForMPFCalc);
			Assert("It is a secondary line of the parent's sup line", ((IDutyData)dutyData3).IsSecondaryTariffLine);
			AssertEquals("no secondary line of own", 0, ((IFeeCalculationDataProvider)dutyData3).SecondaryLines.Count());
		}

		[TestDate(2017, 12, 1)]
		public void TestHasMPF()
		{
			originalEntry.US_R_DutyRateDate = new ZDateTime(2009, 1, 1);
			invoiceLine.US_SupTariff = "99126241";
			invoiceLine.JI_Tariff = "6102.20.00 10";//15.9%
			invoiceLine.JI_LinePrice = 2m;
			invoiceLine.US_R_OrigSupTariff = "99126241";
			invoiceLine.US_R_OrigTariff = "6102.20.00 10";//15.9%
			invoiceLine.US_R_OrigCV = 2m;

			reconDec.CalculateDutyFeesForAllEntries();

			Assert(invoiceLine.US_HasMPF);
			Assert(invoiceLine.US_R_OrigHasMPF);

			AssertEquals(25m, originalEntry.ReconMPF);
			AssertEquals(25m, originalEntry.OriginalMPF);
		}

		[TestDate(2007, 3, 19)]
		public void Test9902ReductionInRatesOfDuty()
		{
			invoiceLine.US_SupTariff = "9902.01.21";    // 6%
			invoiceLine.JI_Tariff = "2933.19.2300";
			invoiceLine.JI_LinePrice = 20000m;  // 6.5%
			invoiceLine.JI_CustomsQuantity = 10m;

			invoiceLine.US_R_OrigSupTariff = "9902.01.21";  // 6%
			invoiceLine.US_R_OrigTariff = "2933.19.2300";
			invoiceLine.US_R_OrigCV = 25000m;   // 6.5%
			invoiceLine.US_R_OrigFirstQty = 10m;

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals(1200m, originalEntry.ReconDuty);

			AssertEquals(1500m, originalEntry.OriginalDuty);
		}

		[TestDate(2007, 3, 19)]
		public void Test9903InLieu()
		{
			invoiceLine.US_SupTariff = "9903.02.21";    // 100%
			invoiceLine.JI_Tariff = "0201100510";   // normally 4.4c/kg
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 20000m;

			invoiceLine.US_R_OrigSupTariff = "9903.02.21";  // 100%
			invoiceLine.US_R_OrigTariff = "0201100510"; // normally 4.4c/kg
			invoiceLine.US_R_OrigFirstQty = 1000m;
			invoiceLine.US_R_OrigCV = 24000m;

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals("Duty Amount", 20000.00m, originalEntry.ReconDuty);
			AssertEquals("Duty Amount", 24000.00m, originalEntry.OriginalDuty);
		}

		[TestDate(2007, 3, 19)]
		public void Test9906InLieuMexico()
		{
			invoiceLine.US_SupTariff = "9906.07.19";    // 1.6%
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.JI_Tariff = "0704904020";   // normally 20%
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.US_SPI = "MX";

			invoiceLine.US_R_OrigSupTariff = "9906.07.19";  // 1.6%
			invoiceLine.US_R_OrigTariff = "0704904020"; // normally 20%
			invoiceLine.US_R_OrigCV = 24000m;
			invoiceLine.US_R_OrigSPI = "MX";

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals(320.00m, originalEntry.ReconDuty);
			AssertEquals(384.00m, originalEntry.OriginalDuty);
		}

		[TestDate(2008, 12, 31)]
		public void Test99010050FromKR()
		{
			invoiceLine.US_SupTariff = "99010050";//0.14270000 * first Qty
			invoiceLine.US_SupQty1 = 5000m;
			invoiceLine.JI_Tariff = "2207106000";//0.025%
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_LinePrice = 10000m;

			invoiceLine.US_R_OrigSupTariff = "99010050";
			invoiceLine.US_R_OrigSupQty1 = 6000m;
			invoiceLine.US_R_OrigTariff = "2207106000";
			invoiceLine.US_R_OrigFirstQty = 6000m;
			invoiceLine.US_R_OrigCV = 15000m;

			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("Total Duty Amount for normal additional duty calculation", 963.5m, originalEntry.ReconDuty);
			AssertEquals("Total Duty Amount for normal additional duty calculation", 1231.20m, originalEntry.OriginalDuty);
		}

		public void TestQuantityImplementation()
		{
			IDutyData supDutyData = new ReconSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);

			invoiceLine.US_SupUQ1 = "KG";
			invoiceLine.US_SupUQ2 = "LT";
			invoiceLine.US_SupUQ3 = "NO";
			invoiceLine.US_SupQty1 = 2.523m;
			invoiceLine.US_SupQty2 = 4.625m;
			invoiceLine.US_SupQty3 = 8.867m;

			AssertEquals("Quantity1 rounded", 3m, supDutyData.Quantity1);
			AssertEquals("UQ1", "KG", supDutyData.UQ1);
			AssertEquals("Quantity2", 5m, supDutyData.Quantity2);
			AssertEquals("UQ2", "LT", supDutyData.UQ2);
			AssertEquals("Quantity3", 9m, supDutyData.Quantity3);
			AssertEquals("UQ3", "NO", supDutyData.UQ3);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 1.44m;

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.US_SupUQ1 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_SupUQ2 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_SupUQ3 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_SupQty1 = 2.523m;
			invoiceLine.US_SupQty2 = 4.625m;
			invoiceLine.US_SupQty3 = 8.867m;

			AssertEquals("Quantity1 rounded to two decimals", 2.52m, supDutyData.Quantity1);

			invoiceLine.US_R_Textile = true;
			AssertEquals("Quantity1 should be rounded to whole number, because is textile", 3m, supDutyData.Quantity1);
			invoiceLine.US_R_Textile = false;

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
			tariff.UE_Column2RateSpecific = 1.44m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CU";

			AssertEquals("Quantity2 rounded to two decimals", 4.63m, supDutyData.Quantity2);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			dutyRate.UD_TaxFeeAdvalorem = 1.4m;
			dutyRate.UD_TaxFeeSpecificRate = 1.6m;

			AssertEquals("Quantity3 rounded to two decimals", 8.87m, supDutyData.Quantity3);
		}

		[TestDate(2019, 6, 1)]
		public void TestCalculateDutyAndFeeWithSupChangedLinesOnly()
		{
			originalEntry.US_R_DutyRateDate = new ZDateTime(2019, 6, 1);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 1123.32m);
			originalEntry.US_R_ChangedLinesOnly = true;
			invoiceLine.US_SupTariff = "9904.12.18";    // 54.6%
			invoiceLine.JI_Tariff = "2909.19.1800";     // 5.5%
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigSupTariff = "9904.12.18";
			invoiceLine.US_R_OrigTariff = "2909.19.1800";
			invoiceLine.US_R_OrigCV = 1500m;
			invoiceLine.US_R_OrigDuty = 82.5;
			invoiceLine.US_R_OrigSupDuty = 819m;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals(55m, invoiceLine.US_Duty);
			AssertEquals(546m, invoiceLine.US_SupDuty);
			AssertEquals("Total Duty payable", 822.82000m, originalEntry.ReconDuty);

			AssertEquals(82.5m, invoiceLine.US_R_OrigDuty);
			AssertEquals(819m, invoiceLine.US_R_OrigSupDuty);
			AssertEquals("Total Duty payable", 1123.32000m, originalEntry.OriginalDuty);
		}

		[TestDate(2020, 05, 29)]
		public void TestCombineLineInterface()
		{
			#region Setup Tariffs

			var tariff99038801 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038801")).LastOrDefault();
			if (tariff99038801 == null)
			{
				tariff99038801 = Factory.New<USCTariff>();
				tariff99038801.UE_Tariff = "99038801";
				tariff99038801.UE_DutyComputationCode = "7";
				tariff99038801.UE_Column1RateAdValorem = 0.25m;
			}
			tariff99038801.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038801.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff99038804 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038804")).LastOrDefault();
			if (tariff99038804 == null)
			{
				tariff99038804 = Factory.New<USCTariff>();
				tariff99038804.UE_Tariff = "99038804";
				tariff99038804.UE_DutyComputationCode = "7";
				tariff99038804.UE_Column1RateAdValorem = 0.15m;
			}
			tariff99038804.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038804.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff8517620020 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8517620020")).LastOrDefault();
			if (tariff8517620020 == null)
			{
				tariff8517620020 = Factory.New<USCTariff>();
				tariff8517620020.UE_Tariff = "8517620020";
				tariff8517620020.UE_DutyComputationCode = "7";
				tariff8517620020.UE_Column1RateAdValorem = 0.05m;
			}
			tariff8517620020.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff8517620020.UE_DateTo = new ZDateTime(2021, 01, 01);

			Factory.Save();

			#endregion

			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = originalEntry.CH_PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_R_OrigSupTariff = tariff99038801.UE_Tariff;
			invoiceLine.US_SupTariff = tariff99038801.UE_Tariff;

			var invoiceLineTwo = invoice.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLine.PK;
			invoiceLineTwo.US_R_OrigTariff = tariff8517620020.UE_Tariff;
			invoiceLineTwo.JI_Tariff = tariff8517620020.UE_Tariff;
			invoiceLineTwo.US_R_OrigSupTariff = tariff99038804.UE_Tariff;
			invoiceLineTwo.US_SupTariff = tariff99038804.UE_Tariff;
			invoiceLineTwo.US_R_OrigCV = 10000m;
			invoiceLineTwo.JI_LinePrice = 11000m;
			Factory.Save();

			IEntryLineOrInvoiceLineDutyData supDutyData = new ReconSupDutyData(invoiceLine, ZDateTime.Today);
			CombineAssertions("Current duty data on first invoice line", () =>
			{
				AssertEquals("Customs value on duty data is zero.", ZDecimal.Zero, supDutyData.CustomsValue);
				AssertEquals("Tariff on duty data is 99038801.", tariff99038801.UE_Tariff, supDutyData.Tariff);
				Assert("Sup tariff on duty data is 99038801", supDutyData.SupTariffs.Contains(tariff99038801.UE_Tariff));
				AssertNull("Duty data shouldn't have combine parent line.", supDutyData.CombineParentLine);
				var childLines = supDutyData.ChildLines.ToArray();
				AssertEquals("Duty data should have three child lines.", 3, childLines.Length);
				var combineChildLines = supDutyData.CombineChildLines.ToArray();
				AssertEquals("Duty data should hava one combine child line.", 1, combineChildLines.Length);
				AssertEquals("Tariff on combine child line is 8517620020", tariff8517620020.UE_Tariff, combineChildLines[0].Tariff);
				Assert("Sup tariff on combine child line is 99038804", combineChildLines[0].SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineAllLines = supDutyData.CombineAllLines.ToArray();
				AssertEquals("Duty data should have four combine lines.", 4, combineAllLines.Length);
				AssertContainsExactElementsInAnyOrder(new[] { tariff99038801.UE_Tariff, tariff99038804.UE_Tariff, tariff8517620020.UE_Tariff }, combineAllLines.Where(x => !x.Tariff.IsEmpty).Select(y => y.Tariff));
			});

			supDutyData = new ReconSupDutyData(invoiceLineTwo, ZDateTime.Today);
			CombineAssertions("Current duty data on second invoice line", () =>
			{
				AssertEquals("Customs value on duty data is zero.", ZDecimal.Zero, supDutyData.CustomsValue);
				AssertEquals("Tariff on duty data is 99038804", tariff99038804.UE_Tariff, supDutyData.Tariff);
				Assert("Sup tariff on duty data is 99038804", supDutyData.SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineParentLine = supDutyData.CombineParentLine;
				AssertNotNull("Duty data should have combine parent line.", combineParentLine);
				AssertEquals("Tariff on combine parent line is empty", ZString.Empty, combineParentLine.Tariff);
				Assert("Sup tariff on combine parent line is 99038801", combineParentLine.SupTariffs.Contains(tariff99038801.UE_Tariff));
				var childLines = supDutyData.ChildLines.ToArray();
				AssertEquals("This is child sup line, duty data should have one child lines.", 1, childLines.Length);
				AssertEquals("Tariff on child line is 8517620020", tariff8517620020.UE_Tariff, childLines[0].Tariff);
				Assert("Sup tariff on child line is 99038804", childLines[0].SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineChildLines = supDutyData.CombineChildLines.ToArray();
				AssertEquals("This is child invoice line, duty data shouldn't have combine child lines.", 0, combineChildLines.Length);
				var combineAllLines = supDutyData.CombineAllLines.ToArray();
				AssertEquals("Duty data should have four combine lines.", 4, combineAllLines.Length);
				AssertContainsExactElementsInAnyOrder(new[] { tariff99038801.UE_Tariff, tariff99038804.UE_Tariff, tariff8517620020.UE_Tariff }, combineAllLines.Where(x => !x.Tariff.IsEmpty).Select(y => y.Tariff));
			});
		}

		ReconDeclaration reconDec;
		JobComInvoiceLine invoiceLine;
		ReconOriginalEntryHeader originalEntry;

		protected override void SetUp()
		{
			base.SetUp();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			reconDec = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDec.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Today;
			originalEntry = reconDec.OriginalEntries[0];
			originalEntry.US_R_CalcOrigDuty = true;

			invoiceLine = reconDec.InvoiceLines[0];
		}
	}
}
