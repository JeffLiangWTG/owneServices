using System;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SE.GUI.Testing
{
	[TestedType(typeof(CustomsBrokerageUserControl))]
	sealed class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
	{
		protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

		protected override Type ImportSupplierHeaderUserControlType => typeof(EU.GUI.EUNonLayoutImportSupplierHeaderUserControl);

		protected override Type ExportSupplierHeaderUserControlType => typeof(EU.GUI.EUNonLayoutExportSupplierHeaderUserControl);

		protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

		protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

		protected override Type MessageUserControlType => typeof(EU.GUI.EntryMessageUserControl);

		protected override Type EntryInstructionDetailsUserControlType => typeof(EU.GUI.EntryInstructionDetailsUserControl);
	}
}
