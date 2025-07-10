using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DutyCalculationHelperTest : TestCaseWithFactory
	{
		[TestDate(2019, 07, 14)]
		[ExpectNoExceptions]
		public void TestGetPaymentMethod()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeDTY = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var refCusRateTypeCOM = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM");
			var refCusRateTypeSSG = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG");
			var refCusRateTypeAA = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "AA");
			var refCusProcedure = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "AA", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var rateCodeDTA = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", refCusRateTypeDTY.PK);
			var rateCodeDTS = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTS", refCusRateTypeDTY.PK);
			var rateCodeHWS = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "HWS", refCusRateTypeCOM.PK);
			var rateCodeCTA = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTA", refCusRateTypeCOM.PK);
			var rateCodeCTS = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTS", refCusRateTypeCOM.PK);
			var rateCodeSSG = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "SSG", refCusRateTypeSSG.PK);
			var rateCodeTAT = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "TAT", refCusRateTypeAA.PK);
			var rateCodeTPF = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "TPF", refCusRateTypeCOM.PK);
			var rateCodeVAT = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "VAT", refCusRateTypeDTY.PK);
			var rateCodeDDF = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DDF", refCusRateTypeSSG.PK);
			var startDate = ZDate.Today.AddMonths(-3);
			var endDate = ZDate.Today.AddMonths(3);
			referenceDataHelper.CreateTaxOrFee("DDF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("VAT", 0.15000000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			refCusProcedure.Attributes.AddNew("COMPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("DTYPaymentMethod", "DEF");
			refCusProcedure.Attributes.AddNew("SSGPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("TATPaymentMethod", "DEF");
			refCusProcedure.Attributes.AddNew("TPFPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("VATPaymentMethod", "DEF");
			Factory.Save();
			ZDateTime dateOfValuation = new ZDateTime(2019, 06, 27);
			var tariffType = referenceDataHelper.CreateTariffType("TW", "TY");
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff("TW", tariffType.PK, "123456", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateViewDTA = referenceDataHelper.CreateRate(tariff, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0.3*VFD");
			var rateViewDTS = referenceDataHelper.CreateRate(tariff, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0.5*VFD");
			var rateViewTAT = referenceDataHelper.CreateRate(tariff, rateCodeTAT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			var rateViewHWS = referenceDataHelper.CreateRate(tariff, rateCodeHWS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			var rateViewCTA = referenceDataHelper.CreateRate(tariff, rateCodeCTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			var rateViewCTS = referenceDataHelper.CreateRate(tariff, rateCodeCTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			var rateViewSSG = referenceDataHelper.CreateRate(tariff, rateCodeSSG.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			var rateViewTPF = referenceDataHelper.CreateRate(tariff, rateCodeTPF.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			var rateViewVAT = referenceDataHelper.CreateRate(tariff, rateCodeVAT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			var rateViewDDF = referenceDataHelper.CreateRate(tariff, rateCodeDDF.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "1.1*VFD");
			Factory.Save();
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewDTA, dateOfValuation), NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewDTS, dateOfValuation), NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewTAT, dateOfValuation), NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewHWS, dateOfValuation), NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewCTA, dateOfValuation), NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewCTS, dateOfValuation), NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewSSG, dateOfValuation), NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewTPF, dateOfValuation), NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewVAT, dateOfValuation), NUnit.Framework.Is.EqualTo("DEF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(refCusProcedure, rateViewDDF, dateOfValuation), NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewDTA, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewDTS, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewTAT, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewHWS, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewCTA, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewCTS, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewSSG, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewTPF, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewVAT, dateOfValuation).ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(DutyCalculationHelper.GetPaymentMethod(null, rateViewDDF, dateOfValuation), NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetGroupedDuties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.CustomsEntryHeaders.AddNew();
			var duties = new List<DutyCalculationIntermediateResult> { new DutyCalculationIntermediateResult()
			{ Amount = 10.1m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusRateCodes.DTA }, new DutyCalculationIntermediateResult()
			{ Amount = 15m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusRateCodes.DTS }, new DutyCalculationIntermediateResult()
			{ Amount = 20.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusRateCodes.DTA }, new DutyCalculationIntermediateResult()
			{ Amount = 21.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusRateCodes.DTS }, new DutyCalculationIntermediateResult()
			{ Amount = 22.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = SpecialDutyRateCodeList.Codes.CountervailingDuty }, new DutyCalculationIntermediateResult()
			{ Amount = 23.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = SpecialDutyRateCodeList.Codes.AntiDumpingDuty }, new DutyCalculationIntermediateResult()
			{ Amount = 24.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = SpecialDutyRateCodeList.Codes.RetaliatoryDuty }, new DutyCalculationIntermediateResult()
			{ Amount = 25.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = SpecialDutyRateCodeList.Codes.AdditionalDuty }, new DutyCalculationIntermediateResult()
			{ Amount = 26.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusRateCodes.TAT }, new DutyCalculationIntermediateResult()
			{ Amount = 27.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusRateCodes.TAT }, new DutyCalculationIntermediateResult()
			{ Amount = 28.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusRateCodes.CTA }, new DutyCalculationIntermediateResult()
			{ Amount = 29.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusRateCodes.CTS }, new DutyCalculationIntermediateResult()
			{ Amount = 30.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusRateCodes.CTA }, new DutyCalculationIntermediateResult()
			{ Amount = 31.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusRateCodes.CTS }, new DutyCalculationIntermediateResult()
			{ Amount = 32.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusRateCodes.HWS }, new DutyCalculationIntermediateResult()
			{ Amount = 33.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusRateCodes.HWS }, new DutyCalculationIntermediateResult()
			{ Amount = 34.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusTaxOrFeeCodes.VAT }, new DutyCalculationIntermediateResult()
			{ Amount = 35.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusTaxOrFeeCodes.VAT }, new DutyCalculationIntermediateResult()
			{ Amount = 36.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF }, new DutyCalculationIntermediateResult()
			{ Amount = 37.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF }, new DutyCalculationIntermediateResult()
			{ Amount = 38.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusRateCodes.SSG }, new DutyCalculationIntermediateResult()
			{ Amount = 39.7m, PaymentMethod = EntryChargePaymentMethod.Codes.DEF, RateCode = UniversalReferenceConstants.RefCusRateCodes.SSG }, new DutyCalculationIntermediateResult()
			{ Amount = 40.7m, PaymentMethod = EntryChargePaymentMethod.Codes.CAS, RateCode = UniversalReferenceConstants.RefCusTaxOrFeeCodes.DDF } };
			var groupedDuties = duties.GetGroupedDuties(header.IsImport);
			NUnit.Framework.Assert.That(groupedDuties.Count(), NUnit.Framework.Is.EqualTo(19));
			var casA10 = groupedDuties.SingleOrDefault(x => x.TypeCode == "A10" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casA10.Amount, NUnit.Framework.Is.EqualTo(25m).Using(CustomComparers.TypeComparison));
			var defA19 = groupedDuties.SingleOrDefault(x => x.TypeCode == "A19" && x.PaymentMethod == "DEF");
			NUnit.Framework.Assert.That(defA19.Amount, NUnit.Framework.Is.EqualTo(42m).Using(CustomComparers.TypeComparison));
			var casA20 = groupedDuties.SingleOrDefault(x => x.TypeCode == "A20" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casA20.Amount, NUnit.Framework.Is.EqualTo(22m).Using(CustomComparers.TypeComparison));
			var casA30 = groupedDuties.SingleOrDefault(x => x.TypeCode == "A30" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casA30.Amount, NUnit.Framework.Is.EqualTo(23m).Using(CustomComparers.TypeComparison));
			var casA40 = groupedDuties.SingleOrDefault(x => x.TypeCode == "A40" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casA40.Amount, NUnit.Framework.Is.EqualTo(24m).Using(CustomComparers.TypeComparison));
			var casA50 = groupedDuties.SingleOrDefault(x => x.TypeCode == "A50" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casA50.Amount, NUnit.Framework.Is.EqualTo(25m).Using(CustomComparers.TypeComparison));
			var casB31 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B31" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casB31.Amount, NUnit.Framework.Is.EqualTo(26m).Using(CustomComparers.TypeComparison));
			var defB69 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B69" && x.PaymentMethod == "DEF");
			NUnit.Framework.Assert.That(defB69.Amount, NUnit.Framework.Is.EqualTo(27m).Using(CustomComparers.TypeComparison));
			var casB10 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B10" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casB10.Amount, NUnit.Framework.Is.EqualTo(58m).Using(CustomComparers.TypeComparison));
			var defB19 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B19" && x.PaymentMethod == "DEF");
			NUnit.Framework.Assert.That(defB19.Amount, NUnit.Framework.Is.EqualTo(62m).Using(CustomComparers.TypeComparison));
			var casB32 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B32" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casB32.Amount, NUnit.Framework.Is.EqualTo(32m).Using(CustomComparers.TypeComparison));
			var defB79 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B79" && x.PaymentMethod == "DEF");
			NUnit.Framework.Assert.That(defB79.Amount, NUnit.Framework.Is.EqualTo(33m).Using(CustomComparers.TypeComparison));
			var casB40 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B40" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casB40.Amount, NUnit.Framework.Is.EqualTo(34m).Using(CustomComparers.TypeComparison));
			var defB49 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B49" && x.PaymentMethod == "DEF");
			NUnit.Framework.Assert.That(defB49.Amount, NUnit.Framework.Is.EqualTo(35m).Using(CustomComparers.TypeComparison));
			var importCasB51 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B51" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(importCasB51.Amount, NUnit.Framework.Is.EqualTo(36m).Using(CustomComparers.TypeComparison));
			var exportCasB52 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B52" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(exportCasB52, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.DutyCalculationIntermediateResult)));
			var defB59 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B59" && x.PaymentMethod == "DEF");
			NUnit.Framework.Assert.That(defB59.Amount, NUnit.Framework.Is.EqualTo(37m).Using(CustomComparers.TypeComparison));
			var casB60 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B60" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casB60.Amount, NUnit.Framework.Is.EqualTo(38m).Using(CustomComparers.TypeComparison));
			var defB89 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B89" && x.PaymentMethod == "DEF");
			NUnit.Framework.Assert.That(defB89.Amount, NUnit.Framework.Is.EqualTo(39m).Using(CustomComparers.TypeComparison));
			var casC10 = groupedDuties.SingleOrDefault(x => x.TypeCode == "C10" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(casC10.Amount, NUnit.Framework.Is.EqualTo(40m).Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			groupedDuties = duties.GetGroupedDuties(header.IsImport);
			importCasB51 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B51" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(importCasB51, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.DutyCalculationIntermediateResult)));
			exportCasB52 = groupedDuties.SingleOrDefault(x => x.TypeCode == "B52" && x.PaymentMethod == "CAS");
			NUnit.Framework.Assert.That(exportCasB52.Amount, NUnit.Framework.Is.EqualTo(36m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetGroupedFees()
		{
			var duties = new List<DutyCalculationIntermediateResult> { new DutyCalculationIntermediateResult()
			{ Amount = 10.1234m, RateCode = "AAA", PaymentMethod = "CAS", UnitOfCalculation = "A", Rate = 10m }, new DutyCalculationIntermediateResult()
			{ Amount = 15.2345m, RateCode = "AAA", PaymentMethod = "CAS", UnitOfCalculation = "A", Rate = 10m }, new DutyCalculationIntermediateResult()
			{ Amount = 20.5678m, RateCode = "AAA", PaymentMethod = "DEF", UnitOfCalculation = "C", Rate = 30m }, new DutyCalculationIntermediateResult()
			{ Amount = 25.7899m, RateCode = "BBB", PaymentMethod = "CAS", UnitOfCalculation = "B", Rate = 20m }, new DutyCalculationIntermediateResult()
			{ Amount = 15.8902m, RateCode = "BBB", PaymentMethod = "CAS", UnitOfCalculation = "B", Rate = 20m }, new DutyCalculationIntermediateResult()
			{ Amount = 15.2345m, RateCode = "BBB", PaymentMethod = "CAS", UnitOfCalculation = "C", Rate = 20m }, new DutyCalculationIntermediateResult()
			{ Amount = 15.2451m, RateCode = "TPF", PaymentMethod = "CAS", UnitOfCalculation = "C", Rate = 20m }, new DutyCalculationIntermediateResult()
			{ Amount = 15.2424m, RateCode = "TPF", PaymentMethod = "DEF", UnitOfCalculation = "C", Rate = 20m }, new DutyCalculationIntermediateResult()
			{ Amount = 16.2325m, RateCode = "TPF", PaymentMethod = "DEF", UnitOfCalculation = "C", Rate = 20m } };
			var groupedDuties = duties.GetGroupedFees();
			NUnit.Framework.Assert.That(groupedDuties.Count(), NUnit.Framework.Is.EqualTo(6));
			var aaaCas = groupedDuties.SingleOrDefault(x => x.RateCode == "AAA" && x.PaymentMethod == "CAS" && x.UnitOfCalculation == "A");
			NUnit.Framework.Assert.That(aaaCas.Amount, NUnit.Framework.Is.EqualTo(25.358m).Using(CustomComparers.TypeComparison));
			var aaaDef = groupedDuties.SingleOrDefault(x => x.RateCode == "AAA" && x.PaymentMethod == "DEF" && x.UnitOfCalculation == "C");
			NUnit.Framework.Assert.That(aaaDef.Amount, NUnit.Framework.Is.EqualTo(20.568m).Using(CustomComparers.TypeComparison));
			var bbbCasB = groupedDuties.SingleOrDefault(x => x.RateCode == "BBB" && x.PaymentMethod == "CAS" && x.UnitOfCalculation == "B");
			NUnit.Framework.Assert.That(bbbCasB.Amount, NUnit.Framework.Is.EqualTo(41.68m).Using(CustomComparers.TypeComparison));
			var bbbCasC = groupedDuties.SingleOrDefault(x => x.RateCode == "BBB" && x.PaymentMethod == "CAS" && x.UnitOfCalculation == "C");
			NUnit.Framework.Assert.That(bbbCasC.Amount, NUnit.Framework.Is.EqualTo(15.235m).Using(CustomComparers.TypeComparison));
			var tpfCas = groupedDuties.SingleOrDefault(x => x.RateCode == "TPF" && x.PaymentMethod == "CAS" && x.UnitOfCalculation == "C");
			NUnit.Framework.Assert.That(tpfCas.Amount, NUnit.Framework.Is.EqualTo(15.245m).Using(CustomComparers.TypeComparison));
			var tpfDef = groupedDuties.SingleOrDefault(x => x.RateCode == "TPF" && x.PaymentMethod == "DEF" && x.UnitOfCalculation == "C");
			NUnit.Framework.Assert.That(tpfDef.Amount, NUnit.Framework.Is.EqualTo(31.475m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddEntryLineLevelIntermediateDuties()
		{
			var duties = new List<DutyCalculationIntermediateResult> { new DutyCalculationIntermediateResult()
			{ Amount = 10m, PaymentMethod = "AAA", RateCode = "AAA" } };
			NUnit.Framework.Assert.That(duties.Count, NUnit.Framework.Is.EqualTo(1));
			var dutiesToAdd = new List<DutyCalculationIntermediateResult> { new DutyCalculationIntermediateResult()
			{ Amount = 11m, PaymentMethod = "BBB", RateCode = "BBB" }, new DutyCalculationIntermediateResult()
			{ Amount = 12m, PaymentMethod = "CCC", RateCode = "CCC" } };
			duties.AddEntryLineLevelIntermediateDuties(dutiesToAdd);
			NUnit.Framework.Assert.That(duties.Count, NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestCalculateDaysDelayedDeclaration()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(DutyCalculationHelper.CalculateDaysDelayedDeclaration(new ZDateTime(2019, 03, 16), new ZDateTime(2019, 03, 01)), NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(DutyCalculationHelper.CalculateDaysDelayedDeclaration(new ZDateTime(2019, 03, 20), new ZDateTime(2019, 03, 01)), NUnit.Framework.Is.EqualTo(4).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(DutyCalculationHelper.CalculateDaysDelayedDeclaration(new ZDateTime(2022, 12, 13), new ZDateTime(2022, 11, 18, 17, 3, 0)), NUnit.Framework.Is.EqualTo(10).Using(CustomComparers.TypeComparison));
			});
		}
	}
}
