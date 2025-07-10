using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCRuleSecondaryTariffCollection : ActiveBusinessObjectCollection<USCRuleSecondaryTariff>
	{
		public USCRuleSecondaryTariffCollection(USCTariffRule tariffRule)
			: base(tariffRule)
		{
		}

		public USCRuleSecondaryTariff GetABroaderSecondaryTariffThan(USCRuleSecondaryTariff passedSecondaryTariff)
		{
			foreach (USCRuleSecondaryTariff secondaryTariff in this)
			{
				if (secondaryTariff != passedSecondaryTariff)
				{
					bool secondaryTariffHasBroaderDate = ((secondaryTariff.U3_DateFrom.IsEmpty || (secondaryTariff.U3_DateFrom <= passedSecondaryTariff.U3_DateFrom)) &&
						(secondaryTariff.U3_DateTo.IsEmpty || (secondaryTariff.U3_DateTo >= passedSecondaryTariff.U3_DateTo)));

					if (secondaryTariffHasBroaderDate && secondaryTariff.HasBroaderTariffRangesThan(passedSecondaryTariff))
					{
						return secondaryTariff;
					}
				}
			}

			return null;
		}

		public USCRuleSecondaryTariff GetMatchingSecondaryTariffRule(ZString tariffNumber, ZDateTime effectiveDate)
		{
			foreach (USCRuleSecondaryTariff secondaryTariff in this)
			{
				if (secondaryTariff.IsValid(tariffNumber, effectiveDate))
				{
					return secondaryTariff;
				}
			}
			return null;
		}
	}
}
