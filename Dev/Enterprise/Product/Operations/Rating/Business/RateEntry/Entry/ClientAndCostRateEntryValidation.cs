using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class ClientAndCostRateEntryValidation : RateEntryValidation
	{
		public ClientAndCostRateEntryValidation(AutoRateEntry parent)
			: base(parent)
		{
		}

		#region Date Validator

		DateValidator dateValidator;
		DateValidator DateValidator
		{
			get { return dateValidator ?? (dateValidator = new DateValidator(Parent, Parent.TI_RateStartDateInfo, Parent.TI_RateEndDateInfo)); }
		}

		#endregion

		#region TI_RateStartDate

		protected override void CheckTI_RateStartDate()
		{
			base.CheckTI_RateStartDate();
			if (Parent.HasChanges)
			{
				DateValidator.ValidateStartDate();
				ValidateTI_RateEndDate();
				var entryStartDate = Parent.TI_RateStartDate;
				if (entryStartDate.IsValid &&
					Parent is RateEntry entry &&
					entry.ChildRateLines.Any(x => !x.TL_RateStartDate.IsEmpty && x.TL_RateStartDate < entryStartDate))
				{
					Parent.TI_RateStartDateInfo.AddError(Res.GetString("3FBA9876-D2F6-4702-8989-00290AAB6D15", "The Start Date of Rate Entry must be the same day or before the Start Date of all Rate Lines under this Rate Entry."));
				}
			}
		}

		#endregion

		#region TI_RateEndDate

		protected override void CheckTI_RateEndDate()
		{
			base.CheckTI_RateEndDate();
			if (Parent.HasChanges)
			{
				DateValidator.ValidateEndDate(true);
				ValidateTI_RateStartDate();
				var entryEndDate = Parent.TI_RateEndDate;
				if (entryEndDate.IsValid &&
					Parent is RateEntry entry &&
					entry.ChildRateLines.Any(x => !x.TL_RateEndDate.IsEmpty && x.TL_RateEndDate > entryEndDate))
				{
					Parent.TI_RateEndDateInfo.AddError(Res.GetString("12AE10E8-B231-4153-8F89-A0D81E1EFBC4", "The Expiry Date of Rate Entry must be the same day or after the Expiry Date of all Rate Lines under this Rate Entry."));
				}
			}
		}

		#endregion

		#region TI_OA_CartagePickupAddress / TI_OA_CartageDeliveryAddress

		protected override void CheckTI_OA_CartageDeliveryAddressOverride()
		{
			base.CheckTI_OA_CartageDeliveryAddressOverride();
			ListValidation.ErrorIfInvalidPK(Parent.TI_OA_CartageDeliveryAddressOverrideInfo, Parent.Lookups.CartageDeliveryAddressOverrides);
		}

		protected override void CheckTI_OA_CartagePickupAddressOverride()
		{
			base.CheckTI_OA_CartagePickupAddressOverride();
			ListValidation.ErrorIfInvalidPK(Parent.TI_OA_CartagePickupAddressOverrideInfo, Parent.Lookups.CartagePickupAddressOverrides);
		}

		#endregion
	}
}

