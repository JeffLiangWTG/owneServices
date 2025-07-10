using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class GatePassDetailsTest : BaseFreightTest
	{
		const string BindMessage = "{0} binds via OuterPackLines, this is a big no-no on this control.\n" +
			"instead you should bind via ContainerLegs.TransportPackLineDivots.PackLine otherwise \n" +
			"it will show the details for the first packline instead of the selected packline";

		public void TestDontBindToPackLines()
		{
			using (GatePassDetails control = new GatePassDetails())
			{
				CheckControl(control);
			}
		}

		public void TestDetailsButton()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			CFSShipmentStatusProvider.SetDummyForTest(new CFSShipmentStatusProviderDummyObject(shipment));

			using (GatePassDetails detailsControl = new GatePassDetails())
			{
				detailsControl.GatePassShipment = shipment;
				detailsControl.Show();
				detailsControl.DetailsButton.PerformClick();
				AssertEquals("Cuckoo Squeaker of Message Details", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDetailsButtonForCA()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			CFSShipmentStatusProvider.SetDummyForTest(new CFSShipmentStatusProviderDummyObject(shipment));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				using (GatePassDetails detailsControl = new GatePassDetails())
				{
					detailsControl.GatePassShipment = shipment;
					detailsControl.Show();
					detailsControl.DetailsButton.PerformClick();

					var form = ZFormModaliser.LastFormShownDialogForTest as HtmlInterpretationForm;
					AssertNotNull("A HtmlInterpretationForm should be showed", form);
				}
			}
		}

		void CheckControl(Control control)
		{
			if (!string.IsNullOrEmpty(control.GetBindingMember()))
			{
				ZString actual = control.GetBindingMember();
				ZString psudoExpected = actual.Replace("OuterPackLines", "ContainerLegs.TransportPackLineDivots.PackLine");
				AssertEquals(string.Format(BindMessage, control.Name), psudoExpected, actual);
			}
			else
			{
				foreach (Control child in control.Controls)
				{
					CheckControl(child);
				}
			}
		}

		public void TestHouseCCNVisibility()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			CFSShipmentStatusProvider.SetDummyForTest(new CFSShipmentStatusProviderDummyObject(shipment));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				using (GatePassDetails detailsControl = new GatePassDetails())
				{
					detailsControl.GatePassShipment = shipment;
					detailsControl.Show();

					ZTextBox houseCCNTextBox = (ZTextBox)detailsControl.Controls.Find("HouseCCNTextBox", true)[0];

					Assert("HouseCCNTextBox should be visible", houseCCNTextBox.Visible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				using (GatePassDetails detailsControl = new GatePassDetails())
				{
					detailsControl.GatePassShipment = shipment;
					detailsControl.Show();

					ZTextBox houseCCNTextBox = (ZTextBox)detailsControl.Controls.Find("HouseCCNTextBox", true)[0];

					Assert("HouseCCNTextBox should not be visible", !houseCCNTextBox.Visible);
				}
			}
		}
	}
}
