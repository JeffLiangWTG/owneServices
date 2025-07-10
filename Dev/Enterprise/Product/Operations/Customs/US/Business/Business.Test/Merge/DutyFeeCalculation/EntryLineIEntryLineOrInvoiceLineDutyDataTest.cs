using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryLineIEntryLineDutyDataTest : TestCaseWithFactory
	{
		[TestDate(2007, 3, 19)]
		public void TestCalculateMPFForSupTariff()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra
			invoiceLine.US_SupQty1 = 1000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_Tariff = "2909.19.1800"; // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			// Duty should be $609.90 (.0599 * 1000 + (5.5% * 10000) = $550 + 59.90

			CusEntryLine supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			CusEntryLine classificationLine = invoiceLine.CusEntryLine;

			AssertEquals(21m, supLine.MPFAmount);
			AssertEquals(10000m, supLine.CL_CustomsValue);
			AssertEquals(59.90m, supLine.DutyAmount);
			AssertEquals(0m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(550m, invoiceLine.CusEntryLine.DutyAmount);

			AssertEquals("Total Duty payable", 609.90m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
			AssertEquals(25m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);
		}

		[TestDate(2011, 09, 14)]
		public void TestRollUpDutyAndFeeWhenOverridden()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra
			invoiceLine.JI_Tariff = "2909.19.1800"; // 5.5%

			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_Duty = 1m;

			invoiceLine.US_OverrideSupDuty = true;
			invoiceLine.US_SupDuty = 2m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			invoiceLine.FeeCusCodes[0].CY_IsOverridden = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			CusEntryLine classificationLine = invoiceLine.CusEntryLine;

			AssertEquals(0m, supLine.MPFAmount);
			AssertEquals(21m, classificationLine.MPFAmount);

			AssertEquals(2m, supLine.DutyAmount);
			AssertEquals(1m, classificationLine.DutyAmount);

			AssertEquals(25m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
		}

		[TestDate(2007, 3, 19)]
		public void TestCalculateMPFForTariffWhenMerged()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_Tariff = "2909.19.1800"; // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_Weight = 100m;
			invoiceLine2.JI_Tariff = "2909.19.1800";    // 5.5%
			invoiceLine2.JI_CustomsQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(invoiceLine.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(21m, invoiceLine.CusEntryLine.MPFAmount);
			AssertEquals(21m, invoiceLine2.CusEntryLine.MPFAmount);
			AssertEquals(42m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);
		}

		public void TestGetOrSetDutyFeeCharge()
		{
			dutyData.SetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 10m, new FeeCalculationInternalData());

			AssertEquals(10m, dutyData.GetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));

			dutyData.SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey, 25m, new FeeCalculationInternalData());

			AssertEquals(25m, dutyData.GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey));

			DutyResult dutyResult = new DutyResult();
			dutyResult.TotalAmount = new Money(35m, JobDeclaration.GetLocalCurrency());
			dutyResult.PercentOfValue = 20m;

			dutyData.SetDutyResult(dutyResult);

			AssertEquals(35m, dutyData.GetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
			AssertEquals(20m, entryLine.CL_DutyPercent);
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

			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.CusEntryLine.CL_AdValoremTariff = "0000000000";
			AssertNotNull(dutyData.ImportTariff);

			AssertEquals("IsDutyFreeSPIClaimed", false, dutyData.IsDutyFreeSPIClaimed);

			invoiceLine.US_SPI = "AU";
			AssertEquals("IsDutyFreeSPIClaimed", true, dutyData.IsDutyFreeSPIClaimed);

			invoiceLine.US_SPI = "MX";
			AssertEquals("IsDutyFreeSPIClaimed", false, dutyData.IsDutyFreeSPIClaimed);
		}

		public void TestIsDutyFreeSPIClaimed2()
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

			USCTariff supTariff = Factory.New<USCTariff>();
			supTariff.UE_Tariff = "9999999999";
			supTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			supTariff.UE_DateTo = ZDateTime.Today;
			supTariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			supTariff.UE_Column1RateAdValorem = 0.0321m;
			supTariff.UE_SPICode = "AUMX";

			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.US_SupTariff = "9999999999";
			invoiceLine.CusEntryLine.CL_AdValoremTariff = "0000000000";
			AssertNotNull(dutyData.ImportTariff);

			AssertEquals("IsDutyFreeSPIClaimed", false, dutyData.IsDutyFreeSPIClaimed);

			invoiceLine.US_SPI = "AU";
			AssertEquals("IsDutyFreeSPIClaimed", true, dutyData.IsDutyFreeSPIClaimed);

			invoiceLine.US_SPI = "MX";
			AssertEquals("IsDutyFreeSPIClaimed", false, dutyData.IsDutyFreeSPIClaimed);
		}

		[TestDate(2009, 6, 1)]
		public void TestRoundedAmountShouldBeAdded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			AddInvoiceLine(declaration, 794m);
			AddInvoiceLine(declaration, 885m);
			AddInvoiceLine(declaration, 521m);
			AddInvoiceLine(declaration, 1858m);
			AddInvoiceLine(declaration, 214m);
			AddInvoiceLine(declaration, 3553m);
			AddInvoiceLine(declaration, 3m);
			AddInvoiceLine(declaration, 953m);
			AddInvoiceLine(declaration, 212m);
			AddInvoiceLine(declaration, 233m);
			AddInvoiceLine(declaration, 184m);
			AddInvoiceLine(declaration, 254m);
			AddInvoiceLine(declaration, 93m);
			AddInvoiceLine(declaration, 46m);
			AddInvoiceLine(declaration, 3m);
			AddInvoiceLine(declaration, 1143m);
			AddInvoiceLine(declaration, 92m);
			AddInvoiceLine(declaration, 247m);
			AddInvoiceLine(declaration, 224m);
			AddInvoiceLine(declaration, 166m);
			AddInvoiceLine(declaration, 111m);
			AddInvoiceLine(declaration, 261m);
			AddInvoiceLine(declaration, 123m);
			AddInvoiceLine(declaration, 78m);
			AddInvoiceLine(declaration, 18m);
			AddInvoiceLine(declaration, 163m);
			AddInvoiceLine(declaration, 232m);
			AddInvoiceLine(declaration, 103m);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Total MPF", 26.83m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);
		}

		JobComInvoiceLine AddInvoiceLine(JobDeclaration declaration, ZDecimal value)
		{
			JobComInvoiceLine result = declaration.InvoiceLines.AddNew();
			result.JI_LinePrice = value;
			return result;
		}

		public void TestIsFeeoverriden()
		{
			AssertEquals(false, ((IFeeCalculationDataProvider)entryLine).IsFeeOverriden(Core.Constants.USCustoms.FeeCodes.Mushroom));

			FeeCusCodeData feeData = invoiceLine.FeeCusCodes.AddNew();
			feeData.CY_Code = Core.Constants.USCustoms.FeeCodes.Mushroom;
			feeData.CY_IsOverridden = true;
			AssertEquals(true, ((IFeeCalculationDataProvider)entryLine).IsFeeOverriden(Core.Constants.USCustoms.FeeCodes.Mushroom));

			AssertEquals(ZString.Empty, ((IFeeCalculationDataProvider)entryLine).GetSelectedRateType(Core.Constants.USCustoms.FeeCodes.Mushroom));

			feeData.CY_SelectedRateType = "P";
			AssertEquals("P", ((IFeeCalculationDataProvider)entryLine).GetSelectedRateType(Core.Constants.USCustoms.FeeCodes.Mushroom));
		}

		[TestDate(2008, 3, 25)]
		public void TestADD_CVDDetails()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;

				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency);
				var rate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDateTime.Today, currency.RX_Code, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK) ?? invoiceLine.InvoiceHeader.Invoice_Currency.ExchangeRates.AddNew();
				rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				rate.RE_StartDate = ZDateTime.Today;
				rate.RE_ExpiryDate = ZDateTime.Today;
				rate.RE_SellRate = 0.78m;

				invoiceLine.CusEntryLine.ResetTotalsAndCachedValues();
				invoiceLine.US_ADDCaseNo = "A";
				invoiceLine.US_ADDDepositValue = 5743.45m;//NZD
				AssertEquals("ADD Deposit value in local currency", 4480m, dutyData.ValueForADD);

				invoiceLine.US_CVDCaseNo = "D";
				invoiceLine.US_CVDDepositValue = 7789.90m;//NZD
				AssertEquals("CVD Deposit value in local currency", 6076m, dutyData.ValueForCVD);
			}
		}

		public void TestCalculateException()
		{
			invoiceLine.TariffCalculateExceptionMessage = "Test error1";
			AssertEquals("Test error1", ((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException);

			((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException = "Test error2";
			AssertEquals("Test error2", invoiceLine.TariffCalculateExceptionMessage);
		}

		public void TestSecondaryLines()
		{
			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			JobComInvoiceLine invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2, new List<IEntryLineOrInvoiceLineDutyData>(dutyData.SecondaryLines).Count);
		}

		public void TestParentLine()
		{
			AssertNull(dutyData.ParentTariffLine);

			CusEntryLine secondaryLine = entryLine.Header.MergedLines.AddNew();
			secondaryLine.US_CL_ParentLine = entryLine.PK;
			EntryLineIEntryLineOrInvoiceLineDutyData dutyData2 = new EntryLineIEntryLineOrInvoiceLineDutyData(secondaryLine);
			AssertNotNull(dutyData2.ParentTariffLine);
		}

		public void TestStoreAdjustedDerivedCustomsValue()
		{
			entryLine.CL_CustomsValue = 10000m;
			AssertEquals("DerivedCV", 10000m, entryLine.CL_CustomsValue);

			dutyData.StoreAdjustedDerivedCustomsValue(50m);
			AssertEquals("DerivedCV: Display purpose", 50m, entryLine.CL_CustomsValue);
		}

		public void TestRollUpFeesWhenNothingIsOverridden()
		{
			entryLine.Fees.RemoveAndDeleteAll();
			entryLine.Header.Charges.RemoveAndDeleteAll();

			//nothing is overridden
			invoiceLine.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20m);
			invoiceLine.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Mango, 40m);

			IDutyDataLineHeader header = entryLine.Header;
			dutyData.RollUpFees(header);

			AssertEquals("MPF is rolled up", 0m, header.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("Mango is rolled up", 0m, header.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Mango));
		}

		public void TestRollUpFeesWhenFeesAreOverridden()
		{
			entryLine.Fees.RemoveAndDeleteAll();
			entryLine.Header.Charges.RemoveAndDeleteAll();

			FeeCusCodeData mpf = invoiceLine.FeeCusCodes.AddNew();
			mpf.CY_IsOverridden = true;
			mpf.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			mpf.CY_FeeAmount = 10m;

			IDutyDataLineHeader header = entryLine.Header;
			dutyData.RollUpFees(header);

			AssertEquals("MPF is rolled up", 10m, header.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals("MPF is rolled up", 10m, entryLine.Fees.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestRollUpFeesWhenProvDutyIsOverridden_ValueShouldNotCopiedToAdditionalProvDuty()
		{
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "9403.20.0082";
			invoiceLine.SupTariffFormatted = "9903.81.91";
			invoiceLine.US_OverrideSupDuty = true;
			invoiceLine.US_SupDuty = 2500m;

			invoiceLine.SupFormattedAdditionalTariff1 = "9817.00.5000";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.88.03";
			invoiceLine.SupFormattedAdditionalTariff3 = "9903.01.24";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(0m, invoiceLine.US_SupAdditionalTariff1Duty);
			AssertEquals(0m, invoiceLine.US_SupAdditionalTariff2Duty);
			AssertEquals(0m, invoiceLine.US_SupAdditionalTariff3Duty);
		}

		public void TestIDutyData()
		{
			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
			AssertEquals("SelectedRateType", RateTypeList.Codes.Primary, dutyData.SelectedRateType);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertEquals("SpecialProgramsIndicatorSecondary", SecondarySpecProgIndicatorList.Codes.F, dutyData.SpecialProgramsIndicatorSecondary);

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.E;
			AssertEquals("SpecialProgramsIndicatorPrimary", PrimarySpecProgramIndicatorList.Codes.E, dutyData.SpecialProgramsIndicatorPrimary);
			AssertEquals("SpecialProgramsIndicatorCountry", ZString.Empty, dutyData.SpecialProgramsIndicatorCountry);

			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertEquals("SpecialProgramsIndicatorCountry", SpecialProgramList.Codes.AU, dutyData.SpecialProgramsIndicatorCountry);
			AssertEquals("SpecialProgramsIndicatorPrimary", ZString.Empty, dutyData.SpecialProgramsIndicatorPrimary);

			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			AssertEquals("CountryOfOrigin", "TT", dutyData.CountryOfOrigin);

			entryLine.CL_CustomsValue = 10000m;
			AssertEquals("CustomsValue", 10000m, dutyData.CustomsValue);

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

			entryLine.CL_AdValoremTariff = USCTariff.AGOABenefitsApplicable;
			AssertEquals("Tariff", USCTariff.AGOABenefitsApplicable, dutyData.Tariff);
		}

		[TestDate(2009, 6, 1)]
		public void TestShouldRoundAfterFeeIsRolledUp()
		{
			declaration.US_EnableCRL = false;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoiceLine.JI_LinePrice = 13077.00m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine vLine1 = invoice.JobComInvoiceLines.AddNew();
			vLine1.JI_LinePrice = 5158.00m;
			vLine1.JI_ParentID = invoiceLine.PK;

			JobComInvoiceLine vLine2 = invoice.JobComInvoiceLines.AddNew();
			vLine2.JI_LinePrice = 535.00m;
			vLine2.JI_ParentID = invoiceLine.PK;

			JobComInvoiceLine vLine3 = invoice.JobComInvoiceLines.AddNew();
			vLine3.JI_LinePrice = 5049.00m;
			vLine3.JI_ParentID = invoiceLine.PK;

			JobComInvoiceLine vLine4 = invoice.JobComInvoiceLines.AddNew();
			vLine4.JI_LinePrice = 1801.00m;
			vLine4.JI_ParentID = invoiceLine.PK;

			JobComInvoiceLine vLine5 = invoice.JobComInvoiceLines.AddNew();
			vLine5.JI_LinePrice = 535.00m;
			vLine5.JI_ParentID = invoiceLine.PK;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 683.00m;

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 453.50m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Total MPF", 29.84m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);
			AssertEquals("Total HMF", 17.77m, declaration.CustomsEntryHeaders[0].HMFAmountForEntry);
		}

		public void TestEntryLineNotDeletedForDerivedSetsInWrongOrder()
		{
			#region Setup Tariffs

			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, ZString.Empty);
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			var tariff8206000000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8206000000", "9", 1m, "PCS");
			var tariff4823690040 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8203204000", "7", 0.12m, "DOZ");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_FormattedTariff = "8206.00.0000";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_FormattedTariff = "8203.20.4000";
			invoiceLine2.SupTariffFormatted = "9903.01.20";
			invoiceLine2.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine2.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.FormalEntry;
			AssertEquals(4, entry.MergedLines.Count);
			var parentEntryLine = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "8206000000").FirstOrDefault();
			AssertEquals(3, parentEntryLine.ChildLines.Count);
		}

		public void TestDepositRateForParentWhichHasADD_CVDSecondaryLines()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570204006";
			addCase.U5_ISOCountryCode = "IT";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "8211930030";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 0.19m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C570926001";
			cvdCase.U5_ISOCountryCode = "IT";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "8211930030";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.01m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			invoice.JZ_InvoiceAmount = 24000m;

			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8205.20.3000";   // .4c per unit + 6.1% = 24000 * .061 = $1464 (this is sent)
			line2.JI_Description = "line with the highest duty rate";
			line2.JI_CustomsQuantity = 0m;
			line2.JI_LinePrice = 16000m;
			line2.US_ADDCaseNo = "A570204006";
			line2.US_ADDDepositValue = 16000m;

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (24000 * .054) = $1296
			line3.JI_Description = "line with the lower duty rate";
			line3.JI_CustomsQuantity = 0m;//due to this
			line3.JI_LinePrice = 8000m;
			line3.US_CVDCaseNo = "C570926001";
			line3.US_CVDDepositValue = 8000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			EntryLineIEntryLineOrInvoiceLineDutyData parentDutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(invoiceLine.CusEntryLine);
			AssertEquals("DEPOSIT rate for parent should not return a secondary line's one", 0m, parentDutyData.ADDDepositRate);
			AssertEquals("DEPOSIT rate for parent should not return a secondary line's one", 0m, parentDutyData.CVDDepositRate);

			AssertEquals(line2.CusEntryLine, line3.CusEntryLine);

			EntryLineIEntryLineOrInvoiceLineDutyData secondaryDutyData2 = new EntryLineIEntryLineOrInvoiceLineDutyData(line2.CusEntryLine);
			AssertEquals("DEPOSIT rate for a secondary line's one", 0.19m, secondaryDutyData2.ADDDepositRate);
			AssertEquals("DEPOSIT rate for a secondary line's one", 0m, secondaryDutyData2.CVDDepositRate);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		CusEntryLine entryLine;
		JobComInvoiceLine invoiceLine;
		EntryLineIEntryLineOrInvoiceLineDutyData dutyData;

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entryLine = invoiceLine.CusEntryLine;

			dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine);
		}
	}
}
