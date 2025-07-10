using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public class SupportingDocumentValidation : CusSupportingInfoValidation
{
	public SupportingDocumentValidation(SupportingDocument parent)
		: base(parent)
	{
	}
	new SupportingDocument Parent => (SupportingDocument)base.Parent;

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		var parent = Parent;
		var referenceNumberInfo = parent.CSI_ReferenceNumberInfo;

		MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(referenceNumberInfo, parent.CSI_CodeInfo);

		if(parent.CSI_Code != "TXT" && parent.CSI_ReferenceNumber.Length > 17)
		{
			referenceNumberInfo.AddMessageError(Res.GetString("8B5BCB2C-9561-4809-B66F-BE73AFD56D38", "The length of Reference cannot exceed 17 characters when Type is other than TXT."));
		}
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_CodeInfo, Parent.CSI_ReferenceNumberInfo);
	}
}
