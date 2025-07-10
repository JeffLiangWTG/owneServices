using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MPFCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2022, 10, 30)]
		public void TestPayableMPFWhenMPFIsZero()
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
			invoiceLine1.JI_LinePrice = 5000m;
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_Tariff = "1901.10.1600";
			invoiceLine1.US_SupTariff = "9903.19.19";
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Switzerland;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Switzerland;
			var mpf1 = invoiceLine1.FeeCusCodes.AddNew();
			mpf1.CY_IsOverridden = true;
			mpf1.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			mpf1.CY_FeeAmount = 0m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 5000m;
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_Tariff = "1901.10.1600";
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Switzerland;
			invoiceLine2.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Switzerland;
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, invoiceLine1.FeeCusCodes.Count);
			AssertEquals(0m, invoiceLine1.US_PayableMPF);
			AssertEquals(25.67m, invoiceLine2.US_PayableMPF);

			mpf1.CY_FeeAmount = 1m;
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, invoiceLine1.FeeCusCodes.Count);
			AssertEquals(12.84m, invoiceLine1.US_PayableMPF);
			AssertEquals(12.83m, invoiceLine2.US_PayableMPF);
		}

		public void TestMPFWithXVVLines()
		{
			#region Setup fee and tariff data

			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			var tariff99038803 = Factory.New<USCTariff>();
			tariff99038803.UE_Tariff = "99038803";
			tariff99038803.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff99038803.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff99038803.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff99038803.UE_Column1RateAdValorem = 0.25m;

			var tariff8424201000 = Factory.New<USCTariff>();
			tariff8424201000.UE_Tariff = "8424201000";
			tariff8424201000.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff8424201000.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff8424201000.UE_DutyComputationCode = ComputationCodeList.Codes.Derived;
			tariff8424201000.UE_Column1RateAdValorem = 1m;

			var tariff9503000013 = Factory.New<USCTariff>();
			tariff9503000013.UE_Tariff = "9503000013";
			tariff9503000013.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff9503000013.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff9503000013.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff9503000013.UE_Column1RateAdValorem = 0.053m;

			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 22259.00m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8424201000";
			invoiceLine1.US_SupTariff = "99038803";
			invoiceLine1.JI_CustomsQuantity = 1151933m;
			invoiceLine1.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Number;
			invoiceLine1.US_IsParent = true;
			invoiceLine1.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "8424201000";
			invoiceLine2.US_SupTariff = "99038803";
			invoiceLine2.JI_CustomsQuantity = 6183m;
			invoiceLine2.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Number;
			invoiceLine2.US_IsParent = true;
			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.JI_LinePrice = 16694m;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine2.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "9503000013";
			invoiceLine3.JI_CustomsQuantity = 6183m;
			invoiceLine3.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Number;
			invoiceLine3.US_IsParent = true;
			invoiceLine3.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.JI_LinePrice = 5565m;
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine3.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				AssertEquals("Payable MPF for invoice line 1.", 77.11m, invoiceLine1.US_PayableMPF);
				AssertEquals("Payable MPF for invoice line 2.", 0m, invoiceLine2.US_PayableMPF);
				AssertEquals("Payable MPF for invoice line 3.", 0m, invoiceLine3.US_PayableMPF);
				AssertEquals("MPF for invoice line 1.", 77.11m, invoiceLine1.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals("MPF for invoice line 2.", 0m, invoiceLine2.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals("MPF for invoice line 3.", 0m, invoiceLine3.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			});
		}

		public void TestMPFCalculationFor98020090()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802.00.90 00";
			invoiceLine.US_98GoodsValue = 8000m;
			invoiceLine.JI_LinePrice = 10000.00m;
			invoiceLine.JI_Tariff = "6110.90.90 10";

			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestMPFCalculationForTemporaryImportationBond()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 600m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2011, 09, 14)]
		public void TestMPFCalculationForTIBWhenPrinting()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "7326908587";
			invoiceLine.JI_LinePrice = 10000m;
			AssertEquals(0m, new MPFCalculator(Factory).CalculateFee(invoiceLine).Amount);
			AssertEquals(21m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine).Amount);

			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_98GoodsValue = 24055m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			CusEntryLine normalLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);

			AssertEquals(0m, new MPFCalculator(Factory).CalculateFee(normalLine).Amount);
			AssertEquals(50.5155m, new MPFCalculator(Factory, true).CalculateFee(supLine).Amount);
		}

		public void TestCAEnteredInInvoiceHeaderForMPFCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfOrigin = "CA";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 600m;
			invoiceLine.US_SPI = "CA";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2011, 11, 07)]
		public void TestMPFCalculationForReconciliation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "7326908587";
			invoiceLine.JI_LinePrice = 9900;
			AssertEquals(34.2936m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine).Amount);

			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_98GoodsValue = 24055m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry = declaration.CustomsEntryHeaders[0];

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry.EntryNumber;
			reconOriginalEntry.US_R_ReleaseDate = new ZDateTime(2011, 09, 19);
			reconOriginalEntry.US_R_DutyRateDate = new ZDateTime(2011, 09, 20);
			reconOriginalEntry.US_R_DateForMPFCalc = new ZDateTime(2011, 09, 20);
			reconOriginalEntry.US_R_CalcOrigDuty = true;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			var reconInvoiceLine = reconDeclaration.InvoiceLines[0];

			reconInvoiceLine.US_SupTariff = "9802008068";
			reconInvoiceLine.JI_Tariff = "7326908587";
			reconInvoiceLine.JI_CustomsQuantity = 2;
			reconInvoiceLine.US_98GoodsValue = 2300m;
			reconInvoiceLine.JI_LinePrice = 10000m;

			reconInvoiceLine.US_R_OrigSupTariff = "9802008068";
			reconInvoiceLine.US_R_OrigTariff = "7326908587";
			reconInvoiceLine.US_R_OrigFirstQty = 2;
			reconInvoiceLine.US_R_Orig98Value = 2300m;
			reconInvoiceLine.US_R_OrigCV = 10000m;

			reconDeclaration.CalculateDutyFeesForAllEntries();

			AssertEquals(21m, reconInvoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);

			var reconOriginalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry2.CH_OrigEntryReference = "XJ570060031";
			reconOriginalEntry2.US_R_DutyRateDate = new ZDateTime(2011, 09, 14);
			reconOriginalEntry2.US_R_DateForMPFCalc = new ZDateTime(2011, 09, 14);
			reconOriginalEntry2.US_R_CalcOrigDuty = true;

			var reconInvoiceLine2 = reconOriginalEntry2.Invoice.InvoiceLines.AddNew();
			reconInvoiceLine2.US_SupTariff = "9802008068";
			reconInvoiceLine2.JI_Tariff = "7326908587";
			reconInvoiceLine2.JI_CustomsQuantity = 5;
			reconInvoiceLine2.US_98GoodsValue = 2500m;
			reconInvoiceLine2.JI_LinePrice = 20000m;

			reconInvoiceLine2.US_R_OrigSupTariff = "9802008068";
			reconInvoiceLine2.US_R_OrigTariff = "7326908587";
			reconInvoiceLine2.US_R_OrigFirstQty = 5;
			reconInvoiceLine2.US_R_Orig98Value = 2500m;
			reconInvoiceLine2.US_R_OrigCV = 20000m;

			reconDeclaration.CalculateDutyFeesForAllEntries();
			AssertEquals(42m, reconInvoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
		}

		[TestDate(2021, 3, 18)]
		public void TestMPFCalculationForReconciliation_CombinedLine()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;

			var invoice = declaration.Invoices.AddNew();
			invoice.US_CH_ReconEntry = originalEntry.CH_PK;

			var invoiceLineOne = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineTwo = invoice.JobComInvoiceLines.AddNew();
			invoiceLineOne.US_R_OrigSupTariff = "9802005060";
			invoiceLineOne.US_SupTariff = "9802005060";
			invoiceLineTwo.US_R_OrigTariff = "8466939885";
			invoiceLineTwo.JI_Tariff = "8466939885";
			invoiceLineTwo.US_R_OrigSupTariff = "99038803";
			invoiceLineTwo.US_SupTariff = "99038803";
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			Assert("Should not have MPF", !new MPFCalculator(Factory).ShouldHaveFee(new ReconCurrentDutyData(invoiceLineOne, ZDateTime.Today)));
			Assert("Should not have MPF", !new MPFCalculator(Factory).ShouldHaveFee(new ReconSupDutyData(invoiceLineOne, ZDateTime.Today)));
			Assert("Should not have MPF", !new MPFCalculator(Factory).ShouldHaveFee(new ReconCurrentDutyData(invoiceLineTwo, ZDateTime.Today)));
			Assert("Should not have MPF", !new MPFCalculator(Factory).ShouldHaveFee(new ReconSupDutyData(invoiceLineTwo, ZDateTime.Today)));
		}

		public void TestInformalEntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertEquals("Precondition", false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));

			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			AssertEquals("Precondition", false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestAUFTA()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = ZString.Empty;
			invoiceLine.JI_Tariff = "9913";
			AssertEquals(true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestCAFTA()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = SpecialProgramList.Codes.PPlus;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestBahrainFTA()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = SpecialProgramList.Codes.BH;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = ZString.Empty;
			invoiceLine.JI_Tariff = "9914";
			AssertEquals(true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestChileFTA()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = SpecialProgramList.Codes.CL;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = ZString.Empty;
			invoiceLine.JI_Tariff = "991199";
			AssertEquals(true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestMexicanNAFTA()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = SpecialProgramList.Codes.MX;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = ZString.Empty;
			invoiceLine.JI_Tariff = "9906";
			AssertEquals(true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestSingaporeFTA()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = SpecialProgramList.Codes.SG;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = ZString.Empty;
			invoiceLine.JI_Tariff = "9910";
			AssertEquals(true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestCanadianNAFTA()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestCBERA()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.W;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestPartOfSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLineX = declaration.InvoiceLines.AddNew();
			invoiceLineX.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals(true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLineX));
			var invoiceLineV = declaration.InvoiceLines.AddNew();
			invoiceLineV.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLineV.JI_ParentID = invoiceLineX.PK;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLineV));
		}

		public void TestInsularPossesion()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Y;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestChapter98ForMPFCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("Precondition", true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.JI_Tariff = "9802.00.60";
			AssertEquals("Precondition", false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
			invoiceLine.JI_LinePrice = 10000m;
			AssertEquals(0m, new MPFCalculator(Factory).CalculateFee(invoiceLine).Amount);
			invoiceLine.JI_Tariff = "9802.00.80";
			AssertEquals(0m, new MPFCalculator(Factory).CalculateFee(invoiceLine).Amount);
		}

		public void TestLDDCCountry()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Vanuatu;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestCaribbeanBasinEconomicRecoveryAct()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.E;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestATPDEASpecific()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.J;
			AssertEquals(true, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestIsraelMPF()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Israel;
			AssertEquals(false, new MPFCalculator(Factory).ShouldHaveFee(invoiceLine));
		}

		public void TestCombineChildMPFForChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802006000Tariff.UE_Tariff;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(!Chapter98Helper.IsExemptMPFForCombineLines(testHelper.ParentLine));
			AssertNull(testHelper.ChildLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertNotNull(testHelper.ParentLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 0m;
			testHelper.ChildLine.US_98ValueInvCurr = 0m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var mpfFee = testHelper.ParentLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals(3.46m, mpfFee);
			AssertNull(testHelper.ChildLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			mpfFee = testHelper.ParentLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals("JI_LinePrice * rate", 3.46m, mpfFee);
			AssertNull(testHelper.ChildLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}
	}
}
