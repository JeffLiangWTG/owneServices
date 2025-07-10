using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.PortHubs.GUI.Testing
{
	sealed class PortHubFilterStripControlTest : TestCaseWithFactory
	{
		public void TestClearAllFilterStripValuesAndReload()
		{
			var collectionWrapper = new PortHubSelectionCollectionWrapper(Factory);
			var collection = collectionWrapper.Collection;
			collection.AddNew().TY_Direction = "PIC";
			var dlvSelection = collection.AddNew();
			dlvSelection.TY_Direction = "DLV";

			using (var form = new PortHubSelectionForm(collectionWrapper))
			{
				form.Show();
				var control = form.FindSingle<PortHubFilterStripControl>("portHubFilterStripControl");
				var filterBizo = control.FindSingle<PortHubStripControl>("portHubStripControl").FilterBusinessObject;
				var unlocoFilter = filterBizo[PortHubFilterStripBusinessObject.Descriptions.Direction] as ModuleTextFilter;
				unlocoFilter.IsActive = true;
				unlocoFilter.Property = "PIC";
				collection.Load(unlocoFilter.Query);
				Assert(!collection.Contains(dlvSelection));

				control.ClearAllFilterStripValuesAndReload();
				Assert(collection.Contains(dlvSelection));
			}
		}

		public void TestStripControl()
		{
			var collectionWrapper = new PortHubSelectionCollectionWrapper(Factory);

			using (var form = new PortHubSelectionForm(collectionWrapper))
			{
				form.Show();

				var filterStripControl = form.Controls.Find("portHubFilterStripControl", true)[0] as PortHubFilterStripControl;
				var panel = filterStripControl.Controls.Find("filterStripPanel", false)[0] as ZPanel;
				AssertEquals(1, panel.Controls.OfType<PortHubStripControl>().Count());
				var stripControl = panel.Controls[0] as PortHubStripControl;
				CombineAssertions(() =>
				{
					Assert(!stripControl.AutoScroll);
					Assert(stripControl.AutoSize);
					Assert(panel.AutoSize);
				});
			}
		}
	}
}
