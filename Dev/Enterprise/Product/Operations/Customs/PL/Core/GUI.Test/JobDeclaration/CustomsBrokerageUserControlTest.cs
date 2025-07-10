using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(CustomsBrokerageUserControl))]
sealed class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
{
	public void TestImportEntryInstructionDetailsUserControl() => TestPLEntryInstructionDetailsUserControlType(MessageTypeList.Codes.Import, typeof(ImportEntryInstructionDetailsUserControl));

	public void TestExportEntryInstructionDetailsUserControl() => TestPLEntryInstructionDetailsUserControlType(MessageTypeList.Codes.Export, typeof(ExportEntryInstructionDetailsUserControl));

	public new void TestEntryInstructionDetailsUserControl() => Assert("PL uses different Entry instructions dependant on Message Type", true);

	protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

	protected override Type ImportSupplierHeaderUserControlType => typeof(ImportSupplierHeaderUserControl);

	protected override Type ExportSupplierHeaderUserControlType => typeof(ExportSupplierHeaderUserControl);

	protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

	protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

	protected override Type MessageUserControlType => typeof(PLEntryMessageUserControl);

	protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);

	protected override Type MiscOptionsUserControlType => typeof(MiscOptionsUserControl);

	protected override Type DV1UserControlType => typeof(DV1UserControl);

	void TestPLEntryInstructionDetailsUserControlType(ZString messageType, Type expectedType)
	{
		declaration.JE_MessageType = messageType;
		control.MainTabControl.SelectedTab = control.EntryInstructionDetailsTabPage;
		Assertion.AssertEquals(expectedType, control.CustomsEntryInstructionUserControl.GetType());
	}
}
