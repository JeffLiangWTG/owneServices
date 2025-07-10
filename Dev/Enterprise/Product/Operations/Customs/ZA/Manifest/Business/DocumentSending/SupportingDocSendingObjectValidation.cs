using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class SupportingDocSendingObjectValidation : ZA.Business.SupportingDocSendingObjectValidation
	{
		public SupportingDocSendingObjectValidation(SupportingDocSendingObject parent) : base(parent)
		{
		}

		protected override void CheckDocumentType()
		{
			MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DocumentTypeInfo);
		}

		protected override void CheckLocalReferenceNumber()
		{
		}
	}
}
