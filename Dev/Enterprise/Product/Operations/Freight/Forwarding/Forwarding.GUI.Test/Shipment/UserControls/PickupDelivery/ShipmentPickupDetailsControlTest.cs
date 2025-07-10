using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ShipmentPickupDetailsControlTest : BaseFreightTest
	{
		#region ACIZone Label Visibility

		public void TestACIZoneLabelVisibility()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			{
				ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
					using (ShipmentPickupDetailsControl control = new ShipmentPickupDetailsControl(shipment))
					{
						form.Controls.Add(control);
						form.Show();
						Assert("ACI Zone label not visible", !control.ACIConsignorOriginZoneLabel.Visible);
					}

					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
					using (ShipmentPickupDetailsControl control = new ShipmentPickupDetailsControl(shipment))
					{
						form.Controls.Add(control);
						form.Show();
						Assert("ACI Zone label visible", control.ACIConsignorOriginZoneLabel.Visible);
					}

					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Canada);
					using (ShipmentPickupDetailsControl control = new ShipmentPickupDetailsControl(shipment))
					{
						form.Controls.Add(control);
						form.Show();
						Assert("ACI Zone label visible", control.ACIConsignorOriginZoneLabel.Visible);
					}
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(storedCompany);
				}
			}
		}

		#endregion

		#region TestRenameDemurrageAndStorage

		[RequiresSTA]
		public void TestRenameDemurrageAndStorage()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			using (var control = new ShipmentPickupDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();

				var demurrageOnPickupCharge = (ZLabel)control.Controls.Find("zLabelTruckWaitTime", true)[0];
				AssertEquals("Truck Wait Time", demurrageOnPickupCharge.Extensions.Get<ILabelCaptionRenderer>().Caption);
			}
		}

		#endregion

		#region TestControlVisibility

		public void TestControlVisibility()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			using (var form = new ZForm(shipment))
			using (var control = new ShipmentPickupDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertFCLControlsVisibility(control, false);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_PackingMode = Constants.ContainerModes.LCL;
				AssertFCLControlsVisibility(control, false);

				shipment.JS_PackingMode = Constants.ContainerModes.FCL;
				AssertFCLControlsVisibility(control, true, true);
			}
		}

		public void TestControlVisibility_ShipmentStatusDropEdit()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (var form = new ZForm(shipment))
			using (var control = new ShipmentPickupDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				Assert("ShipmentStatusDropEdit is only for SEA jobs", !control.ShipmentStatusDropEdit.Visible);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				Assert("ShipmentStatusDropEdit is only for SEA jobs", control.ShipmentStatusDropEdit.Visible);
			}
		}

		void AssertFCLControlsVisibility(ShipmentPickupDetailsControl userControl, bool visible, bool penaltyGridVisible = false)
		{
			AssertEquals("Detention label visible", visible, userControl.DetentionLabel.Visible);
			AssertEquals("Free label visible", visible, userControl.FreeLabel.Visible);
			AssertEquals("Detention days label visible", visible, userControl.DetentionDaysLabel.Visible);
			AssertEquals("Detention Free Days visible", visible, userControl.DetentionFreeDaysCalcEdit.Visible);
			AssertEquals("Detention Days visible", visible, userControl.DetentionDaysCalcEdit.Visible);
			AssertEquals("Detention Charge visible", visible, userControl.DetentionChargeCalcEdit.Visible);

			if (visible && penaltyGridVisible)
			{
				Assert("Pickup Penalties Tab visible", userControl.PickupPenaltiesTabPage.TabVisible);
			}
			else
			{
				Assert("Pickup Penalties Tab invisible", !userControl.PickupPenaltiesTabPage.TabVisible);
			}
		}

		#endregion

		#region TestPickupRequiredFromField

		public void TestPickupRequiredFromField()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			using (var control = new ShipmentPickupDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();

				var requiredFrom = (ZDateEdit)control.Controls.Find("JP_PickupRequiredFromDateEdit", true)[0];
				AssertEquals("Pickup Required From", requiredFrom.Extensions.Get<ILabelCaptionRenderer>().Caption);
			}
		}

		#endregion

		public void TestPlugIns()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			using (var control = new ShipmentPickupDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();

				var requiredFrom = (ZTabControl)control.Controls.Find("ContainerTabControl", true)[0];
				AssertNotNull(requiredFrom);
				AssertEquals(1, requiredFrom.PlugIns.Instances.Length);
				AssertEquals(ControllerIDs.DtbBookingTabPlugIn, requiredFrom.PlugIns.Instances[0].ControllerID);
			}
		}
	}
}
