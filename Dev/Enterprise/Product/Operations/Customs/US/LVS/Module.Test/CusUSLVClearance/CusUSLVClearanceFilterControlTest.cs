using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	public class CusUSLVClearanceFilterControlTest : ZFilterStripControlTest
	{
		public void TestFilterGridColumns()
		{
			using (var module = new CusUSLVClearanceModule())
			using (var form = new ZForm())
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();

				Application.DoEvents();

				AssertColumn(grid, CusUSLVClearance.Schema.ULH_JobNumber, ResourceStringData.Empty, "Job Number", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_EntryDate, ResourceStringData.Empty, "Arrival Date", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_CarrierSCAC, ResourceStringData.Empty, "Carrier SCAC", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_ContactName, ResourceStringData.Empty, "Contact Name", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_ContactPhone, ResourceStringData.Empty, "Contact Phone", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_ContainerMode, ResourceStringData.Empty, "Container Mode", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_ConveyanceName, ResourceStringData.Empty, "Conveyance", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_DepartureDate, ResourceStringData.Empty, "Dep. Date", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_DischargeDate, ResourceStringData.Empty, "Dis. Date", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_EntryFilerCode, ResourceStringData.Empty, "Entry Filer Code", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_GB, ResourceStringData.Empty, "Branch", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_IORReference, ResourceStringData.Empty, "IOR. Ref.", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_IORType, ResourceStringData.Empty, "IOR. Type", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_MasterBill, ResourceStringData.Empty, "Master Bill", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_MasterBillIssuerSCAC, ResourceStringData.Empty, "Master Bill Issuer SCAC", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_OH_Client, ResourceStringData.Empty, "Client", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_OH_Importer, ResourceStringData.Empty, "Importer", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_PortOfEntry, ResourceStringData.Empty, "Entry Port", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_RemoteLocationFiling, ResourceStringData.Empty, "Remote Location Filing", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_RL_NKPortOfDischarge, ResourceStringData.Empty, "Discharge Port", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_RL_NKPortOfLoading, ResourceStringData.Empty, "Loading Port", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_TransportMode, ResourceStringData.Empty, "Transport Mode", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_VoyageFlightNo, ResourceStringData.Empty, "Voyage/Flight/Trip Number", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_PortOfDischarge, ResourceStringData.Empty, "Discharge Port (Sched D/K)", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_PortOfLoading, ResourceStringData.Empty, "Loading Port (Sched D/K)", true, ZString.Empty);
				AssertColumn(grid, CusUSLVClearance.Schema.ULH_MatchingKey, ResourceStringData.Empty, "Matching Key", true, ZString.Empty);
			}
		}
	}
}
