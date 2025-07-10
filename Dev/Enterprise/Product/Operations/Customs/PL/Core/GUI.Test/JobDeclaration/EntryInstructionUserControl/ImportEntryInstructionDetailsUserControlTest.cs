using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.GUI.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ImportEntryInstructionDetailsUserControl))]
sealed class ImportEntryInstructionDetailsUserControlTest : EntryInstructionDetailsUserControlTest<ImportEntryInstructionDetailsUserControl>
{
	protected override ZString MessageType => MessageTypeList.Codes.Import;

	protected override Type ExpectedAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

	protected override Type ExpectedEntryInstructionDetailBasicUserControlTest() => typeof(ImportEntryInstructionDetailBasicUserControl);

	protected override Type ExpectedPreviousDocumentsUserControlType() => typeof(LayoutPreviousDocumentsUserControl);

	protected override Type ExpectedSupportingDocumentsUserControlType() => typeof(LayoutSupportingDocumentsUserControl);

	protected override string GetExpectedAdditionalInfoTabPageCaption() => "[44] Additional Info";

	protected override ImportEntryInstructionDetailsUserControl GetNewEntryInstructionDetailsUserControl() => new ImportEntryInstructionDetailsUserControl();
}
