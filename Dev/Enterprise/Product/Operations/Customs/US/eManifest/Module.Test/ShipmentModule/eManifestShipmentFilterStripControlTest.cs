using System.Windows.Forms;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	sealed class eManifestShipmentFilterStripControlTest : ZFilterStripControlTest
	{
		public void TestColumns()
		{
			using (var module = new eManifestShipmentModule())
			using (var form = new ZForm())
			{
				EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
				AssertColumn(grid, "Trip+" + Trip.Schema.BH_JobReference, ResourceStringData.Empty, "Trip Ref.", true, "MAN0000001");
				AssertColumn(grid, Shipment.Schema.B0_ShipmentType, ResourceStringData.Empty, "Type", true, "INB");
				AssertColumn(grid, Shipment.Schema.B0_MasterBillNumber, ResourceStringData.Empty, "Shipment Control Number", true, "AAGC123456789012");
				AssertColumn(grid, Shipment.Schema.B0_RL_NKPortOfLading, ResourceStringData.Empty, "Loading", true, "CATOR");
				AssertColumn(grid, Shipment.Schema.B0_PortOfLadingKCode, ResourceStringData.Empty, "Schedule K", true, "01535");
				AssertColumn(grid, "Shipper+Organisation+OH_Code", ResourceStringData.Empty, "Shipper", true, "TSTSHIPPER");
				AssertColumn(grid, "Consignee+Organisation+OH_Code", ResourceStringData.Empty, "Consignee", true, "TSTCONSIGNEE");
				AssertColumn(grid, Shipment.Schema.B0_ManifestQty, ResourceStringData.Empty, "Quantity", true, "500");
				AssertColumn(grid, Shipment.Schema.B0_ManifestUQ, ResourceStringData.Empty, "UQ", true, "PCE");
				AssertColumn(grid, Shipment.Schema.B0_Weight, ResourceStringData.Empty, "Weight", true, "200.000000");
				AssertColumn(grid, Shipment.Schema.B0_WeightUQ, ResourceStringData.Empty, "UQ", true, "KG");
				AssertColumn(grid, Shipment.Schema.B0_ReleaseStatusCodeDescription, ResourceStringData.Empty, "Rel. Status", true, "ACP - Shipment accepted but not linked to the trip (unassociated)");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var trip = Factory.New<Trip>();
			trip.BH_JobReference = "MAN0000001";
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			shipment.B0_MasterBillNumber = "AAGC123456789012";
			shipment.B0_RL_NKPortOfLading = "CATOR";
			shipment.B0_PortOfLadingKCode = "01535";
			shipment.B0_ManifestQty = 500;
			shipment.B0_ManifestUQ = "PCE";
			shipment.B0_Weight = 200;
			shipment.B0_WeightUQ = "KG";
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTCONSIGNEE";
			shipment.Consignee.OrganisationPK = org.PK;
			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTSHIPPER";
			shipment.Shipper.OrganisationPK = org.PK;
			Factory.Save();
		}
	}
}
