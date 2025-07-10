using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Customs.ZA;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestJZ_ROOType_ListAttribute()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(JobComInvoiceHeader),
				JobComInvoiceHeader.Schema.JZ_ROOType,
				true,
				attrib => attrib.ListDataSourceMember == nameof(JobComInvoiceHeader.Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ROOTypesList));
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestUniversalCopyAttributes()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();

			var componentType = invoiceHeader.GetType();
			AssertEquals("JobComInvoiceHeader should have UniversalCopyWithExtendedEntitiesAttribute.", true, componentType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Length > 0);

			var jobComInvoiceLinesInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "JobComInvoiceLines");
			AssertEquals("JobComInvoiceLines collection should have UniversalCopyCollectionEntityAttribute.", true, jobComInvoiceLinesInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), true).First() != null);
		}

		public void TestSetDefaultInvoiceDate()
		{
			var zaInvoice = NewInvoiceHeader;
			AssertEquals("For ZA, invoice date should not default to a value", ZDateTime.Empty, zaInvoice.JZ_InvoiceDate);
		}

		public void TestJZ_Calc_FOBAmount()
		{
			var header = NewInvoiceHeader;
			header.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_IncoTerm = "DDP";

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);

			var line = header.InvoiceLines.AddNew();
			line.Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue, 50);

			header.JZ_InvoiceAmount = 10600;
			ZDecimal expectedFOB = new ZDecimal(10600 - 10 - 100 - 200);
			header.JobDeclaration.ResumeApportionment();
			AssertEquals("FOB Value", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public void TestJZ_Calc_CIFAmount()
		{
			var header = NewInvoiceHeader;
			header.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			var line = header.InvoiceLines.AddNew();
			line.Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue, 50);

			header.JZ_IncoTerm = "FOB";
			ZDecimal expected = 10600m;
			header.JobDeclaration.ResumeApportionment();

			AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestJZ_RX_NKInvoice_Currency_ReadOnly()
		{
			var invoiceHeader = NewInvoiceHeader;
			var jobDeclaration = invoiceHeader.JobDeclaration;
			jobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("Not locked in imports", false, invoiceHeader.JZ_RX_NKInvoice_Currency_ReadOnly);

			jobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals("Not locked in exports", false, invoiceHeader.JZ_RX_NKInvoice_Currency_ReadOnly);

			jobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals("Locked on ex-Bonds", true, invoiceHeader.JZ_RX_NKInvoice_Currency_ReadOnly);
		}

		public void TestValuationCodeWhenRelatedIndicatorChanged()
		{
			JobComInvoiceHeader invoiceHeader = NewInvoiceHeader;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_ValuationCode = ValuationCodeList.Codes.Section1;
			invoiceHeader.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Yes;
			AssertEquals(RelatedIndicatorList.Codes.Yes, invoiceHeader.JZ_RelatedIndicator);
			AssertEquals(ValuationCodeList.Codes.Section1, invoiceHeader.JZ_ValuationCode);

			invoiceHeader.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Exempt;
			AssertEquals(RelatedIndicatorList.Codes.Exempt, invoiceHeader.JZ_RelatedIndicator);
			AssertEquals(ZString.Empty, invoiceHeader.JZ_ValuationCode);
		}

		public void TestValuationCodeCopiedWhenSupplierChanged()
		{
			CombineAssertions(() =>
			{
				JobComInvoiceHeader invoiceHeader = NewInvoiceHeader;
				OrgHeader importer = Factory.New<OrgHeader>();
				importer.OH_RL_NKClosestPort = "CATOR";
				OrgHeader supplier = Factory.New<OrgHeader>();
				OrgSupplierBuyerLink link1 = supplier.BuyerLinks.AddNew(importer);
				link1.OL_ValuationBasis = "1";
				link1.OL_RelatedParty = "R";
				link1.OL_RN_NKImporterCountry = "CA";
				OrgSupplierBuyerLink link2 = supplier.BuyerLinks.AddNew(importer);
				link2.OL_ValuationBasis = "2";
				link2.OL_RelatedParty = "R";

				link2.OL_RN_NKImporterCountry = "ZA";

				JobDeclaration declaration = invoiceHeader.JobDeclaration;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				invoiceHeader.JZ_OH_Supplier = supplier.PK;
				AssertEquals("R", invoiceHeader.JZ_RelatedIndicator);
				AssertEquals("1", invoiceHeader.JZ_ValuationCode);

				invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
				declaration.JE_RL_NKFinalDestination = "ZASDD";
				invoiceHeader.JZ_OH_Supplier = supplier.PK;
				AssertEquals("R", invoiceHeader.JZ_RelatedIndicator);
				AssertEquals("1", invoiceHeader.JZ_ValuationCode);
			});
		}

		public void TestValuationCodeIsBlankWhenSupplierChangedAndNoVDNIsSpecified()
		{
			var headerMock = Factory.NewMoq<JobComInvoiceHeader>();
			JobComInvoiceHeader invoiceHeader = headerMock.Object;
			invoiceHeader.JZ_JE = declaration.PK;
			OrgHeader importer = OrgHeader.New(Factory);
			OrgHeader supplier = OrgHeader.New(Factory);

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = importer.PK;
			link.OL_OH_Supplier = supplier.PK;
			link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			link.OL_ValuationBasis = "R1";
			link.OL_ValuationBasisDeterminationNum = "VDN";
			invoiceHeader.JobDeclaration.JE_OH_Importer = importer.PK;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			AssertEquals(ZString.Empty, invoiceHeader.JZ_RelatedIndicator);
			AssertEquals(ZString.Empty, invoiceHeader.JZ_ValuationCode);
		}

		public override void TestIWeightApportioneeRoundingIssue()
		{
			var foreignCurrency = RefCurrency.New(Factory);
			foreignCurrency.RX_Code = "XYZ";
			foreignCurrency.SetCustomsRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), RatesAreReciprocal ? 1.231678m : 0.8119m);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice1.JZ_InvoiceAmount = 8000m;

			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice2.JZ_InvoiceAmount = 2000m;

			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice1, true);
			invoice1.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 0.25m : 4m;

			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, true);
			invoice2.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 4M : 0.25m;
			AssertEquals("Weight", 2000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 8000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice1, false);
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, false);
			declaration.ApportionInvoiceWeight(null);
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, true);
			invoice2.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 4M : 0.25m;
			AssertEquals("Weight", 5000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 5000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);
		}

		public void TestWeightApportionmentCustomsQuantity_MetresCubedIsNotWeight()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "280421", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "MC");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 8000m;
			invoice.JZ_NetWeight = 10000m;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_Tariff = "280421";
			CombineAssertions(() =>
			{
				AssertEquals("NetWeight", 10000m, line.JI_NetWeight);
				AssertEquals("NetWeightUQ", Core.Constants.Weight.Kilograms, line.JI_NetWeightUQ);

				AssertEquals("CustomsQuantity", 0m, line.JI_CustomsQuantity);
				AssertEquals("CustomsUnitQty", "MC", line.JI_CustomsUnitQty);
			});
		}

		public void TestDefaultValuesFromSupplierBuyerLink()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var invoiceHeader = declaration.Invoices.AddNew();
			var importer = OrgHeader.New(Factory);
			var supplier = OrgHeader.New(Factory);

			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = importer.PK;
			link.OL_OH_Supplier = supplier.PK;
			link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			link.OL_ValuationBasisDeterminationNum = "VDN";
			link.OL_ValuationBasisMarkupPercent = 12.25m;
			link.OL_ValuationBasis = "OLVB";
			link.OL_RelatedParty = "Y";

			invoiceHeader.JobDeclaration.JE_OH_Importer = importer.PK;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertEquals("VDN", invoiceHeader.JZ_VDN);
			AssertEquals("Y", invoiceHeader.JZ_RelatedIndicator);
			AssertEquals("OL", invoiceHeader.JZ_ValuationCode);
			AssertEquals(12.25m, invoiceHeader.JZ_ValuationMarkup);
		}

		public void TestROOTypeFromDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_ROOType = "EUR";

			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals(declaration.JE_ROOType, invoiceHeader.JZ_ROOType);

			declaration.JE_ROOType = "ETA";
			AssertEquals(declaration.JE_ROOType, invoiceHeader.JZ_ROOType);

			invoiceHeader.JZ_ROOType = "EUR";
			AssertNotEquals(declaration.JE_ROOType, invoiceHeader.JZ_ROOType);

			declaration.JE_ROOType = "GSP";
			AssertNotEquals(declaration.JE_ROOType, invoiceHeader.JZ_ROOType);
		}

		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			JobComInvoiceHeader invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice.JZ_InvoiceAmount = 10500m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var line = invoice.InvoiceLines.AddNew();

			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			BaseJobComInvHeaderCharge adjustedOFT = invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsIncludedInITOT = false;

			line.JI_LinePrice = 10000m;
			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10200m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);

			oFT.J7_IsIncludedInITOT = true;
			adjustedOFT.J7_IsIncludedInITOT = true;

			line.JI_LinePrice = 10500m;
			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10200m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestIDA63ValueRecalculationParent()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
			Factory.Save();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "XX";
			testInvoiceLine2.JI_CEI = testInstruction.PK;
			testInvoiceLine2.JI_Procedure = testInvoiceLine2.EntryInstruction.CEI_Style + "YY";
			AssertEquals(true, (testInvoice as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(false, (testInvoiceLine as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(true, (testInvoiceLine2 as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			testInvoice.RecalculateDA63Values();
			AssertEquals(false, (testInvoice as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(false, (testInvoiceLine as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(false, (testInvoiceLine2 as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
		}

		public void TestMaximumDaysToFallback()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, "ZAR"));
			currency.ExchangeRates.DeleteAll();
			RefExchangeRate rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = new ZDateTime(2016, 12, 1);
			rate.RE_ExpiryDate = new ZDateTime(2016, 12, 1);
			rate.RE_SellRate = 0.70m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 12, 2);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			AssertEquals("Fall back days", 0, ((ICurrencyConverterDataProvider)invoice).MaximumDaysToFallback);
			AssertEquals("JZ_InvoiceCurrExRate", 0m, invoice.JZ_InvoiceCurrExRate);
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");
		}

		public void TestIsZAROnlyInvoice()
		{
			CombineAssertions("IMP", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("Foreign InvoiceCurrency", false, invoice.IsZAROnlyInvoice);
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				AssertEquals("Local InvoiceCurrency", true, invoice.IsZAROnlyInvoice);
				var invCharge = invoice.Charges.AddNew("xxx", 1m, Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals("Foreign InvoiceCharge", false, invoice.IsZAROnlyInvoice);
				invCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
				AssertEquals("Local InvoiceCharge", true, invoice.IsZAROnlyInvoice);
				invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("Foreign CVOverride", false, invoice.IsZAROnlyInvoice);
				invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = Core.Constants.CurrencyCodes.SouthAfrica;
				AssertEquals("Local CVOverride", true, invoice.IsZAROnlyInvoice);
				var invLineCharge = invoiceLine.Charges.AddNew("xxx", 1m, Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals("Foreign LineCharge", false, invoice.IsZAROnlyInvoice);
				invLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
				AssertEquals("Foreign LineCharge", true, invoice.IsZAROnlyInvoice);
			});

			CombineAssertions("EMP", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("Foreign InvoiceCurrency", false, invoice.IsZAROnlyInvoice);
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				AssertEquals("Local InvoiceCurrency", true, invoice.IsZAROnlyInvoice);
				var invCharge = invoice.Charges.AddNew("xxx", 1m, Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals("Foreign InvoiceCharge", false, invoice.IsZAROnlyInvoice);
				invCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
				AssertEquals("Local InvoiceCharge", true, invoice.IsZAROnlyInvoice);
				invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("Foreign CVOverride", true, invoice.IsZAROnlyInvoice);
				invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = Core.Constants.CurrencyCodes.SouthAfrica;
				AssertEquals("Local CVOverride", true, invoice.IsZAROnlyInvoice);
				var invLineCharge = invoiceLine.Charges.AddNew("xxx", 1m, Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals("Foreign LineCharge", false, invoice.IsZAROnlyInvoice);
				invLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
				AssertEquals("Foreign LineCharge", true, invoice.IsZAROnlyInvoice);
			});
		}

		[TestDate(2016, 3, 1, 13, 13, 13)]
		public void TestEffectiveValuationDateCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryInst = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = entryInst.PK;
			invoiceLine.JI_CEI = entryInst.PK;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var entryInst1 = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine1.JI_CEI = entryInst1.PK;
			entryInst.CEI_ExchangeRateDate = new ZDateTime(2016, 4, 5);
			entryInst1.CEI_ExchangeRateDate = new ZDateTime(2016, 5, 5);
			entryInst.CEI_Style = "CCC";
			entryInst.CEI_Description = "CCCccc";
			entryInst1.CEI_Style = "AAA";
			entryInst1.CEI_Description = "AAAaaa";
			invoice.JZ_ValuationDateOverride = new ZDateTime(2016, 3, 3);
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 3, 4);
			Factory.Save();
			invoice = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoice.PK);
			AssertNotNull("Instruction for this invoice should not be null", invoice.InvoiceLines[0].EntryInstruction);
			invoice.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("Inv's EffectiveValuationDateCore is Base.JZ_ValuationDateOverride", new ZDateTime(2016, 3, 3), invoice.EffectiveValuationDate);

			invoice.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			AssertEquals("Inv's EffectiveValuationDateCore is Base.JZ_ValuationDateOverride", new ZDateTime(2016, 3, 3), invoice.EffectiveValuationDate);

			invoice.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals("Inv's EffectiveValuationDateCore is CEI_ExchangeRateDate", new ZDateTime(2016, 5, 5), invoice.EffectiveValuationDate);

			entryInst.CEI_ExchangeRateDate = ZDateTime.Empty;
			entryInst1.CEI_ExchangeRateDate = ZDateTime.Empty;
			Factory.Save();
			invoice = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("Inv's EffectiveValuationDateCore is Base.JZ_ValuationDateOverride", new ZDateTime(2016, 3, 3), invoice.EffectiveValuationDate);

			invoice.JZ_ValuationDateOverride = ZDateTime.Empty;
			AssertEquals("Inv's EffectiveValuationDateCore is JobDeclaration's DateOfValuation (EXP -> Today-1)", new ZDateTime(2016, 2, 29), invoice.EffectiveValuationDate);
		}

		public void TestConcurrencyExceptionHandlingForAddInfo()
		{
			var invoice = NewInvoiceHeader;
			invoice.JZ_ROOCert = "Cert 1";
			Factory.RefreshEnabled = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceReloaded = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			invoiceReloaded.JZ_ROOCert = "Cert 2";
			newFactory.RefreshEnabled = false;
			newFactory.Save();

			invoice.Delete();
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());
			ZExceptionReporting.HandleSaveException(ex);
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.SouthAfrica;

		protected override void AfterInitialise(BaseJobDeclaration declaration)
		{
			base.AfterInitialise(declaration);
			var zaDec = (JobDeclaration)declaration;
			zaDec.JE_MasterBillIssuedDate = ZDateTime.Empty;
		}

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override BusinessObject GetNewBusinessObject() => NewInvoiceHeader;

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<ApportionedCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		new JobComInvoiceHeader NewInvoiceHeader
		{
			get
			{
				var declaration = (JobDeclaration)GetNewDeclaration();
				var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				return groupHeader.JobComInvoiceHeaders.AddNew();
			}
		}
	}
}
