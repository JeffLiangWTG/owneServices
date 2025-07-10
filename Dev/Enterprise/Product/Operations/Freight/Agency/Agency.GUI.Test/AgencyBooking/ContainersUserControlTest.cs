using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class ContainersUserControlTest : BaseAgencyTest
	{
		public void TestShowWorkflowForm()
		{
			var shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.FCLBookedContainers.AddNew();
			using (ZForm form = new ZForm(shipment))
			{
				form.ControllerID = ControllerIDs.AgencyBooking;
				var control = new ContainersUserControl();
				control.Dock = DockStyle.Fill;
				control.SetDataBinding(shipment, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				ZGrid containersGrid = (ZGrid)control.Controls.Find("FCLContainersGrid", true)[0];
				new AgencyContainerWorkflowFormHelperTest().AssertMenuItems(containersGrid, shipment.ShippingContainers[0]);
			}
		}

		public void TestVerifiedByAddressDetailsColumns()
		{
			using (var containerUserControl = new ContainersUserControl())
			{
				var containersGrid = (ZGrid)containerUserControl.Controls.Find("FCLContainersGrid", true)[0];
				ZGridColumnInfo columnInfo = null;
				foreach (var columnName in new string[] { "GrossWeightVerifiedByAddress+E2_AddressOverride", "GrossWeightVerifiedByAddress+E2_Address1", "GrossWeightVerifiedByAddress+E2_Address2", "GrossWeightVerifiedByAddress+E2_City", "GrossWeightVerifiedByAddress+E2_State", "GrossWeightVerifiedByAddress+E2_RN_NKCountryCode", "GrossWeightVerifiedByAddress+E2_Postcode", })
				{
					columnInfo = FindGridColumnByName(containersGrid, columnName);
					Assert(columnInfo != null);
					AssertEquals("All columns belong to VGM group", "Verified By Address Details", columnInfo.GroupName.Caption);
					AssertEquals("All columns are not visible", false, columnInfo.IsVisible);
				}
			}
		}

		ZGridColumnInfo FindGridColumnByName(ZGrid grid, string columnName)
		{
			return (
				from columnStyleInfo in grid.ColumnStyles.Cast<ZGridColumnInfo>()
				where columnStyleInfo.ColumnName == columnName
				select columnStyleInfo).FirstOrDefault();
		}
	}
}
