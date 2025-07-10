using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
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
	sealed class eManifestFilterStripControlTest : ZFilterStripControlTest
	{
		public void TestColumns()
		{
			using (Enterprise.Environment.DisposableEnvironment.ForBranch("Ts1"))
			using (var module = new eManifestModule())
			using (var form = new ZForm())
			{
				EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
				AssertColumn(grid, "Branch+GB_Code", ResourceStringData.Empty, "Branch", true, "Ts1");
				AssertColumn(grid, Trip.Schema.BH_JobReference, ResourceStringData.Empty, "Job Ref.", true, "MAN0000001");
				AssertColumn(grid, Trip.Schema.BH_VoyageNumber, ResourceStringData.Empty, "Trip Ref.", true, "T1");
				AssertColumn(grid, Trip.Schema.BH_ImportTransportMode, ResourceStringData.Empty, "MOT", true, "RD");
				AssertColumn(grid, Trip.Schema.BH_CarrierSCAC, ResourceStringData.Empty, "Carrier", true, "CAR1");
				AssertColumn(grid, Trip.Schema.BH_ETA, ResourceStringData.Empty, "Estimated Date Of Arrival", true, "27-FEB-12 17:09");
				AssertColumn(grid, Trip.Schema.BH_RL_NKPortUnlading, ResourceStringData.Empty, "Port Of Arrival", true, "USLA2");
				AssertColumn(grid, Trip.Schema.BH_PortUnladingDCode, ResourceStringData.Empty, "Schedule D", true, "2704");
				AssertColumn(grid, Trip.Schema.BH_TransitDirection, ResourceStringData.Empty, "Direction", true, "I");
				AssertColumn(grid, "Conveyance+BJ_RegistrationNumber", ResourceStringData.Empty, "Conveyance", true, "AABB32");
				AssertColumn(grid, Trip.Schema.BH_MessageStatusCodeDescription, ResourceStringData.Empty, "Msg. Status", true, "AWC - Awaiting Change");
				AssertColumn(grid, Trip.Schema.BH_ReleaseStatusCodeDescription, ResourceStringData.Empty, "Rel. Status", true, "ARV - Trip arrived");
				AssertColumn(grid, "ImporterCode", ResourceStringData.Empty, "Client Code", true, "DJCKYN");
				AssertColumn(grid, "ImporterName", ResourceStringData.Empty, "Client Name", true, "Daniel Inc");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_FullName = "Daniel Inc";
			client.OH_Code = "DJCKYN";
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "CAR1";
			trip.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip.BH_JobReference = "MAN0000001";
			trip.BH_VoyageNumber = "T1"; // Trip
			trip.BH_ETA = new ZDateTime(2012, 02, 27, 17, 09, 00);
			trip.BH_RL_NKPortUnlading = "USLA2";
			trip.BH_PortUnladingDCode = "2704";
			trip.BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.TripArrived;
			trip.BH_OA_Importer = client.MainAddress.PK;
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			trip.BH_GB = branch.PK;
			eManifestFilterStripTest.AddRefEquipment(trip.Conveyance, "AABB32");
			Factory.Save();
		}
	}
}
