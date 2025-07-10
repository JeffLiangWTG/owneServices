using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestShipmentModule))]
	sealed class eManifestShipmentModuleTest : ZModuleBasherTest
	{
		public void TestOverrides()
		{
			Factory.New<Trip>().Shipments.AddNew();
			Factory.Save();
			using (var module = new eManifestShipmentModule())
			using (var form = new EmbeddedModulePopup(module))
			{
				form.Show();
				var filterControl = ((ZFilterStripControl)module.EmbeddedControl);
				filterControl.FirePerformSearch();
				filterControl.FilteredGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("Licence Checkpoint", Env.Licence.Core, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.USeManifest, module.SecurityCheckpoint);
				AssertEquals("FilterBusinessObject", typeof(eManifestShipmentFilterStrip), module.FilterBusinessObject.GetType());
				AssertEquals("EmbeddedControl", typeof(eManifestShipmentFilterStripControl), module.EmbeddedControl.GetType());
				AssertEquals("HasActions", false, module.HasActions);
				AssertEquals("AllowView", false, module.AllowView);
				AssertEquals("AllowEdit", false, module.AllowEdit);
				AssertEquals("AllowNew", false, module.AllowNew);
				AssertEquals("AllowDelete", false, module.AllowDelete);
			}
		}

		public void TestGridCollection()
		{
			var trip = Factory.New<Trip>();
			trip.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			trip.Shipments.AddNew();
			trip = Factory.New<Trip>();
			trip.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			trip.Shipments.AddNew();
			trip = Factory.New<Trip>();
			trip.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			trip.Shipments.AddNew();
			trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Error;
			var shipment3 = trip.Shipments.AddNew();
			shipment3.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Cancelled;
			var shipment4 = trip.Shipments.AddNew();
			shipment4.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			trip.Shipments.AddNew().B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			Factory.Save();
			using (var module = new eManifestShipmentModule())
			using (var form = new ZForm())
			{
				Enterprise.ZArchitecture.Environment.EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				System.Windows.Forms.Application.DoEvents();
				var collection = module.GridCollection;
				AssertEquals("GridCollection count", 4, collection.Count);
				Assert("GridCollection contains shipment 1", collection.Contains(shipment1.PK));
				Assert("GridCollection contains shipment 2", collection.Contains(shipment2.PK));
				Assert("GridCollection contains shipment 3", collection.Contains(shipment3.PK));
				Assert("GridCollection contains shipment 4", collection.Contains(shipment4.PK));
			}
		}

		public void TestDiffertBranchGridCollection()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			var branch2 = glbCompany.Branches.AddNew();
			branch2.GB_GC = glbCompany.PK;
			branch2.GB_Code = "Ts2";
			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_GB = branch.PK;
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip2 = Factory.NewWithValidTestData<Trip>();
			trip2.BH_GB = branch2.PK;
			trip2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var shipment1 = trip1.Shipments.AddNew().B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			var shipment2 = trip1.Shipments.AddNew();
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Error;
			var shipment3 = trip2.Shipments.AddNew();
			shipment3.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Cancelled;
			var shipment4 = trip2.Shipments.AddNew();
			shipment4.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			using (var module = new eManifestShipmentModule())
			using (var form = new ZForm())
			{
				Enterprise.ZArchitecture.Environment.EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				System.Windows.Forms.Application.DoEvents();
				var collection = module.GridCollection;
				AssertEquals("GridCollection count", 3, collection.Count);
				Assert("GridCollection contains shipment 2", collection.Contains(shipment2.PK));
				Assert("GridCollection contains shipment 3", collection.Contains(shipment3.PK));
				Assert("GridCollection contains shipment 4", collection.Contains(shipment4.PK));
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.eManifestShipment;

		protected override bool HasController() => true;
	}
}
