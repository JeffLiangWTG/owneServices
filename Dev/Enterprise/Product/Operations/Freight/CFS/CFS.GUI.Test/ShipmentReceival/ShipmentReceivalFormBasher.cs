using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	[TestedType(typeof(ShipmentReceivalForm))]
	sealed class ShipmentReceivalFormBasher : ZFormBasherTest
	{
		public void TestDeliveryOrderHandedOverMenu()
		{
			bool menuFound = false;
			using (ShipmentReceivalForm shipmentForm = (ShipmentReceivalForm)GetFormToBashCore())
			{
				foreach (MenuItem menu in shipmentForm.Menu.MenuItems)
				{
					if (menu.Text == "Actio&ns")
					{
						foreach (MenuItem innerMenu in menu.MenuItems)
						{
							if (innerMenu.Text == "Delivery Order Handed Over")
							{
								menuFound = true;
							}
						}
					}
				}
			}
			Assert("Delivery Order Handed Over is in Actions Menu", menuFound);
		}

		public void TestWorkflowTabHiddenIfForwardRegistered()
		{
			var workflowTabPageField = typeof(ShipmentReceivalForm).GetField("WorkflowTabPage", BindingFlags.NonPublic | BindingFlags.Instance);

			var shipment = Factory.New<CFSShipment>();
			shipment.JS_IsForwardRegistered = true;

			using (var form = new ShipmentReceivalForm(shipment))
			{
				var tabPage = (ZWorkflowTabPage)workflowTabPageField.GetValue(form);
				Assert("Should not be visible", !tabPage.TabVisible);
				Assert("Should not be initialized", !((IWorkflowTabPage)tabPage).Initialized);

				shipment.JS_IsForwardRegistered = false;

				form.FireValidateAllForTest();
				UserIdleWorker.Flush();
				AssertEquals("", ErrorReporter.LastMessageReported);
				Assert("Should be visible", tabPage.TabVisible);
				Assert("Should be initialized", ((IWorkflowTabPage)tabPage).Initialized);
			}

			shipment.JS_IsForwardRegistered = false;

			using (var form = new ShipmentReceivalForm(shipment))
			{
				var tabPage = (ZWorkflowTabPage)workflowTabPageField.GetValue(form);
				Assert("Should be visible", tabPage.TabVisible);
				Assert("Should be initialized", ((IWorkflowTabPage)tabPage).Initialized);

				shipment.JS_IsForwardRegistered = true;
				Assert("Should not be visible", !tabPage.TabVisible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertNotNull("Shipment should have a JobDocsAndCartage.", shipment.DocsAndCartage);
			ShipmentReceivalForm result = new ShipmentReceivalForm(shipment);
			result.ControllerID = ControllerIDs.ShipmentReceival;
			MissingResourceStringChecker.ExcludeFromTest(result.ShipmentDetails.ConsignorDocumentaryDocAddressControl);
			return result;
		}
	}
}
