using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class AmendmentWithdrawalReason : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructor

		public AmendmentWithdrawalReason()
		{
		}

		#endregion

		#region IsCancelled

		public ZBool IsCancelled;

		#endregion

		#region ReasonText

		[MaxLength(512)]
		public virtual ZString ReasonText
		{
			get { return fReasonText; }
			set
			{
				var shortenedReasonText = value.Left(ReasonTextInfo.MaxLength);
				CheckMaximumLength(ReasonTextInfo, shortenedReasonText);
				SetNonPersistentPropertyValue(ReasonTextInfo, ref fReasonText, shortenedReasonText);
				ValidateReasonText();
				RefreshBinding();
			}
		}
		ZString fReasonText;

		public ZPropertyInfo ReasonTextInfo
		{
			get { return GetZPropertyInfo(nameof(ReasonText)); }
		}

		public void ValidateReasonText()
		{
			if (!IsValidationSuspended)
			{
				ReasonTextInfo.ClearAllNotifications();
				CheckReasonText();
			}
		}

		protected virtual void CheckReasonText()
		{
			if (ReasonText.IsEmpty)
			{
				ReasonTextInfo.AddError(Res.GetString("42946883-888c-4060-b665-0d95a651d906", "You should enter reason for amendment or withdrawal."));
			}
		}

		#endregion

		#region ValidateAll

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateReasonText();
		}

		#endregion

	}
}
