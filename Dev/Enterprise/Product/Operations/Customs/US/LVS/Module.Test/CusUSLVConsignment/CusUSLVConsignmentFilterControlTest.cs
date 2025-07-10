using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	public class CusUSLVConsignmentFilterControlTest : ZFilterStripControlTest
	{
		public void TestFilterGridColumns()
		{
			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();

				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_JobReference), ResourceStringData.Empty, "Job Number", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_EntryDate), ResourceStringData.Empty, "Arrival Date", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.CarrierSCAC), ResourceStringData.Empty, "Carrier SCAC", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.ContactName), ResourceStringData.Empty, "Contact Name", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.ContactPhone), ResourceStringData.Empty, "Contact Phone", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.ContainerMode), ResourceStringData.Empty, "Container Mode", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_ConveyanceName), ResourceStringData.Empty, "Conveyance", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_DepartureDate), ResourceStringData.Empty, "Dep. Date", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_DischargeDate), ResourceStringData.Empty, "Dis. Date", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.EntryFilerCode), ResourceStringData.Empty, "Entry Filer Code", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_GB), ResourceStringData.Empty, "Branch", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_MasterBill), ResourceStringData.Empty, "Master Bill", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.MasterBillIssuerSCAC), ResourceStringData.Empty, "Master Bill Issuer SCAC", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_OH_Client), ResourceStringData.Empty, "Client", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_OH_Importer), ResourceStringData.Empty, "IOR", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_PortOfEntry), ResourceStringData.Empty, "Entry Port", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_RL_NKPortOfDischarge), ResourceStringData.Empty, "Discharge Port", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_RL_NKPortOfLoading), ResourceStringData.Empty, "Loading Port", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_TransportMode), ResourceStringData.Empty, "Transport Mode", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_VoyageFlightNo), ResourceStringData.Empty, "Voyage/Flight/Trip Number", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_PortOfDischarge), ResourceStringData.Empty, "Discharge Port (Sched D/K)", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_PortOfLoading), ResourceStringData.Empty, "Loading Port (Sched D/K)", true, ZString.Empty);

					AssertColumn(grid, nameof(USConsignmentCombined.HouseBillIssuerSCAC), ResourceStringData.Empty, "Issuer SCAC", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_HouseBill), ResourceStringData.Empty, "House Bill", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_SubmittedDate), ResourceStringData.Empty, "Submitted", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_ReleaseDate), ResourceStringData.Empty, "Release Date", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_ReleaseStatus), ResourceStringData.Empty, "Release Status", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_MessageStatus), ResourceStringData.Empty, "Message Status", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_EntryNum), ResourceStringData.Empty, "Entry Number", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.OwnerReferenceNumber), ResourceStringData.Empty, "Owner Ref.", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.EquipmentNumber), ResourceStringData.Empty, "Container", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.GoodsValue), ResourceStringData.Empty, "Goods Value", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.Currency), ResourceStringData.Empty, "Currency", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.NumberOfPacks), ResourceStringData.Empty, "Quantity", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.PackType), ResourceStringData.Empty, "UQ", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.NonAMSIndicator), ResourceStringData.Empty, "Non-AMS", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_OH_Consignee), ResourceStringData.Empty, "Consignee", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.ConsigneeAddress), ResourceStringData.Empty, "Consignee Address", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_ConsigneeName), ResourceStringData.Empty, "Consignee Name", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_OH_Seller), ResourceStringData.Empty, "Seller", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.SellerAddress), ResourceStringData.Empty, "Seller Address", true, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_SellerName), ResourceStringData.Empty, "Seller Name", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.RailReferenceNumber), ResourceStringData.Empty, "Rail Ref.", false, ZString.Empty);
					AssertColumn(grid, nameof(USConsignmentCombined.UBV_IsActive), ResourceStringData.Empty, "Active", false, ZString.Empty);
				});
			}
		}

		public void TestConvertToFormalDeclarationMenuItem()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			AssertNullOrEmpty("precondition: No stand alone declarations created", consignment1.CE_EntryLineReference);
			Assert(consignment1.ULB_IsActive);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.FirePerformSearch();
				AssertEquals(1, grid.VisibleRowCount);

				grid.SelectAllElements();
				grid.ContextMenu.DoPopup();

				var convertToStandAloneDeclarationMenuItem = grid.ContextMenu.MenuItems.FindByText("Convert to Stand Alone Declaration", false);
				AssertNotNull(convertToStandAloneDeclarationMenuItem);
				AssertEquals("Convert to Stand Alone Declaration", convertToStandAloneDeclarationMenuItem.Text);
				Assert("convertToStandAloneDeclarationMenuItem should show", convertToStandAloneDeclarationMenuItem.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				convertToStandAloneDeclarationMenuItem.PerformClick();
				AssertMultilineASCIIEquals(@"1 Stand Alone Declarations queued for processing, check the individual consignment for status", UnitTestUserNotification.Instance.LastMessage.Text);

				Application.DoEvents();
				AssertEquals(0, grid.VisibleRowCount);
				Assert(!consignment1.ULB_IsActive);

				var logs = clearance.Logs.Find(new ZQuery());
				var logForConsignment1 = clearance.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.TransferToCustomsImportsDecCode)).Where(x => x.SL_Reference.Contains(consignment1.PK.ToString())).ToArray();
				AssertEquals("precondition: Convert to stand alone declarations event created", 1, logForConsignment1.Length);

				grid.ContextMenu.DoPopup();
				Assert("convertToStandAloneDeclarationMenuItem should be hidden", !convertToStandAloneDeclarationMenuItem.Visible);

				var consignment2 = clearance.CusUSLVConsignments.AddNew();
				Factory.Save();

				filterControl.FirePerformSearch();
				AssertEquals(1, grid.VisibleRowCount);

				grid.SelectAllElements();
				grid.ContextMenu.DoPopup();
				Assert("convertToFormalDeclarationMenuItem should show", convertToStandAloneDeclarationMenuItem.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				convertToStandAloneDeclarationMenuItem.PerformClick();
				AssertMultilineASCIIEquals(
@"1 Consignment(s) have been selected
0 Consignment(s) have previously been converted and will be ignored
1 Consignment(s) will be converted to a Stand Alone Declaration
", UnitTestUserNotification.Instance.LastMessage.Text);
				var logForConsignment2 = clearance.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.TransferToCustomsImportsDecCode)).Where(x => x.SL_Reference.Contains(consignment2.PK.ToString())).ToArray();
				AssertEquals("No convert to stand alone declaration log created for consignment #2 because operation cancelled", 0, logForConsignment2.Length);
				Assert(consignment2.ULB_IsActive);
			}
		}
	}
}
