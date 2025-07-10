using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	[TestedType(typeof(MultiSelectModuleForm))]
	sealed class MultiSelectModuleFormTest : ZFormBasherTest
	{
		public void TestMultiSelection()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var property = TypeDescriptor.GetProperties(typeof(Shipment))[Shipment.Schema.B0_MasterBillNumber];
				var splitContainer = (SplitContainer)ManifestUserControlTestCase.FindControl(form, "SplitContainer");
				var panel1 = splitContainer.Panel1;
				var findControl = (ZFilterStripControl)ManifestUserControlTestCase.FindControl(panel1, "ZFilterStripControl");
				var grid1 = (ZGrid)ManifestUserControlTestCase.FindControl(findControl, "FilteredGrid");
				grid1.List.ApplySort(property, ListSortDirection.Ascending);
				var panel2 = splitContainer.Panel2;
				var grid2 = (ZGrid)ManifestUserControlTestCase.FindControl(panel2, "FilteredGrid");
				grid2.List.ApplySort(property, ListSortDirection.Ascending);
				var addButton = (ZButton)ManifestUserControlTestCase.FindControl(panel2, "AddToSelectionButton");
				var removeButton = (ZButton)ManifestUserControlTestCase.FindControl(panel2, "RemoveFromSelectionButton");
				//Form loaded, nothing in the grids
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertEquals("Grid 1 elements count", 0, grid1.SelectedRowCount);
				AssertEquals("Grid 2 elements count", 0, grid2.SelectedRowCount);
				//Perform search, should not include parent's shipments
				var originalFactory = findControl.GridCollection.Factory;
				var factory = originalFactory;
				findControl.FirePerformSearch();
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertNotEquals("Factory changed", factory, findControl.GridCollection.Factory);
				AssertEquals("Grid 1 elements count", 4, grid1.SelectedRowCount);
				AssertContains("Grid 1 contains shipment 1", grid1, shipment1);
				AssertContains("Grid 1 contains shipment 2", grid1, shipment2);
				AssertContains("Grid 1 contains shipment 3", grid1, shipment3);
				AssertContains("Grid 1 contains shipment 4", grid1, shipment4);
				AssertEquals("Grid 2 elements count", 0, grid2.SelectedRowCount);
				//Filter by trip reference
				factory = findControl.GridCollection.Factory;
				var filter = (ModuleGuidFilter)findControl.FilterBusinessObject["Trip Reference"];
				filter.Property = trip1.PK;
				filter.IsActive = true;
				findControl.FirePerformSearch();
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertNotEquals("Factory changed", factory, findControl.GridCollection.Factory);
				AssertEquals("Grid 1 elements count", 3, grid1.SelectedRowCount);
				AssertContains("Grid 1 contains shipment 1", grid1, shipment1);
				AssertContains("Grid 1 contains shipment 2", grid1, shipment2);
				AssertContains("Grid 1 contains shipment 3", grid1, shipment3);
				AssertEquals("Grid 2 elements count", 0, grid2.SelectedRowCount);
				//Add two rows to the selection grid
				factory = findControl.GridCollection.Factory;
				grid1.UnSelect(1);
				addButton.PerformClick();
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertEquals("Factory not changed", factory, findControl.GridCollection.Factory);
				AssertEquals("Grid 1 elements count", 1, grid1.SelectedRowCount);
				AssertContains("Grid 1 contains shipment 2", grid1, shipment2);
				AssertEquals("Grid 2 elements count", 2, grid2.SelectedRowCount);
				AssertContains("Grid 2 contains shipment 1", grid2, shipment1);
				AssertContains("Grid 2 contains shipment 3", grid2, shipment3);
				//Remove one row from the selection grid
				grid2.UnSelect(1);
				removeButton.PerformClick();
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertEquals("Factory not changed", factory, findControl.GridCollection.Factory);
				AssertEquals("Grid 1 elements count", 2, grid1.SelectedRowCount);
				AssertContains("Grid 1 contains shipment 1", grid1, shipment1);
				AssertContains("Grid 1 contains shipment 2", grid1, shipment2);
				AssertEquals("Grid 2 elements count", 1, grid2.SelectedRowCount);
				AssertContains("Grid 2 contains shipment 3", grid2, shipment3);
				//No popups yet
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				//Add to selection if nothing selected
				grid1.UnSelectAll();
				addButton.PerformClick();
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertEquals("Factory not changed", factory, findControl.GridCollection.Factory);
				AssertEquals("Add To Selection", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please select rows from the first grid to add.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Grid 1 elements count", 2, grid1.SelectedRowCount);
				AssertEquals("Grid 2 elements count", 1, grid2.SelectedRowCount);
				//Rollback if nothing selected
				grid2.UnSelectAll();
				removeButton.PerformClick();
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertEquals("Factory not changed", factory, findControl.GridCollection.Factory);
				AssertEquals("Rollback", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please select rows from the second grid to rollback.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Grid 1 elements count", 2, grid1.SelectedRowCount);
				AssertEquals("Grid 2 elements count", 1, grid2.SelectedRowCount);
				//Rollback all rows
				grid2.SelectAllElements();
				removeButton.PerformClick();
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertEquals("Factory not changed", factory, findControl.GridCollection.Factory);
				AssertEquals("Grid 1 elements count", 3, grid1.SelectedRowCount);
				AssertContains("Grid 1 contains shipment 1", grid1, shipment1);
				AssertContains("Grid 1 contains shipment 2", grid1, shipment2);
				AssertContains("Grid 1 contains shipment 3", grid1, shipment3);
				AssertEquals("Grid 2 elements count", 0, grid2.SelectedRowCount);
				//Add all rows
				grid1.SelectAllElements();
				addButton.PerformClick();
				Application.DoEvents();
				grid1.SelectAllElements();
				grid2.SelectAllElements();
				AssertEquals("Factory not changed", factory, findControl.GridCollection.Factory);
				AssertEquals("Grid 1 elements count", 0, grid1.SelectedRowCount);
				AssertEquals("Grid 2 elements count", 3, grid2.SelectedRowCount);
				AssertContains("Grid 2 contains shipment 1", grid2, shipment1);
				AssertContains("Grid 2 contains shipment 2", grid2, shipment2);
				AssertContains("Grid 2 contains shipment 3", grid2, shipment3);
				//Perform search if everything is selected
				findControl.FirePerformSearch();
				Application.DoEvents();
				AssertNotEquals("Factory changed", factory, findControl.GridCollection.Factory);
				AssertEquals("There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				//Remove one row and confirm selection
				factory = findControl.GridCollection.Factory;
				grid2.UnSelectAll();
				grid2.Select(0);
				removeButton.PerformClick();
				Application.DoEvents();
				var okButton = (ZButton)ManifestUserControlTestCase.FindControl(form, "OK_Button");
				okButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Factory should be restored to original", originalFactory, findControl.GridCollection.Factory);
				Assert("Form should be closed", form.IsDisposed);
				Assert("Trip should have changes", trip3.HasChanges);
				AssertEquals("Trip 3 shipments count", 4, trip3.Shipments.Count);
				Assert("Trip 3 contains shipment 2", trip3.Shipments.Contains(shipment2));
				Assert("Trip 3 contains shipment 3", trip3.Shipments.Contains(shipment3));
				Assert("Trip 3 contains shipment 4", trip3.Shipments.Contains(shipment5));
				Assert("Trip 3 contains shipment 5", trip3.Shipments.Contains(shipment6));
			}
		}

		public void TestMultiSelectionCancelled()
		{
			using (var form = (MultiSelectModuleForm)GetFormToBashCore())
			{
				form.Show();
				var splitContainer = (SplitContainer)ManifestUserControlTestCase.FindControl(form, "SplitContainer");
				var panel1 = splitContainer.Panel1;
				var findControl = (ZFilterStripControl)ManifestUserControlTestCase.FindControl(panel1, "ZFilterStripControl");
				var grid1 = (ZGrid)ManifestUserControlTestCase.FindControl(findControl, "FilteredGrid");
				var panel2 = splitContainer.Panel2;
				var grid2 = (ZGrid)ManifestUserControlTestCase.FindControl(panel2, "FilteredGrid");
				var addButton = (ZButton)ManifestUserControlTestCase.FindControl(panel2, "AddToSelectionButton");
				findControl.FirePerformSearch();
				grid1.Select(0);
				grid1.Select(1);
				addButton.PerformClick();
				grid2.SelectAllElements();
				Application.DoEvents();
				AssertEquals("Grid 2 elements count", 2, grid2.SelectedRowCount);
				AssertContains("Grid 2 contains shipment 1", grid2, shipment1);
				AssertContains("Grid 2 contains shipment 2", grid2, shipment2);
				var cancelButton = (ZButton)ManifestUserControlTestCase.FindControl(form, "Cancel_Button");
				cancelButton.PerformClick();
				Application.DoEvents();
				Assert("Form should be closed", form.IsDisposed);
				Assert("Trip should not have changes", !trip3.HasChanges);
				AssertEquals("Trip 3 shipments count", 2, trip3.Shipments.Count);
				Assert("Trip 3 contains shipment 4", trip3.Shipments.Contains(shipment5));
				Assert("Trip 3 contains shipment 5", trip3.Shipments.Contains(shipment6));
			}
		}

		static void AssertContains(string message, ZGrid grid, Shipment shipment)
		{
			Assert(message, grid.SelectedElements.Any(s => s.PK == shipment.PK));
		}

		protected override Form GetFormToBashCore()
		{
			trip1 = Factory.New<Trip>();
			shipment1 = trip1.Shipments.AddNew();
			shipment1.B0_MasterBillNumber = "1";
			shipment2 = trip1.Shipments.AddNew();
			shipment2.B0_MasterBillNumber = "2";
			shipment3 = trip1.Shipments.AddNew();
			shipment3.B0_MasterBillNumber = "3";
			var shipment7 = trip1.Shipments.AddNew();
			shipment7.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			shipment7.B0_MasterBillNumber = "7";
			var trip2 = Factory.New<Trip>();
			shipment4 = trip2.Shipments.AddNew();
			shipment4.B0_MasterBillNumber = "4";
			trip3 = Factory.New<Trip>();
			shipment5 = trip3.Shipments.AddNew();
			shipment5.B0_MasterBillNumber = "5";
			shipment6 = trip3.Shipments.AddNew();
			shipment6.B0_MasterBillNumber = "6";
			Factory.Save();
			return new MultiSelectModuleForm(trip3);
		}

		Shipment shipment1;
		Shipment shipment2;
		Shipment shipment3;
		Shipment shipment4;
		Shipment shipment5;
		Shipment shipment6;
		Trip trip1;
		Trip trip3;
	}
}
