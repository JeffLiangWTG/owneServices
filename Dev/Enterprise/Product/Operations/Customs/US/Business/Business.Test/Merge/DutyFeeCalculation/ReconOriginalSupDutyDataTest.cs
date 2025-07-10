using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconOriginalSupDutyDataTest : TestCaseWithFactory
	{
		public void TestCalculateException()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 63650m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "8457.20.0010";
			invoiceLine.US_R_OrigSupTariff = "9802.00.8068";
			invoiceLine.US_R_Orig98Value = 319m;
			invoiceLine.US_R_OrigCV = 1356m;
			invoiceLine.TariffCalculateExceptionMessage = "Test error1";

			var dutyData = new ReconOriginalSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
			AssertEquals("Test error1", ((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException);

			((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException = "Test error2";
			AssertEquals("Test error2", invoiceLine.TariffCalculateExceptionMessage);
		}

		public void TestSecondaryLines()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 63650m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "8457.20.0010";
			invoiceLine.US_R_OrigSupTariff = "9802.00.8068";
			invoiceLine.US_R_Orig98Value = 319m;
			invoiceLine.US_R_OrigCV = 1356m;

			var dutyData = new ReconOriginalSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
			Assert(!((IDutyData)dutyData).IsSecondaryTariffLine);
			AssertEquals(1, ((IFeeCalculationDataProvider)dutyData).SecondaryLines.Count());

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_R_OrigSupTariff = "9802.00.8068";
			invoiceLine2.US_R_OrigTariff = "9102.11.1010";
			AssertEquals("PreCondition:SecondaryTariffLines are added", 3, invoiceLine2.SecondaryTariffLines.Count());
			var dutyData2 = new ReconOriginalSupDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);

			Assert(!((IDutyData)dutyData2).IsSecondaryTariffLine);
			AssertEquals("include supData for the first child line", 7, ((IFeeCalculationDataProvider)dutyData2).SecondaryLines.Count());

			var firstChildLine = invoiceLine2.SecondaryTariffLines.ElementAt(0);
			var dutyData3 = new ReconOriginalSupDutyData(firstChildLine, originalEntry.US_R_DateForMPFCalc);
			Assert("It is a secondary line of the parent's sup line", ((IDutyData)dutyData3).IsSecondaryTariffLine);
			AssertEquals("no secondary line of own", 0, ((IFeeCalculationDataProvider)dutyData3).SecondaryLines.Count());
		}

		[TestDate(2017, 12, 1)]
		public void TestCalculateWatchAssemblyWithSup()
		{
			using (reconDec.ReconWrappedJobDeclaration.SuspendDefaultingSecondaryTariffLines())
			{
				originalEntry.US_R_CalcOrigDuty = true;
				invoiceLine.US_SupTariff = "9802.00.8068";
				invoiceLine.US_98GoodsValue = 1852m;
				invoiceLine.JI_Tariff = "9102.11.1010";
				invoiceLine.JI_LinePrice = 3406m;
				invoiceLine.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine);

				JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine2.US_SupTariff = "9802.00.8068";
				invoiceLine2.US_98GoodsValue = 1010m;
				invoiceLine2.JI_Tariff = "9102.11.1020";
				invoiceLine2.JI_LinePrice = 1609m;
				invoiceLine2.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine2);

				JobComInvoiceLine invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine3.US_SupTariff = "9802.00.8068";
				invoiceLine3.US_98GoodsValue = 0m;
				invoiceLine3.JI_Tariff = "9102.11.1030";
				invoiceLine3.JI_LinePrice = 1345m;
				invoiceLine3.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine3);

				JobComInvoiceLine invoiceLine4 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine4.US_SupTariff = "9802.00.8068";
				invoiceLine4.US_98GoodsValue = 204m;
				invoiceLine4.JI_Tariff = "9102.11.1040";
				invoiceLine4.JI_LinePrice = 0m;
				invoiceLine4.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine4);

				reconDec.CalculateDutyFeesForAllEntries();

				AssertEquals(25m, originalEntry.ReconMPF);
				AssertEquals(25m, originalEntry.OriginalMPF);

				AssertEquals(537.25m, originalEntry.ReconDuty);
				AssertEquals(537.25m, originalEntry.OriginalDuty);
			}
		}

		[TestDate(2009, 6, 1)]
		public void TestCalculateDutyAndFeeWithSup()
		{
			originalEntry.US_R_DutyRateDate = new ZDateTime(2007, 3, 19);
			originalEntry.US_R_CalcOrigDuty = true;
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra
			invoiceLine.US_SupQty1 = 1000m;
			invoiceLine.JI_Tariff = "2909.19.1800"; // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 10000m;
			// Duty should be $609.90 (.0599 * 1000 + (5.5% * 10000) = $550 + 59.90

			invoiceLine.US_R_OrigSupTariff = "9901.00.52"; // 5.99 cents/litre extra
			invoiceLine.US_R_OrigSupQty1 = 1500m;
			invoiceLine.US_R_OrigTariff = "2909.19.1800";   // 5.5%
			invoiceLine.US_R_OrigFirstQty = 1500m;
			invoiceLine.US_R_OrigCV = 15000m;

			// Duty should be $914.85 (.0599 * 1500 + (5.5% * 15000) = $825 + 89.85

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals(550m, invoiceLine.US_Duty);
			AssertEquals(59.90m, invoiceLine.US_SupDuty);
			AssertEquals("Total Duty payable", 609.90m, originalEntry.ReconDuty);
			AssertEquals(21m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(825m, invoiceLine.US_R_OrigDuty);
			AssertEquals(89.85m, invoiceLine.US_R_OrigSupDuty);
			AssertEquals("Total Duty payable", 914.85m, originalEntry.OriginalDuty);
			AssertEquals(31.5m, invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestQuantityImplementation()
		{
			IDutyData supDutyData = new ReconOriginalSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);

			invoiceLine.US_R_OrigSupUQ1 = "KG";
			invoiceLine.US_R_OrigSupUQ2 = "LT";
			invoiceLine.US_R_OrigSupUQ3 = "NO";
			invoiceLine.US_R_OrigSupQty1 = 2.523m;
			invoiceLine.US_R_OrigSupQty2 = 4.625m;
			invoiceLine.US_R_OrigSupQty3 = 8.867m;

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

			invoiceLine.US_R_OrigSupTariff = tariff.UE_Tariff;
			invoiceLine.US_R_OrigSupUQ1 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_R_OrigSupUQ2 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_R_OrigSupUQ3 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_R_OrigSupQty1 = 2.523m;
			invoiceLine.US_R_OrigSupQty2 = 4.625m;
			invoiceLine.US_R_OrigSupQty3 = 8.867m;

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
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 123.32m);
			originalEntry.US_R_ChangedLinesOnly = true;
			invoiceLine.US_SupTariff = "9904.12.18";    // 54.6%
			invoiceLine.US_SupQty1 = 1000m;
			invoiceLine.JI_Tariff = "2909.19.1800";     // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 10000m;

			invoiceLine.US_R_OrigSupTariff = "9904.12.18";
			invoiceLine.US_R_OrigSupQty1 = 1500m;
			invoiceLine.US_R_OrigTariff = "2909.19.1800";
			invoiceLine.US_R_OrigFirstQty = 1500m;
			invoiceLine.US_R_OrigCV = 15000m;

			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("Total Original Duty does not changed.", 123.32m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
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

			IEntryLineOrInvoiceLineDutyData originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine, ZDateTime.Today);
			CombineAssertions("Current duty data on first invoice line", () =>
			{
				AssertEquals("Customs value on duty data is zero.", ZDecimal.Zero, originalSupDutyData.CustomsValue);
				AssertEquals("Tariff on duty data is 99038801.", tariff99038801.UE_Tariff, originalSupDutyData.Tariff);
				Assert("Sup tariff on duty data is 99038801", originalSupDutyData.SupTariffs.Contains(tariff99038801.UE_Tariff));
				AssertNull("Duty data shouldn't have combine parent line.", originalSupDutyData.CombineParentLine);
				var childLines = originalSupDutyData.ChildLines.ToArray();
				AssertEquals("Duty data should have three child lines.", 3, childLines.Length);
				var combineChildLines = originalSupDutyData.CombineChildLines.ToArray();
				AssertEquals("Duty data should hava one combine child line.", 1, combineChildLines.Length);
				AssertEquals("Tariff on combine child line is 8517620020", tariff8517620020.UE_Tariff, combineChildLines[0].Tariff);
				Assert("Sup tariff on combine child line is 99038804", combineChildLines[0].SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineAllLines = originalSupDutyData.CombineAllLines.ToArray();
				AssertEquals("Duty data should have four combine lines.", 4, combineAllLines.Length);
				AssertContainsExactElementsInAnyOrder(new[] { tariff99038801.UE_Tariff, tariff99038804.UE_Tariff, tariff8517620020.UE_Tariff }, combineAllLines.Where(x => !x.Tariff.IsEmpty).Select(y => y.Tariff));
			});

			originalSupDutyData = new ReconOriginalSupDutyData(invoiceLineTwo, ZDateTime.Today);
			CombineAssertions("Current duty data on second invoice line", () =>
			{
				AssertEquals("Customs value on duty data is zero.", ZDecimal.Zero, originalSupDutyData.CustomsValue);
				AssertEquals("Tariff on duty data is 99038804", tariff99038804.UE_Tariff, originalSupDutyData.Tariff);
				Assert("Sup tariff on duty data is 99038804", originalSupDutyData.SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineParentLine = originalSupDutyData.CombineParentLine;
				AssertNotNull("Duty data should have combine parent line.", combineParentLine);
				AssertEquals("Tariff on combine parent line is empty", ZString.Empty, combineParentLine.Tariff);
				Assert("Sup tariff on combine parent line is 99038801", combineParentLine.SupTariffs.Contains(tariff99038801.UE_Tariff));
				var childLines = originalSupDutyData.ChildLines.ToArray();
				AssertEquals("This is child sup line, duty data should have one child lines.", 1, childLines.Length);
				AssertEquals("Tariff on child line is 8517620020", tariff8517620020.UE_Tariff, childLines[0].Tariff);
				Assert("Sup tariff on child line is 99038804", childLines[0].SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineChildLines = originalSupDutyData.CombineChildLines.ToArray();
				AssertEquals("This is child invoice line, duty data shouldn't have combine child lines.", 0, combineChildLines.Length);
				var combineAllLines = originalSupDutyData.CombineAllLines.ToArray();
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
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			reconDec = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			originalEntry = reconDec.OriginalEntries[0];
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;

			invoiceLine = reconDec.InvoiceLines[0];
		}

		void SetReconOriginalValues(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigCV = invoiceLine.JI_CustomsValue >= invoiceLine.US_98GoodsValue ? (ZDecimal)(invoiceLine.JI_CustomsValue - invoiceLine.US_98GoodsValue) : invoiceLine.JI_CustomsValue;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;
			invoiceLine.US_R_OrigSPI = invoiceLine.US_SPI;
			invoiceLine.US_R_OrigSecondQty = invoiceLine.JI_CustomsSecondQuantity;

			invoiceLine.US_R_OrigSupTariff = invoiceLine.US_SupTariff;
			invoiceLine.US_R_Orig98Value = invoiceLine.US_98GoodsValue;
			invoiceLine.US_R_OrigSupQty1 = invoiceLine.US_SupQty1;
			invoiceLine.US_R_OrigSupQty2 = invoiceLine.US_SupQty2;
			invoiceLine.US_R_OrigSupQty3 = invoiceLine.US_SupQty3;
		}
	}
}
