using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI.Testing
{
	public class EntryGridTest : TestCaseWithFactory
	{
		public void TestDoesNotAutoCommit()
		{
			var header = Factory.NewWithValidTestData<Costing>();

			using (var form = new CostingForm(header))
			{
				form.Show();

				var mainTabs = (CostingTabControl)form.BaseTabControl;
				var rateEntryGrid = mainTabs.FindTabPage("AIR").RateEntryGrid as EntryGrid;
				rateEntryGrid.CurrentCell = new DataGridCell(0, 1);
				Application.DoEvents();

				var rateLineGrid = mainTabs.CurrentRateLinesAndItemsControl.RateLinesGrid;
				rateLineGrid.Focus();
				Application.DoEvents();

				AssertEquals("One for the new row", 1, rateEntryGrid.VisibleRowCount);
			}
		}

		public void TestHandleDelete()
		{
			var header = Factory.NewWithValidTestData<Costing>();
			string[] unlocos = new string[] { "AUMEL", "AUBNE", "AUPER", "USCHI", "USLAX", "CNSHA", "NZAKL", "SGSIN", "UAODS", "NLAMS" };
			for (int i = 0; i < 10; i++)
			{
				header.AddRateEntry("AIR", "LSE", "AUSYD", unlocos[i]);
			}

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedHeader = loadFactory.Load<Costing>(header.PK);

			using (var form = new CostingForm(loadedHeader))
			{
				form.Show();
				var grid = form.BaseTabControl.FindTabPage("AIR").RateEntryGrid as EntryGrid;
				grid.SelectAllElements();
				grid.CurrentRowIndex = grid.EntryCollection.Count - 1;
				grid.OnDeleteKeyPressed();
			}

			var inMemoryQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			int loadedRateLineItemCount = loadFactory.Load<RateLineItem>(inMemoryQuery).Length;

			AssertEquals("All RateLineItems should be deleted", 0, loadedRateLineItemCount);
			loadFactory.Save();
			AssertEquals(1, loadedHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "0/0/10 entries added/edited/deleted")).Length);
		}

		public void TestDragAndDropWithResetCreationSource()
		{
			var sourceCosting = Factory.NewWithValidTestData<Costing>();
			var sourceEntry = sourceCosting.AddRateEntry("AIR", "LSE", "AUSYD", "");
			AssertEquals("Precondition", string.Empty, sourceEntry.TI_CreationSource);
			sourceEntry.TI_CreationSource = RateEntryCreator.Sources.FromQuotation;
			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedSource = loadFactory.Load<Costing>(sourceCosting.PK);
			var loadedDestination = loadFactory.NewWithValidTestData<Costing>();

			using (var destinationForm = new CostingForm(loadedDestination))
			{
				destinationForm.Show();
				destinationForm.Activate();
				Application.DoEvents();

				var destinationAirTab = destinationForm.BaseTabControl.FindTabPage("AIR");
				var destinationGrid = destinationAirTab.RateEntryGrid as EntryGrid;
				var fltTextbox = destinationAirTab.FindAll<ZCalcEdit>(x => true, 20);
				destinationGrid.Focus();
				destinationGrid.ListManager.RemoveAt(0);
				Application.DoEvents();

				var rateEntries = new ArrayList(loadedSource.ChildRateEntries.ToList());
				var sourceRateEntry = rateEntries[0] as RateEntry;
				var dragArg = new DragEventArgs(new DataObject(rateEntries), 0, 0, 0, DragDropEffects.All, DragDropEffects.Copy);

				// Initial drag and drop.
				destinationGrid.OnDragDrop_ForTest(dragArg);
				Application.DoEvents();
				var destinationRateEntry = destinationGrid.EntryCollection.Single() as RateEntry;

				AssertEquals("The source RateEntry has the original creation source", RateEntryCreator.Sources.FromQuotation, sourceRateEntry.TI_CreationSource);
				AssertEquals("The D'n'D copy should have no creation source", string.Empty, destinationRateEntry.TI_CreationSource);
				AssertEquals("The D'n'D copy should have no creation source", RateEntryCreator.Sources.Manual, destinationRateEntry.TI_CreationSource);
			}
		}

		public void TestDragAndDropWithCorrectValueShown()
		{
			var sourceCosting = Factory.NewWithValidTestData<Costing>();
			sourceCosting.Header.OH_Code = "Src";
			var sourceEntry = sourceCosting.AddRateEntry("AIR", "LSE", "AUSYD", "");
			sourceEntry.RateLines.RemoveAndDeleteAll();
			var sourceLine = sourceEntry.AddRateLine("BAF", "FLT", "KG");
			sourceLine.GetCalculator<FlatCalculator>().BaseRate = 123;

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedSource = loadFactory.Load<Costing>(sourceCosting.PK);
			var loadedDestination = loadFactory.NewWithValidTestData<Costing>();
			loadedDestination.Header.OH_Code = "Dest";

			using (var destinationForm = new CostingForm(loadedDestination))
			{
				destinationForm.Show();
				destinationForm.Activate();
				Application.DoEvents();

				var destinationAirTab = destinationForm.BaseTabControl.FindTabPage("AIR");
				var destinationGrid = destinationAirTab.RateEntryGrid as EntryGrid;
				var fltTextbox = destinationAirTab.FindAll<ZCalcEdit>(x => true, 20);
				destinationGrid.Focus();
				destinationGrid.ListManager.RemoveAt(0);
				Application.DoEvents();

				var rateEntries = new ArrayList(loadedSource.ChildRateEntries.ToList());
				var dragArg = new DragEventArgs(new DataObject(rateEntries), 0, 0, 0, DragDropEffects.All, DragDropEffects.Copy);

				// Initial drag and drop.
				destinationGrid.OnDragDrop_ForTest(dragArg);
				Application.DoEvents();
				Assert("Text should show monetary value", destinationAirTab.FindSingle<ZCalcEdit>(maxLevelsDeep: 100).Text == "123.0000");

				// Delete
				destinationGrid.EntryCollection.RemoveAll();
				Application.DoEvents();

				// Drag and drop the second time.
				destinationGrid.OnDragDrop_ForTest(dragArg);
				Application.DoEvents();
				Assert("Text should show monetary value", destinationAirTab.FindSingle<ZCalcEdit>(maxLevelsDeep: 100).Text == "123.0000");
			}
		}
	}
}
