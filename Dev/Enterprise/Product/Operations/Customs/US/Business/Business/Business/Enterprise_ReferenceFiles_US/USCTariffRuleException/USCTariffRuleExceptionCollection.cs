using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCTariffRuleExceptionCollection : ActiveBusinessObjectCollection<USCTariffRuleException>
	{
		public USCTariffRuleExceptionCollection(USCTariffRule tariffRule)
			: base(tariffRule)
		{
			this.tariffRule = tariffRule;
		}

		readonly USCTariffRule tariffRule;

		public bool Applies(ZString tariffNumber, ZDateTime dateOfException)
		{
			foreach (USCTariffRuleException exception in this)
			{
				if (exception.Applies(tariffNumber, dateOfException))
				{
					return true;
				}
			}
			return false;
		}

		public USCTariffRuleException GetABroaderExceptionThan(USCTariffRuleException passedException)
		{
			foreach (USCTariffRuleException exception in this)
			{
				if (exception != passedException &&
					exception.HasBroaderTariffRangesThan(passedException) &&
					(exception.U2_DateFrom.IsEmpty || exception.U2_DateFrom <= passedException.U2_DateFrom) &&
					(exception.U2_DateTo.IsEmpty || exception.U2_DateTo >= passedException.U2_DateTo)
					)
				{
					return exception;
				}
			}

			return null;
		}

		protected override void SetDefaultsForNewElementCore(USCTariffRuleException newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.U2_DateFrom = tariffRule.U1_DateFrom;
			newElement.U2_DateTo = tariffRule.U1_DateTo;
		}
	}
}
