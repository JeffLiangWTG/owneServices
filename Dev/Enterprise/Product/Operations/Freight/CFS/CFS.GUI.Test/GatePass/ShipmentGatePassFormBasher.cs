using System.Drawing;
using System.Windows.Forms;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	[TestedType(typeof(ShipmentGatePassForm))]
	sealed class ShipmentGatePassFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].JL_PackageCount = 10;
			shipment.Consols.AddNew();

			Factory.Save();

			ShipmentGatePassForm result = new ShipmentGatePassForm(shipment);
			result.ControllerID = ControllerIDs.ShipmentGatePass;
			return result;
		}

		public void TestSeaCargoPlugInAustralia()
		{
			string storedCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				new ConstantsAndReusables(Factory).SetCountryCode(Enterprise.Core.Constants.CountryCodes.Australia);

				const string AUSeaCargoDepotPlugInClassName = "Enterprise.Customs.AU.SeaCargo.GUI.SCDGatePassPlugIn";
				bool plugInFound = false;
				GatePassShipment shipment = Factory.New<GatePassShipment>();
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				using (ShipmentGatePassForm form = new ShipmentGatePassForm(shipment))
				{
					foreach (ZArchitecture.PlugIn.ZPlugIn plugIn in form.PlugIns.Instances)
					{
						if (plugIn.GetType().FullName == AUSeaCargoDepotPlugInClassName)
						{
							plugInFound = true;
							break;
						}
					}
					AssertEquals("Sea Cargo Depot Plug In should of been found for Country Code AU", true, plugInFound);
				}
				plugInFound = false;
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				using (ShipmentGatePassForm form = new ShipmentGatePassForm(shipment))
				{
					foreach (ZArchitecture.PlugIn.ZPlugIn plugIn in form.PlugIns.Instances)
					{
						if (plugIn.GetType().FullName == AUSeaCargoDepotPlugInClassName)
						{
							plugInFound = true;
							break;
						}
					}
					AssertEquals("Sea Cargo Depot Plug In should not of been found for Air Shipments", false, plugInFound);
				}
				plugInFound = false;
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
				using (ShipmentGatePassForm form = new ShipmentGatePassForm(shipment))
				{
					foreach (ZArchitecture.PlugIn.ZPlugIn plugIn in form.PlugIns.Instances)
					{
						if (plugIn.GetType().FullName == AUSeaCargoDepotPlugInClassName)
						{
							plugInFound = true;
							break;
						}
					}
					AssertEquals("Sea Cargo Depot Plug In should not of been found for Export Shipments", false, plugInFound);
				}
				plugInFound = false;
				new ConstantsAndReusables(Factory).SetCountryCode("ER");
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				using (ShipmentGatePassForm form = new ShipmentGatePassForm(shipment))
				{
					foreach (ZArchitecture.PlugIn.ZPlugIn plugIn in form.PlugIns.Instances)
					{
						if (plugIn.GetType().FullName == AUSeaCargoDepotPlugInClassName)
						{
							plugInFound = true;
							break;
						}
					}
					AssertEquals("Sea Cargo Depot Plug In should not of been found for Country Code ER", false, plugInFound);
				}
			}
			finally
			{
				new ConstantsAndReusables(Factory).SetCountryCode(storedCountryCode);
			}
		}

		#region Status Colours

		class StatusColourTestShipmentGatePassForm : ShipmentGatePassForm
		{
			public StatusColourTestShipmentGatePassForm(GatePassShipment shipment)
				: base(shipment)
			{
			}

			protected override IStatusClassProvider StatusClassProviderCore()
			{
				return StatusClassProviderExposed;
			}

			public IStatusClassProvider StatusClassProviderExposed;
		}

		class DummyStatusClassProvider : IStatusClassProvider
		{
			public DummyStatusClassProvider(StatusClass statusClass)
			{
				this.statusClass = statusClass;
			}

			public StatusClass StatusClass
			{
				get { return statusClass; }
			}
			readonly StatusClass statusClass;
		}

		public void TestStatusColour()
		{
			AssertTextBoxColor(StatusClass.Clear, Color.Green, Color.White);
			AssertTextBoxColor(StatusClass.Warning, Color.Yellow, Color.Red);
			AssertTextBoxColor(StatusClass.Held, Color.Red, Color.Yellow);
			AssertTextBoxColor(StatusClass.Underbonded, Color.CornflowerBlue, Color.Wheat);
		}

		void AssertTextBoxColor(StatusClass statusClass, Color backColor, Color foreColor)
		{
			using (StatusColourTestShipmentGatePassForm form = new StatusColourTestShipmentGatePassForm(Shipment))
			{
				form.StatusClassProviderExposed = new DummyStatusClassProvider(statusClass);
				form.Show();
				AssertEquals("Text box backcolor incorrect", backColor, form.GatePassDetailsUserControl.JS_GatePassStatusBoundTextBox.BackColor);
				AssertEquals("Text box forecolor incorrect", foreColor, form.GatePassDetailsUserControl.JS_GatePassStatusBoundTextBox.ForeColor);
			}
		}

		GatePassShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<GatePassShipment>();
				}
				return shipment;
			}
		}
		GatePassShipment shipment;

		#endregion
	}
}
