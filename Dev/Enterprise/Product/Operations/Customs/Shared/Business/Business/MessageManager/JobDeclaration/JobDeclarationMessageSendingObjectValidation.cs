using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationMessageSendingObjectValidation : AutoJobDeclarationMessageSendingObjectValidation
	{
		public JobDeclarationMessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent) : base(parent)
		{
		}

		public new JobDeclarationMessageSendingObject Parent => base.Parent as JobDeclarationMessageSendingObject;

		public void ValidateShouldSend()
		{
			((IValidationInternals)this).Validate(Parent.ShouldSendInfo, () => { CheckShouldSend(); });
		}

		protected virtual void CheckShouldSend()
		{
		}

		protected virtual string GetVOCReason()
		{
			string message = GetMessageError(Parent.Header, amendmentReasonWarning);

			return message;
		}

		public static ZString GetMessageError(CusEntryHeader entry, ZString message) => ZString.Format("{0} - {1}", entry.CH_BGMReference, message);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "Baseline")]
		readonly string amendmentReasonWarning = Res.GetString("6D88AE0D-4689-48D5-BDA0-7CA84A215DE8", "Amendment reason must be entered when amending or canceling a declaration");
	}
}
