using System;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.GUI
{
	sealed class CFSLoadListConsolFormInternalTest : BaseFreightTest
	{
		public void TestCoLoadWizardValidates()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.AutomaticallyUpdatePackLineContainers = true;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "JFDU8392839";
			container.JC_RC = RC_40GP_PK;

			var transport = consol.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.OuterPackLines.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.OuterPackLines.AddNew();

			using (var form = new CFSLoadListConsolForm(consol))
			{
				form.Show();
				form.CoLoadWizardMenuItem_ClickInternal(form, new EventArgs());
				AssertEquals("Please select a Co-Load Master Shipment to run this wizard", UnitTestUserNotification.Instance.LastMessage.Text);

				form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.Select(1);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CoLoadWizardMenuItem_ClickInternal(form, new EventArgs());
				AssertEquals("Please save the current records before running the wizard", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				form.CoLoadWizardMenuItem_ClickInternal(form, new EventArgs());
				AssertEquals("The selected shipment is not a co-load Master. Please tick Coload Master on this record to be able to add sub-shipments", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((CommonShipment)form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.SelectedElements[0]).JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
				Factory.Save();
				form.CoLoadWizardMenuItem_ClickInternal(form, new EventArgs());
				AssertEquals("CoLoad Wizard would be hit", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
