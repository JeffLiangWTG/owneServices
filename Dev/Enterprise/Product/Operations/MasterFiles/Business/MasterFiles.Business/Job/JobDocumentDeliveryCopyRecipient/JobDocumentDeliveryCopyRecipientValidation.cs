using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDeliveryCopyRecipientValidation : AutoJobDocumentDeliveryCopyRecipientValidation
	{
		public JobDocumentDeliveryCopyRecipientValidation(AutoJobDocumentDeliveryCopyRecipient parent)
			: base(parent)
		{
		}

		protected override void CheckJDR_EmailAddress()
		{
			base.CheckJDR_EmailAddress();
			MandatoryValidation.CheckEntered(Parent.JDR_EmailAddressInfo);
			if (!EmailAddressValidation.IsEmailAddressValid(Parent.JDR_EmailAddress))
			{
				Parent.JDR_EmailAddressInfo.AddError(Res.GetString("0fd8bb5f-e711-4202-aedc-1276a78bbb88", "The email address is invalid."));
			}
		}
	}
}