using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryLineValidation : EU.Business.Declaration.CusEntryLineValidation
{
	public CusEntryLineValidation(EU.Business.Declaration.CusEntryLine parent) : base(parent)
	{
	}

	protected new CusEntryLine Parent => (CusEntryLine)base.Parent;

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		CheckAllMaximumCollectionsAmounts();
	}

	protected void CheckAllMaximumCollectionsAmounts()
	{
		CheckMaximumContainersAmount();
		CheckMaximumAdditionalDocumentsAmount();
		CheckMaximumPreviousDocumentsAmount();
		CheckMaximumSupportingDocumentsAmount();
	}

	void CheckMaximumContainersAmount()
	{
		if (Parent.ContainerCount > MaximumBusinessObjectsAmounts.MaximumEntryLineContainers)
		{
			Parent.AddRowMessageError(Res.GetString("PLCusEntryLineValidation|CheckMaximumContainersAmount", "Container count exceeds maximum of 99"));
		}
	}

	void CheckMaximumAdditionalDocumentsAmount()
	{
		if (Parent.AdditionalInfoCount > MaximumBusinessObjectsAmounts.MaximumAdditionalInformation)
		{
			Parent.AddRowMessageError(Res.GetString("PLCusEntryLineValidation|CheckMaximumAdditionalDocumentsAmount", "Additional information count exceeds maximum of 99"));
		}
	}

	void CheckMaximumPreviousDocumentsAmount()
	{
		if (Parent.PreviousDocumentCount > MaximumBusinessObjectsAmounts.MaximumPreviousDocuments)
		{
			Parent.AddRowMessageError(Res.GetString("PLCusEntryLineValidation|CheckMaximumPreviousDocumentsAmount", "Previous documents count exceeds maximum of 99"));
		}
	}

	void CheckMaximumSupportingDocumentsAmount()
	{
		if (Parent.SupportingDocumentCount > MaximumBusinessObjectsAmounts.MaximumSupportingDocuments)
		{
			Parent.AddRowMessageError(Res.GetString("PLCusEntryLineValidation|CheckMaximumSupportingDocumentsAmount", "Supporting documents count exceeds maximum of 99"));
		}
	}
}
