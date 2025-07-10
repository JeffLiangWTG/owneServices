using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceLineIEntryLineOrInvoiceLineDutyDataTest : TestCaseWithFactory
	{
		public void TestGetOrSetDutyFeeCharge()
		{
			dutyData.SetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 10m, new FeeCalculationInternalData());

			AssertEquals(10m, dutyData.GetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));

			dutyData.SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey, 25m, new FeeCalculationInternalData());

			AssertEquals(25m, dutyData.GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey));

			DutyResult dutyResult = new DutyResult();
			dutyResult.TotalAmount = new Money(35m, JobDeclaration.GetLocalCurrency());
			dutyData.SetDutyResult(dutyResult);

			AssertEquals(35m, dutyData.GetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));

			dutyData.SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 3.22498400m, new FeeCalculationInternalData());
			AssertEquals("round it with 2 decimals directly", 3.22m, dutyData.GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestIsDutyFreeSPIClaimed()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.0543m;
			tariff.UE_SPICode = "AUMX";

			USCTariffDutyRate dutyRateForMX = tariff.DutyRates.AddNew();
			dutyRateForMX.UD_ISOCountryCode = "MX";
			dutyRateForMX.UD_TaxFeeComputationCode = ComputationCodeList.Codes.AdValorem;
			dutyRateForMX.UD_AdValoremSpecialRate = 0.0243m;

			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			invoiceLine.JI_Tariff = "0000000000";
			AssertNotNull(invoiceLine.ImportTariff);

			AssertEquals("IsDutyFreeSPIClaimed", false, dutyData.IsDutyFreeSPIClaimed);

			invoiceLine.US_SPI = "AU";
			AssertEquals("IsDutyFreeSPIClaimed", true, dutyData.IsDutyFreeSPIClaimed);

			invoiceLine.US_SPI = "MX";
			AssertEquals("IsDutyFreeSPIClaimed", false, dutyData.IsDutyFreeSPIClaimed);
		}

		public void TestSecondaryLines()
		{
			originalEntry.US_R_DutyRateDate = new ZDateTime(2013, 1, 1);
			invoiceLine.JI_Tariff = "8457.20.0010";
			invoiceLine.US_SupTariff = "9802.00.8068";
			invoiceLine.US_98GoodsValue = 319m;
			invoiceLine.JI_LinePrice = 1356m;

			Assert("IsSecondaryLine of its own sup line", dutyData.IsSecondaryTariffLine);
			AssertEquals(0, ((IFeeCalculationDataProvider)dutyData).SecondaryLines.Count());

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9102.11.1010";
			AssertEquals("PreCondition:SecondaryTariffLines are added", 3, invoiceLine2.SecondaryTariffLines.Count());
			var dutyData2 = new ReconCurrentDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);

			Assert(!dutyData2.IsSecondaryTariffLine);
			var firstChildLine = invoiceLine2.SecondaryTariffLines.ElementAt(0);
			firstChildLine.US_SupTariff = "9802.00.8068";
			AssertEquals("include supData for the first child line", 4, ((IFeeCalculationDataProvider)dutyData2).SecondaryLines.Count());

			var dutyData3 = new ReconCurrentDutyData(firstChildLine, originalEntry.US_R_DateForMPFCalc);
			AssertEquals("No secondary line", 0, ((IFeeCalculationDataProvider)dutyData3).SecondaryLines.Count());
		}

		[TestDate(2008, 3, 25)]
		public void TestADD_CVDDetails()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
				var rate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDateTime.Today, invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK) ?? invoiceLine.InvoiceHeader.Invoice_Currency.ExchangeRates.AddNew();
				rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				rate.RE_StartDate = ZDateTime.Today;
				rate.RE_ExpiryDate = ZDateTime.Today;
				rate.RE_SellRate = 0.78m;

				invoiceLine.US_ADDDepositValue = 5743.45m;//NZD
				AssertEquals("ADD Deposit value in local currency", 0m, dutyData.ValueForADD);

				invoiceLine.US_CVDDepositValue = 7789.90m;//NZD
				AssertEquals("CVD Deposit value in local currency", 0m, dutyData.ValueForCVD);
			}
		}

		public void TestCalculateException()
		{
			invoiceLine.TariffCalculateExceptionMessage = "Test error1";
			AssertEquals("Test error1", ((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException);

			((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException = "Test error2";
			AssertEquals("Test error2", invoiceLine.TariffCalculateExceptionMessage);
		}

		public void TestSecondaryLines2()
		{
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();

			AssertEquals(2, new List<IEntryLineOrInvoiceLineDutyData>(dutyData.SecondaryLines).Count);
		}

		public void TestParentLine()
		{
			AssertNull(dutyData.ParentTariffLine);

			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			ReconCurrentDutyData dutyData2 = new ReconCurrentDutyData(secondaryLine, originalEntry.US_R_DateForMPFCalc);
			AssertNotNull(dutyData2.ParentTariffLine);
		}

		public void TestStoreAdjustedDerivedCustomsValue()
		{
			invoiceLine.JI_LinePrice = 10000m;
			AssertEquals("DerivedCV", 0m, invoiceLine.US_DerivedCV);

			dutyData.StoreAdjustedDerivedCustomsValue(50m);
			AssertEquals("DerivedCV: Display purpose", 50m, invoiceLine.US_DerivedCV);
		}

		public void TestRollUpFees()
		{
			IDutyDataLineHeader reconEntryHeader = new ReconCurrentDutyDataLineHeader(originalEntry);
			IDutyDataLineHeader originalEntryHeader = new ReconOriginalDutyDataLineHeader(originalEntry);

			invoiceLine.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20.2356m);
			invoiceLine.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Mango, 40.2508m);

			dutyData.RollUpFees(reconEntryHeader);

			AssertEquals("MPF is rolled up", 20.24m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("Mango is rolled up", 40.25m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango));

			JobComInvoiceLine invoiceLine2 = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 30m);
			invoiceLine2.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Mango, 60m);
			invoiceLine2.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 70m);
			ReconCurrentDutyData dutyData2 = new ReconCurrentDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);

			dutyData2.RollUpFees(reconEntryHeader);

			AssertEquals("MPF is rolled up", 50.24m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("Mango fee is rolled up", 100.25m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango));
			AssertEquals("ADD is rolled up", 70m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
		}

		public void TestRollUpFeesChangedLinesOnly()
		{
			originalEntry.US_R_ChangedLinesOnly = true;
			originalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 17.33m);
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 17m);
			var reconLineFee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			reconLineFee.CY_FeeAmount = 5.3m;
			dutyData.RollUpFees(new ReconCurrentDutyDataLineHeader(originalEntry));
			AssertEquals("Recon MPF", 5.63m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestRollUpFeesChangedLinesOnlyWithoutHMF()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDeclaration.OriginalEntries.AddNew();
			reconEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 6.35m);
			reconEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 35.2m);

			reconEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			reconEntry.US_R_IsHMFApplicable = "N";
			reconEntry.US_R_ChangedLinesOnly = false;

			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("Recon should not have MPC yet", 2, reconEntry.OriginalCharges.Count);

			reconEntry.US_R_ChangedLinesOnly = true;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("Recon should have MPC", 3, reconEntry.OriginalCharges.Count);

			var ordered = reconEntry.OriginalCharges.OfType<ReconEntryOriginalCharge>().OrderBy(x => x.CY_Code).ToArray();
			AssertEquals("499", Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, ordered[0].CY_Code);
			AssertEquals("DTY", Core.Constants.USCustoms.FeeCodes.Duty, ordered[1].CY_Code);
			AssertEquals("MPC", Core.Constants.USCustoms.FeeCodes.MPC, ordered[2].CY_Code);
		}

		public void TestRollUpFeesChangedLinesOnlyWhenReconChargeIsEmpty()
		{
			originalEntry.US_R_ChangedLinesOnly = true;
			originalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 17.33m);
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 17m);
			dutyData.RollUpFees(new ReconCurrentDutyDataLineHeader(originalEntry));
			AssertEquals("Recon MPF", 0.33m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestRollUpFeesForMonthlyFiling()
		{
			originalEntry.US_R_MonthlyFiling = true;
			originalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 50m);

			invoiceLine.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20.2356m);

			dutyData.RollUpFees(new ReconCurrentDutyDataLineHeader(originalEntry));
			AssertEquals("MPF is not rolled up", 50m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestIDutyData()
		{
			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
			AssertEquals("SelectedRateType", RateTypeList.Codes.Primary, dutyData.SelectedRateType);

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			AssertEquals("SpecialProgramsIndicatorPrimary", PrimarySpecProgramIndicatorList.Codes.B, dutyData.SpecialProgramsIndicatorPrimary);
			AssertEquals("SpecialProgramsIndicatorCountry", ZString.Empty, dutyData.SpecialProgramsIndicatorCountry);

			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertEquals("SpecialProgramsIndicatorCountry", SpecialProgramList.Codes.AU, dutyData.SpecialProgramsIndicatorCountry);
			AssertEquals("SpecialProgramsIndicatorPrimary", ZString.Empty, dutyData.SpecialProgramsIndicatorPrimary);

			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			AssertEquals("CountryOfOrigin", "TT", dutyData.CountryOfOrigin);

			invoiceLine.JI_LinePrice = 10000.51m;
			AssertEquals("CustomsValue", 10001m, ((IDutyData)dutyData).CustomsValue);

			invoiceLine.JI_LinePrice = 0.01m;
			AssertEquals("CustomsValue", 0m, ((IDutyData)dutyData).CustomsValue);

			invoiceLine.JI_CustomsQuantity = 2m;
			AssertEquals("Quantity1", 2m, dutyData.Quantity1);

			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("UQ1", "KG", dutyData.UQ1);

			invoiceLine.JI_CustomsSecondQuantity = 4m;
			AssertEquals("Quantity2", 4m, dutyData.Quantity2);

			invoiceLine.JI_CustomsSecondUnitQty = "LT";
			AssertEquals("UQ2", "LT", dutyData.UQ2);

			invoiceLine.JI_CustomsThirdQuantity = 8m;
			AssertEquals("Quantity3", 8m, dutyData.Quantity3);

			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			AssertEquals("UQ3", "NO", dutyData.UQ3);

			invoiceLine.JI_Tariff = new TariffFormatter().DisplayFormat(USCTariff.AGOABenefitsApplicable);
			AssertEquals("Tariff", USCTariff.AGOABenefitsApplicable, dutyData.Tariff);

			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals(false, dutyData.IsAMSFeeExempt);
			AssertEquals(true, dutyData.IsCottonFeeExemptIndicated);

			invoiceLine.US_CottonCertificateNo = "ORGANICXX";
			AssertEquals(true, dutyData.IsAMSFeeExempt);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals(true, dutyData.IsSetVLine);
			AssertEquals("SpecialProgramsIndicatorSecondary", SecondarySpecProgIndicatorList.Codes.V, dutyData.SpecialProgramsIndicatorSecondary);

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals(true, invoiceLine2.IsSetVLine);

			ReconCurrentDutyData invoiceLine2DutyData = new ReconCurrentDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
			AssertEquals("IsSecondaryTariffLine", false, invoiceLine2DutyData.IsSecondaryTariffLine);

			invoiceLine.US_SecondarySPI = ZString.Empty;
			invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals(false, invoiceLine2.IsSetVLine);

			invoiceLine2DutyData = new ReconCurrentDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
			AssertEquals("IsSecondaryTariffLine", true, invoiceLine2DutyData.IsSecondaryTariffLine);
		}

		[TestDate(2008, 3, 25)]
		public void TestRollUpExciseFees()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Excise Tax", 2300.84m, declaration.CustomsEntryHeaders[0].TotalEstimatedTax);
			Factory.Save();

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Today;

			AssertEquals("1 original entry created", 1, reconDeclaration.OriginalEntries.Count);
			reconDeclaration.OriginalEntries[0].OriginalCharges[0].CY_Amount = 10m;

			reconDeclaration.OriginalEntries[0].ResetReconChargesIfNecessary();
			new DutyFeeCalculationManager(reconDeclaration).Calculate();
			AssertEquals("Correctly rolled up as 'Excise Payable' in 'ReconCharges'", 2300.84m, reconDeclaration.OriginalEntries[0].ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
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

			IEntryLineOrInvoiceLineDutyData currentDutyData = new ReconCurrentDutyData(invoiceLine, ZDateTime.Today);
			CombineAssertions("Current duty data on first invoice line", () =>
			{
				AssertEquals("Customs value on duty data is zero.", ZDecimal.Zero, currentDutyData.CustomsValue);
				AssertEquals("Tariff on duty data is empty.", ZString.Empty, currentDutyData.Tariff);
				Assert("Sup tariff on duty data is 99038801", currentDutyData.SupTariffs.Contains(tariff99038801.UE_Tariff));
				AssertNull("Duty data shouldn't have combine parent line.", currentDutyData.CombineParentLine);
				var childLines = currentDutyData.ChildLines.ToArray();
				AssertEquals("As sup tariff is not empty, duty data shouldn't have child lines.", 0, childLines.Length);
				var combineChildLines = currentDutyData.CombineChildLines.ToArray();
				AssertEquals("Duty data should hava one combine child line.", 1, combineChildLines.Length);
				AssertEquals("Tariff on combine child line is 8517620020", tariff8517620020.UE_Tariff, combineChildLines[0].Tariff);
				Assert("Sup tariff on combine child line is 99038804", combineChildLines[0].SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineAllLines = currentDutyData.CombineAllLines.ToArray();
				AssertEquals("Duty data should have four combine lines.", 4, combineAllLines.Length);
				AssertContainsExactElementsInAnyOrder(new[] { tariff99038801.UE_Tariff, tariff99038804.UE_Tariff, tariff8517620020.UE_Tariff }, combineAllLines.Where(x => !x.Tariff.IsEmpty).Select(y => y.Tariff));
			});

			currentDutyData = new ReconCurrentDutyData(invoiceLineTwo, ZDateTime.Today);
			CombineAssertions("Current duty data on second invoice line", () =>
			{
				AssertEquals("Customs value on duty data is 11000.", 11000m, currentDutyData.CustomsValue);
				AssertEquals("Tariff on duty data is 8517620020", tariff8517620020.UE_Tariff, currentDutyData.Tariff);
				Assert("Sup tariff on duty data is 99038804", currentDutyData.SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineParentLine = currentDutyData.CombineParentLine;
				AssertNotNull("Duty data should have combine parent line.", combineParentLine);
				AssertEquals("Tariff on combine parent line is empty", ZString.Empty, combineParentLine.Tariff);
				Assert("Sup tariff on combine parent line is 99038801", combineParentLine.SupTariffs.Contains(tariff99038801.UE_Tariff));
				var childLines = currentDutyData.ChildLines.ToArray();
				AssertEquals("This is child invoice line, duty data shouldn't have child lines.", 0, childLines.Length);
				var combineChildLines = currentDutyData.CombineChildLines.ToArray();
				AssertEquals("This is child invoice line, duty data shouldn't have combine child lines.", 0, combineChildLines.Length);
				var combineAllLines = currentDutyData.CombineAllLines.ToArray();
				AssertEquals("Duty data should have four combine lines.", 4, combineAllLines.Length);
				AssertContainsExactElementsInAnyOrder(new[] { tariff99038801.UE_Tariff, tariff99038804.UE_Tariff, tariff8517620020.UE_Tariff }, combineAllLines.Where(x => !x.Tariff.IsEmpty).Select(y => y.Tariff));
			});
		}

		ReconOriginalEntryHeader originalEntry;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		ReconCurrentDutyData dutyData;

		protected override void SetUp()
		{
			base.SetUp();

			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			ReconDeclaration declaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = originalEntry.CH_PK;

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			dutyData = new ReconCurrentDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
		}
	}
}
