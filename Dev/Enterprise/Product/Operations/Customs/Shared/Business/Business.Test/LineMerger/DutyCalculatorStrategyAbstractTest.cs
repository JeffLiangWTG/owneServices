using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(DutyCalculatorStrategy))]
	public abstract class DutyCalculatorStrategyAbstractTest<T> : TestCaseWithFactory where T : DutyCalculatorStrategy
	{
		protected abstract T GetDutyCalculatorStrategy();

		public virtual void TestCalculateDuties()
		{
			SetupReferenceData();
			var declaration = Declaration;
			var entryLine1 = GetEntryHeader(declaration, TariffCode_0101100000).MergedLines[0];
			var entryLine2 = GetEntryHeader(declaration, TariffCode_0102200000).MergedLines[0];

			GetDutyCalculatorStrategy().CalculateDuties();
			CombineAssertions(() =>
			{
				if (ExpectedShouldCalculateDuties)
				{
					AssertEquals("entryLine1: Duty calculated and added to Fee", 1, entryLine1.Fees.Count);
					AssertEquals("entryLine1: CF_ChargeAmount", ExpectedShouldTruncate ? 17m : 17.60m, entryLine1.Fees[0].CF_ChargeAmount);
					AssertEquals("entryLine1: EntryLineFeeType", ExpectedEntryLineFeeType, entryLine1.Fees[0].CF_ChargeType);

					if (ExpectedCanBeNegative)
					{
						AssertEquals("entryLine2: Duty calculated and added to Fee", 1, entryLine2.Fees.Count);
						AssertEquals("entryLine2: CF_ChargeAmount", -5m, entryLine2.Fees[0].CF_ChargeAmount);
						AssertEquals("entryLine2: EntryLineFeeType", ExpectedEntryLineFeeType, entryLine2.Fees[0].CF_ChargeType);
					}
					else
					{
						AssertEquals("entryLine2: Duty calculated and 0 not added to Fee", 0, entryLine2.Fees.Count);
					}
				}
				else
				{
					AssertEquals("entryLine1: Not calculate", 0, entryLine1.Fees.Count);
					AssertEquals("entryLine2: Not calculate", 0, entryLine2.Fees.Count);
				}
			});
		}

		public void TestCanBeNegative()
		{
			var strategy = GetDutyCalculatorStrategy();
			AssertEquals(ExpectedCanBeNegative, (bool)strategy.GetType().GetProperty("CanBeNegative", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(strategy));
		}

		protected virtual bool ExpectedCanBeNegative => false;

		public void TestShouldTruncate()
		{
			var strategy = GetDutyCalculatorStrategy();
			AssertEquals(ExpectedShouldTruncate, (bool)strategy.GetType().GetProperty("ShouldTruncate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(strategy));
		}

		protected virtual bool ExpectedShouldTruncate => false;

		public void TestShouldCalculateDuties()
		{
			var strategy = GetDutyCalculatorStrategy();
			AssertEquals(ExpectedShouldCalculateDuties, (bool)strategy.GetType().GetProperty("ShouldCalculateDuties", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(strategy));
		}

		protected virtual bool ExpectedShouldCalculateDuties => false;

		public void TestGetEntryLineFeeType()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = referenceDataHelper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.Country.Code, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, ExpectedEntryLineFeeType, rateType.PK);
			Factory.Save();

			var strategy = GetDutyCalculatorStrategy();
			var rate = Factory.New<RateView>();
			rate.ZZ2_ZY1_RateCode = rateCode.PK;
			AssertEquals(ExpectedEntryLineFeeType, (ZString)strategy.GetType().GetMethod("GetEntryLineFeeType", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(strategy, new object[] { rate }));
		}

		protected virtual ZString ExpectedEntryLineFeeType => "RC1";

		public void TestEntryLineUniversalRateType()
		{
			var strategy = GetDutyCalculatorStrategy();
			var getEntryLineUniversalDataMethod = strategy.GetType().GetMethod("GetEntryLineUniversalData", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertEquals("EntryLineUniversalRate Type", EntryLineUniversalRateType, getEntryLineUniversalDataMethod.Invoke(strategy, new object[] { Declaration.ActiveEntryHeaders[0].MergedLines[0] }).GetType());
		}

		protected virtual Type EntryLineUniversalRateType => typeof(EntryLineUniversalRate);

		public void TestDutyDecimalPlace()
		{
			var strategy = GetDutyCalculatorStrategy();
			var dutyDecimalPlaceField = strategy.GetType().GetProperty("DutyDecimalPlace", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertEquals("DutyDecimalPlace", ExpectedDutyDecimalPlace, dutyDecimalPlaceField.GetValue(strategy));
		}

		protected virtual int ExpectedDutyDecimalPlace => GlbCompany.CurrentCompany.LocalCurrency.ISODecimals;

		protected virtual BaseJobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CreateEntryLineAndInvoiceLine(declaration, TariffCode_0101100000, 22m);
			CreateEntryLineAndInvoiceLine(declaration, TariffCode_0102200000, -10m);
			return declaration;
		}

		protected (CusEntryLine, BaseJobComInvoiceLine) CreateEntryLineAndInvoiceLine(BaseJobDeclaration declaration, ZString tariffCode, ZDecimal linePrice)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffCode;
			invoiceLine.JI_LinePrice = linePrice;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = tariffCode;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = linePrice;
			entryLine.CL_AdValoremTariff = tariffCode;
			invoiceLine.JI_CL = entryLine.PK;

			return (entryLine, invoiceLine);
		}

		protected CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration, ZString reference)
		{
			return declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single(x => x.CH_BGMReference == reference);
		}

		protected void SetupReferenceData(string cusRateType = null, string cusRateCode = null, string customsValueFormula = null)
		{
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, UniversalTariffType);
			var tariff = referenceDataHelper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, TariffCode_0101100000, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Tariff");
			var tariff2 = referenceDataHelper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, TariffCode_0102200000, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Tariff");
			var tariff3 = referenceDataHelper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, TariffCode_0103300000, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Tariff");
			referenceDataHelper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, TariffCode_0104400000, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Tariff");
			var rateType = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, cusRateType ?? Universal.Constants.RateTypes.Duty);
			if (customsValueFormula != null)
			{
				rateType.ZZR_CustomsValueFormula = customsValueFormula;
			}
			var rateCode = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, cusRateCode ?? "RC1", rateType.PK);
			var rateForTariff = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.8", dataGrouping: dataGrouping);
			var rateForTariff2 = referenceDataHelper.CreateRate(tariff2, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.5", dataGrouping: dataGrouping);
			var rateForTariff3 = referenceDataHelper.CreateRate(tariff3, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, @"({DECIMAL(6,3):""User Enter Value""})*0.1", dataGrouping: dataGrouping);

			var tradeGroup = referenceDataHelper.LoadOrCreateTradeGroup(dataGrouping, "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.CreateCusApplicability(rateForTariff, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateForTariff2, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateForTariff3, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
		}

		protected virtual ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected BaseJobDeclaration Declaration => _declaration ?? (_declaration = GetDeclaration());
		BaseJobDeclaration _declaration;

		protected const string TariffCode_0101100000 = "0101100000";
		protected const string TariffCode_0102200000 = "0102200000";
		protected const string TariffCode_0103300000 = "0103300000";
		protected const string TariffCode_0104400000 = "0104400000";

		protected void AssertEntryLineFee(CusEntryLine entryLine, ExpectedFeeRecord[] expectedFees, IFeeRounder feeRounder)
		{
			var orderedFees = entryLine.Fees
				.Cast<CusEntryLineFee>()
				.OrderBy(f => $"{f.CF_RateOverrideReasonCode}.{f.CF_ChargeType}.{f.CF_MethodOfCalculation}")
				.ToArray();
			var expectedOrderedFees = expectedFees
				.OrderBy(f => $"{f.OverrideReason}.{f.ChargeType}.{f.MethodOfCalculation}")
				.ToArray();

			var upperBound = Math.Max(orderedFees.Length, expectedOrderedFees.Length);
			var lowerBound = Math.Min(orderedFees.Length, expectedOrderedFees.Length);
			CombineAssertions(() =>
			{
				AssertEquals("Number of Fees", expectedOrderedFees.Length, orderedFees.Length);

				for (int index = 0; index < lowerBound; index++)
				{
					var expectedFee = expectedOrderedFees[index];
					var actualFee = orderedFees[index];

					var feeId = $"Fee ({actualFee.CF_RateOverrideReasonCode}.{actualFee.CF_ChargeType}.{actualFee.CF_MethodOfCalculation})";
					AssertEquals($"{feeId} CF_ChargeType", expectedFee.ChargeType, actualFee.CF_ChargeType);
					AssertEquals($"{feeId} CF_ChargeAmount", GetExpectedChargeAmount(expectedFee, feeRounder), actualFee.CF_ChargeAmount);
					AssertEquals($"{feeId} CF_BaseValue", expectedFee.BaseValue, actualFee.CF_BaseValue);
					AssertEquals($"{feeId} CF_Rate", expectedFee.Rate, actualFee.CF_Rate);
					AssertEquals($"{feeId} CF_MethodOfCalculation", expectedFee.MethodOfCalculation, actualFee.CF_MethodOfCalculation);
					AssertEquals($"{feeId} CF_RateOverrideReasonCode", expectedFee.OverrideReason, actualFee.CF_RateOverrideReasonCode);
				}

				const string notMatchingFeesMessageFormat = "{0} (Type [{1}], Amount [{2}], BaseValue [{3}], Rate [{4}], MethodOfCalculation [{5}], OverrideReason [{6}])";

				for (int i = lowerBound; i < upperBound; i++)
				{
					var notMatchingFeesInfo = (expectedOrderedFees.Length > lowerBound)
						? string.Format(notMatchingFeesMessageFormat, "Expected fee not found", expectedFees[i].ChargeType, expectedFees[i].ChargeAmount, expectedFees[i].BaseValue, expectedFees[i].Rate, expectedFees[i].MethodOfCalculation, expectedFees[i].OverrideReason)
						: string.Format(notMatchingFeesMessageFormat, "Unexpected fee found", orderedFees[i].CF_ChargeType, orderedFees[i].CF_ChargeAmount, orderedFees[i].CF_BaseValue, orderedFees[i].CF_Rate, orderedFees[i].CF_MethodOfCalculation, orderedFees[i].CF_RateOverrideReasonCode);
					Fail(notMatchingFeesInfo);
				}
			});

			ZDecimal GetExpectedChargeAmount(ExpectedFeeRecord expectedFee, IFeeRounder rounder) => rounder.Round(expectedFee.ChargeAmount);
		}
	}
}
