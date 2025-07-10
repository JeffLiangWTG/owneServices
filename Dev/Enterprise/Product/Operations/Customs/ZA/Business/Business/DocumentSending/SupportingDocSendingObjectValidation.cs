using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class SupportingDocSendingObjectValidation : Customs.Business.SupportingDocSendingObjectValidation
	{
		public SupportingDocSendingObjectValidation(Customs.Business.SupportingDocSendingObject parent) : base(parent)
		{
		}

		protected override void CheckCaseNumber()
		{
			MandatoryValidation.CheckEntered(Parent.CaseNumberInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CaseNumberInfo);
			var caseNumber = Parent.CaseNumber;
			if (!caseNumber.IsEmpty)
			{
				if (caseNumber.KeepAlphanumericCharacters() != caseNumber)
				{
					Parent.CaseNumberInfo.AddMessageError(ValidationConstants.SupportingDocSendingObject.CaseNumberOnlyAlphanumeric);
				}
			}
		}

		public override ZInt MaxEDocFileSizeInBytes => 5_242_880;
	}
}
