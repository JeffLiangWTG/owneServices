//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionRuleRateValidation
//
//    This class should be used for overriding validation in AutoAccCommissionRuleRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleRateValidation : AutoAccCommissionRuleRateValidation
	{
		public AccCommissionRuleRateValidation(AutoAccCommissionRuleRate parent) : base(parent)
		{
		}

		new AccCommissionRuleRate Parent
		{
			get { return (AccCommissionRuleRate)base.Parent; }
		}

		protected override void CheckACT_CommissionType()
		{
			base.CheckACT_CommissionType();

			MandatoryValidation.CheckEntered(Parent.ACT_CommissionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACT_CommissionTypeInfo);

			var parentRatesProvider = Parent.ParentCommissionRatesProvider;
			if (parentRatesProvider != null)
			{
				var hasDifferentTypeRate = parentRatesProvider.Rates.Any(rate => rate.PK != Parent.PK && rate.ACT_CommissionType != Parent.ACT_CommissionType);
				if (hasDifferentTypeRate)
				{
					Parent.ACT_CommissionTypeInfo.AddError(Res.GetString("ccbbc7d6-94de-4032-aeac-bb7587529a38", "All Rates must have the same Commission Type."));
				}
			}
		}

		protected override void CheckACT_CommissionPercentageIsValidZDecimal()
		{
			if (Parent.ACT_CommissionType == CommissionTypes.Codes.PCT)
			{
				CompareValidation.CheckWithinRange(Parent.ACT_CommissionPercentageInfo, 0, 100);
			}
		}

		protected override void CheckACT_CommissionAmount()
		{
			base.CheckACT_CommissionAmount();

			if (Parent.ACT_CommissionType == CommissionTypes.Codes.FIX)
			{
				CompareValidation.CheckNumberGreaterThanZero(Parent.ACT_CommissionAmountInfo);
			}
		}

		protected override void CheckACT_RX_NKCommissionCurrency()
		{
			base.CheckACT_RX_NKCommissionCurrency();

			if (Parent.IsCurrencyCommissionType())
			{
				MandatoryValidation.CheckEntered(Parent.ACT_RX_NKCommissionCurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ACT_RX_NKCommissionCurrencyInfo);
			}
		}

		protected override void CheckACT_CommissionPeriod()
		{
			base.CheckACT_CommissionPeriod();

			MandatoryValidation.CheckEntered(Parent.ACT_CommissionPeriodInfo);
			ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.ACT_CommissionPeriodInfo, Parent.Lookups.CommissionPeriods, Parent.Lookups.EnabledCommissionPeriods);

			var parentRatesProvider = Parent.ParentCommissionRatesProvider;
			if (parentRatesProvider != null)
			{
				var hasOverlappingRate = parentRatesProvider.Rates.Any(rate => rate.PK != Parent.PK && rate.HasOverlappingCommissionPeriods(Parent));
				if (hasOverlappingRate)
				{
					Parent.ACT_CommissionPeriodInfo.AddError(Res.GetString("2a0ab74f-511b-4be1-b483-14cbe21506e3", "Another rate has overlapping Entitlement Period."));
				}
			}
		}
	}
}
