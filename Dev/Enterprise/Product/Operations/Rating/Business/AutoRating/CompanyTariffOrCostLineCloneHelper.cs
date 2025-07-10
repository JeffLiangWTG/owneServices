using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	class CompanyTariffOrCostLineCloneHelper
	{
		internal CompanyTariffOrCostLineCloneHelper(RateLine companyTariffOrCostLine, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(companyTariffOrCostLine, "RateLine");
			Argument.NotNull(companyTariffOrCostLine.Parent, "RateEntry");
			Argument.NotNull(companyTariffOrCostLine.Calculator as CompanyTariffOrCostBasedCalculator, "Calculator");

			this.companyTariffOrCostLine = companyTariffOrCostLine;
			Factory = factory ?? new ReadOnlyBusinessObjectFactory();
		}

		internal RateLine GetClone()
		{
			var originalRateLine = GetOriginalRateLineFromCostingOrCompanyTariff();
			if (originalRateLine == null || originalRateLine.PK == companyTariffOrCostLine.PK)
			{
				return null;
			}

			if (companyTariffOrCostLine.UsesCompanyTariffBasedCalculator() && originalRateLine.UsesCostBasedCalculator())
			{
				originalRateLine = new CompanyTariffOrCostLineCloneHelper(originalRateLine, Factory).GetClone();
			}
			else if (originalRateLine.UsesCompanyTariffBasedCalculator() && originalRateLine.TL_CompanyTariffLevel > 1)
			{
				//A new factory required because different level company tariffs have to be loaded in different factories to work properly
				originalRateLine = new CompanyTariffOrCostLineCloneHelper(originalRateLine).GetClone();
			}

			return originalRateLine == null || originalRateLine.UsesCompanyTariffOrCostBasedCalculator() ? null : CreateClone(originalRateLine, Factory);
		}

		RateLine GetOriginalRateLineFromCostingOrCompanyTariff()
		{
			if (Calculator == null || RatingCriteria == null)
			{
				return null;
			}

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(RatingCriteria, new FreightAutoRater(new RatingContext()));
			var originalLines = Calculator.GetRelatedLines(calculatorParameters).ToList();

			if (originalLines.Count == 0)
			{
				return null;
			}

			if (originalLines.Count > 1)
			{
				var remover = new OverriddenRateLineRemover(RatingCriteria, Calculator.IsCostBasedCalculator, true, includeContractNumberComparer: true);
				remover.Remove(originalLines);
			}

			return originalLines[0] as RateLine;
		}

		internal RateLine CreateClone(RateLine originalRateLine, BusinessObjectFactory factory = null, RateLineItem.RateTypeToUpdate rateTypeToUpdate = RateLineItem.RateTypeToUpdate.Relevant)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			var clonedLine = companyTariffOrCostLine.Clone(factory);

			using (clonedLine.GetValidationSuspender())
			using (clonedLine.SuspendSettingHasChanges())
			{
				clonedLine.TL_RateCalculator = originalRateLine.Calculator.GetCloneCode(Calculator);
				if (clonedLine.TL_RateCalculator.IsEmpty)
				{
					return null;
				}

				originalRateLine.Calculator.CloneLineItemsUpdatingRateValues(Calculator, clonedLine, rateTypeToUpdate);

				clonedLine.TL_RX_NKCurrency = originalRateLine.TL_RX_NKCurrency;
				clonedLine.TL_WeightVolume = originalRateLine.TL_WeightVolume;

				// ConversionFactor should be set after TL_WeightVolume because when ConversionFactor is empty, setting TL_WeightVolume would set ConversionFactory to hard-coded default value
				clonedLine.ConversionFactor = originalRateLine.ConversionFactor;

				clonedLine.TL_ContainerOwnership = originalRateLine.TL_ContainerOwnership;
				clonedLine.TL_WeightVolumeMultiple = originalRateLine.TL_WeightVolumeMultiple;
				clonedLine.TL_ActualPercentage = originalRateLine.TL_ActualPercentage;

				clonedLine.ClonedLineMaster = companyTariffOrCostLine;
				clonedLine.TL_Condition = originalRateLine.TL_Condition;
				clonedLine.TL_ConditionalExpression = originalRateLine.TL_ConditionalExpression;
				clonedLine.TL_ConditionalExpressionDescription = originalRateLine.TL_ConditionalExpressionDescription;

				if (Calculator.ShowMessageTypeSubType && clonedLine.Calculator.ShowMessageTypeSubType)
				{
					clonedLine.Calculator.MessageType = Calculator.MessageType;
					clonedLine.Calculator.MessageSubType = Calculator.MessageSubType;
				}

				clonedLine.SendErrorReporterIfParentIsNull("CompanyTariffOrCostBasedCalculator.GetClone_EmptyParent");

				return clonedLine;
			}
		}

		#region Implementation

		readonly RateLine companyTariffOrCostLine;
		readonly BusinessObjectFactory Factory;

		CompanyTariffOrCostBasedCalculator Calculator
		{
			get { return calculator ?? (calculator = companyTariffOrCostLine.GetCalculator<CompanyTariffOrCostBasedCalculator>()); }
		}
		CompanyTariffOrCostBasedCalculator calculator;

		RatingCriteria RatingCriteria
		{
			get { return ratingCriteria ?? (ratingCriteria = new RatingCriteria(RateEntryAdapter, Factory)); }
		}
		RatingCriteria ratingCriteria;

		RateEntryAdapter RateEntryAdapter
		{
			get { return rateEntryAdapter ?? (rateEntryAdapter = new RateEntryAdapter(companyTariffOrCostLine)); }
		}
		RateEntryAdapter rateEntryAdapter;

		#endregion
	}
}

