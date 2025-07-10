using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ExportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>))]
sealed class ExportAdditionalDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ExportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>, BaseMessageSendingObject, ExportAdditionalDetailsControlBag>
{
	public void TestControlsVisibility()
	{
		CombineAssertions(() =>
		{
			AssertControlVisibilityDependentOnMessageTypeAndEntryStyle(ExportAdditionalDetailsControlBag.Instance.SecurityDropEdit, x =>
				(x.Action == ExportMessageSendingObjectActionList.Codes.CC513
				|| x.Action == ExportMessageSendingObjectActionList.Codes.CC515)
				&& x.Header.Declaration.JE_EntryStyle != EntryStyleListExportUCC.Codes.ExportToSpecialTerritory);
			AssertControlVisibilityDependentOnMessageType(ExportAdditionalDetailsControlBag.Instance.AmendmentInvalidationReasonUserControl, x => x.Action == ExportMessageSendingObjectActionList.Codes.CC514);
			AssertControlVisibilityDependentOnMessageType(ExportAdditionalDetailsControlBag.Instance.CorrectionAcceptanceDropEdit, x => x.Action == ExportMessageSendingObjectActionList.Codes.CC566);
			AssertControlVisibilityDependentOnMessageType(ExportAdditionalDetailsControlBag.Instance.AcceptanceCommentUserControl, x => x.Action == ExportMessageSendingObjectActionList.Codes.CC566);
		});
	}

	void AssertControlVisibilityDependentOnMessageType(ControlReference controlReference, Func<BaseMessageSendingObject, bool> visible)
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.CustomsEntryHeaders.AddNew();
		var sendingObject = new BaseMessageSendingObject(header);

		foreach (var messageType in MessageTypes)
		{
			sendingObject.Action = messageType;
			AssertEquals($"{messageType} - {controlReference.Name}", visible(sendingObject), ((IPanelLayoutProvider)new ExportAdditionalDetailsLayout()).Layout.IsVisible(controlReference, sendingObject));
		}
	}

	void AssertControlVisibilityDependentOnMessageTypeAndEntryStyle(ControlReference controlReference, Func<BaseMessageSendingObject, bool> visible)
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.CustomsEntryHeaders.AddNew();
		var sendingObject = new BaseMessageSendingObject(header);

		foreach (var messageType in MessageTypes)
		{
			sendingObject.Action = messageType;
			foreach (var entryStyle in EntryStyles)
			{
				declaration.JE_EntryStyle = entryStyle;
				AssertEquals($"{messageType} - {entryStyle} - {controlReference.Name}", visible(sendingObject), ((IPanelLayoutProvider)new ExportAdditionalDetailsLayout()).Layout.IsVisible(controlReference, sendingObject));
			}
		}
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	protected override int ExpectedMaxColumns => 2;

	protected override ExportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject> GetColumnLayoutBuilderForTesting()
	{
		var builder = new ExportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>();
		builder.AddControlBag(ExportAdditionalDetailsControlBag.Instance);
		return builder;
	}

	string[] MessageTypes => new[]
	{
		ExportMessageSendingObjectActionList.Codes.CC511,
		ExportMessageSendingObjectActionList.Codes.CC513,
		ExportMessageSendingObjectActionList.Codes.CC514,
		ExportMessageSendingObjectActionList.Codes.CC515,
		ExportMessageSendingObjectActionList.Codes.CC566,
		ExportMessageSendingObjectActionList.Codes.CC583,
		string.Empty
	};

	string[] EntryStyles => new[]
	{
		EntryStyleListExportUCC.Codes.ExportNormal,
		EntryStyleListExportUCC.Codes.ExportToSpecialTerritory,
		string.Empty
	};
}
