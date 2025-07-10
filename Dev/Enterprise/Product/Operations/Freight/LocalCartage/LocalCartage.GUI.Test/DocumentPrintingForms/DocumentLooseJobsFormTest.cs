using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(DocumentLooseJobsForm))]
	public class DocumentLooseJobsFormTest : ZFormBasherTest
	{
		public void TestReadOnlyColumns()
		{
			var now = ZDateTime.Now;
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var bookedMove1 = commonCartage.BookedMovesCollection.AddNew();
			var bookedMove2 = commonCartage.BookedMovesCollection.AddNew();
			var cartageLeg1 = bookedMove1.CartageLegs.AddNew();
			var cartageLeg2 = bookedMove2.CartageLegs.AddNew();
			var pickUpOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var pickUpOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			using (var form = new DocumentLooseJobsForm(new DocumentCartageLegOptions(legCollection)))
			{
				form.Show();
				var grid = form.Controls.Find("LooseLegsGrid", true)[0] as ZGrid;
				IEnumerable<ZGridColumnInfo> columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();
				ZGridColumnInfo pickUpCompanyName = columns.First(c => c.ColumnName == "CartageLeg+PickupFromDocAddress+E2_CompanyName");
				ZGridColumnInfo pickUpFromCity = columns.First(c => c.ColumnName == "CartageLeg+PickupFromCity");
				ZGridColumnInfo plannedPickUptime = columns.First(c => c.ColumnName == "CartageLeg+JU_PlannedPickupTime");
				ZGridColumnInfo deliveryCompanyName = columns.First(c => c.ColumnName == "CartageLeg+DeliverToDocAddress+E2_CompanyName");
				ZGridColumnInfo deliveryCity = columns.First(c => c.ColumnName == "CartageLeg+DeliverToCity");
				ZGridColumnInfo deliveryTime = columns.First(c => c.ColumnName == "CartageLeg+JU_EstimatedDeliveryTime");
				ZGridColumnInfo totalPackCount = columns.First(c => c.ColumnName == "TotalPackCount");
				ZGridColumnInfo totalPackType = columns.First(c => c.ColumnName == "TotalPackType");
				ZGridColumnInfo dropMode = columns.First(c => c.ColumnName == "CartageLeg+BookedCtgMove+EW_DropMode");
				ZGridColumnInfo printCartageLeg = columns.First(c => c.ColumnName == "PrintCartageLeg");
				AssertEquals("Pick Up company name should be read only.", true, pickUpCompanyName.IsReadOnly);
				AssertEquals("Pick up from city should be read only.", true, pickUpFromCity.IsReadOnly);
				AssertEquals("Planned pick up time should be read only.", true, plannedPickUptime.IsReadOnly);
				AssertEquals("Delivery company name should be read only.", true, deliveryCompanyName.IsReadOnly);
				AssertEquals("Delivery city should be read only.", true, deliveryCity.IsReadOnly);
				AssertEquals("Delivery time should be read only.", true, deliveryTime.IsReadOnly);
				AssertEquals("Total pack count should be read only.", true, totalPackCount.IsReadOnly);
				AssertEquals("Total pack type should be read only.", true, totalPackType.IsReadOnly);
				AssertEquals("Drop Mode should be read only.", true, dropMode.IsReadOnly);
				AssertEquals("Print cartage leg should be read only.", false, printCartageLeg.IsReadOnly);
			}
		}

		public void TestDialogResult()
		{
			using (var form = (DocumentLooseJobsForm)GetFormToBash())
			{
				form.Show();
				var printButton = form.Controls.Find("PrintButton", true)[0] as ZButton;
				printButton.PerformClick();
				AssertEquals(form.DialogResult, DialogResult.Yes);
			}

			using (var form = (DocumentLooseJobsForm)GetFormToBash())
			{
				form.Show();
				var cancelPrintButton = form.Controls.Find("CancelPrintButton", true)[0] as ZButton;
				cancelPrintButton.PerformClick();
				AssertEquals(form.DialogResult, DialogResult.No);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var options = new DocumentCartageLegOptions(new DocumentCartageLegCollection(new CommonCartageLegCollection(Factory)));
			return new DocumentLooseJobsForm(options);
		}
	}
}
