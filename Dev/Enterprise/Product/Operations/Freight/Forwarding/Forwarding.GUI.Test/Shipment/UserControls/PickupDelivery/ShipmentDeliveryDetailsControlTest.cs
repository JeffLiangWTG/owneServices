using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class ShipmentDeliveryDetailsControlTest : BaseFreightTest
	{
		[ExpectNoExceptions]
		public void TestDeliverToCaption()
		{
			using (ZForm form = new ZForm())
			using (TestShipmentDeliveryDetailsControl control = new TestShipmentDeliveryDetailsControl(Shipment))
			{
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(Shipment, "");
				AssertEquals("Deliver To", control.ConsigneePickupDocAddressControl.CaptionResourceString.Caption);
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestConsigneePickupDocAddressControl_Leave_ShipmentIsNull()
		{
			using (ZForm form = new ZForm())
			using (TestShipmentDeliveryDetailsControl control = new TestShipmentDeliveryDetailsControl(Shipment))
			using (TextBox tb = new TextBox())
			{
				form.Controls.Add(control);
				form.Controls.Add(tb);
				form.Show();
				control.ConsigneePickupDocAddressControl.Focus();
				tb.Focus();
			}
		}

		[ExpectNoExceptions]
		public void TestConsigneePickupDocAddressControl_Enter_ShipmentIsNull()
		{
			using (ZForm form = new ZForm())
			using (TestShipmentDeliveryDetailsControl control = new TestShipmentDeliveryDetailsControl(Shipment))
			using (TextBox tb = new TextBox())
			{
				form.Controls.Add(control);
				form.Controls.Add(tb);
				form.Show();
				tb.Focus();
				control.ConsigneePickupDocAddressControl.Focus();
			}
		}

		public void TestPlugIns()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			using (var control = new ShipmentDeliveryDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();

				var requiredFrom = (ZTabControl)control.Controls.Find("TransportBookingContainerControl", true)[0];
				AssertNotNull(requiredFrom);
				AssertEquals(1, requiredFrom.PlugIns.Instances.Length);
				AssertEquals(ControllerIDs.DtbBookingTabPlugIn, requiredFrom.PlugIns.Instances[0].ControllerID);
			}
		}

		#region ACIZone Label Visibility

		[RequiresSTA]
		public void TestACIZoneLabelVisibility()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			{
				ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
					using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(shipment))
					{
						form.Controls.Add(control);
						form.Show();
						Assert("ACI Zone label not visible", !control.ACIConsigneeDestinationZoneLabel.Visible);
					}

					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
					using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(shipment))
					{
						form.Controls.Add(control);
						form.Show();
						Assert("ACI Zone label visible", control.ACIConsigneeDestinationZoneLabel.Visible);
					}

					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Canada);
					using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(shipment))
					{
						form.Controls.Add(control);
						form.Show();
						Assert("ACI Zone label visible", control.ACIConsigneeDestinationZoneLabel.Visible);
					}
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(storedCompany);
				}
			}
		}

		#endregion

		#region FCL v LCL Label Visibility

		public void TestFCLvLCLLabelVisibility()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();

			using (ZForm form = new ZForm(shipment))
			using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertFCLvLCLLabelVisiblityCorrect(control, shipment);

				shipment.JS_TransportMode = "";
				shipment.JS_PackingMode = "";
				foreach (ZString containerMode in ContainerModes)
				{
					shipment.JS_PackingMode = containerMode;
					foreach (ZString transportMode in TransportModes)
					{
						shipment.JS_TransportMode = transportMode;

						consol.JK_TransportMode = Constants.TransportModes.Sea;
						AssertFCLvLCLLabelVisiblityCorrect(control, shipment);
						consol.JK_TransportMode = Constants.TransportModes.Air;
						AssertFCLvLCLLabelVisiblityCorrect(control, shipment);
					}
				}

				shipment.JS_TransportMode = "";
				shipment.JS_PackingMode = "";
				foreach (ZString transportMode in TransportModes)
				{
					shipment.JS_TransportMode = transportMode;
					foreach (ZString containerMode in ContainerModes)
					{
						shipment.JS_PackingMode = containerMode;
						AssertFCLvLCLLabelVisiblityCorrect(control, shipment);
					}
				}
			}
		}

		void AssertFCLvLCLLabelVisiblityCorrect(ShipmentDeliveryDetailsControl userControl, ForwardingShipment shipment)
		{
			bool hasConsolSeaTransport = false;
			foreach (ForwardingConsol consol in shipment.Consols)
			{
				if (consol.JK_ConsolMode == Constants.TransportModes.Sea)
				{
					hasConsolSeaTransport = true;
					break;
				}
			}

			var showPenaltyGrid = hasConsolSeaTransport && (shipment.JS_PackingMode == Constants.ContainerModes.FCL || shipment.JS_PackingMode == Constants.ContainerModes.BuyersConsol && shipment.JS_ShipmentType == Constants.ShipmentTypes.BuyersConsolLead);

			if (shipment.IsAir)
			{
				AssertLCLLabelVisible(userControl, showPenaltyGrid);
			}
			else if (Constants.ContainerModes.IsLCLType(shipment.JS_PackingMode))
			{
				AssertLCLLabelVisible(userControl, showPenaltyGrid);
			}
			else
			{
				AssertFCLLabelVisible(userControl, showPenaltyGrid);
			}
		}

		void AssertLCLLabelVisible(ShipmentDeliveryDetailsControl userControl, bool showPenaltyGrid)
		{
			Assert("CFS storage date edit visilble", userControl.LCLStorageDateEdit.Visible);
			Assert("CFS available date edit visilble", userControl.LCLAvailableDateEdit.Visible);
			Assert("CFS date override checkbox visible", userControl.JP_LCLDatesOverrideConsolBoundCheckBox.Visible);
			Assert("CFS storage date edit invisilble", !userControl.FCLStorageDateEdit.Visible);
			Assert("CFS available date edit invisilble", !userControl.FCLAvailableDateEdit.Visible);

			Assert("Detention label invisible", !userControl.DetentionLabel.Visible);
			Assert("Free label invisible", !userControl.FreeLabel.Visible);
			Assert("Detention days label invisible", !userControl.DetentionDaysLabel.Visible);
			Assert("Detention Free Days invisible", !userControl.DetentionFreeDaysCalcEdit.Visible);
			Assert("Detention Days invisible", !userControl.DetentionDaysCalcEdit.Visible);
			Assert("Detention Charge invisible", !userControl.DetentionChargeCalcEdit.Visible);

			if (showPenaltyGrid)
			{
				Assert("Delivery Penalties Tab visible", userControl.DeliveryPenaltiesTabPage.TabVisible);
			}
			else
			{
				Assert("Delivery Penalties Tab invisible", !userControl.DeliveryPenaltiesTabPage.TabVisible);
			}
		}

		void AssertFCLLabelVisible(ShipmentDeliveryDetailsControl userControl, bool showPenaltyGrid)
		{
			Assert("CFS storage control invisilble", !userControl.LCLStorageDateEdit.Visible);
			Assert("CFS available control invisilble", !userControl.LCLAvailableDateEdit.Visible);
			Assert("CFS date override checkbox invisible", !userControl.JP_LCLDatesOverrideConsolBoundCheckBox.Visible);
			Assert("CTO storage control visilble", userControl.FCLStorageDateEdit.Visible);
			Assert("CTO available control visilble", userControl.FCLAvailableDateEdit.Visible);
			Assert("Detention label visible", userControl.DetentionLabel.Visible);
			Assert("Free label visible", userControl.FreeLabel.Visible);
			Assert("Detention days label visible", userControl.DetentionDaysLabel.Visible);
			Assert("Detention Free Days visible", userControl.DetentionFreeDaysCalcEdit.Visible);
			Assert("Detention Days visible", userControl.DetentionDaysCalcEdit.Visible);
			Assert("Detention Charge visible", userControl.DetentionChargeCalcEdit.Visible);

			if (showPenaltyGrid)
			{
				Assert("Delivery Penalties Tab visible", userControl.DeliveryPenaltiesTabPage.TabVisible);
			}
			else
			{
				Assert("Delivery Penalties Tab invisible", !userControl.DeliveryPenaltiesTabPage.TabVisible);
			}
		}

		#region Transport and Container mode enumerators

		string[] TransportModes
		{
			get
			{
				if (fTransportModes == null)
				{
					FieldInfo[] transportModeFields = typeof(Constants.TransportModes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
					fTransportModes = new string[transportModeFields.Length];
					long count = 0;
					foreach (FieldInfo transportMode in transportModeFields)
					{
						fTransportModes[count] = (string)transportMode.GetValue(null);
						count++;
					}
				}
				return fTransportModes;
			}
		}

		string[] ContainerModes
		{
			get
			{
				if (fContainerModes == null)
				{
					FieldInfo[] containerModeFields = typeof(Constants.ContainerModes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
					fContainerModes = new string[containerModeFields.Length];
					long count = 0;
					foreach (FieldInfo containerMode in containerModeFields)
					{
						fContainerModes[count] = (string)containerMode.GetValue(null);
						count++;
					}
				}
				return fContainerModes;
			}
		}
		string[] fTransportModes;
		string[] fContainerModes;

		#endregion

		#endregion

		#region Port Trn. Advised Value Visiblity

		public void TestDeliveryLocalTransportAdvisedTimeShow()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			using (TestShipmentDeliveryDetailsControl control = new TestShipmentDeliveryDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Now;
				AssertEquals(shipment.DocsAndCartage.JP_DeliveryCartageAdvised, control.LocalTrnAdvisedDateTime);
			}
		}

		#endregion

		#region Organisation Fetcher

		public void TestOrgFetcherPopulatesFieldsCorrectlyWhenSettingTransportMode()
		{
			OrgHeader fCLCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();

			consignee.SetRelatedParty(fCLCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;

			using (ZForm form = new ZForm(shipment))
			using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				AssertEquals("Sea FCL", ZGuid.Empty, shipment.DocsAndCartage.DeliveryCartageCoPK);

				shipment.ConsigneePK = consignee.PK;
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("Sea FCL", fCLCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
			}
		}

		[RequiresSTA]
		public void TestOrgFetcherPopulatesFieldsCorrectlyWhenSettingPackingMode()
		{
			OrgHeader fCLCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();

			consignee.SetRelatedParty(fCLCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;

			using (ZForm form = new ZForm(shipment))
			using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				AssertEquals("Sea FCL", ZGuid.Empty, shipment.DocsAndCartage.DeliveryCartageCoPK);

				shipment.ConsigneePK = consignee.PK;
				shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
				AssertEquals("Sea FCL", fCLCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
			}
		}

		public void TestOrgFetcherPopulatesFieldsCorrectlyWhenSettingConsignee()
		{
			OrgHeader fCLCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();

			consignee.SetRelatedParty(fCLCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;

			using (ZForm form = new ZForm(shipment))
			using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				AssertEquals("Sea FCL", ZGuid.Empty, shipment.DocsAndCartage.DeliveryCartageCoPK);

				shipment.ConsigneePK = consignee.PK;
				AssertEquals("Sea FCL", fCLCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
			}
		}

		public void TestOrgFetcherPopulatesFieldsCorrectlyWhenSettingConsignor()
		{
			RefUNLOCO originPort = Factory.New<RefUNLOCO>();
			originPort.RL_Code = GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2) + "FAK";

			OrgHeader exBroker = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();

			consignor.SetRelatedParty(exBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = originPort.RL_Code;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			using (ZForm form = new ZForm(shipment))
			using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				AssertEquals("Sea FCL", ZGuid.Empty, shipment[JobShipmentSchema.JS_OH_ExportBroker.Name]);

				shipment.ConsignorPK = consignor.PK;
				AssertEquals("Sea FCL", exBroker.PK, shipment[JobShipmentSchema.JS_OH_ExportBroker.Name]);
			}
		}

		#endregion

		#region TestRenameDemurrageAndStorage

		[RequiresSTA]
		public void TestRenameDemurrageAndStorage()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			using (var control = new ShipmentDeliveryDetailsControl(shipment))
			{
				form.Controls.Add(control);
				form.Show();

				var demurrageOnDeliveryCharge = (ZCalcEdit)control.Controls.Find("zCalcEditDeliveryTruckWaitCharge", true)[0];
				AssertEquals("Truck Wait Time", demurrageOnDeliveryCharge.Extensions.Get<ILabelCaptionRenderer>().Caption);
			}
		}

		#endregion

		#region Test Classes

		class TestShipmentDeliveryDetailsControl : ShipmentDeliveryDetailsControl
		{
			public TestShipmentDeliveryDetailsControl(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			public new ZDocAddressControl ConsigneePickupDocAddressControl
			{
				get { return base.ConsigneePickupDocAddressControl; }
			}

			public ZDateTime LocalTrnAdvisedDateTime
			{
				get { return base.JP_DeliveryCartageAdvisedBoundDateEdit.DateTimeValue; }
			}
		}

		#endregion

		#region Implementation

		ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<ForwardingShipment>()); }
		}
		ForwardingShipment shipment;

		#endregion

		#region TestDeliveryRequiredFromField

		public void TestDeliveryRequiredFromField()
		{
			using (ZForm form = new ZForm())
			using (TestShipmentDeliveryDetailsControl control = new TestShipmentDeliveryDetailsControl(Shipment))
			{
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(Shipment, "");
				var requiredFrom = (ZDateEdit)control.Controls.Find("JP_DeliveryRequiredFromDateEdit", true)[0];
				AssertEquals("Delivery Required From", requiredFrom.Extensions.Get<ILabelCaptionRenderer>().Caption);
			}
		}

		#endregion

		#region Revised Delivery Due Date

		public void TestRevisedDeliveryDueDateVisibility()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertDeliveryDueDateVisibility(true, "JS_RevisedDeliveryDueDateDateEdit");
			}
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertDeliveryDueDateVisibility(false, "JS_RevisedDeliveryDueDateDateEdit");
			}
		}

		#endregion

		#region Delivery Due Date

		public void TestDeliveryDueDateVisibility()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertDeliveryDueDateVisibility(true, "JS_DeliveryDueDateDateEdit");
			}
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertDeliveryDueDateVisibility(false, "JS_DeliveryDueDateDateEdit");
			}
		}

		void AssertDeliveryDueDateVisibility(bool expectedVisibility, ZString zDateEditName)
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			using (ZForm form = new ZForm(Shipment))
			using (ShipmentDeliveryDetailsControl control = new ShipmentDeliveryDetailsControl(Shipment))
			{
				form.Controls.Add(control);
				control.SetDataBinding(Shipment, "");
				form.Show();

				var dateEdit = (ZDateEdit)control.Controls.Find(zDateEditName, true)[0];
				AssertEquals(expectedVisibility, dateEdit.Visible);
			}
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		#endregion
	}
}
