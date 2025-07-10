using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class DeferredSubmissionValidation : ZValidation
	{
		public DeferredSubmissionValidation(DeferredSubmission parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly DeferredSubmission parent;

		public override Type AutoValidationType => typeof(DeferredSubmissionValidation);

		public override void ValidateAll()
		{
			ValidateSubmissionDate();
			ValidateDeferredAccount();
			ValidatePaymentMethod();
		}

		#region SubmissionDate

		public void ValidateSubmissionDate()
		{
			ValidateCalculatedProperty(parent.SubmissionDateInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This method is implicitly referenced by ZAttribute")]
		void CheckSubmissionDate()
		{
			if (parent.IsOverwritten)
			{
				var targetInfo = parent.SubmissionDateInfo;

				var submissionDate = parent.SubmissionDate;
				if (submissionDate < ZDate.Today)
				{
					targetInfo.AddError(ResString.GetMultilingualString("2186BEA6-2366-4941-9BC7-562BE941DBF9", "Submission Date cannot be earlier than today."));
				}
				else if (submissionDate > parent.DateOfArrival && parent.DateOfArrival >= ZDate.Today)
				{
					targetInfo.AddError(ResString.GetMultilingualString("EF619AE0-343F-442D-8539-A137F2CA5304", "Submission Date cannot be later than Arrival Date."));
				}
				else if (submissionDate > ZDate.Today.AddDays(21))
				{
					targetInfo.AddError(ResString.GetMultilingualString("AEBE91CF-7D24-406C-A072-7BF089C5919D", "Submission Date cannot be more than 21 days from today."));
				}
			}
		}

		#endregion

		#region DeferredAccount

		public void ValidateDeferredAccount()
		{
			ValidateCalculatedProperty(parent.DeferredAccountInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This method is implicitly referenced by ZAttribute")]
		void CheckDeferredAccount()
		{
			if (parent.IsOverwritten && parent.DeferredAccount.IsEmpty)
			{
				parent.DeferredAccountInfo.AddError(ResString.GetMultilingualString("f4c7f962-23d3-4f2d-999d-c56642c7a125", "An Account must be selected."));
			}
		}

		#endregion

		#region Payment Method

		public void ValidatePaymentMethod()
		{
			ValidateCalculatedProperty(parent.PaymentMethodInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This method is implicitly referenced by ZAttribute")]
		void CheckPaymentMethod()
		{
			if (parent.IsOverwritten && parent.PaymentMethod.IsEmpty)
			{
				parent.PaymentMethodInfo.AddError(ResString.GetMultilingualString("36ab97bc-9296-4b16-99a7-29260855573e", "A Payment Method must be selected."));
			}
		}

		#endregion
	}
}
