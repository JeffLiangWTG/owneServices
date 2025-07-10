using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	[TestedType(typeof(AWBViewEditForm))]
	public class AWBViewEditFormTest : ZFormBasherTest
	{
		public void TestClosing()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OverrideWaybillDefaults = true;

			using (AWBViewEditForm form = new AWBViewEditForm(shipment))
			{
				form.Show();
				ZTextBox textBox = GetTextBox(form.hawbUserControl1);
				textBox.Focus();
				textBox.Text = "A";
				form.Close();
			}

			using (AWBViewEditForm form = new AWBViewEditForm(shipment))
			{
				form.Show();
				ZTextBox textBox = GetTextBox(form.hawbUserControl1);
				AssertEquals("A", textBox.Text);
			}
		}

		public void TestReplaceMacrosExceptionOccur_Shipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			using (FreightDataRegistry.Instance.HAWBNatureAndQtyOfGoodsExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"{First}\" == \"1\").First()>"))
			using (var form = new AWBViewEditForm(shipment))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals($@"HAWB cannot be generated.
Please correct macro format in Registry -> {FreightDataRegistry.Instance.HAWBNatureAndQtyOfGoodsExtraText.HumanReadableRegistryPath()}.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form been closed", false, form.Visible);
			}
		}

		public void TestReplaceMacrosExceptionOccur_Consol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			using (FreightDataRegistry.Instance.MAWBNatureAndQtyOfGoodsExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"{First}\" == \"1\").First()>"))
			using (var form = new AWBViewEditForm(consol))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals($@"MAWB cannot be generated.
Please correct macro format in Registry -> {FreightDataRegistry.Instance.MAWBNatureAndQtyOfGoodsExtraText.HumanReadableRegistryPath()}.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form been closed", false, form.Visible);
			}
		}

		ZTextBox GetTextBox(Control control)
		{
			ZTextBox result = null;

			foreach (Control ctrl in control.Controls)
			{
				ZTextBox ctrlAsTextBox = ctrl as ZTextBox;
				if (ctrlAsTextBox != null && ctrlAsTextBox.Name == "HandlingInformationTextBox")
				{
					result = ctrlAsTextBox;
				}

				if (result == null)
				{
					result = GetTextBox(ctrl);
				}
			}

			return result;
		}

		protected override Form GetFormToBashCore()
		{
			var airDepartment = GlbDepartment.CurrentDepartment;
			airDepartment.GE_Sea = false;
			airDepartment.GE_Air = true;
			var shipment = Factory.New<ForwardingShipment>();

			return new AWBViewEditForm(shipment);
		}
	}
}
