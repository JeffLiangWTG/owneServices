using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	class MPFCalculator : ILineFeeCalculator
	{
		internal static class Constants
		{
			public const string Rate3464 = "0.3464%";
			public const string Rate21 = "0.21%";
		}

		public MPFCalculator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public MPFCalculator(BusinessObjectFactory factory, ZBool ignoreTIBExemptionCondition)
		{
			this.factory = factory;
			this.ignoreTIBExemptionCondition = ignoreTIBExemptionCondition;
		}

		readonly BusinessObjectFactory factory;
		readonly ZBool ignoreTIBExemptionCondition;

		public FeeResult CalculateFee(IFeeCalculationDataProvider dataProvider)
		{
			var result = ZDecimal.Zero;

			var isRequired = ShouldHaveFee(dataProvider);
			var feeCalculationInternalData = new FeeCalculationInternalData();
			if (isRequired)
			{
				result = GetAmount(GetTotalCustomsValueIncludingSecondaryLines(dataProvider), dataProvider.DateForMPFCalculation, feeCalculationInternalData);
			}

			return new FeeResult(result, isRequired, feeCalculationInternalData.PercentOfRate, feeCalculationInternalData.NoneCustomsValueAmount);
		}

		ZDecimal GetAmount(ZDecimal customsValue, ZDateTime dateForMPFCalculation, FeeCalculationInternalData feeCalculationInternalData)
		{
			return new FeeCalculationHelper(factory, dateForMPFCalculation).GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, customsValue, feeCalculationInternalData);
		}

		ZDecimal GetTotalCustomsValueIncludingSecondaryLines(IFeeCalculationDataProvider line)
		{
			var result = line.CustomsValue;
			if (CalculateDutyForSetsHelper.IsCombinedXLine(line))
			{
				if (line.IsSecondaryTariffLine)
				{
					result = ZDecimal.Zero;
				}
				else
				{
					foreach (var secondaryLine in line.SecondaryLines)
					{
						result += secondaryLine.CustomsValue;
					}
				}
			}
			return result;
		}

		#region Implementation

		public bool ShouldHaveFee(IDutyData dataProvider)
		{
			return dataProvider.ShouldHaveMPFOrInformalFee(x => ShouldHaveFeeCore(x));
		}

		bool ShouldHaveFeeCore(IDutyData dataProvider)
		{
			ZString entryType = dataProvider.EntryType;

			return
				!EntryTypeList.IsInformal(entryType)
				&& (ignoreTIBExemptionCondition || entryType != EntryTypeList.Codes.TemporaryImportationBond)
				&& !dataProvider.IsSetVLine
				&& !new MPFAndInformalFeeExemptConditionChecker(ignoreTIBExemptionCondition).IsExempt(dataProvider)
				&& USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, dataProvider.Tariff, dataProvider.DateForDutyCalculation, TariffAttributeTypes.Codes.TYPE, TariffAttributeTypes.Values.BabyFomula) == null;
		}

		#endregion
	}
}
