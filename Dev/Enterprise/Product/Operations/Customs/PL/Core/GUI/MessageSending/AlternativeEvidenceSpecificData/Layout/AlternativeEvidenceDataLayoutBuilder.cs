using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class AlternativeEvidenceDataLayoutBuilder<T> : ColumnLayoutBuilder<T, CC583SpecificDataControlBag> where T : BaseMessageSendingObjectParent
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	public override CC583SpecificDataControlBag CommonBag => CC583SpecificDataControlBag.Instance;

	protected override int MaxColumns => 2;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		SetVisibility(CommonBag.AlternativeEvidenceGrid, AlternativeEvidenceGridVisibility, x => x.EnquiryInformationCodeInfo);
		SetVisibility(CommonBag.OfficeOfExitCodeFindBox, OfficeOfExitVisibility, x => x.EnquiryInformationCodeInfo);
		SetVisibility(CommonBag.ExitDateDateTimeOffsetEdit, ExitDateVisibility, x => x.EnquiryInformationCodeInfo);
	}

	bool AlternativeEvidenceGridVisibility(BaseMessageSendingObjectParent messageSendingObjectParent)
	{
		const string alternativeEvidenceGridVisibilityCode = "4";
		return messageSendingObjectParent.EnquiryInformationCode == alternativeEvidenceGridVisibilityCode;
	}

	bool OfficeOfExitVisibility(BaseMessageSendingObjectParent messageSendingObjectParent)
	{
		var officeOfExitVisibilityCodes = new ZString[] { "3", "4" };
		return officeOfExitVisibilityCodes.Contains(messageSendingObjectParent.EnquiryInformationCode);
	}

	bool ExitDateVisibility(BaseMessageSendingObjectParent messageSendingObjectParent)
	{
		var exitDateVisibleCodes = new ZString[] { "2", "3", "4" };
		return exitDateVisibleCodes.Contains(messageSendingObjectParent.EnquiryInformationCode);
	}
}
