using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public class NominatedMessageErrorValidation : AutoNominatedMessageErrorValidation
	{
		public NominatedMessageErrorValidation(AutoNominatedMessageError parent)
			: base(parent)
		{
		}

		public new NominatedMessageError Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (NominatedMessageError)base.Parent; }
		}

		protected override void CheckFieldName()
		{
			base.CheckFieldName();
			MandatoryValidation.CheckEntered(Parent.FieldNameInfo, "Field Name");
		}

		protected override void CheckMessageErrorText()
		{
			base.CheckMessageErrorText();
			MandatoryValidation.CheckEntered(Parent.MessageErrorTextInfo, "Message Error Text");
		}
	}
}
