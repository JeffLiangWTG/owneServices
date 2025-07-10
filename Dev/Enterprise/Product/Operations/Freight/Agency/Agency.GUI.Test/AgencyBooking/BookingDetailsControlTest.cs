using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BookingDetailsControlTest : BaseAgencyTest
	{
		public void TestPacksCountCalcDropEditCaption()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			using (BookingDetailsControlTestForm form = new BookingDetailsControlTestForm(shipment))
			{
				form.Show();
				ZCalcDropEdit calcDropEdit = GetControl<ZCalcDropEdit>(form.Control, "PacksCountCalcDropEdit");
				AssertEquals("Packs", calcDropEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
				AssertEquals("Vehicles", calcDropEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				AssertEquals("Packs", calcDropEdit.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestBookingContentTabControlSetForPackingModeCalledCorrectly()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			using (var form = new BookingDetailsControlTestForm(shipment))
			{
				form.Show();
				AssertEquals("The first tab should be TopLevelPacksTab", "TopLevelPacksTab", form.ContentTabControl.TabPages[0].Name);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				AssertEquals("The first tab should have changed to ContainersTab", "ContainersTab", form.ContentTabControl.TabPages[0].Name);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
				AssertEquals("The first tab should have changed to VehiclesTab", "VehiclesTab", form.ContentTabControl.TabPages[0].Name);
			}
		}

		public void TestEnableButtonsWhenOpeningANonReadOnlyShipment()
		{
			var booking = Factory.New<AgencyBooking>();
			AssertButtonsAreEnabled(booking, true);
		}

		public void TestDisableButtonsWhenOpeningACanceledShipment()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_IsCancelled = true;
			AssertButtonsAreEnabled(booking, false);
		}

		public void TestDisableButtonsWhenOpeningAConfirmedShipment()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.Confirm();
			AssertButtonsAreEnabled(booking, false);
		}

		public void TestWeightAndVolumeAndPacksVisibility_FCL()
		{
			WeightAndVolumeAndPacksVisibilityGeneric(Core.Constants.ContainerModes.FCL);
		}

		public void TestWeightAndVolumeAndPacksVisibility_BLK()
		{
			WeightAndVolumeAndPacksVisibilityGeneric(Core.Constants.ContainerModes.Bulk);
		}

		public void TestWeightAndVolumeAnsPacksVisibility_RollOnRollOff()
		{
			WeightAndVolumeAndPacksVisibilityGeneric(Core.Constants.ContainerModes.FCL);
		}

		public void TestChangeCommissionedShipment()
		{
			var shipment = Factory.NewWithValidTestData<AgencyBooking>();
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";
			Factory.Save();
			using (ZForm form = new ZForm(shipment))
			{
				using (var control = new BookingDetailsControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_RL_NKOrigin = "UAIEV";
					AssertEquals(shipment.JS_RL_NKOriginInfo, control.LastConfirmedInfo);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_RL_NKDestination = "USLAX";
					AssertEquals(shipment.JS_RL_NKDestinationInfo, control.LastConfirmedInfo);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_PackingMode = "ROR";
					AssertEquals(shipment.JS_PackingModeInfo, control.LastConfirmedInfo);
				}
			}
		}

		class BookingDetailsControlForTest : BookingDetailsControl
		{
			public ZPropertyInfo LastConfirmedInfo { get; set; }

			protected override void ConfirmReversal(ZPropertyInfo info)
			{
				LastConfirmedInfo = info;
				base.ConfirmReversal(info);
			}
		}

		#region Implementation
		void AssertButtonsAreEnabled(AgencyBooking booking, bool shouldBeEnabled)
		{
			var builder = new StringBuilder();
			using (var form = new BookingDetailsControlTestForm(booking))
			{
				form.Show();
				foreach (Control button in form.Control.ButtonsExposedForTesting)
				{
					if (button.Enabled != shouldBeEnabled)
					{
						builder.AppendLine(button.Name);
					}
				}
			}

			string message = string.Format("This buttons should be {0}\r\n{1}", shouldBeEnabled ? "enabled" : "disabled", builder.ToString());
			AssertEquals(message, 0, builder.Length);
		}

		void WeightAndVolumeAndPacksVisibilityGeneric(string packingMode)
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = packingMode;
			bool weightAndVolumeAndPacksVisibility = (packingMode != Core.Constants.ContainerModes.FCL);
			using (BookingDetailsControlTestForm form = new BookingDetailsControlTestForm(shipment))
			{
				form.Show();
				AssertWeightAndVolumeAndPacksVisibility(form.Control, weightAndVolumeAndPacksVisibility);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
				AssertWeightAndVolumeAndPacksVisibility(form.Control, true);
				shipment.JS_PackingMode = packingMode;
				AssertWeightAndVolumeAndPacksVisibility(form.Control, weightAndVolumeAndPacksVisibility);
			}
		}

		void AssertWeightAndVolumeAndPacksVisibility(BookingDetailsControl control, bool expectedVisibility)
		{
			AssertControlVisibility(control, expectedVisibility, "JS_ActualVolumeCalcDropEdit"); // This is a control name not a database field name.
			AssertControlVisibility(control, expectedVisibility, "JS_ActualWeightCalcDropEdit"); // This is a control name not a database field name.
			AssertControlVisibility(control, expectedVisibility, "PacksCountCalcDropEdit");
		}

		void AssertControlVisibility(BookingDetailsControl control, bool expectedVisibility, string controlName)
		{
			Control childControl = GetControl<Control>(control, controlName);
			string message = controlName + (expectedVisibility ? " should be visible" : " should not be visible");
			AssertEquals(message, expectedVisibility, childControl.Visible);
		}

		T GetControl<T>(BookingDetailsControl control, string name)
		{
			return (T)typeof(BookingDetailsControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}
		#endregion
	}
}
