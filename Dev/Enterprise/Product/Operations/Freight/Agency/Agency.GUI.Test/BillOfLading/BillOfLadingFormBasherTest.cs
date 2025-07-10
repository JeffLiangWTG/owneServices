using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(BillOfLadingForm))]
	internal sealed class BillOfLadingFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.HasChanges = false;

			Factory.Save();

			var result = new BillOfLadingForm(shipment);
			result.ControllerID = ControllerIDs.AgencyBillOfLading;
			return result;
		}

		protected override void BashControl(Control controlToBash)
		{
			base.BashControl(controlToBash);
			if (controlToBash.Name == "JS_PackingModeBoundDropEdit")
			{
				var modeDropEdit = (ZDropEdit)controlToBash;
				var form = (BillOfLadingForm)modeDropEdit.FindForm();
				var helper = new BillOfLadingForm.TestHelper(form);

				modeDropEdit.Text = Constants.ContainerModes.FCL;
				BashControl(helper.ContainersTabPage);

				modeDropEdit.Text = Constants.ContainerModes.RollOnRollOff;
				BashControl(helper.VehicleTabPage);

				modeDropEdit.Text = Constants.ContainerModes.Bulk;
			}
		}

		#endregion
	}
}
