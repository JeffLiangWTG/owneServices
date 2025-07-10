using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LineMerger))]
	sealed class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override bool ApplyClassificationToKeyForLineForMerge => false;

		[TestDate(2021, 07, 08)]
		[ExpectNoExceptions]
		public void TestCalculateTPFAndClearTpfUnderMinimumThreshold()
		{
			var factory = new BusinessObjectFactory();

			var helper = new UniversalReferenceTestDataHelper(factory);

			#region TW TradeGroup
			var tradeGroupAll = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "All Countries");
			var tradeGroupWTO = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "WTO Countries (Column I)");
			var tradeGroupFTA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "FTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "FTA Countries (Column I)");
			var tradeGroupSPE = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SPE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Special Countries (Column II)");
			var tradeGroupLDC = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "LDC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Least Developed Countries");
			var tradeGroupPA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Panama");
			var tradeGroupSZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Eswatini");
			var tradeGroupSG = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Singapore");
			var tradeGroupCN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "China");
			var tradeGroupHN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "HN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Honduras");
			var tradeGroupSV = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SV", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "El Salvador");
			var tradeGroupNI = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nicaragua");
			var tradeGroupPY = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Paraguay");
			var tradeGroupNZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "New Zealand");
			var tradeGroupGT = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "GT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Guatemala");

			helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			#endregion

			#region TaxOrFee
			var taxOrFeeTPF = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, description: "推廣貿易服務費", startDate: ZDateTime.Today.AddDays(1), endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeTPF.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			taxOrFeeTPF.ZZF_Threshold = 100m;

			var taxOrFeeDDF = helper.CreateTaxOrFee("DDF", 200m, Core.Constants.CountryCodes.Taiwan, description: "滯報費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeDDF.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			#endregion

			#region CusRateType
			var refCusRateTypeSSG = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG", description: "Specifically Selected Goods or Services Tax");
			refCusRateTypeSSG.ZZR_IsPayable = true;
			refCusRateTypeSSG.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD + VAT + CTA + CTS + TAT + HWS";

			var refCusRateTypeCOM = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM", description: "Commodity Taxes");
			refCusRateTypeCOM.ZZR_IsPayable = true;
			refCusRateTypeCOM.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD";

			var refCusRateTypeDTY = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY", description: "Duty");
			refCusRateTypeDTY.ZZR_IsPayable = true;
			refCusRateTypeDTY.ZZR_CustomsValueFormula = "CV";

			factory.Save();
			#endregion

			#region CusRateCode
			var rateCodeSSG = helper.LoadOrCreateNewCusRateCode(factory, "SSG", refCusRateTypeSSG.PK, description: "特種貨物及勞務稅");

			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(factory, "CTA", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "CTS", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "HWS", refCusRateTypeCOM.PK, description: "健康福利捐");
			helper.LoadOrCreateNewCusRateCode(factory, "TAT", refCusRateTypeCOM.PK, description: "菸酒稅");

			helper.LoadOrCreateNewCusRateCode(factory, "DTS", refCusRateTypeDTY.PK, description: "進口稅");
			var rateCodeDTA = helper.LoadOrCreateNewCusRateCode(factory, "DTA", refCusRateTypeDTY.PK, description: "進口稅");

			factory.Save();
			#endregion

			#region TariffType
			var tariffTypeTT = helper.CreateNewOrGetExistingTariffType("TW", "TT");
			tariffTypeTT.ZZI_Description = "Tobacco Tax";

			var tariffTypeSS = helper.CreateNewOrGetExistingTariffType("TW", "SS");
			tariffTypeSS.ZZI_Description = "Specifically Selected Goods and Services Tax";

			var tariffTypeCT = helper.CreateNewOrGetExistingTariffType("TW", "CT");
			tariffTypeCT.ZZI_Description = "Commodity Tax";

			var tariffTypeAT = helper.CreateNewOrGetExistingTariffType("TW", "AT");
			tariffTypeAT.ZZI_Description = "Alcohol Tax";

			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType("TW", "HSN", nomenclatureGroupType: "TW");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			factory.Save();
			#endregion

			#region Preference For Taiwan
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", "TW");
			var preferencePR2 = helper.CreatePreferenceForCountry("PR2", "Column II Rates(FTA/LDC)", "TW");
			var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Column III Rates(Standard Rates)", "TW");
			factory.Save();
			#endregion

			var cusProcedure62 = helper.CreateRefCusProcedure("TW", "IM", "62", "", "", "估價未決", "IMP", true);
			cusProcedure62.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("SSGPaymentMethod", "CAS");
			cusProcedure62.Attributes.AddNew("TATPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("VATPaymentMethod", "DEF");

			var tariff87034000319 = helper.CreateTariff("TW", tariffTypeHSN.PK, "87034000319", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, compositeKey: "17.87..03.4.10.60", description: "Sedan (including convertible, sports) and station wagons, of a cylinder capacity exceeding 1,500 c.c. but not exceeding 3,000 c.c.");
			var tariffDTARate87034000319 = helper.CreateRate(tariff87034000319, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.175*VFD", preferencePR1.PK, "0.175", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffDTARate87034000319, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(tariffDTARate87034000319, tradeGroupFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(2);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 2948836m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8703.40.00.31-9";
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_CountryOfOrigin = "GB";
			invoiceLine.JI_Procedure = "62";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;
			invoiceLine.JI_NetWeight = 2665m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 2665m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_CustomsSecondUnitQty = "NIU";
			invoiceLine.JI_VatPymntMthd = "CAS";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			var tpfFee = entryLineFees.Single(x => x.CF_ChargeType == "TPF");
			NUnit.Framework.Assert.That(tpfFee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(1179.534m).Using(CustomComparers.TypeComparison));

			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.EqualTo(false), "Does not have an effective TPF RefCusTaxOrFee, and the fee is not populated.");

			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_EnteredUnitPrice = 100000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.EqualTo(false), "Clear TPF fee when amount less than Threshold.");
		}

		[ExpectNoExceptions]
		public void TestCalculateDutiesContainsOverrideRateFees()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			#region TW TradeGroup
			var tradeGroupAll = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "All Countries");
			var tradeGroupWTO = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "WTO Countries (Column I)");
			var tradeGroupFTA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "FTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "FTA Countries (Column I)");
			var tradeGroupSPE = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SPE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Special Countries (Column II)");
			var tradeGroupLDC = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "LDC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Least Developed Countries");
			var tradeGroupPA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Panama");
			var tradeGroupSZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Eswatini");
			var tradeGroupSG = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Singapore");
			var tradeGroupCN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "China");
			var tradeGroupHN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "HN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Honduras");
			var tradeGroupSV = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SV", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "El Salvador");
			var tradeGroupNI = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nicaragua");
			var tradeGroupPY = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Paraguay");
			var tradeGroupNZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "New Zealand");
			var tradeGroupGT = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "GT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Guatemala");

			helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			#endregion

			#region TaxOrFee
			var taxOrFeeTPF = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, description: "推廣貿易服務費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeTPF.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			taxOrFeeTPF.ZZF_Threshold = 100m;

			var taxOrFeeDDF = helper.CreateTaxOrFee("DDF", 200m, Core.Constants.CountryCodes.Taiwan, description: "滯報費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeDDF.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			#endregion

			#region CusRateType
			var refCusRateTypeSSG = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG", description: "Specifically Selected Goods or Services Tax");
			refCusRateTypeSSG.ZZR_IsPayable = true;
			refCusRateTypeSSG.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD + VAT + CTA + CTS + TAT + HWS";

			var refCusRateTypeCOM = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM", description: "Commodity Taxes");
			refCusRateTypeCOM.ZZR_IsPayable = true;
			refCusRateTypeCOM.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD";

			var refCusRateTypeDTY = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY", description: "Duty");
			refCusRateTypeDTY.ZZR_IsPayable = true;
			refCusRateTypeDTY.ZZR_CustomsValueFormula = "CV";

			factory.Save();
			#endregion

			#region CusRateCode
			var rateCodeSSG = helper.LoadOrCreateNewCusRateCode(factory, "SSG", refCusRateTypeSSG.PK, description: "特種貨物及勞務稅");

			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(factory, "CTA", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "CTS", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "HWS", refCusRateTypeCOM.PK, description: "健康福利捐");
			helper.LoadOrCreateNewCusRateCode(factory, "TAT", refCusRateTypeCOM.PK, description: "菸酒稅");

			helper.LoadOrCreateNewCusRateCode(factory, "DTS", refCusRateTypeDTY.PK, description: "進口稅");
			var rateCodeDTA = helper.LoadOrCreateNewCusRateCode(factory, "DTA", refCusRateTypeDTY.PK, description: "進口稅");

			factory.Save();
			#endregion

			#region TariffType
			var tariffTypeTT = helper.CreateNewOrGetExistingTariffType("TW", "TT");
			tariffTypeTT.ZZI_Description = "Tobacco Tax";

			var tariffTypeSS = helper.CreateNewOrGetExistingTariffType("TW", "SS");
			tariffTypeSS.ZZI_Description = "Specifically Selected Goods and Services Tax";

			var tariffTypeCT = helper.CreateNewOrGetExistingTariffType("TW", "CT");
			tariffTypeCT.ZZI_Description = "Commodity Tax";

			var tariffTypeAT = helper.CreateNewOrGetExistingTariffType("TW", "AT");
			tariffTypeAT.ZZI_Description = "Alcohol Tax";

			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType("TW", "HSN", nomenclatureGroupType: "TW");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			factory.Save();
			#endregion

			#region Preference For Taiwan
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", "TW");
			var preferencePR2 = helper.CreatePreferenceForCountry("PR2", "Column II Rates(FTA/LDC)", "TW");
			var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Column III Rates(Standard Rates)", "TW");
			factory.Save();
			#endregion

			var cusProcedure62 = helper.CreateRefCusProcedure("TW", "IM", "62", "", "", "估價未決", "IMP", true);
			cusProcedure62.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("SSGPaymentMethod", "CAS");
			cusProcedure62.Attributes.AddNew("TATPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("VATPaymentMethod", "DEF");

			var tariff87034000319 = helper.CreateTariff("TW", tariffTypeHSN.PK, "87034000319", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, compositeKey: "17.87..03.4.10.60", description: "Sedan (including convertible, sports) and station wagons, of a cylinder capacity exceeding 1,500 c.c. but not exceeding 3,000 c.c.");
			var tariffDTARate87034000319 = helper.CreateRate(tariff87034000319, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.175*VFD", preferencePR1.PK, "0.175", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffDTARate87034000319, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(tariffDTARate87034000319, tradeGroupFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffPASSENGERCAR = helper.CreateTariff("TW", tariffTypeSS.PK, "PASSENGERCAR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "小客車");
			var tariffSSGRatePASSENGERCAR = helper.CreateRate(tariffPASSENGERCAR, rateCodeSSG.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1 * VFD", null, "0.1", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffSSGRatePASSENGERCAR, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffSedanAbove2000CC = helper.CreateTariff("TW", tariffTypeCT.PK, "SedanAbove2000CC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "小客車(汽缸排氣量在二千零一立方公分以上) - 包括駕駛人座位在內，座位在九座以下之載人汽車");
			var tariffSSGRateSedanAbove2000CC = helper.CreateRate(tariffSedanAbove2000CC, rateCodeCTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3 * VFD", null, "0.3", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffSSGRateSedanAbove2000CC, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 2948836m;

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18436.00m, Core.Constants.CurrencyCodes.Taiwan);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 5441.00m, Core.Constants.CurrencyCodes.Taiwan);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8703.40.00.31-9";
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_CountryOfOrigin = "GB";
			invoiceLine.JI_Procedure = "62";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;
			invoiceLine.JI_NetWeight = 2665m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 2665m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_CustomsSecondUnitQty = "NIU";
			invoiceLine.JI_VatPymntMthd = "CAS";
			var lineTax1 = invoiceLine.Taxes.AddNew();
			lineTax1.JLT_Type = "SS";
			lineTax1.JLT_Tariff = "PASSENGERCAR";

			var lineTax2 = invoiceLine.Taxes.AddNew();
			lineTax2.JLT_Type = "CT";
			lineTax2.JLT_Tariff = "SEDANABOVE2000CC";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			var dtaFee = entryLineFees.Single(x => x.CF_ChargeType == "DTA");
			dtaFee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			dtaFee.CF_ChargeAmount = 1000m;

			var ctaFee = entryLineFees.Single(x => x.CF_ChargeType == "CTA");
			ctaFee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Additional;
			ctaFee.CF_ChargeAmount = 100m;

			var vatFee = entryLineFees.Single(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS");
			vatFee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			vatFee.CF_ChargeAmount = 1500m;

			var ssgFee = entryLineFees.Single(x => x.CF_ChargeType == "SSG");
			ssgFee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			ssgFee.CF_ChargeAmount = 800m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryLineFeesAfterReMerge = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFeesAfterReMerge.Count(), NUnit.Framework.Is.EqualTo(6));
			dtaFee = entryLineFeesAfterReMerge.Single(x => x.CF_ChargeType == "DTA");
			NUnit.Framework.Assert.That(dtaFee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));

			var ctaFees = entryLineFeesAfterReMerge.Where(x => x.CF_ChargeType == "CTA").OrderByDescending(x => x.CF_ChargeAmount);
			NUnit.Framework.Assert.That(ctaFees.First().CF_ChargeAmount, NUnit.Framework.Is.EqualTo(884950.8m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(ctaFees.ElementAt(1).CF_ChargeAmount, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison));

			vatFee = entryLineFeesAfterReMerge.Single(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS");
			NUnit.Framework.Assert.That(vatFee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(1500m).Using(CustomComparers.TypeComparison));

			ssgFee = entryLineFeesAfterReMerge.Single(x => x.CF_ChargeType == "SSG");
			NUnit.Framework.Assert.That(ssgFee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(800m).Using(CustomComparers.TypeComparison));

			var tpfFee = entryLineFeesAfterReMerge.Single(x => x.CF_ChargeType == "TPF");
			NUnit.Framework.Assert.That(tpfFee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(1179.534m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCalculateAdditionalDuties_PaymentMethod()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			#region TW TradeGroup
			var tradeGroupAll = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "All Countries");
			var tradeGroupWTO = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "WTO Countries (Column I)");
			var tradeGroupFTA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "FTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "FTA Countries (Column I)");
			var tradeGroupSPE = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SPE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Special Countries (Column II)");
			var tradeGroupLDC = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "LDC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Least Developed Countries");
			var tradeGroupPA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Panama");
			var tradeGroupSZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Eswatini");
			var tradeGroupSG = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Singapore");
			var tradeGroupCN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "China");
			var tradeGroupHN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "HN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Honduras");
			var tradeGroupSV = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SV", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "El Salvador");
			var tradeGroupNI = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nicaragua");
			var tradeGroupPY = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Paraguay");
			var tradeGroupNZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "New Zealand");
			var tradeGroupGT = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "GT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Guatemala");

			helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			#endregion

			#region TaxOrFee
			var taxOrFeeTPF = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, description: "推廣貿易服務費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeTPF.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			taxOrFeeTPF.ZZF_Threshold = 100m;

			var taxOrFeeDDF = helper.CreateTaxOrFee("DDF", 200m, Core.Constants.CountryCodes.Taiwan, description: "滯報費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeDDF.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			#endregion

			#region CusRateType
			var refCusRateTypeSSG = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG", description: "Specifically Selected Goods or Services Tax");
			refCusRateTypeSSG.ZZR_IsPayable = true;
			refCusRateTypeSSG.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD + VAT + CTA + CTS + TAT + HWS";

			var refCusRateTypeCOM = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM", description: "Commodity Taxes");
			refCusRateTypeCOM.ZZR_IsPayable = true;
			refCusRateTypeCOM.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD";

			var refCusRateTypeDTY = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY", description: "Duty");
			refCusRateTypeDTY.ZZR_IsPayable = true;
			refCusRateTypeDTY.ZZR_CustomsValueFormula = "CV";

			factory.Save();
			#endregion

			#region CusRateCode
			var rateCodeSSG = helper.LoadOrCreateNewCusRateCode(factory, "SSG", refCusRateTypeSSG.PK, description: "特種貨物及勞務稅");

			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(factory, "CTA", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "CTS", refCusRateTypeCOM.PK, description: "貨物稅");
			var rateCodeHWS = helper.LoadOrCreateNewCusRateCode(factory, "HWS", refCusRateTypeCOM.PK, description: "健康福利捐");
			var rateCodeTAT = helper.LoadOrCreateNewCusRateCode(factory, "TAT", refCusRateTypeCOM.PK, description: "菸酒稅");

			helper.LoadOrCreateNewCusRateCode(factory, "DTS", refCusRateTypeDTY.PK, description: "進口稅");
			var rateCodeDTA = helper.LoadOrCreateNewCusRateCode(factory, "DTA", refCusRateTypeDTY.PK, description: "進口稅");

			factory.Save();
			#endregion

			#region TariffType
			var tariffTypeTT = helper.CreateNewOrGetExistingTariffType("TW", "TT");
			tariffTypeTT.ZZI_Description = "Tobacco Tax";

			var tariffTypeSS = helper.CreateNewOrGetExistingTariffType("TW", "SS");
			tariffTypeSS.ZZI_Description = "Specifically Selected Goods and Services Tax";

			var tariffTypeCT = helper.CreateNewOrGetExistingTariffType("TW", "CT");
			tariffTypeCT.ZZI_Description = "Commodity Tax";

			var tariffTypeAT = helper.CreateNewOrGetExistingTariffType("TW", "AT");
			tariffTypeAT.ZZI_Description = "Alcohol Tax";

			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType("TW", "HSN", nomenclatureGroupType: "TW");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			factory.Save();
			#endregion

			#region Preference For Taiwan
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", "TW");
			var preferencePR2 = helper.CreatePreferenceForCountry("PR2", "Column II Rates(FTA/LDC)", "TW");
			var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Column III Rates(Standard Rates)", "TW");
			factory.Save();
			#endregion

			var cusProcedure71 = helper.CreateRefCusProcedure("TW", "IM", "71", "", "", "國貨待復運出口", "IMP", true);
			cusProcedure71.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("SSGPaymentMethod", "CAS");
			cusProcedure71.Attributes.AddNew("TATPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("VATPaymentMethod", "DEF");

			var tariff24039990002 = helper.CreateTariff("TW", tariffTypeHSN.PK, "24039990002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, compositeKey: "04.24..03.9.9.20.10", description: "OTHER MANUFACTURED TOBACCO AND MANUFACTURED TOBACCO SUBSTITUTES");
			var tariffDTA_PR1_Rate24039990002 = helper.CreateRate(tariff24039990002, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15*VFD", preferencePR1.PK, "0.15", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffDTA_PR1_Rate24039990002, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(tariffDTA_PR1_Rate24039990002, tradeGroupFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffDTA_STD_Rate24039990002 = helper.CreateRate(tariff24039990002, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.5*VFD", preferenceSTD.PK, "0.5", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffDTA_STD_Rate24039990002, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(tariffDTA_STD_Rate24039990002, tradeGroupFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffTOBACCO = helper.CreateTariff("TW", tariffTypeTT.PK, "TOBACCO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "菸絲");
			var tariffHWSRateTOBACCO = helper.CreateRate(tariffTOBACCO, rateCodeHWS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "1000 * [KGM]", null, "1000/KGM", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffHWSRateTOBACCO, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffTATRateTOBACCO = helper.CreateRate(tariffTOBACCO, rateCodeTAT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "1590 * [KGM]", null, "1590/KGM", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffTATRateTOBACCO, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2403.99.90.00-2";
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_Procedure = "71";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "PCE";
			invoiceLine.JI_EnteredUnitPrice = 10000m;
			invoiceLine.JI_NetWeight = 0m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 500m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = 500m;
			invoiceLine.JI_CustomsSecondUnitQty = "KGM";
			Factory.Save();

			var lineTax = invoiceLine.Taxes.AddNew();
			lineTax.JLT_Type = "TT";
			lineTax.JLT_Tariff = "TOBACCO";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "HWS").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "TAT").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));

			lineTax.JLT_MethodOfPayment = "CAS";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "HWS").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "TAT").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetImportDutyPaymentMethodFromTW_DtyPymntMthd()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			#region TW TradeGroup
			var tradeGroupAll = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "All Countries");
			var tradeGroupWTO = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "WTO Countries (Column I)");
			var tradeGroupFTA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "FTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "FTA Countries (Column I)");
			var tradeGroupSPE = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SPE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Special Countries (Column II)");
			var tradeGroupLDC = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "LDC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Least Developed Countries");
			var tradeGroupPA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Panama");
			var tradeGroupSZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Eswatini");
			var tradeGroupSG = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Singapore");
			var tradeGroupCN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "China");
			var tradeGroupHN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "HN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Honduras");
			var tradeGroupSV = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SV", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "El Salvador");
			var tradeGroupNI = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nicaragua");
			var tradeGroupPY = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Paraguay");
			var tradeGroupNZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "New Zealand");
			var tradeGroupGT = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "GT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Guatemala");

			helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			#endregion

			#region TaxOrFee
			var taxOrFeeTPF = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, description: "推廣貿易服務費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeTPF.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			taxOrFeeTPF.ZZF_Threshold = 100m;

			var taxOrFeeDDF = helper.CreateTaxOrFee("DDF", 200m, Core.Constants.CountryCodes.Taiwan, description: "滯報費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeDDF.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			#endregion

			#region CusRateType
			var refCusRateTypeSSG = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG", description: "Specifically Selected Goods or Services Tax");
			refCusRateTypeSSG.ZZR_IsPayable = true;
			refCusRateTypeSSG.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD + VAT + CTA + CTS + TAT + HWS";

			var refCusRateTypeCOM = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM", description: "Commodity Taxes");
			refCusRateTypeCOM.ZZR_IsPayable = true;
			refCusRateTypeCOM.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD";

			var refCusRateTypeDTY = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY", description: "Duty");
			refCusRateTypeDTY.ZZR_IsPayable = true;
			refCusRateTypeDTY.ZZR_CustomsValueFormula = "CV";

			factory.Save();
			#endregion

			#region CusRateCode
			var rateCodeSSG = helper.LoadOrCreateNewCusRateCode(factory, "SSG", refCusRateTypeSSG.PK, description: "特種貨物及勞務稅");

			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(factory, "CTA", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "CTS", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "HWS", refCusRateTypeCOM.PK, description: "健康福利捐");
			helper.LoadOrCreateNewCusRateCode(factory, "TAT", refCusRateTypeCOM.PK, description: "菸酒稅");

			helper.LoadOrCreateNewCusRateCode(factory, "DTS", refCusRateTypeDTY.PK, description: "進口稅");
			var rateCodeDTA = helper.LoadOrCreateNewCusRateCode(factory, "DTA", refCusRateTypeDTY.PK, description: "進口稅");

			factory.Save();
			#endregion

			#region TariffType
			var tariffTypeTT = helper.CreateNewOrGetExistingTariffType("TW", "TT");
			tariffTypeTT.ZZI_Description = "Tobacco Tax";

			var tariffTypeSS = helper.CreateNewOrGetExistingTariffType("TW", "SS");
			tariffTypeSS.ZZI_Description = "Specifically Selected Goods and Services Tax";

			var tariffTypeCT = helper.CreateNewOrGetExistingTariffType("TW", "CT");
			tariffTypeCT.ZZI_Description = "Commodity Tax";

			var tariffTypeAT = helper.CreateNewOrGetExistingTariffType("TW", "AT");
			tariffTypeAT.ZZI_Description = "Alcohol Tax";

			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType("TW", "HSN", nomenclatureGroupType: "TW");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			factory.Save();
			#endregion

			#region Preference For Taiwan
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", "TW");
			var preferencePR2 = helper.CreatePreferenceForCountry("PR2", "Column II Rates(FTA/LDC)", "TW");
			var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Column III Rates(Standard Rates)", "TW");
			factory.Save();
			#endregion

			var cusProcedure62 = helper.CreateRefCusProcedure("TW", "IM", "62", "", "", "估價未決", "IMP", true);
			cusProcedure62.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("SSGPaymentMethod", "CAS");
			cusProcedure62.Attributes.AddNew("TATPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("VATPaymentMethod", "DEF");

			var tariff87034000319 = helper.CreateTariff("TW", tariffTypeHSN.PK, "87034000319", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, compositeKey: "17.87..03.4.10.60", description: "Sedan (including convertible, sports) and station wagons, of a cylinder capacity exceeding 1,500 c.c. but not exceeding 3,000 c.c.");
			var tariffDTARate87034000319 = helper.CreateRate(tariff87034000319, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.175*VFD", preferencePR1.PK, "0.175", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffDTARate87034000319, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(tariffDTARate87034000319, tradeGroupFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffPASSENGERCAR = helper.CreateTariff("TW", tariffTypeSS.PK, "PASSENGERCAR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "小客車");
			var tariffSSGRatePASSENGERCAR = helper.CreateRate(tariffPASSENGERCAR, rateCodeSSG.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1 * VFD", preferencePR1.PK, "0.1", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffSSGRatePASSENGERCAR, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffSedanAbove2000CC = helper.CreateTariff("TW", tariffTypeCT.PK, "SedanAbove2000CC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "小客車(汽缸排氣量在二千零一立方公分以上) - 包括駕駛人座位在內，座位在九座以下之載人汽車");
			var tariffSSGRateSedanAbove2000CC = helper.CreateRate(tariffSedanAbove2000CC, rateCodeCTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3 * VFD", preferencePR1.PK, "0.3", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffSSGRateSedanAbove2000CC, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 2948836m;

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18436.00m, Core.Constants.CurrencyCodes.Taiwan);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 5441.00m, Core.Constants.CurrencyCodes.Taiwan);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8703.40.00.31-9";
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_CountryOfOrigin = "GB";
			invoiceLine.JI_Procedure = "62";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;
			invoiceLine.JI_NetWeight = 2665m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 2665m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_CustomsSecondUnitQty = "NIU";
			var lineTax1 = invoiceLine.Taxes.AddNew();
			lineTax1.JLT_Type = "SS";
			lineTax1.JLT_Tariff = "PASSENGERCAR";

			var lineTax2 = invoiceLine.Taxes.AddNew();
			lineTax2.JLT_Type = "CT";
			lineTax2.JLT_Tariff = "SEDANABOVE2000CC";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "DTA").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_DtyPymntMthd = "CAS";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "DTA").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_DtyPymntMthd = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "DTA"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

			invoiceLine.JI_DtyPymntMthd = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "DTA"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
		}

		[ExpectNoExceptions]
		public void TestGetImportVATPaymentMethodFromTW_VatPymntMthd()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			#region TaxOrFee
			var taxOrFeeTPF = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, description: "推廣貿易服務費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeTPF.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			taxOrFeeTPF.ZZF_Threshold = 100m;

			var taxOrFeeDDF = helper.CreateTaxOrFee("DDF", 200m, Core.Constants.CountryCodes.Taiwan, description: "滯報費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeDDF.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			factory.Save();
			#endregion

			var cusProcedure71 = helper.CreateRefCusProcedure("TW", "IM", "71", "", "", "國貨待復運出口", "IMP", true);
			cusProcedure71.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("SSGPaymentMethod", "CAS");
			cusProcedure71.Attributes.AddNew("TATPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("VATPaymentMethod", "DEF");
			factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 2948836m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "71";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "VAT").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_VatPymntMthd = "CAS";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "VAT").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_VatPymntMthd = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "VAT"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

			invoiceLine.JI_VatPymntMthd = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "VAT"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
		}

		[ExpectNoExceptions]
		public void TestGetImportTPFPaymentMethodFromTW_TpfPymntMthd()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			#region TaxOrFee
			var taxOrFeeTPF = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, description: "推廣貿易服務費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeTPF.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			taxOrFeeTPF.ZZF_Threshold = 100m;

			var taxOrFeeDDF = helper.CreateTaxOrFee("DDF", 200m, Core.Constants.CountryCodes.Taiwan, description: "滯報費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeDDF.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			factory.Save();
			#endregion

			var cusProcedure71 = helper.CreateRefCusProcedure("TW", "IM", "71", "", "", "國貨待復運出口", "IMP", true);
			cusProcedure71.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("SSGPaymentMethod", "CAS");
			cusProcedure71.Attributes.AddNew("TATPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedure71.Attributes.AddNew("VATPaymentMethod", "DEF");
			factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 2948836m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "71";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "TPF").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "TPF").CF_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_TpfPymntMthd = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

			invoiceLine.JI_TpfPymntMthd = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
		}

		[ExpectNoExceptions]
		public void TestCalculateAndSetBusinessTaxBase()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			#region TW TradeGroup
			var tradeGroupAll = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "All Countries");
			var tradeGroupWTO = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "WTO Countries (Column I)");
			var tradeGroupFTA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "FTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "FTA Countries (Column I)");
			var tradeGroupSPE = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SPE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Special Countries (Column II)");
			var tradeGroupLDC = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "LDC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Least Developed Countries");
			var tradeGroupPA = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Panama");
			var tradeGroupSZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Eswatini");
			var tradeGroupSG = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Singapore");
			var tradeGroupCN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "China");
			var tradeGroupHN = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "HN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Honduras");
			var tradeGroupSV = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "SV", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "El Salvador");
			var tradeGroupNI = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nicaragua");
			var tradeGroupPY = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "PY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Paraguay");
			var tradeGroupNZ = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "NZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "New Zealand");
			var tradeGroupGT = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "GT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Guatemala");

			helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			#endregion

			#region TaxOrFee
			var taxOrFeeTPF = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, description: "推廣貿易服務費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeTPF.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			taxOrFeeTPF.ZZF_Threshold = 100m;

			var taxOrFeeDDF = helper.CreateTaxOrFee("DDF", 200m, Core.Constants.CountryCodes.Taiwan, description: "滯報費", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeDDF.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			#endregion

			#region CusRateType
			var refCusRateTypeSSG = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG", description: "Specifically Selected Goods or Services Tax");
			refCusRateTypeSSG.ZZR_IsPayable = true;
			refCusRateTypeSSG.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD + VAT + CTA + CTS + TAT + HWS";

			var refCusRateTypeCOM = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM", description: "Commodity Taxes");
			refCusRateTypeCOM.ZZR_IsPayable = true;
			refCusRateTypeCOM.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD";

			var refCusRateTypeDTY = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY", description: "Duty");
			refCusRateTypeDTY.ZZR_IsPayable = true;
			refCusRateTypeDTY.ZZR_CustomsValueFormula = "CV";

			factory.Save();
			#endregion

			#region CusRateCode
			var rateCodeSSG = helper.LoadOrCreateNewCusRateCode(factory, "SSG", refCusRateTypeSSG.PK, description: "特種貨物及勞務稅");

			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(factory, "CTA", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "CTS", refCusRateTypeCOM.PK, description: "貨物稅");
			helper.LoadOrCreateNewCusRateCode(factory, "HWS", refCusRateTypeCOM.PK, description: "健康福利捐");
			helper.LoadOrCreateNewCusRateCode(factory, "TAT", refCusRateTypeCOM.PK, description: "菸酒稅");

			helper.LoadOrCreateNewCusRateCode(factory, "DTS", refCusRateTypeDTY.PK, description: "進口稅");
			var rateCodeDTA = helper.LoadOrCreateNewCusRateCode(factory, "DTA", refCusRateTypeDTY.PK, description: "進口稅");

			factory.Save();
			#endregion

			#region TariffType
			var tariffTypeTT = helper.CreateNewOrGetExistingTariffType("TW", "TT");
			tariffTypeTT.ZZI_Description = "Tobacco Tax";

			var tariffTypeSS = helper.CreateNewOrGetExistingTariffType("TW", "SS");
			tariffTypeSS.ZZI_Description = "Specifically Selected Goods and Services Tax";

			var tariffTypeCT = helper.CreateNewOrGetExistingTariffType("TW", "CT");
			tariffTypeCT.ZZI_Description = "Commodity Tax";

			var tariffTypeAT = helper.CreateNewOrGetExistingTariffType("TW", "AT");
			tariffTypeAT.ZZI_Description = "Alcohol Tax";

			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType("TW", "HSN", nomenclatureGroupType: "TW");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			factory.Save();
			#endregion

			#region Preference For Taiwan
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", "TW");
			var preferencePR2 = helper.CreatePreferenceForCountry("PR2", "Column II Rates(FTA/LDC)", "TW");
			var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Column III Rates(Standard Rates)", "TW");
			factory.Save();
			#endregion

			var cusProcedure62 = helper.CreateRefCusProcedure("TW", "IM", "62", "", "", "估價未決", "IMP", true);
			cusProcedure62.Attributes.AddNew("COMPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("DTYPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("SSGPaymentMethod", "CAS");
			cusProcedure62.Attributes.AddNew("TATPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("TPFPaymentMethod", "DEF");
			cusProcedure62.Attributes.AddNew("VATPaymentMethod", "DEF");

			var tariff87034000319 = helper.CreateTariff("TW", tariffTypeHSN.PK, "87034000319", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, compositeKey: "17.87..03.4.10.60", description: "Sedan (including convertible, sports) and station wagons, of a cylinder capacity exceeding 1,500 c.c. but not exceeding 3,000 c.c.");
			var tariffDTARate87034000319 = helper.CreateRate(tariff87034000319, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.175*VFD", preferencePR1.PK, "0.175", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffDTARate87034000319, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(tariffDTARate87034000319, tradeGroupFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffPASSENGERCAR = helper.CreateTariff("TW", tariffTypeSS.PK, "PASSENGERCAR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "小客車");
			var tariffSSGRatePASSENGERCAR = helper.CreateRate(tariffPASSENGERCAR, rateCodeSSG.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1 * VFD", null, "0.1", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffSSGRatePASSENGERCAR, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffSedanAbove2000CC = helper.CreateTariff("TW", tariffTypeCT.PK, "SedanAbove2000CC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "小客車(汽缸排氣量在二千零一立方公分以上) - 包括駕駛人座位在內，座位在九座以下之載人汽車");
			var tariffSSGRateSedanAbove2000CC = helper.CreateRate(tariffSedanAbove2000CC, rateCodeCTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3 * VFD", null, "0.3", dataGrouping: "TW");
			helper.CreateCusApplicability(tariffSSGRateSedanAbove2000CC, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 2948836m;

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18436.00m, Core.Constants.CurrencyCodes.Taiwan);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 5441.00m, Core.Constants.CurrencyCodes.Taiwan);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8703.40.00.31-9";
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_CountryOfOrigin = "GB";
			invoiceLine.JI_Procedure = "62";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;
			invoiceLine.JI_NetWeight = 2665m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 2665m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_CustomsSecondUnitQty = "NIU";
			invoiceLine.JI_VatPymntMthd = "CAS";
			invoiceLine.JI_DtyPymntMthd = "CAS";
			var lineTax1 = invoiceLine.Taxes.AddNew();
			lineTax1.JLT_Type = "SS";
			lineTax1.JLT_Tariff = "PASSENGERCAR";
			lineTax1.JLT_MethodOfPayment = "CAS";

			var lineTax2 = invoiceLine.Taxes.AddNew();
			lineTax2.JLT_Type = "CT";
			lineTax2.JLT_Tariff = "SEDANABOVE2000CC";
			lineTax2.JLT_MethodOfPayment = "CAS";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();
			NUnit.Framework.Assert.That(entryLineFees.Where(x => x.CF_ChargeType == "DTA" || x.CF_ChargeType == "CTA").Sum(f => f.CF_ChargeAmount), NUnit.Framework.Is.EqualTo(516046.3m + 1039464.69m));
			NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(4504346.99m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(4504346m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetLineNumberAssigner()
		{
			CreateInvoiceLinesForMerging("tariff1", "PART1", "description1", "B", 1);
			CreateInvoiceLinesForMerging("tariff1", "PART1", "description1", "", 1);
			CreateInvoiceLinesForMerging("tariff1", "PART1", "description1", "A", 1);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge();
			var mergedLines = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>();
			var line1 = mergedLines.First(line => line.CL_Grouping == "A");
			var line2 = mergedLines.First(line => line.CL_Grouping == "B");
			var line3 = mergedLines.First(line => line.CL_Grouping == "");

			NUnit.Framework.Assert.That(line1.CL_LineNumber, NUnit.Framework.Is.EqualTo((ZShort)3));
			NUnit.Framework.Assert.That(line2.CL_LineNumber, NUnit.Framework.Is.EqualTo((ZShort)1));
			NUnit.Framework.Assert.That(line3.CL_LineNumber, NUnit.Framework.Is.EqualTo((ZShort)2));
		}

		[ExpectNoExceptions]
		public void TestCL_Description()
		{
			declaration.JE_MergeBy = MergeByCodeList.Codes.CondensedDeclaration;
			CreateInvoiceLinesForMerging("tariff1", "PART1", "description1", "group1", 2);

			declaration.DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (CusEntryLine mergedLine in declaration.CustomsEntryHeaders[0].MergedLines)
			{
				NUnit.Framework.Assert.That(mergedLine.CL_Description, NUnit.Framework.Is.EqualTo("SUMMAR1, INCLUDING ITEMS 1-2").Using(CustomComparers.TypeComparison));
			}

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (CusEntryLine mergedLine in declaration.CustomsEntryHeaders[0].MergedLines)
			{
				NUnit.Framework.Assert.That(mergedLine.CL_Description, NUnit.Framework.Is.EqualTo(ZString.Empty));
			}
		}

		[ExpectNoExceptions]
		public void TestDoMerge()
		{
			CreateInvoiceLinesForMerging("tariff1", "PART1", "description1", "group1", 2);
			CreateInvoiceLinesForMerging("tariff2", "PART1", "description1", "group2", 2);
			CreateInvoiceLinesForMerging("tariff3", "PART2", "description1", "group1", 2);
			CreateInvoiceLinesForMerging("tariff3", "PART2", "description2", "group1", 2);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Count, NUnit.Framework.Is.EqualTo(8));

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Count, NUnit.Framework.Is.EqualTo(3));

			declaration.JE_MergeBy = MergeByCodeList.Codes.CondensedDeclaration;
			declaration.DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Count, NUnit.Framework.Is.EqualTo(3));

			int tariff = 0;
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.JI_Tariff = (tariff++).ToString());
			declaration.DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Count, NUnit.Framework.Is.EqualTo(8));

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.JI_Tariff = ZString.Empty);
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.JI_Description = ZString.Empty);
			declaration.DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Count, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestSummarLineItems()
		{
			var lineNos = new List<int> { 1, 2, 3, 4, 7, 8, 10, 12, 13 };
			NUnit.Framework.Assert.That(LineMerger.CalSequenceNumGenerator.SummarLineItems(lineNos, 1), NUnit.Framework.Is.EqualTo("SUMMAR1, INCLUDING ITEMS 1-4,7-8,10,12-13").Using(CustomComparers.TypeComparison));

			lineNos = new List<int> { 1, 2, 3, 4, 7, 8, 10, 12 };
			NUnit.Framework.Assert.That(LineMerger.CalSequenceNumGenerator.SummarLineItems(lineNos, 1), NUnit.Framework.Is.EqualTo("SUMMAR1, INCLUDING ITEMS 1-4,7-8,10,12").Using(CustomComparers.TypeComparison));

			lineNos = new List<int> { 12, 1, 2, 8, 3, 4, 7, 10 };
			NUnit.Framework.Assert.That(LineMerger.CalSequenceNumGenerator.SummarLineItems(lineNos, 2), NUnit.Framework.Is.EqualTo("SUMMAR2, INCLUDING ITEMS 1-4,7-8,10,12").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCalSequenceNumGeneratorSort()
		{
			var inv1 = declaration.Invoices.AddNew();
			inv1.JZ_InvoiceNumber = "INV001";
			var inv2 = declaration.Invoices.AddNew();
			inv2.JZ_InvoiceNumber = "INV009";
			var inv3 = declaration.Invoices.AddNew();
			inv3.JZ_InvoiceNumber = "INV011";
			var inv4 = declaration.Invoices.AddNew();
			inv4.JZ_InvoiceNumber = "INV019";

			CreateInvoiceLinesForInvoiceHeader(inv4, 10, 20);
			CreateInvoiceLinesForInvoiceHeader(inv4, 1, 10);

			CreateInvoiceLinesForInvoiceHeader(inv1, 10, 20);
			CreateInvoiceLinesForInvoiceHeader(inv1, 1, 10);

			CreateInvoiceLinesForInvoiceHeader(inv2, 10, 20);
			CreateInvoiceLinesForInvoiceHeader(inv2, 1, 10);

			CreateInvoiceLinesForInvoiceHeader(inv3, 10, 20);
			CreateInvoiceLinesForInvoiceHeader(inv3, 1, 10);

			var invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>();
			var mapping = LineMerger.CalSequenceNumGenerator.Sort(invoiceLines);
			var list = LineMerger.CalSequenceNumGenerator.SortToList(invoiceLines);

			var sortInvoiceLine = invoiceLines.First(x => x.PK == mapping.First().Key);
			NUnit.Framework.Assert.That(list.First().InvoiceHeader.JZ_InvoiceNumber, NUnit.Framework.Is.EqualTo(sortInvoiceLine.InvoiceHeader.JZ_InvoiceNumber));
			NUnit.Framework.Assert.That(sortInvoiceLine.InvoiceHeader.JZ_InvoiceNumber, NUnit.Framework.Is.EqualTo("INV001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(sortInvoiceLine.JI_LineNo, NUnit.Framework.Is.EqualTo(new ZShort(1)));

			sortInvoiceLine = invoiceLines.First(x => x.PK == mapping.Last().Key);
			NUnit.Framework.Assert.That(list.Last().InvoiceHeader.JZ_InvoiceNumber, NUnit.Framework.Is.EqualTo(sortInvoiceLine.InvoiceHeader.JZ_InvoiceNumber));
			NUnit.Framework.Assert.That(sortInvoiceLine.InvoiceHeader.JZ_InvoiceNumber, NUnit.Framework.Is.EqualTo("INV019").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(sortInvoiceLine.JI_LineNo, NUnit.Framework.Is.EqualTo(new ZShort(30)));
		}

		void CreateInvoiceLinesForInvoiceHeader(JobComInvoiceHeader invoiceheader, ZShort startIndex, ZShort numberOfLines)
		{
			var endIndex = startIndex + numberOfLines;
			for (var i = startIndex; i < endIndex; i++)
			{
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_JZ = invoiceheader.PK;
				invoiceLine.JI_LineNo = i;
			}
		}

		void CreateInvoiceLinesForMerging(ZString tariff, ZString partNo, ZString description, ZString grouping, int numberOfLines)
		{
			for (int i = 0; i < numberOfLines; i++)
			{
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_JZ = invoiceHeader.PK;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Tariff = tariff;
				invoiceLine.JI_PartNo = partNo;
				invoiceLine.JI_Description = ZString.Empty;
				invoiceLine.JI_Group = grouping;
			}
		}

		[ExpectNoExceptions]
		public void TestImportDutyCalculation_ForImportProcedure()
		{
			SetupUniversalReferenceData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			invoiceHeader.JZ_InvoiceAmount = 1000m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_ConcessionOrder = "";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtaCharge = invoiceHeader.Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 300M);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.3M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_BaseValue, NUnit.Framework.Is.EqualTo(1000M).Using(CustomComparers.TypeComparison));
			});
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();

			invoiceLine.JI_DtyPymntMthdInfo.ClearValue();
			invoiceLine.JI_VatPymntMthdInfo.ClearValue();
			invoiceLine.JI_TpfPymntMthdInfo.ClearValue();
			invoiceLine.JI_Tariff = "03035400900";
			invoiceLine.JI_Procedure = "TT";
			invoiceLine.JI_PrimaryPreference = "PRE";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_ConcessionOrder = "";
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "A10"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "B40"), NUnit.Framework.Is.True);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtsCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTS");
				NUnit.Framework.Assert.That(dtsCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTS" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 3600m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(3.6M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_BaseValue, NUnit.Framework.Is.EqualTo(1000M).Using(CustomComparers.TypeComparison));
			});
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();

			var newInvoiceLine = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			newInvoiceLine.JI_CEI = entryInstruction.PK;
			newInvoiceLine.JI_Procedure = "TT";
			newInvoiceLine.JI_Tariff = "03035400900";
			newInvoiceLine.JI_CountryOfOrigin = "AU";
			newInvoiceLine.JI_PrimaryPreference = "PRE";
			newInvoiceLine.JI_InvoiceQuantity = 1;
			newInvoiceLine.JI_EnteredUnitPrice = 500m;
			newInvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			newInvoiceLine.JI_ConcessionOrder = "";
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "A10"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "B40"), NUnit.Framework.Is.True);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtsCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTS");
				NUnit.Framework.Assert.That(dtsCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTS" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 3600m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(3.6M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_BaseValue, NUnit.Framework.Is.EqualTo(1000M).Using(CustomComparers.TypeComparison));

				fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTS" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 1800m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(3.6M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_BaseValue, NUnit.Framework.Is.EqualTo(500M).Using(CustomComparers.TypeComparison));
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();

			newInvoiceLine.JI_DtyPymntMthdInfo.ClearValue();
			newInvoiceLine.JI_VatPymntMthdInfo.ClearValue();
			newInvoiceLine.JI_TpfPymntMthdInfo.ClearValue();
			newInvoiceLine.JI_Procedure = "PR";
			newInvoiceLine.JI_Tariff = "87031000002";
			newInvoiceLine.JI_CountryOfOrigin = "AU";
			newInvoiceLine.JI_PrimaryPreference = "STD";
			newInvoiceLine.JI_InvoiceQuantity = 1;
			newInvoiceLine.JI_EnteredUnitPrice = 1000m;
			newInvoiceLine.JI_ConcessionOrder = "";
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "A10"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "B40"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "A19"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "B49"), NUnit.Framework.Is.True);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));

				var dtaRate = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				var dtsRate = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTS");

				NUnit.Framework.Assert.That(dtaRate, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));
				NUnit.Framework.Assert.That(dtsRate, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 300m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.3M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_BaseValue, NUnit.Framework.Is.EqualTo(1000M).Using(CustomComparers.TypeComparison));

				fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTS" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 3600m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(3.6M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_BaseValue, NUnit.Framework.Is.EqualTo(1000M).Using(CustomComparers.TypeComparison));
			});

			CombineAssertions(() =>
			{
				newInvoiceLine.JI_CusValueConvRatio = 0.2;
				new LineMerger(declaration).DoMerge();
				var dtaRate = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaRate, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_ChargeAmount == 60m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});
		}

		void SetupUniversalReferenceData()
		{
			TariffDataForTestHelper.NewData(Factory);
		}

		[ExpectNoExceptions]
		public void TestDutyCalculation_NotCalculatedForExportAndRefCusProcedureNoDutyForImportExport()
		{
			SetupUniversalReferenceData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "AA";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});

			invoiceLine.JI_Procedure = "AB";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});

			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_Procedure = "BB";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});
		}

		[ExpectNoExceptions]
		public void TestBusinessTaxCalculation()
		{
			SetupUniversalReferenceData();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);
			entryInstruction.CEI_WaiverOfExemption = true;

			invoiceHeader.JZ_InvoiceAmount = 1000m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_VatPymntMthd = "CAS";
			invoiceLine.JI_DtyPymntMthd = "CAS";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				var entryLine = invoiceLine.CusEntryLine;
				var entryHeader = (CusEntryHeader)entryInstruction.EntryHeader;
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(1300m).Using(CustomComparers.TypeComparison), "Entry line business tax base should be");
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(1300m).Using(CustomComparers.TypeComparison), "Entry header business tax base should be");
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			var newInvoiceLine = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			newInvoiceLine.JI_CEI = entryInstruction.PK;
			newInvoiceLine.JI_Procedure = "TT";
			newInvoiceLine.JI_Tariff = "03035400900";
			newInvoiceLine.JI_CountryOfOrigin = "AU";
			newInvoiceLine.JI_PrimaryPreference = "PRE";
			newInvoiceLine.JI_InvoiceQuantity = 1;
			newInvoiceLine.JI_EnteredUnitPrice = 500m;
			newInvoiceLine.JI_ConcessionOrder = "";
			newInvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			newInvoiceLine.JI_DtyPymntMthd = "CAS";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				var entryLine = invoiceLine.CusEntryLine;
				var entryLine1 = newInvoiceLine.CusEntryLine;
				var entryHeader = (CusEntryHeader)entryInstruction.EntryHeader;
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(1300m).Using(CustomComparers.TypeComparison), "Entry line business tax base should be");
				NUnit.Framework.Assert.That(entryLine1.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2300m).Using(CustomComparers.TypeComparison), "Entry line 1 business tax base should be");
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(3600m).Using(CustomComparers.TypeComparison), "Entry header business tax base should be");
			});

			entryInstruction.CEI_WaiverOfExemption = false;
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				var entryLine = invoiceLine.CusEntryLine;
				var entryLine1 = newInvoiceLine.CusEntryLine;
				var entryHeader = (CusEntryHeader)entryInstruction.EntryHeader;
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(ZDecimal.Zero), "Entry line business tax base should be");
				NUnit.Framework.Assert.That(entryLine1.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(ZDecimal.Zero), "Entry line 1 business tax base should be");
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero), "Entry header business tax base should be");
			});

			entryInstruction.CEI_WaiverOfExemption = true;
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			invoiceLine.JI_Procedure = "BB";
			invoiceLine.JI_DtyPymntMthd = "DEF";
			invoiceLine.JI_VatPymntMthd = "DEF";
			invoiceLine.JI_TpfPymntMthd = "DEF";
			newInvoiceLine.JI_Procedure = "BB";
			newInvoiceLine.JI_DtyPymntMthd = "CAS";
			newInvoiceLine.JI_VatPymntMthd = "CAS";
			newInvoiceLine.JI_TpfPymntMthd = "CAS";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				var entryLine = invoiceLine.CusEntryLine;
				var entryLine1 = newInvoiceLine.CusEntryLine;
				var entryHeader = (CusEntryHeader)entryInstruction.EntryHeader;
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(1300m).Using(CustomComparers.TypeComparison), "Entry line business tax base should be");
				NUnit.Framework.Assert.That(entryLine1.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2300m).Using(CustomComparers.TypeComparison), "Entry line 1 business tax base should be");
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(2300m).Using(CustomComparers.TypeComparison), "Entry header business tax base should be");
			});

			entryInstruction.CEI_WaiverOfExemption = false;
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			invoiceLine.JI_Procedure = "PR";
			newInvoiceLine.JI_Procedure = "TT";
			var newInvoiceLine2 = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			newInvoiceLine2.JI_CEI = entryInstruction.PK;
			newInvoiceLine2.JI_Procedure = "TT";
			newInvoiceLine2.JI_Tariff = "210390901";
			newInvoiceLine2.JI_CountryOfOrigin = "AU";
			newInvoiceLine2.JI_PrimaryPreference = "PRE";
			newInvoiceLine2.JI_InvoiceQuantity = 1;
			newInvoiceLine2.JI_EnteredUnitPrice = 200m;
			newInvoiceLine2.JI_ConcessionOrder = "";
			newInvoiceLine2.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				var entryLine = invoiceLine.CusEntryLine;
				var entryLine1 = newInvoiceLine.CusEntryLine;
				var entryLine2 = newInvoiceLine2.CusEntryLine;
				var entryHeader = (CusEntryHeader)entryInstruction.EntryHeader;
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(1300m).Using(CustomComparers.TypeComparison), "Entry line business tax base should be");
				NUnit.Framework.Assert.That(entryLine1.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2300m).Using(CustomComparers.TypeComparison), "Entry line 1 business tax base should be");
				NUnit.Framework.Assert.That(entryLine2.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(200m).Using(CustomComparers.TypeComparison), "Entry line 2 business tax base should be");
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(2500m).Using(CustomComparers.TypeComparison), "Entry header business tax base should be");
			});
		}

		[ExpectNoExceptions]
		public void TestVATCalculation()
		{
			SetupUniversalReferenceData();
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			NUnit.Framework.Assert.That(invoiceLine.JI_VatPymntMthd, NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_ConcessionOrder = "";
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			CombineAssertions(() =>
			{
				var vatCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "VAT" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(vatCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 286m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.22M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});

			invoiceLine.JI_VatPymntMthd = "CAS";
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			CombineAssertions(() =>
			{
				var vatCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "VAT" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(vatCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.22M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(286m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			var newInvoiceLine = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			newInvoiceLine.JI_CEI = entryInstruction.PK;
			newInvoiceLine.JI_Procedure = "TT";
			NUnit.Framework.Assert.That(newInvoiceLine.JI_VatPymntMthd, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

			newInvoiceLine.JI_Tariff = "03035400900";
			newInvoiceLine.JI_CountryOfOrigin = "AU";
			newInvoiceLine.JI_PrimaryPreference = "PRE";
			newInvoiceLine.JI_InvoiceQuantity = 1;
			newInvoiceLine.JI_EnteredUnitPrice = 500;
			newInvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			newInvoiceLine.JI_ConcessionOrder = "";
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(3));
			CombineAssertions(() =>
			{
				var vatChargeDEF = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "VAT" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(vatChargeDEF, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 286m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.22M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));

				var vatChargeCAS = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "VAT" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(vatChargeCAS, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 506m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.22M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();

			newInvoiceLine.JI_VatPymntMthd = "DEF";
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(4));
			CombineAssertions(() =>
			{
				var vatChargeDEF = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "VAT" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(vatChargeDEF, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 286m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.22M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));

				var vatChargeCAS = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "VAT" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(vatChargeCAS, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "VAT" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 110m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.22M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestTPFCalculation()
		{
			SetupUniversalReferenceData();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1002485m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1002485m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_TpfPymntMthd = ZString.Empty;
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			CombineAssertions(() =>
			{
				var entry = declaration.CustomsEntryHeaders[0];
				var tpfCharge = entry.Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "TPF" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(tpfCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));
				var fee = entry.MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 400.994m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				NUnit.Framework.Assert.That(!entry.MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});

			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(3));
			CombineAssertions(() =>
			{
				var tpfCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "TPF" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(tpfCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 400.994m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.0004M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});

			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(3));
			CombineAssertions(() =>
			{
				var tpfCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "TPF" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(tpfCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 400.994m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.0004M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			var invoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "TT";
			NUnit.Framework.Assert.That(invoiceLine2.JI_TpfPymntMthd, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

			invoiceLine2.JI_Tariff = "03035400900";
			invoiceLine2.JI_CountryOfOrigin = "AU";
			invoiceLine2.JI_PrimaryPreference = "PRE";
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_EnteredUnitPrice = 502488m;
			invoiceLine2.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine2.JI_ConcessionOrder = "";
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(5));
			CombineAssertions(() =>
			{
				var tpfChargeDEF = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "TPF" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(tpfChargeDEF, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var tpfChargeCAS = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "TPF" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(tpfChargeCAS, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var ddd = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>());
				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 400.994m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.0004M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));

				fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 200.995m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.0004M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});

			invoiceLine2.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(6));
			CombineAssertions(() =>
			{
				var tpfChargeDEF = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "TPF" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(tpfChargeDEF, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var tpfChargeCAS = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "TPF" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(tpfChargeCAS, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 200.995m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.0004M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));

				fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "TPF" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 400.994m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.0004M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestTPFCalculationWhenTW_TpfPymntMthdIsEmpty()
		{
			SetupUniversalReferenceData();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1002485m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_EnteredUnitPrice = 1002485m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_TpfPymntMthd = ZString.Empty;
			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			var invoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "TT";
			invoiceLine2.JI_Tariff = "03035400900";
			invoiceLine2.JI_CountryOfOrigin = "AU";
			invoiceLine2.JI_PrimaryPreference = "PRE";
			invoiceLine2.JI_LinePrice = 502488m;
			invoiceLine2.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine2.JI_ConcessionOrder = "";
			invoiceLine2.JI_TpfPymntMthd = ZString.Empty;
			invoiceLine2.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			invoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.NB;
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			invoiceLine.JI_DtyPymntMthdInfo.ClearValue();
			invoiceLine.JI_Procedure = "QQ";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			invoiceLine.JI_DtyPymntMthdInfo.ClearValue();
			invoiceLine.JI_Procedure = "KK";
			invoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.CN;
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
			invoiceLine.JI_TpfPymntMthd = ZString.Empty;
			invoiceLine2.JI_DtyPymntMthdInfo.ClearValue();
			invoiceLine2.JI_Procedure = "QQ";
			invoiceLine2.JI_TpfPymntMthd = ZString.Empty;
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			invoiceLine.JI_PreviousEntryNumber = "X2";
			invoiceLine.JI_LinePrice = 9000;
			invoiceLine2.JI_LinePrice = 8000m;
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			invoiceLine.PreviousBondedEntryNumber = "X2";
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});

			invoiceLine.JI_LinePrice = 999999m;
			invoiceLine.JI_DtyPymntMthdInfo.ClearValue();
			invoiceLine.JI_Procedure = "BB";
			declaration.Invoices[0].InvoiceLines.RemoveAndDelete(invoiceLine2);
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Any(x => x.CF_ChargeType == "TPF"), NUnit.Framework.Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestCommodityTaxCalculation()
		{
			SetupUniversalReferenceData();
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1000m;
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = "PR";
			invoiceLine1.JI_Tariff = "87031000003";
			invoiceLine1.JI_CountryOfOrigin = "AU";
			invoiceLine1.JI_PrimaryPreference = "STD";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_EnteredUnitPrice = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "KLT";
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine1.JI_ConcessionOrder = "";

			var invoiceLine2 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "PR";
			invoiceLine2.JI_Tariff = "87031000003";
			invoiceLine2.JI_CountryOfOrigin = "AU";
			invoiceLine2.JI_PrimaryPreference = "STD";
			invoiceLine2.JI_CustomsUnitQty = "LTR";
			invoiceLine2.JI_CustomsQuantity = 1000;
			invoiceLine2.JI_AlcoholPercentage = 0.6;
			invoiceLine2.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine2.JI_ConcessionOrder = "";

			var invoiceLine3 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Procedure = "PR";
			invoiceLine3.JI_Tariff = "87031000003";
			invoiceLine3.JI_CountryOfOrigin = "AU";
			invoiceLine3.JI_PrimaryPreference = "STD";
			invoiceLine3.JI_CustomsUnitQty = "KGM";
			invoiceLine3.JI_CustomsQuantity = 15;
			invoiceLine3.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine3.JI_ConcessionOrder = "";

			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(5));
			CombineAssertions(() =>
			{
				var ctsCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "CTS" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(ctsCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var tatCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "TAT" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(tatCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var hwsCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "HWS" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(hwsCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var list = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).ToList();
				var fee = list.SingleOrDefault(x => x.CF_ChargeType == "CTS" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 6100m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "CTS" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 610m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "TAT" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 4200m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "HWS" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 15000m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});

			invoiceLine1.JI_CustomsQuantity = 0.1m;
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(5));
			CombineAssertions(() =>
			{
				var ctsCharges = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().Where(x => x.C1_ChargeType == "CTS" && x.C1_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(ctsCharges.Count(), NUnit.Framework.Is.EqualTo(0));

				var ctaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "CTA");
				NUnit.Framework.Assert.That(ctaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var list = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).ToList();
				var fee = list.SingleOrDefault(x => x.CF_ChargeType == "CTS" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 610m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_ChargeAmount == 130m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});

			invoiceLine1.JI_EnteredUnitPrice = 1000m;
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_EnteredUnitPrice = 1000m;
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine3.JI_EnteredUnitPrice = 1000m;
			invoiceLine3.JI_InvoiceQuantity = 1;
			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(5));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var ctaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "CTA");
				NUnit.Framework.Assert.That(ctaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				invoiceLine1.JI_CusValueConvRatio = 0.2;
				invoiceLine2.JI_CusValueConvRatio = 0.2;
				invoiceLine3.JI_CusValueConvRatio = 0.2;

				new LineMerger(declaration).DoMerge();

				dtaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				ctaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "CTA");
				NUnit.Framework.Assert.That(ctaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var list = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).ToList();
				NUnit.Framework.Assert.That(list.Where(x => x.CF_ChargeType == "DTA").Count(), NUnit.Framework.Is.EqualTo(3));
				NUnit.Framework.Assert.That(list.Where(x => x.CF_ChargeType == "DTA").Sum(x => x.CF_ChargeAmount), NUnit.Framework.Is.EqualTo(180m));

				NUnit.Framework.Assert.That(list.Where(x => x.CF_ChargeType == "CTA").Count(), NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(list.Where(x => x.CF_ChargeType == "CTA").Sum(x => x.CF_ChargeAmount), NUnit.Framework.Is.EqualTo(212m));
			});
		}

		[ExpectNoExceptions]
		public void TestCalculateSpecialDutiesAndCommodityTax()
		{
			SetupUniversalReferenceData();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeDTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", description: "Duty");
			refCusRateTypeDTY.ZZR_CustomsValueFormula = "CV + DTA + DTS + ADD + CVD + ADT + RTD";

			var rateCodeADD = helper.LoadOrCreateNewCusRateCode(Factory, SpecialDutyRateCodeList.Codes.AntiDumpingDuty, refCusRateTypeDTY.PK);
			var rateCodeCVD = helper.LoadOrCreateNewCusRateCode(Factory, SpecialDutyRateCodeList.Codes.CountervailingDuty, refCusRateTypeDTY.PK);
			var rateCodeADT = helper.LoadOrCreateNewCusRateCode(Factory, SpecialDutyRateCodeList.Codes.AdditionalDuty, refCusRateTypeDTY.PK);
			var rateCodeRTD = helper.LoadOrCreateNewCusRateCode(Factory, SpecialDutyRateCodeList.Codes.RetaliatoryDuty, refCusRateTypeDTY.PK);
			Factory.Save();

			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = "PR";
			invoiceLine1.JI_Tariff = "87031000003";
			invoiceLine1.JI_CountryOfOrigin = "AU";
			invoiceLine1.JI_PrimaryPreference = "STD";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_EnteredUnitPrice = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_CustomsQuantity = 1;
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine1.JI_ConcessionOrder = "";
			invoiceLine1.JI_AntiDumpingDutyRate = 0.05M;
			invoiceLine1.JI_CountervailingDutyRate = 0.03M;
			invoiceLine1.JI_AdditionalDutyRate = 0.02M;
			invoiceLine1.JI_RetaliatoryDutyRate = 0.01M;
			invoiceLine1.JI_VatPymntMthd = "CAS";

			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				var header = declaration.CustomsEntryHeaders[0];
				var entryLine = header.MergedLines.Cast<CusEntryLine>().FirstOrDefault();
				NUnit.Framework.Assert.That(header.DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(8));

				var list = header.MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).ToList();
				var fee = list.SingleOrDefault(x => x.CF_ChargeType == "ADD" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 50m && x.CF_BaseValue == 1300m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "CVD" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 30m && x.CF_BaseValue == 1350m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "ADT" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 20m && x.CF_BaseValue == 1380m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "RTD" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 10m && x.CF_BaseValue == 1400m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "DTA" && x.CF_MethodOfPayment == "DEF" && x.CF_ChargeAmount == 300m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));

				fee = list.SingleOrDefault(x => x.CF_ChargeType == "CTA" && x.CF_MethodOfPayment == "DEF");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(141m).Using(CustomComparers.TypeComparison), "(CV + DTA + ADD + CVD + ADT + RTD) * 0.1");
				NUnit.Framework.Assert.That(header.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(2551m).Using(CustomComparers.TypeComparison), "BusinessTaxBaseAmount");
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2551m).Using(CustomComparers.TypeComparison), "CL_ValueForVAT");
				NUnit.Framework.Assert.That(header.TotalTaxAmount, NUnit.Framework.Is.EqualTo(2112m).Using(CustomComparers.TypeComparison), "TotalTaxAmount");
			});
		}

		[ExpectNoExceptions]
		public void TestSSGTaxCalculation()
		{
			SetupUniversalReferenceData();
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000000000m;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "TT";
			invoiceLine.JI_Tariff = "03035400901";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000000000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;

			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(3));
			CombineAssertions(() =>
			{
				var ssgCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "SSG" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(ssgCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "SSG" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 122000000m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});
		}

		[ExpectNoExceptions]
		public void TestSSGTaxCalculationBelowThreshold()
		{
			SetupUniversalReferenceData();
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "TT";
			invoiceLine.JI_Tariff = "03035400901";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 200000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;

			var tax = invoiceLine.Taxes.AddNew();
			tax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
			tax.JLT_Tariff = "FORNITURE";
			tax.JLT_MethodOfPayment = "CAS";

			new LineMerger(declaration).DoMerge();
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].DutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(1));
			CombineAssertions(() =>
			{
				var ssgCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "SSG" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(ssgCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "SSG" && x.CF_MethodOfPayment == "CAS" && x.CF_ChargeAmount == 122000000m);
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});
		}

		[ExpectNoExceptions]
		public void TestExemptDutyCalculation()
		{
			SetupUniversalReferenceDataForExemptDutyCalculation();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var tariffCollection = new ZString[] { "24099999999", "210390901", "210390902", "210390903", "220399999", "220499999", "220599999", "220699999", "220799999", "220899999", "99999999999" };
			foreach (var tariff in tariffCollection)
			{
				foreach (var waiverOfExemption in new ZBool[] { ZBool.True, ZBool.False })
				{
					var exemplist = new List<Dictionary<ZString, ExemptData>>();
					bool exemptDuty = (tariff == "99999999999" && !waiverOfExemption);
					if (exemptDuty)
					{
						exemplist.Add(new Dictionary<ZString, ExemptData>
						{
							{ "1234567890", new ExemptData { LinePrice = 400m } },
							{ tariff, new ExemptData { LinePrice = 1599m } }
						});
					}
					else
					{
						exemplist.Add(new Dictionary<ZString, ExemptData>
						{
							{ "1234567890", new ExemptData { LinePrice = 400m, ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTS , Amount = 199.9m } },
							{ tariff, new ExemptData { LinePrice = 1599m ,ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTS, Amount = 199.9m } }
						});

						exemplist.Add(new Dictionary<ZString, ExemptData>
						{
							{ "1234567890", new ExemptData { LinePrice = 400m, ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTS , Amount = 200m } },
							{ tariff, new ExemptData { LinePrice = 1600m ,ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTS, Amount = 200m } }
						});
					}
					foreach (var keys in exemplist)
					{
						declaration.Invoices.RemoveAll();
						declaration.InvoiceLines.RemoveAndDeleteAll();
						entryInstruction.CEI_WaiverOfExemption = waiverOfExemption;
						foreach (var pair in keys)
						{
							var tariffCode = pair.Key;
							var exemptData = pair.Value;
							var invoice = declaration.Invoices.AddNew();
							invoice.JZ_InvoiceAmount = exemptData.LinePrice;
							var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
							invoiceLine.JI_CEI = entryInstruction.PK;
							invoiceLine.JI_Procedure = "FF";
							invoiceLine.JI_CountryOfOrigin = "AU";
							invoiceLine.JI_PrimaryPreference = "STD";
							invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
							invoiceLine.JI_InvoiceQuantity = 1;
							invoiceLine.JI_EnteredUnitPrice = exemptData.LinePrice;
							invoiceLine.JI_Tariff = tariffCode;
							invoiceLine.JI_ConcessionOrder = "";
						}

						new LineMerger(declaration).DoMerge();
						foreach (var pair in keys)
						{
							var tariffCode = pair.Key;
							var exemptData = pair.Value;
							var message = $"tariff code is '{tariffCode}' and WaiverOfExemption is '{entryInstruction.CEI_WaiverOfExemption}'";
							NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1), message);

							var entryHeaderCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == exemptData.ChargeType);
							if (exemptDuty)
							{
								NUnit.Framework.Assert.That(entryHeaderCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)), "message - should be [null]");
								NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].Charges.Count, NUnit.Framework.Is.EqualTo(0), message);
								NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Count(), NUnit.Framework.Is.EqualTo(0), message);
							}
							else
							{
								NUnit.Framework.Assert.That(entryHeaderCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)), "message - should be [null]");
								NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).Where(x => x.CF_ChargeType == exemptData.ChargeType).Sum(x => x.CF_ChargeAmount), NUnit.Framework.Is.EqualTo(exemptData.Amount).Using(CustomComparers.TypeComparison), message);
							}
						}
					}
				}
			}
		}

		class ExemptData
		{
			public ZDecimal LinePrice { get; set; }
			public ZString ChargeType { get; set; }
			public ZDecimal Amount { get; set; }
		}

		void SetupUniversalReferenceDataForExemptDutyCalculation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusProcedure = helper.CreateRefCusProcedure("TW", "IM", "FF", "", "", "", "IMP", false);
			cusProcedure.Attributes.AddNew("VATPaymentMethod", "CAS");
			cusProcedure.Attributes.AddNew("DTYPaymentMethod", "CAS");
			cusProcedure.Attributes.AddNew("COMPaymentMethod", "CAS");
			cusProcedure.Attributes.AddNew("TATPaymentMethod", "CAS");
			Factory.Save();

			var stdPreference = helper.CreatePreferenceForCountry("STD", "STD", "TW");
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType("TW", "HSN").PK;

			var rateTypeDTY = helper.CreateCusRateType("TW", UniversalReferenceConstants.RefCusRateTypes.Duty);
			var rateCodeCTS = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CTS, rateTypeDTY.PK);

			var rateTypeCOM = helper.CreateCusRateType("TW", UniversalReferenceConstants.RefCusRateTypes.CommodityTaxes);
			var rateCodeTAT = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.TAT, rateTypeCOM.PK);
			var rateCodeHWS = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.HWS, rateTypeCOM.PK);
			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CTA, rateTypeCOM.PK);

			Factory.Save();
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariff1 = helper.CreateTariff("TW", tariffTypePK, "24099999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff1RateA = helper.CreateRate(tariff1, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff1RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff2 = helper.CreateTariff("TW", tariffTypePK, "210390901", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff2RateA = helper.CreateRate(tariff2, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff2RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff3 = helper.CreateTariff("TW", tariffTypePK, "210390902", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff3RateA = helper.CreateRate(tariff3, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff3RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff4 = helper.CreateTariff("TW", tariffTypePK, "210390903", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff4RateA = helper.CreateRate(tariff4, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff4RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff5 = helper.CreateTariff("TW", tariffTypePK, "220399999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff5RateA = helper.CreateRate(tariff5, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff5RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff6 = helper.CreateTariff("TW", tariffTypePK, "220499999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff6RateA = helper.CreateRate(tariff6, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff6RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff7 = helper.CreateTariff("TW", tariffTypePK, "220599999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff7RateA = helper.CreateRate(tariff7, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff7RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff8 = helper.CreateTariff("TW", tariffTypePK, "220699999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff8RateA = helper.CreateRate(tariff8, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff8RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff9 = helper.CreateTariff("TW", tariffTypePK, "220799999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff9RateA = helper.CreateRate(tariff9, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff9RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff10 = helper.CreateTariff("TW", tariffTypePK, "220899999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff10RateA = helper.CreateRate(tariff10, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff10RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariff11 = helper.CreateTariff("TW", tariffTypePK, "99999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff11RateA = helper.CreateRate(tariff11, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariff11RateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffA = helper.CreateTariff("TW", tariffTypePK, "1234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffARateA = helper.CreateRate(tariffA, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariffARateA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffB = helper.CreateTariff("TW", tariffTypePK, "55555555555", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffBRateCodeTAT = helper.CreateRate(tariffB, rateCodeTAT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.2*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariffBRateCodeTAT, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffC = helper.CreateTariff("TW", tariffTypePK, "66666666666", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffBRateCodeHWS = helper.CreateRate(tariffC, rateCodeHWS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.3*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariffBRateCodeHWS, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffD = helper.CreateTariff("TW", tariffTypePK, "77777777777", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffBRateCodeCTA = helper.CreateRate(tariffD, rateCodeCTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.4*VFD", stdPreference.PK);
			helper.CreateCusApplicability(tariffBRateCodeCTA, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTariffRelationship(tariffB.PK, tariffTypePK, "55555555555");
			helper.CreateTariffRelationship(tariffC.PK, tariffTypePK, "66666666666");
			helper.CreateTariffRelationship(tariffD.PK, tariffTypePK, "77777777777");

			helper.CreateTaxOrFee("VAT", 0.5, Core.Constants.CountryCodes.Taiwan, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestFDDCalculation()
		{
			SetupUniversalReferenceData();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 07, 03);

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;

			entryInstruction.CEI_DaysOfDelayedDeclaration = 2;
			new LineMerger(declaration).DoMerge();

			CombineAssertions(() =>
			{
				var fddChargeCAS = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DDF" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(fddChargeCAS, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));
				NUnit.Framework.Assert.That(fddChargeCAS.C1_ChargeAmount, NUnit.Framework.Is.EqualTo(400m).Using(CustomComparers.TypeComparison));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DDF" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
			new LineMerger(declaration).DoMerge();

			CombineAssertions(() =>
			{
				var fddChargeCAS = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DDF" && x.C1_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(fddChargeCAS, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DDF" && x.CF_MethodOfPayment == "CAS");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestEntryLineCustomsValueReconcile()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase1();
			CombineAssertions("TestEntryLineCustomsValueReconcile Case 1", () =>
			{
				NUnit.Framework.Assert.That(entryHeader.MergedLines[0].CL_CustomsValue, NUnit.Framework.Is.EqualTo(252815m).Using(CustomComparers.TypeComparison), "MergedLines[0].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[1].CL_CustomsValue, NUnit.Framework.Is.EqualTo(4763m).Using(CustomComparers.TypeComparison), "MergedLines[1].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[2].CL_CustomsValue, NUnit.Framework.Is.EqualTo(252814m).Using(CustomComparers.TypeComparison), "MergedLines[2].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[3].CL_CustomsValue, NUnit.Framework.Is.EqualTo(3528m).Using(CustomComparers.TypeComparison), "MergedLines[3].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[4].CL_CustomsValue, NUnit.Framework.Is.EqualTo(4763m).Using(CustomComparers.TypeComparison), "MergedLines[4].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[5].CL_CustomsValue, NUnit.Framework.Is.EqualTo(10455m).Using(CustomComparers.TypeComparison), "MergedLines[5].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[6].CL_CustomsValue, NUnit.Framework.Is.EqualTo(5750m).Using(CustomComparers.TypeComparison), "MergedLines[6].CL_CustomsValue");
			});

			entryHeader = GetImportFobEntryHeaderTestCase2();
			CombineAssertions("TestEntryLineCustomsValueReconcile Case 2", () =>
			{
				NUnit.Framework.Assert.That(entryHeader.MergedLines[0].CL_CustomsValue, NUnit.Framework.Is.EqualTo(222680m).Using(CustomComparers.TypeComparison), "MergedLines[0].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[1].CL_CustomsValue, NUnit.Framework.Is.EqualTo(297160m).Using(CustomComparers.TypeComparison), "MergedLines[1].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[2].CL_CustomsValue, NUnit.Framework.Is.EqualTo(284620m).Using(CustomComparers.TypeComparison), "MergedLines[2].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[3].CL_CustomsValue, NUnit.Framework.Is.EqualTo(272460m).Using(CustomComparers.TypeComparison), "MergedLines[3].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[4].CL_CustomsValue, NUnit.Framework.Is.EqualTo(272460m).Using(CustomComparers.TypeComparison), "MergedLines[4].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[5].CL_CustomsValue, NUnit.Framework.Is.EqualTo(210140m).Using(CustomComparers.TypeComparison), "MergedLines[5].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[6].CL_CustomsValue, NUnit.Framework.Is.EqualTo(894900m).Using(CustomComparers.TypeComparison), "MergedLines[6].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[7].CL_CustomsValue, NUnit.Framework.Is.EqualTo(346940m).Using(CustomComparers.TypeComparison), "MergedLines[7].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[8].CL_CustomsValue, NUnit.Framework.Is.EqualTo(1112000m).Using(CustomComparers.TypeComparison), "MergedLines[8].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[9].CL_CustomsValue, NUnit.Framework.Is.EqualTo(2092801m).Using(CustomComparers.TypeComparison), "MergedLines[9].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[10].CL_CustomsValue, NUnit.Framework.Is.EqualTo(7212m).Using(CustomComparers.TypeComparison), "MergedLines[10].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[11].CL_CustomsValue, NUnit.Framework.Is.EqualTo(7212m).Using(CustomComparers.TypeComparison), "MergedLines[11].CL_CustomsValue");
			});

			entryHeader = GetImportFobEntryHeaderTestCase3();
			CombineAssertions("TestEntryLineCustomsValueReconcile Case 3", () =>
			{
				NUnit.Framework.Assert.That(entryHeader.MergedLines[0].CL_CustomsValue, NUnit.Framework.Is.EqualTo(189937m).Using(CustomComparers.TypeComparison), "MergedLines[0].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[1].CL_CustomsValue, NUnit.Framework.Is.EqualTo(403765m).Using(CustomComparers.TypeComparison), "MergedLines[1].CL_CustomsValue");
				NUnit.Framework.Assert.That(entryHeader.MergedLines[2].CL_CustomsValue, NUnit.Framework.Is.EqualTo(16477m).Using(CustomComparers.TypeComparison), "MergedLines[2].CL_CustomsValue");
			});
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestInvoiceLineCustomsValueReconcile()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase1();
			var invoiceLines = entryHeader.MergedLines.Cast<CusEntryLine>().SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>());
			NUnit.Framework.Assert.That(invoiceLines.First().JI_CVAfterRecon, NUnit.Framework.Is.EqualTo(252815m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(invoiceLines.ElementAt(1).JI_CVAfterRecon, NUnit.Framework.Is.EqualTo(4763m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(invoiceLines.ElementAt(2).JI_CVAfterRecon, NUnit.Framework.Is.EqualTo(252814m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(invoiceLines.ElementAt(3).JI_CVAfterRecon, NUnit.Framework.Is.EqualTo(3528m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(invoiceLines.ElementAt(4).JI_CVAfterRecon, NUnit.Framework.Is.EqualTo(4763m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(invoiceLines.ElementAt(5).JI_CVAfterRecon, NUnit.Framework.Is.EqualTo(10455m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(invoiceLines.ElementAt(6).JI_CVAfterRecon, NUnit.Framework.Is.EqualTo(5750m).Using(CustomComparers.TypeComparison));
		}

		CusEntryHeader GetImportFobEntryHeaderTestCase1()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 28.57m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 16372m;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 2300m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 50m, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8419.20.00.00-5";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "IL";
			invoiceLine1.JI_Procedure = "50";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 7738.2m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8419.90.20.00-6";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_CountryOfOrigin = "IL";
			invoiceLine2.JI_Procedure = "50";
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 145.8m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "8419.20.00.00-5";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_CountryOfOrigin = "IL";
			invoiceLine3.JI_Procedure = "50";
			invoiceLine3.JI_InvoiceQuantity = 1;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 7738.2m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "4819.10.00.00-1";
			invoiceLine4.JI_PrimaryPreference = "PR1";
			invoiceLine4.JI_CountryOfOrigin = "IL";
			invoiceLine4.JI_Procedure = "50";
			invoiceLine4.JI_InvoiceQuantity = 1;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 108m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "84199020006";
			invoiceLine5.JI_PrimaryPreference = "PR1";
			invoiceLine5.JI_CountryOfOrigin = "IL";
			invoiceLine5.JI_Procedure = "50";
			invoiceLine5.JI_InvoiceQuantity = 1;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 145.8m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "84199020006";
			invoiceLine6.JI_PrimaryPreference = "PR1";
			invoiceLine6.JI_CountryOfOrigin = "IL";
			invoiceLine6.JI_Procedure = "50";
			invoiceLine6.JI_InvoiceQuantity = 2;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 160m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "84199020006";
			invoiceLine7.JI_PrimaryPreference = "PR1";
			invoiceLine7.JI_CountryOfOrigin = "IL";
			invoiceLine7.JI_Procedure = "37";
			invoiceLine7.JI_InvoiceQuantity = 2;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 88m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		CusEntryHeader GetImportFobEntryHeaderTestCase2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 6020585m;

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "91012900007";
			invoiceLine1.JI_CountryOfOrigin = "CH";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_Procedure = Constants.ProcedureCodes._39;
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_EnteredUnitPrice = 222680m;
			invoiceLine1.JI_UseOneTenthCV = ZBool.False;
			invoiceLine1.JI_RAPPrice = 22268m;
			invoiceLine1.JI_RAPCurr = "TWD";

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "91022100004";
			invoiceLine2.JI_CountryOfOrigin = "CH";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_Procedure = Constants.ProcedureCodes._39;
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_EnteredUnitPrice = 297160m;
			invoiceLine2.JI_UseOneTenthCV = ZBool.False;
			invoiceLine2.JI_RAPPrice = 29716m;
			invoiceLine2.JI_RAPCurr = "TWD";

			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "91012100005";
			invoiceLine3.JI_CountryOfOrigin = "CH";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine3.JI_InvoiceQuantity = 1;
			invoiceLine3.JI_EnteredUnitPrice = 284620m;
			invoiceLine3.JI_UseOneTenthCV = ZBool.False;
			invoiceLine3.JI_RAPPrice = 0m;
			invoiceLine3.JI_RAPCurr = "TWD";

			var invoiceLine4 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "91012100005";
			invoiceLine4.JI_CountryOfOrigin = "CH";
			invoiceLine4.JI_PrimaryPreference = "PR1";
			invoiceLine4.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine4.JI_InvoiceQuantity = 1;
			invoiceLine4.JI_EnteredUnitPrice = 272460m;
			invoiceLine4.JI_UseOneTenthCV = ZBool.False;
			invoiceLine4.JI_RAPPrice = 0m;
			invoiceLine4.JI_RAPCurr = "TWD";

			var invoiceLine5 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "91012100003";
			invoiceLine5.JI_CountryOfOrigin = "CH";
			invoiceLine5.JI_PrimaryPreference = "PR1";
			invoiceLine5.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine5.JI_InvoiceQuantity = 1;
			invoiceLine5.JI_EnteredUnitPrice = 272460m;
			invoiceLine5.JI_UseOneTenthCV = ZBool.False;
			invoiceLine5.JI_RAPPrice = 0m;
			invoiceLine5.JI_RAPCurr = "TWD";

			var invoiceLine6 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "91012100004";
			invoiceLine6.JI_CountryOfOrigin = "CH";
			invoiceLine6.JI_PrimaryPreference = "PR1";
			invoiceLine6.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine6.JI_InvoiceQuantity = 1;
			invoiceLine6.JI_EnteredUnitPrice = 210140m;
			invoiceLine6.JI_UseOneTenthCV = ZBool.False;
			invoiceLine6.JI_RAPPrice = 0m;
			invoiceLine6.JI_RAPCurr = "TWD";

			var invoiceLine7 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "91012100004";
			invoiceLine7.JI_CountryOfOrigin = "CH";
			invoiceLine7.JI_PrimaryPreference = "PR1";
			invoiceLine7.JI_Procedure = Constants.ProcedureCodes._38;
			invoiceLine7.JI_InvoiceQuantity = 1;
			invoiceLine7.JI_EnteredUnitPrice = 894900m;
			invoiceLine7.JI_UseOneTenthCV = ZBool.False;
			invoiceLine7.JI_RAPPrice = 894900m;
			invoiceLine7.JI_RAPCurr = "TWD";

			var invoiceLine8 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine8.JI_Tariff = "91012100004";
			invoiceLine8.JI_CountryOfOrigin = "CH";
			invoiceLine8.JI_PrimaryPreference = "PR1";
			invoiceLine8.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine8.JI_InvoiceQuantity = 1;
			invoiceLine8.JI_EnteredUnitPrice = 346940m;
			invoiceLine8.JI_UseOneTenthCV = ZBool.False;
			invoiceLine8.JI_RAPPrice = 0m;
			invoiceLine8.JI_RAPCurr = "TWD";

			var invoiceLine9 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine9.JI_Tariff = "91012100004";
			invoiceLine9.JI_CountryOfOrigin = "CH";
			invoiceLine9.JI_PrimaryPreference = "PR1";
			invoiceLine9.JI_Procedure = Constants.ProcedureCodes._38;
			invoiceLine9.JI_InvoiceQuantity = 1;
			invoiceLine9.JI_EnteredUnitPrice = 1112000m;
			invoiceLine9.JI_UseOneTenthCV = ZBool.False;
			invoiceLine9.JI_RAPPrice = 1112000m;
			invoiceLine9.JI_RAPCurr = "TWD";

			var invoiceLine10 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine10.JI_Tariff = "91012100004";
			invoiceLine10.JI_CountryOfOrigin = "CH";
			invoiceLine10.JI_PrimaryPreference = "PR1";
			invoiceLine10.JI_Procedure = Constants.ProcedureCodes._38;
			invoiceLine10.JI_InvoiceQuantity = 1;
			invoiceLine10.JI_EnteredUnitPrice = 2092800m;
			invoiceLine10.JI_UseOneTenthCV = ZBool.False;
			invoiceLine10.JI_RAPPrice = 2092800m;
			invoiceLine10.JI_RAPCurr = "TWD";

			var invoiceLine11 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine11.JI_Tariff = "91139090008";
			invoiceLine11.JI_CountryOfOrigin = "CH";
			invoiceLine11.JI_PrimaryPreference = "PR1";
			invoiceLine11.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine11.JI_InvoiceQuantity = 1;
			invoiceLine11.JI_EnteredUnitPrice = 7212.4m;
			invoiceLine11.JI_UseOneTenthCV = ZBool.False;
			invoiceLine11.JI_RAPPrice = 0m;
			invoiceLine11.JI_RAPCurr = "TWD";

			var invoiceLine12 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine12.JI_Tariff = "91139090009";
			invoiceLine12.JI_CountryOfOrigin = "CH";
			invoiceLine12.JI_PrimaryPreference = "PR1";
			invoiceLine12.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine12.JI_InvoiceQuantity = 1;
			invoiceLine12.JI_EnteredUnitPrice = 7212.4m;
			invoiceLine12.JI_UseOneTenthCV = ZBool.False;
			invoiceLine12.JI_RAPPrice = 0m;
			invoiceLine12.JI_RAPCurr = "TWD";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		CusEntryHeader GetImportFobEntryHeaderTestCase3()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 740650m;

			var invoiceCharge1 = invoice.Charges.AddNew();
			invoiceCharge1.J7_ChargeType = "OFT";
			invoiceCharge1.J7_Amount = 12934m;
			invoiceCharge1.J7_RX_NKCurrency = "TWD";

			var invoiceCharge2 = invoice.Charges.AddNew();
			invoiceCharge2.J7_ChargeType = "ADD";
			invoiceCharge2.J7_Amount = 725m;
			invoiceCharge2.J7_RX_NKCurrency = "TWD";

			var invoiceCharge3 = invoice.Charges.AddNew();
			invoiceCharge3.J7_ChargeType = "DED";
			invoiceCharge3.J7_Amount = 144130m;
			invoiceCharge3.J7_RX_NKCurrency = "TWD";

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "84563000007";
			invoiceLine1.JI_CountryOfOrigin = "US";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_EnteredUnitPrice = 230550m;
			invoiceLine1.JI_CustomsSecondQuantity = 1m;
			invoiceLine1.JI_UseOneTenthCV = ZBool.False;
			invoiceLine1.JI_RAPPrice = 0m;
			invoiceLine1.JI_RAPCurr = "TWD";

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "84563000007";
			invoiceLine2.JI_CountryOfOrigin = "US";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_Procedure = Constants.ProcedureCodes._31;
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_EnteredUnitPrice = 490100m;
			invoiceLine2.JI_CustomsSecondQuantity = 2m;
			invoiceLine2.JI_UseOneTenthCV = ZBool.False;
			invoiceLine2.JI_RAPPrice = 0m;
			invoiceLine2.JI_RAPCurr = "TWD";

			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "84563000007";
			invoiceLine3.JI_CountryOfOrigin = "US";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_Procedure = Constants.ProcedureCodes._39;
			invoiceLine3.JI_InvoiceQuantity = 1;
			invoiceLine3.JI_EnteredUnitPrice = 20000m;
			invoiceLine3.JI_CustomsSecondQuantity = 1m;
			invoiceLine3.JI_UseOneTenthCV = ZBool.False;
			invoiceLine3.JI_RAPPrice = 2000m;
			invoiceLine3.JI_RAPCurr = "TWD";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		#region Implementation

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;

		protected override void SetUp()
		{
			base.SetUp();
			CustomSetup();
		}

		void CustomSetup()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceHeader = declaration.Invoices.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = declaration.CusEntryInstruction;
		}

		#endregion

		[ExpectNoExceptions]
		public void TestCalculateDutiesWhenHaveConcessionOrder()
		{
			TariffDataForTestHelper.NewData(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);
			entryInstruction.CEI_Style = "G1";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "98050000009";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_PrimaryPreference = "PRE";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;

			new LineMerger(declaration).DoMerge();
			var entryLine = invoiceLine.CusEntryLine;
			NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Entry line business tax base should be");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
			});

			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			new LineMerger(declaration).DoMerge();
			entryLine = invoiceLine.CusEntryLine;
			NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(1258m).Using(CustomComparers.TypeComparison), "Entry line business tax base should be");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(258m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.258m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_BaseValue, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});

			invoiceLine.JI_ConcessionOrder = ZString.Empty;
			new LineMerger(declaration).DoMerge();
			entryLine = invoiceLine.CusEntryLine;
			NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(1125m).Using(CustomComparers.TypeComparison), "Entry line business tax base should be");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders.Count, NUnit.Framework.Is.EqualTo(1));
				var dtaCharge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().SingleOrDefault(x => x.C1_ChargeType == "DTA");
				NUnit.Framework.Assert.That(dtaCharge, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeaderCharges)));

				var fee = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).SingleOrDefault(x => x.CF_ChargeType == "DTA");
				NUnit.Framework.Assert.That(fee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(125m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_Rate, NUnit.Framework.Is.EqualTo(0.125m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_BaseValue, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(fee.CF_MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestCalculateBusinessTaxBaseWithSpecialDuties()
		{
			SetupUniversalReferenceData();

			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 5, 1);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000003";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_AntiDumpingDutyRate = 0.1;
			invoiceLine.JI_CountervailingDutyRate = 0.1;
			invoiceLine.JI_AdditionalDutyRate = 0.1;
			invoiceLine.JI_RetaliatoryDutyRate = 0.1;
			invoiceLine.JI_VatPymntMthd = "CAS";

			var lineMerger = new LineMerger(declaration);

			lineMerger.DoMerge();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault();
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(2870m).Using(CustomComparers.TypeComparison), "CusEntryHeader - Business Tax Base Amount (2870) = Customs Value (1000) + CAS[Anti-Dumping Duty (100) + Countervailing Duty (100) + Additional Duty (100) + Retaliatory Duty (100)] + DEF[DTA(300) + HWS(1000) + CTA(170)]");
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2870m).Using(CustomComparers.TypeComparison), "CusEntryLine - Business Tax Base Amount (2870) = Customs Value (1000) + CAS[Anti-Dumping Duty (100) + Countervailing Duty (100) + Additional Duty (100) + Retaliatory Duty (100)] + DEF[DTA(300) + HWS(1000) + CTA(170)]");
			});

			invoiceLine.JI_AntiDumpingDutyRate = 0;
			lineMerger.DoMerge();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault();
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(2760m).Using(CustomComparers.TypeComparison), "CusEntryHeader - Business Tax Base Amount (2760) = Customs Value (1000) + CAS[Countervailing Duty (100) + Additional Duty (100) + Retaliatory Duty (100)] + DEF[DTA(300) + HWS(1000) + CTA(160)]");
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2760m).Using(CustomComparers.TypeComparison), "CusEntryLine - Business Tax Base Amount (2760) = Customs Value (1000) + CAS[Countervailing Duty (100) + Additional Duty (100) + Retaliatory Duty (100)] + DEF[DTA(300) + HWS(1000) + CTA(160)]");
			});

			invoiceLine.JI_CountervailingDutyRate = 0;
			lineMerger.DoMerge();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault();
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(2650m).Using(CustomComparers.TypeComparison), "CusEntryHeader - Business Tax Base Amount (2650) = Customs Value (1000) + CAS[Additional Duty (100) + Retaliatory Duty (100)] + DEF[DTA(300) + HWS(1000) + CTA(150)]");
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2650m).Using(CustomComparers.TypeComparison), "CusEntryLine - Business Tax Base Amount (2650) = Customs Value (1000) + CAS[Additional Duty (100) + Retaliatory Duty (100)] + DEF[DTA(300) + HWS(1000) + CTA(150)]");
			});

			invoiceLine.JI_AdditionalDutyRate = 0;
			lineMerger.DoMerge();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault();
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(2540m).Using(CustomComparers.TypeComparison), "CusEntryHeader - Business Tax Base Amount (2540) = Customs Value (1000) + CAS[Retaliatory Duty (100)] + DEF[DTA(300) + HWS(1000) + CTA(140)]");
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2540m).Using(CustomComparers.TypeComparison), "CusEntryLine - Business Tax Base Amount (2540) = Customs Value (1000) + CAS[Retaliatory Duty (100)] + DEF[DTA(300) + HWS(1000) + CTA(140)]");
			});

			invoiceLine.JI_RetaliatoryDutyRate = 0;
			lineMerger.DoMerge();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault();
				NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(2430m).Using(CustomComparers.TypeComparison), "CusEntryHeader - Business Tax Base Amount (2430) = Customs Value (1000) + DEF[DTA(300) + HWS(1000) + CTA(130)]");
				NUnit.Framework.Assert.That(entryLine.CL_ValueForVAT, NUnit.Framework.Is.EqualTo(2430m).Using(CustomComparers.TypeComparison), "CusEntryLine - Business Tax Base Amount (2430) = Customs Value (1000) + DEF[DTA(300) + HWS(1000) + CTA(130)]");
			});
		}
	}
}
