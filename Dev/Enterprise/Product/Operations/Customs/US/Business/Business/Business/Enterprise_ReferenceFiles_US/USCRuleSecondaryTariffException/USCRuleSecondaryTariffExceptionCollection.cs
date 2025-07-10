using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCRuleSecondaryTariffExceptionCollection : ActiveBusinessObjectCollection<USCRuleSecondaryTariffException>
	{
		public USCRuleSecondaryTariffExceptionCollection(USCRuleSecondaryTariff secondaryTariffRule)
			: base(secondaryTariffRule)
		{
			this.secondaryTariffRule = secondaryTariffRule;
		}

		readonly USCRuleSecondaryTariff secondaryTariffRule;

		public bool Applies(ZString tariffNumber, ZDateTime dateOfException)
		{
			foreach (USCRuleSecondaryTariffException exception in this)
			{
				if (exception.Applies(tariffNumber, dateOfException))
				{
					return true;
				}
			}
			return false;
		}

		public USCRuleSecondaryTariffException GetABroaderExceptionThan(USCRuleSecondaryTariffException passedException)
		{
			foreach (USCRuleSecondaryTariffException exception in this)
			{
				if (exception != passedException &&
					exception.HasBroaderTariffRangesThan(passedException) &&
					(exception.U4_DateFrom.IsEmpty || exception.U4_DateFrom <= passedException.U4_DateFrom) &&
					(exception.U4_DateTo.IsEmpty || exception.U4_DateTo >= passedException.U4_DateTo)
					)
				{
					return exception;
				}
			}

			return null;
		}

		protected override void SetDefaultsForNewElementCore(USCRuleSecondaryTariffException newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.U4_DateFrom = secondaryTariffRule.U3_DateFrom;
			newElement.U4_DateTo = secondaryTariffRule.U3_DateTo;
		}
	}
}
