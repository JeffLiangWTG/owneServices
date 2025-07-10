using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BillOfLadingContainersPageTest : BaseAgencyTest
	{
		public void TestPackLinesControl_ShowTotalsIsDisabled()
		{
			var shipment = Factory.New<BillOfLading>();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("PackLines totals are not shown", false, form.PackLinesControl.ShowTotals);
			}
		}

		public void TestAdditionalColumns_ContainerCustomColumnAdder()
		{
			var shipment = Factory.New<BillOfLading>();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.ContainersGrid.Columns.Contains(AgencyShipmentContainer.Schema.CustomsEntryNumberType));
			}
		}

		public void TestShowWorkflowForm()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.ShippingContainers.AddNew();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				new AgencyContainerWorkflowFormHelperTest().AssertMenuItems(form.ContainersGrid, shipment.ShippingContainers[0]);
			}
		}

		#region Types
		class FormForTesting : ZForm
		{
			public FormForTesting(BillOfLading billOfLading) : base(billOfLading)
			{
				ControllerID = ControllerIDs.AgencyBillOfLading;
				mainControl = new BillOfLadingContainersPage();
				mainControl.Dock = DockStyle.Fill;
				Controls.Add(mainControl);
				mainControl.SetDataBinding(billOfLading, "");
			}

			public FCLPackLinesControl PackLinesControl
			{
				get
				{
					return (FCLPackLinesControl)mainControl.Controls.Find("packLinesControl", true)[0];
				}
			}

			public ZGrid ContainersGrid
			{
				get
				{
					return (ZGrid)mainControl.Controls.Find("containersGrid", true)[0];
				}
			}

			readonly BillOfLadingContainersPage mainControl;
		}
		#endregion
	}
}
