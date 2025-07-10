using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2022, 10, 30)]
		public void TestIsFeeApplicableWhenTariffHasAttributeBBF()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);

			var uscTariff2 = Factory.New<USCTariff>();
			uscTariff2.UE_Tariff = "99031919";
			uscTariff2.UE_DateFrom = startDate;
			uscTariff2.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff2 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, uscTariff2.UE_Tariff, startDate, endDate);

			var a99 = helper.CreateNewOrGetExistingTariffAttribute(TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff2);
			var bbf = helper.CreateNewOrGetExistingTariffAttribute(TariffAttributeTypes.Codes.TYPE, TariffAttributeTypes.Values.BabyFomula, tariff2);

			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_Tariff = "1901.10.1600";
			invoiceLine1.US_SupTariff = "9903.19.19";
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Switzerland;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Switzerland;
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(0, invoiceLine1.FeeCusCodes.Count);
		}

		public void TestCalculateCoffeeFee()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_ShortDescription = "Coffee Fee Test";
			tariff.UE_Unit1 = "KG";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "TEST NAME", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			Factory.Save();

			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = "SEA";

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_CertifyCargoRelease = true;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 9901.25m;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			invoiceLine1.JI_CustomsQuantity = 840m;
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_Weight = 3285.59m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_CustomsSecondQuantity = 3285.59m;
			Declaration.US_SchDEntry = "1234";
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine = entry.MergedLines[0];
			var coffeeFee = entryLine.Fees.Cast<IFee>().FirstOrDefault(x => x.Code == Core.Constants.USCustoms.FeeCodes.Coffee);
			AssertNotNull(coffeeFee);
			AssertEquals(420m, coffeeFee.Amount);

			Declaration.US_SchDEntry = "5678";
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5678", "TEST NAME", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeNameState.ZXE_Name, USStateList.Codes.Alabama);
			Factory.Save();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryLine = entry.MergedLines[0];
			coffeeFee = entryLine.Fees.Cast<IFee>().FirstOrDefault(x => x.Code == Core.Constants.USCustoms.FeeCodes.Coffee);
			AssertNull(coffeeFee);

			Declaration.US_SchDEntry = "1234";
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Beef;
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryLine = entry.MergedLines[0];
			coffeeFee = entryLine.Fees.Cast<IFee>().FirstOrDefault(x => x.Code == Core.Constants.USCustoms.FeeCodes.Coffee);
			AssertNull(coffeeFee);
		}

		public void TestCalculateHMF()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			FeeCusCodeData fee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertNull(fee);

			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			fee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertNotNull(fee);
			AssertEquals(12.5m, fee.CY_FeeAmount);
		}

		public void TestCalculateFTZ()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.FTZEntry;
			AssertEquals("No payable fees & charges", 0m, entry.TotalAmountPayable);

			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(entry.HMFAmountForEntry > 0m);
			AssertEquals("Only HMF is payable", entry.HMFAmountForEntry, entry.TotalAmountPayable);
		}

		public void TestCalculateHMFIsStillDoneWhenTIBEntry()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			FeeCusCodeData fee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertNull(fee);

			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			fee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertNotNull(fee);
			AssertEquals(12.5m, fee.CY_FeeAmount);
		}

		public void TestCalculateWhenTaxApplyChangesFromYesToOverride()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2403.10.2050";
			invoiceLine.JI_CustomsQuantity = 2574m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertEquals("PreCondition", invoiceLine.ImportTariff.GetTaxFeeRateDescription(invoiceLine.US_TaxCode, ZString.Empty), invoiceLine.US_TaxRateS);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalIRTTaxes);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalIRTTaxes);
		}

		public void TestCalculateFeeForDomesticMerchandise()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			FeeCusCodeData fee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertNotNull(fee);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			fee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF);
			Assert(fee == null || fee.CY_FeeAmount.IsEmpty);
		}

		public void TestCalculateForPrinting()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull("MPF exempt for TIB entries", invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var print = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			//Entry print should not accidentally have added this
			AssertNull("MPF exempt for TIB entries", invoiceLine1.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestCalculateWhenRateTypeIsSpecified()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = "056";
			dutyRate.UD_TaxFeeComputationCode = "C";
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;//primary
			dutyRate.UD_TaxFeeAdvalorem = 0.6m;//secondary
			dutyRate.UD_TaxFeeFlag = "1";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.JI_CustomsQuantity = 100m;
			FeeCusCodeData feeData = invoiceLine.FeeCusCodes.GetFirstElementHaving("056");
			feeData.CY_SelectedRateType = "P";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Fee is calculated by specific rate", 50m, feeData.CY_FeeAmount);

			feeData.CY_SelectedRateType = "S";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Fee is calculated by specific rate", 60m, feeData.CY_FeeAmount);
		}

		public void TestCalculateWhenConditional()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate.UD_TaxFeeComputationCode = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;
			dutyRate.UD_TaxFeeFlag = "2";// conditional

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(50m, invoiceLine.CusEntryLine.SpiritsAmount);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, invoiceLine.CusEntryLine.SpiritsAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
	}
}
