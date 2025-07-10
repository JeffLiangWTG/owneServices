using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class AdditionalDocumentMessageSendingObjectValidation : MessageSendingObjectValidation
	{
		public AdditionalDocumentMessageSendingObjectValidation(AdditionalDocumentMessageSendingObject parent) : base(parent)
		{
		}

		public new AdditionalDocumentMessageSendingObject Parent => base.Parent as AdditionalDocumentMessageSendingObject;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateContactOffice();
		}

		public void ValidateContactOffice()
		{
			ValidateCalculatedProperty(Parent.ContactOfficeInfo);
		}

		protected void CheckContactOffice()
		{
			if (Parent.IsMessageTypeADM)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ContactOfficeInfo);
			}
		}

		protected override void CheckAction()
		{
			if (!Parent.IsMessageTypeADM)
			{
				base.CheckAction();
			}
		}
	}
}
