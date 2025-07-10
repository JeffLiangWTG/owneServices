using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class JobDeclarationMessageSendingEDocsValidation : AutoJobDeclarationMessageSendingEDocsValidation
{
	public JobDeclarationMessageSendingEDocsValidation(AutoJobDeclarationMessageSendingEDocs parent) : base(parent)
	{
	}

	new JobDeclarationMessageSendingEDocs Parent => (JobDeclarationMessageSendingEDocs)base.Parent;

	protected override void CheckEDoc()
	{
		base.CheckEDoc();
		MandatoryValidation.CheckEntered(Parent.EDocInfo);
		ListValidation.ErrorIfInvalidPK(Parent.EDocInfo);
	}

	protected override void CheckAdditionalInformation()
	{
		base.CheckAdditionalInformation();
		if (Parent.AdditionalInformation.IsEmpty && Parent.SupportingDocument.IsEmpty)
		{
			Parent.AdditionalInformationInfo.AddMessageError(EmptyAdditionalInformationOrSupportingDocument);
		}
	}

	protected override void CheckDocumentDescription()
	{
		base.CheckDocumentDescription();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.DocumentDescriptionInfo);
	}

	protected override void CheckSupportingDocument()
	{
		base.CheckSupportingDocument();
		if (Parent.AdditionalInformation.IsEmpty && Parent.SupportingDocument.IsEmpty)
		{
			Parent.SupportingDocumentInfo.AddMessageError(EmptyAdditionalInformationOrSupportingDocument);
		}
	}

	static string EmptyAdditionalInformationOrSupportingDocument => Res.GetString(
		"PLJobDeclarationMessageSendingEDocsValidation|EmptyAdditionalInformationOrSupportingDocument",
		"The Additional Info or Supporting document code is required.");
}
