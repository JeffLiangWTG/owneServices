using System.Collections.Generic;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.GUI.Testing
{
	[TestedType(typeof(ExportSupplierHeaderUserControl))]
	class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestColumnLayoutContext()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}
	}
}
