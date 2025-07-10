using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Freight.PortHubs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Testing
{
	[TestedType(typeof(PortDepotCarrierSelectionDetailsForm))]
	public class PortDepotCarrierSelectionDetailsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var collectionWrapper = new PortHubSelectionCollectionWrapper(Factory).Collection.AddNew();
			var form = new PortDepotCarrierSelectionDetailsForm(collectionWrapper);
			form.ControllerID = ControllerIDs.PortDepotCarrierSelection;
			return form;
		}

		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				AssertEquals("Port/Depot/Carrier Selection", form.FormHeading);
				form.Show();
				AssertEquals("Port/Depot/Carrier Selection", form.Text);
			}
		}

		public void TestFormCaption()
		{
			var selection = Factory.NewWithValidTestData<PortHubSelection>();
			using (var form = new PortDepotCarrierSelectionDetailsForm(selection))
			{
				form.Show();
				AssertEquals("Port/Depot/Carrier Selection", form.FormCaption);
			}
		}

		public void TestSecurity()
		{
			Env.Security.PortDepotSelectionModify.IsAllowed = false;
			using (var form = GetFormToBashCore())
			{
				var dropEdit = form.FindSingle<ZDropEdit>("DirectionDropEdit");
				Assert(dropEdit.ReadOnly);
			}

			Env.Security.PortDepotSelectionModify.IsAllowed = true;
			using (var form = GetFormToBashCore())
			{
				var dropEdit = form.FindSingle<ZDropEdit>("DirectionDropEdit");
				Assert(!dropEdit.ReadOnly);
			}
		}

		#region Implementation

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		#endregion
	}
}
