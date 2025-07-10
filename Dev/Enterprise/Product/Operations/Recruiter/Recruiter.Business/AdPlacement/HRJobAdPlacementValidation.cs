using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRJobAdPlacementValidation : AutoHRJobAdPlacementValidation
	{
		public HRJobAdPlacementValidation(AutoHRJobAdPlacement parent) : base(parent)
		{
		}

		#region HQ_RX_NKAdCostCurrency

		protected override void CheckHQ_RX_NKAdCostCurrency()
		{
			base.CheckHQ_RX_NKAdCostCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.HQ_RX_NKAdCostCurrencyInfo, Parent.Lookups.AdCostCurrencies);
		}

		#endregion

		#region HQ_EffectiveStartDate

		protected override void CheckHQ_EffectiveStartDate()
		{
			base.CheckHQ_EffectiveStartDate();
			if (Parent.HQ_EffectiveStartDate.IsValid && Parent.HQ_EffectiveEndDate.IsValid)
			{
				if (Parent.HQ_EffectiveStartDate.CompareTo(Parent.HQ_EffectiveEndDate) > 0)
				{
					Parent.HQ_EffectiveStartDateInfo.AddError(Res.GetString("36e36f40-8a42-4e14-8bea-7c25f45077bb", "The effective Start Date of an Ad Placement should be before Effective End Date"));
				}
			}
		}

		#endregion

		#region HQ_EffectiveEndDate

		protected override void CheckHQ_EffectiveEndDate()
		{
			base.CheckHQ_EffectiveEndDate();
			if (Parent.HQ_EffectiveStartDate.IsValid && Parent.HQ_EffectiveEndDate.IsValid)
			{
				if (Parent.HQ_EffectiveEndDate.CompareTo(Parent.HQ_EffectiveStartDate) < 0)
				{
					Parent.HQ_EffectiveEndDateInfo.AddError(Res.GetString("4111d559-2435-4e67-9b0f-3a940d219048", "The effective End Date of an Ad Placement should be after Effective Start Date"));
				}
			}
		}

		#endregion

		#region HQ_AdBookedDate

		protected override void CheckHQ_AdBookedDate()
		{
			base.CheckHQ_AdBookedDate();
			MandatoryValidation.CheckEntered(Parent.HQ_AdBookedDateInfo);
		}

		#endregion

		#region HQ_AdBookedIn

		protected override void CheckHQ_AdBookedIn()
		{
			base.CheckHQ_AdBookedIn();
			ListValidation.ErrorIfInvalidCode(Parent.HQ_AdBookedInInfo, Parent.Lookups.AdPlacementPublicationsList);
		}

		#endregion

	}
}
