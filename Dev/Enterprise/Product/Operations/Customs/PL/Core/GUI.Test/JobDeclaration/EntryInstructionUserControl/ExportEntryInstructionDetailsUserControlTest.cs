using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.GUI.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ExportEntryInstructionDetailsUserControl))]
sealed class ExportEntryInstructionDetailsUserControlTest : EntryInstructionDetailsUserControlTest<ExportEntryInstructionDetailsUserControl>
{
	protected override ZString MessageType => MessageTypeList.Codes.Export;

	protected override Type ExpectedAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

	protected override Type ExpectedEntryInstructionDetailBasicUserControlTest() => typeof(ExportEntryInstructionDetailBasicUserControl);

	protected override Type ExpectedPreviousDocumentsUserControlType() => typeof(LayoutPreviousDocumentsUserControl);

	protected override Type ExpectedSupportingDocumentsUserControlType() => typeof(LayoutSupportingDocumentsUserControl);

	protected override string GetExpectedAdditionalInfoTabPageCaption() => "[44] Additional Documents";

	protected override ExportEntryInstructionDetailsUserControl GetNewEntryInstructionDetailsUserControl() => new ExportEntryInstructionDetailsUserControl();
}
