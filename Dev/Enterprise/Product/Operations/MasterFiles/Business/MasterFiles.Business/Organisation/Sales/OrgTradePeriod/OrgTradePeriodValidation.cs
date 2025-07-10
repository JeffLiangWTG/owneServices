//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTradePeriodValidation
//
//    This class should be used for overriding validation in AutoOrgTradePeriodValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradePeriodValidation : AutoOrgTradePeriodValidation
	{
		public OrgTradePeriodValidation(AutoOrgTradePeriod parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			if (!Parent.TradeDetail.ReadOnly) // Header.Clients is a readonly collection and should not be validated
			{
				base.ValidateAll();
			}
		}

		#region PAS_CurrentRate

		protected override void CheckPAS_CurrentRate()
		{
			base.CheckPAS_CurrentRate();
			CompareValidation.CheckNumberNotNegative(Parent.PAS_CurrentRateInfo);
		}

		#endregion

		#region PAS_RX_NKCurrency

		protected override void CheckPAS_RX_NKCurrency()
		{
			base.CheckPAS_RX_NKCurrency();
			MandatoryValidation.CheckEntered(Parent.PAS_RX_NKCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PAS_RX_NKCurrencyInfo);
		}

		#endregion

		#region PAS_Units

		protected override void CheckPAS_Units()
		{
			base.CheckPAS_Units();
			CompareValidation.CheckNumberNotNegative(Parent.PAS_UnitsInfo);
		}

		#endregion

		#region PAS_RateOffered

		protected override void CheckPAS_RateOffered()
		{
			base.CheckPAS_RateOffered();
			CompareValidation.CheckNumberNotNegative(Parent.PAS_RateOfferedInfo);
		}

		#endregion

		#region PAS_RepeatsMnth

		protected override void CheckPAS_RepeatsMnth()
		{
			base.CheckPAS_RepeatsMnth();
			CompareValidation.CheckNumberNotNegative(Parent.PAS_RepeatsMnthInfo);
		}

		#endregion

		#region PAS_Chargeable

		protected override void CheckPAS_Chargeable()
		{
			CompareValidation.CheckNumberNotNegative(Parent.PAS_ChargeableInfo);
			base.CheckPAS_Chargeable();
		}

		#endregion

		#region PAS_Weight / PAS_WeightUQ

		protected override void CheckPAS_Weight()
		{
			CompareValidation.CheckNumberNotNegative(Parent.PAS_WeightInfo);
			base.CheckPAS_Weight();
		}

		protected override void CheckPAS_WeightUQ()
		{
			base.CheckPAS_WeightUQ();
			ListValidation.ErrorIfInvalidCode(Parent.PAS_WeightUQInfo);
		}

		#endregion

		#region PAS_Volume / PAS_VolumeUQ

		protected override void CheckPAS_Volume()
		{
			CompareValidation.CheckNumberNotNegative(Parent.PAS_VolumeInfo);
			base.CheckPAS_Volume();
		}

		protected override void CheckPAS_VolumeUQ()
		{
			base.CheckPAS_VolumeUQ();
			ListValidation.ErrorIfInvalidCode(Parent.PAS_VolumeUQInfo);
		}

		#endregion

		#region PAS_TEUQuantity

		protected override void CheckPAS_TEUQuantity()
		{
			CompareValidation.CheckNumberNotNegative(Parent.PAS_TEUQuantityInfo);
			base.CheckPAS_TEUQuantity();
		}

		#endregion
	}
}
