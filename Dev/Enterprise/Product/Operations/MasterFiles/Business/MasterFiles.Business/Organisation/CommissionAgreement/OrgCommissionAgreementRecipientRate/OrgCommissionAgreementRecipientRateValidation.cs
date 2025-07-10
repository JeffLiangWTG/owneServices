//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementRecipientRateValidation
//
//    This class should be used for overriding validation in AutoOrgCommissionAgreementRecipientRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementRecipientRateValidation : AutoOrgCommissionAgreementRecipientRateValidation
	{
		public OrgCommissionAgreementRecipientRateValidation(AutoOrgCommissionAgreementRecipientRate parent) : base(parent)
		{
			this.ZValidationInternals = this;
		}

		new OrgCommissionAgreementRecipientRate Parent
		{
			get { return (OrgCommissionAgreementRecipientRate)base.Parent; }
		}

		protected override void CheckCAT_CommissionPercentageIsValidZDecimal()
		{
			if (Parent.CommissionAgreementRecipient != null && Parent.CommissionAgreementRecipient.CAR_CommissionType == CommissionTypes.Codes.PCT)
			{
				CompareValidation.CheckWithinRange(Parent.CAT_CommissionPercentageInfo, 0, 100);
			}
		}

		protected override void CheckCAT_CommissionAmount()
		{
			base.CheckCAT_CommissionAmount();

			if (Parent.CommissionAgreementRecipient != null && Parent.CommissionAgreementRecipient.CAR_CommissionType == CommissionTypes.Codes.FIX)
			{
				CompareValidation.CheckNumberGreaterThanZero(Parent.CAT_CommissionAmountInfo);
			}
		}

		protected override void CheckCAT_RX_NKCommissionCurrency()
		{
			base.CheckCAT_RX_NKCommissionCurrency();

			if (Parent.IsCurrencyCommissionType())
			{
				MandatoryValidation.CheckEntered(Parent.CAT_RX_NKCommissionCurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.CAT_RX_NKCommissionCurrencyInfo);
			}
		}

		protected override void CheckCAT_CommissionPeriod()
		{
			base.CheckCAT_CommissionPeriod();

			ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.CAT_CommissionPeriodInfo, Parent.Lookups.CommissionPeriods, Parent.Lookups.EnabledCommissionPeriods);

			var recipient = Parent.CommissionAgreementRecipient;
			if (recipient != null)
			{
				var hasOverlappingRate = recipient.Rates.Any(rate => rate.PK != Parent.PK && rate.HasOverlappingCommissionPeriods(Parent));
				if (hasOverlappingRate)
				{
					Parent.CAT_CommissionPeriodInfo.AddError(Res.GetString("6ff36727-6afa-45da-a8d7-0c4d3133125e", "Another rate has overlapping Entitlement Period."));
				}
			}
		}

		#region CAT_CommissionStartDate

		public void ValidateCAT_CommissionStartDate()
		{
			ZValidationInternals.Validate(Parent.CAT_CommissionStartDateInfo, GetCAT_CommissionStartDateValidationInvoker());
		}

		RunValidationInvoker GetCAT_CommissionStartDateValidationInvoker()
		{
			return delegate
			{
				CheckCAT_CommissionStartDate();
			};
		}

		protected virtual void CheckCAT_CommissionStartDate()
		{
			CompareValidation.CheckDateIsBeforeAnotherDate(Parent.CAT_CommissionStartDateInfo, Parent.CAT_CommissionEndDateInfo);

			if (Parent.CAT_CommissionPeriod.IsEmpty)
			{
				var recipient = Parent.CommissionAgreementRecipient;
				if (recipient != null)
				{
					var hasOverlappingRate = recipient.Rates.Any(rate => rate.PK != Parent.PK
						&& rate.CAT_CommissionPeriod.IsEmpty
						&& ZDateTime.Overlaps(rate.CAT_CommissionStartDate, rate.CAT_CommissionEndDate, Parent.CAT_CommissionStartDate, Parent.CAT_CommissionEndDate));
					if (hasOverlappingRate)
					{
						Parent.CAT_CommissionStartDateInfo.AddError(OverlappingDatesErrorMessage);
					}
				}
			}

			var agreement = Parent.CommissionAgreement;
			if (agreement != null)
			{
				var agreementEffectiveDate = agreement.EffectiveDate;
				if (!agreementEffectiveDate.IsEmpty && (Parent.CAT_CommissionStartDate.IsEmpty || Parent.CAT_CommissionStartDate < agreementEffectiveDate))
				{
					Parent.CAT_CommissionStartDateInfo.AddWarning(Res.GetString("69a3aed6-eea6-4108-8684-31acced065f1", "The agreement effective date of '{0}' will take precedence over this rate start date.", agreementEffectiveDate.ToShortDateString()));
				}
			}
		}

		#endregion

		#region CAT_CommissionEndDate

		public void ValidateCAT_CommissionEndDate()
		{
			ZValidationInternals.Validate(Parent.CAT_CommissionEndDateInfo, GetCAT_CommissionEndDateValidationInvoker());
		}

		RunValidationInvoker GetCAT_CommissionEndDateValidationInvoker()
		{
			return delegate
			{
				CheckCAT_CommissionEndDate();
			};
		}

		protected virtual void CheckCAT_CommissionEndDate()
		{
			CompareValidation.CheckDateIsAfterAnotherDate(Parent.CAT_CommissionEndDateInfo, Parent.CAT_CommissionStartDateInfo);

			var recipient = Parent.CommissionAgreementRecipient;
			if (recipient != null)
			{
				if (Parent.CAT_CommissionPeriod.IsEmpty)
				{
					var hasOverlappingRate = recipient.Rates.Any(rate => rate.PK != Parent.PK
						&& rate.CAT_CommissionPeriod.IsEmpty
						&& ZDateTime.Overlaps(rate.CAT_CommissionStartDate, rate.CAT_CommissionEndDate, Parent.CAT_CommissionStartDate, Parent.CAT_CommissionEndDate));
					if (hasOverlappingRate)
					{
						Parent.CAT_CommissionEndDateInfo.AddError(OverlappingDatesErrorMessage);
					}
				}
			}

			var hasCappedWarning = false;
			var agreement = Parent.CommissionAgreement;
			if (agreement != null)
			{
				var agreementExpiredDate = agreement.CA0_ExpiredDate;
				if (!agreement.CA0_ExpiredDate.IsEmpty && (Parent.CAT_CommissionEndDate.IsEmpty || Parent.CAT_CommissionEndDate > agreement.CA0_ExpiredDate))
				{
					Parent.CAT_CommissionEndDateInfo.AddWarning(Res.GetString("4ca110b5-6793-4be5-b585-e92fd7dd7b91", "The agreement expiry date of '{0}' will take precedence over this rate end date.", agreement.CA0_ExpiredDate.ToShortDateString()));
					hasCappedWarning = true;
				}
			}

			if (!hasCappedWarning && recipient != null)
			{
				var recipientEndDate = recipient.CAR_EndDate;
				if (!recipientEndDate.IsEmpty && (Parent.CAT_CommissionEndDate.IsEmpty || Parent.CAT_CommissionEndDate > recipientEndDate))
				{
					Parent.CAT_CommissionEndDateInfo.AddWarning(Res.GetString("bc09a39f-d2cf-4220-b298-9ad3cb01aae4", "The entity entitlement end date of '{0}' will take precedence over this rate end date.", recipientEndDate.ToShortDateString()));
				}
			}
		}

		#endregion

		string OverlappingDatesErrorMessage
		{
			get { return Res.GetString("a9d54531-1fb7-472c-bbaa-08ab569a4ff9", "Another rate has overlapping Start/End dates."); }
		}

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();

			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCAT_CommissionEndDate();
				ValidateCAT_CommissionStartDate();
			}
		}

		#endregion

		#region Implementation

		readonly IValidationInternals ZValidationInternals;

		#endregion
	}
}
