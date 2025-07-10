using System;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(CustomsBrokerageUserControl))]
class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
{
	public void TestEntryInstructionUserControl()
	{
		control.MainTabControl.SelectedTab = control.EntryInstructionDetailsTabPage;
		AssertType<EntryInstructionDetailsUserControl>(control.CustomsEntryInstructionUserControl);
	}

	protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

	protected override Type ImportSupplierHeaderUserControlType => typeof(NLImportSupplierHeaderUserControl);

	protected override Type ExportSupplierHeaderUserControlType => typeof(NLExportSupplierHeaderUserControl);

	protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

	protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

	protected override Type MessageUserControlType => typeof(EntryMessageUserControl);

	protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);

	protected override Type ContainersUserControlType => typeof(ContainerUserControl);
}
