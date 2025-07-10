using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class ManifestToOpenUserControlTest : TestCaseWithFactory
	{
		public void TestBillGrid()
		{
			using (var control = new ManifestToOpenUserControl())
			{
				var grid = control.FindSingle<ZGrid>("BillGrid");
				var columnStyle = grid.GetColumnStyle("TPD_DocumentNumber");
				AssertEquals("column width TPD_DocumentNumber", 80, columnStyle.Width);

				columnStyle = grid.GetColumnStyle("TPD_IncludeAllItems");
				AssertEquals("column width TPD_IncludeAllItems", 80, columnStyle.Width);

				columnStyle = grid.GetColumnStyle("TPD_IsInWarehouse");
				AssertEquals("column width TPD_IsInWarehouse", 80, columnStyle.Width);

				columnStyle = grid.GetColumnStyle("TPD_IsOtherProcedure");
				AssertEquals("column width TPD_IsOtherProcedure", 80, columnStyle.Width);
			}
		}

		public void TestPackGrid()
		{
			using (var control = new ManifestToOpenUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PackGrid");
				var columnStyle = grid.GetColumnStyle("TPI_LineNumber");
				AssertEquals("column width TPI_LineNumber", 80, columnStyle.Width);

				columnStyle = grid.GetColumnStyle("TPI_Quantity");
				AssertEquals("column width TPI_Quantity", 80, columnStyle.Width);

				columnStyle = grid.GetColumnStyle("TPI_WarehouseCode");
				AssertEquals("column width TPI_WarehouseCode", 90, columnStyle.Width);
			}
		}

		public void TestBillGridDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ClusterKey = 1;

			var header = declaration.ManifestToOpenHeaders.AddNew();
			var bill = header.Bills.AddNew();
			bill.TPD_DocumentNumber = "001";
			bill.TPD_DocumentNumber = "IMP2023";
			var pack = bill.Packs.AddNew();
			pack.TPI_LineNumber = 1;
			var pack1 = bill.Packs.AddNew();
			pack1.TPI_LineNumber = 2;

			var invoce = declaration.Invoices.AddNew();
			var invoiceLine = invoce.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "5800";

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;

				var manifestToOpenUserControl = form.FindSingleOrDefault<ManifestToOpenUserControl>(c => c.Name == "ManifestToOpenUserControl");
				var grid = manifestToOpenUserControl.FindSingle<ZGrid>("BillGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Should be visible when import and procedure ends with 00.", true, manifestToOpenUserControl.Visible);
					AssertNotNull(grid);
					AssertNoExceptionThrown("Deleted row information cannot be accessed through the row.", () => grid.DeleteMenuItem.PerformClick());
					AssertEquals("ManifestToOpenBill deleted", true, bill.IsDeleted);
				});
			}
		}
	}
}
