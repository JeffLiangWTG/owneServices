using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public abstract class AmountOrPercentageBasedRegistryHelper
	{
		protected abstract AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection RegistryValue { get; }

		public AmountOrPercentageBasedThreeLevelAuthorisationRequirement GetApplicableSettings(ZDecimal valueToCompare, ZDecimal valueToCompareAgainstForPercentage)
		{
			var settingCollection = RegistryValue;

			List<AmountOrPercentageBasedThreeLevelAuthorisationRequirement> query = null;
			if (settingCollection != null)
			{
				query = (from AmountOrPercentageBasedThreeLevelAuthorisationRequirement setting in settingCollection
						 orderby
							 setting.Range == AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo ? 0 : 1,
							 (setting.Amount > 0 ? setting.Amount : setting.Percentage)
						 select setting).ToList();
			}

			if (query == null || query.Count == 0)
			{
				return null;
			}

			var isPercentages = !query[0].Percentage.IsEmpty;

			ZDecimal proposedIncreaseForComparison;

			if (isPercentages)
			{
				proposedIncreaseForComparison = valueToCompareAgainstForPercentage == 0m ? 0m : ((valueToCompare / valueToCompareAgainstForPercentage) * 100);
			}
			else
			{
				proposedIncreaseForComparison = valueToCompare;
			}

			foreach (var setting in query)
			{
				var settingAmountForComparison = setting.Amount > 0 ? setting.Amount : setting.Percentage;

				if (proposedIncreaseForComparison <= settingAmountForComparison && setting.Range == AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo)
				{
					return setting;
				}

				if (proposedIncreaseForComparison > settingAmountForComparison && setting.Range == AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above)
				{
					return setting;
				}
			}

			return null;
		}
	}
}
