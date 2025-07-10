using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class QuoteValidation : RatingHeaderValidation
	{
		public QuoteValidation(AutoRatingHeader parent)
			: base(parent)
		{
		}

		public new Quote Parent
		{
			get { return (Quote)base.Parent; }
		}

		#region TH_QuoteDate

		protected override void CheckTH_QuoteDate()
		{
			base.CheckTH_QuoteDate();
			DateValidator.ValidateStartDate();
			ValidateTH_QuoteEndDate();
			ValidateTH_FollowUpDate();
		}

		#endregion

		#region TH_QuoteEndDate

		protected override void CheckTH_QuoteEndDate()
		{
			base.CheckTH_QuoteEndDate();
			DateValidator.ValidateEndDate(!Env.Registry.Rating.QuoteEndDateMandatory);
			ValidateTH_QuoteDate();
			ValidateTH_FollowUpDate();
		}

		#endregion

		#region TH_FollowUpDate

		protected override void CheckTH_FollowUpDate()
		{
			base.CheckTH_FollowUpDate();
			if (Parent.TH_QuoteDate > Parent.TH_FollowUpDate || Parent.TH_QuoteEndDate < Parent.TH_FollowUpDate)
			{
				Parent.TH_FollowUpDateInfo.AddWarning(ErrorMessages.FollowUpDateOutsideQuotationPeriod);
			}
		}

		#endregion

		#region TH_GS_NKFirstSignatory / TH_GS_NKSecondSignatory

		protected override void CheckTH_GS_NKFirstSignatory()
		{
			// For OOQ, Signatures are not visible and only set by default via SetDefaultSignature
			// For SpotQuote created from Web, Signatures are not visible and saved as empty
			// For Quotation module, Signatures are visible and can be set
			if (Parent.TH_OneTimeQuote)
			{
				return;
			}

			base.CheckTH_GS_NKFirstSignatory();

			if (Parent.TH_GS_NKFirstSignatory.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.TH_GS_NKFirstSignatoryInfo);
			}
			else if (Parent.FirstSignatory == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TH_GS_NKFirstSignatoryInfo, Parent.Lookups.FirstSignatories);
			}
		}

		protected override void CheckTH_GS_NKSecondSignatory()
		{
			if (Parent.TH_OneTimeQuote)
			{
				return;
			}

			base.CheckTH_GS_NKSecondSignatory();

			if (!Parent.TH_GS_NKSecondSignatory.IsEmpty && Parent.SecondSignatory == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TH_GS_NKSecondSignatoryInfo, Parent.Lookups.SecondSignatories);
			}
		}

		#endregion

		#region TH_QuoteCancellationReason

		protected override void CheckTH_QuoteCancellationReason()
		{
			base.CheckTH_QuoteCancellationReason();

			if (Parent.IsCancelled && Parent.TH_IsCancelledInfo.HasChanges)
			{
				if (RatingDataRegistry.Instance.IsQuoteCancellationReasonCodeRequired.Value)
				{
					MandatoryValidation.CheckEntered(Parent.TH_QuoteCancellationReasonInfo);
				}
				ListValidation.ErrorIfInvalidCode(Parent.TH_QuoteCancellationReasonInfo);
			}
		}

		#endregion

		#region Validation Objects

		DateValidator DateValidator
		{
			get
			{
				if (fDateValidator == null)
				{
					fDateValidator = new DateValidator(Parent, Parent.TH_QuoteDateInfo, Parent.TH_QuoteEndDateInfo);
				}
				return fDateValidator;
			}
		}

		DateValidator fDateValidator;

		#endregion

		protected override void CheckTH_OHIsValidZGuid()
		{
		}

		protected override void CheckTH_OH()
		{
			Parent.QuotationClientAddress.Validation.ValidateOrganisationPK();
		}
	}
}

