using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ShipmentUnpaidPIAInvoicesForm))]
	sealed class ShipmentUnpaidPIAInvoicesFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		class TestRelatedJobNumber : IRelatedJobNumber
		{
			public TestRelatedJobNumber(string[] jobNumber)
			{
				this.fJobNumber = jobNumber;
			}

			readonly string[] fJobNumber;

			public string[] JobNumber
			{
				get { return fJobNumber; }
			}
		}

		protected override Form GetFormToBashCore()
		{
			IRelatedJobNumber shipment = new TestRelatedJobNumber(new string[] { "S00009999" });
			ShipmentUnpaidPIAInvoicesForm form = new ShipmentUnpaidPIAInvoicesForm(shipment);
			return form;
		}

		[RequiresSTA]
		public void TestFormCaption()
		{
			using (ShipmentUnpaidPIAInvoicesForm form = (ShipmentUnpaidPIAInvoicesForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals("Caption should be Unpaid PIA Invoices", "Unpaid PIA Invoices", form.FormCaption);
			}
		}

		[RequiresSTA]
		public void TestOutstandingAmountCaption()
		{
			using (var form = (ShipmentUnpaidPIAInvoicesForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				var grid = form.InvoicesGrid;
				var columnName = "AH_OutstandingAmount";
				var columnInfo = grid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
				AssertNotNull(columnInfo);
				AssertNull(columnInfo.CaptionResourceString.Caption);
				AssertEquals("Outstanding Amount", grid.Columns[columnName].ColumnStyle.HeaderText);
			}
		}
	}
}
