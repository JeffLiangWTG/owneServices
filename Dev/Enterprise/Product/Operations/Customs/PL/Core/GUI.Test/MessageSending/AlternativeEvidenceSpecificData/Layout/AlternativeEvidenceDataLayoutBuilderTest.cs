using System;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(AlternativeEvidenceDataLayoutBuilder<BaseMessageSendingObjectParent>))]
sealed class CC583SpecificDataLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<AlternativeEvidenceDataLayoutBuilder<BaseMessageSendingObjectParent>, BaseMessageSendingObjectParent, CC583SpecificDataControlBag>
{
	public void TestControlsVisibility() => CombineAssertions(() =>
	{
		AssertControlVisibilityDependentOnEnquiryInformationCode(CC583SpecificDataControlBag.Instance.OfficeOfExitCodeFindBox, x =>
			x.EnquiryInformationCode == "3" || x.EnquiryInformationCode == "4");
		AssertControlVisibilityDependentOnEnquiryInformationCode(CC583SpecificDataControlBag.Instance.ExitDateDateTimeOffsetEdit, x =>
		x.EnquiryInformationCode == "2" || x.EnquiryInformationCode == "3" || x.EnquiryInformationCode == "4");
		AssertControlVisibilityDependentOnEnquiryInformationCode(CC583SpecificDataControlBag.Instance.AlternativeEvidenceGrid, x => x.EnquiryInformationCode == "4");
	});

	void AssertControlVisibilityDependentOnEnquiryInformationCode(ControlReference controlReference, Func<BaseMessageSendingObjectParent, bool> visible)
	{
		var sendingObjectParent = new BaseMessageSendingObjectParent(Factory.New<JobDeclaration>());
		var enquiryInformationCodes = new[] { "1", "2", "3", "4" };
		foreach (var enquiryInformationCode in enquiryInformationCodes)
		{
			sendingObjectParent.EnquiryInformationCode = enquiryInformationCode;
			AssertEquals($"{enquiryInformationCode} - {controlReference.Name}", visible(sendingObjectParent), ((IPanelLayoutProvider)new AlternativeEvidenceDataLayout()).Layout.IsVisible(controlReference, sendingObjectParent));
		}
	}

	protected override AlternativeEvidenceDataLayoutBuilder<BaseMessageSendingObjectParent> GetColumnLayoutBuilderForTesting()
	{
		var builder = new AlternativeEvidenceDataLayoutBuilder<BaseMessageSendingObjectParent>();
		builder.AddControlBag(CC583SpecificDataControlBag.Instance);
		return builder;
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	protected override int ExpectedMaxColumns => 2;
}
