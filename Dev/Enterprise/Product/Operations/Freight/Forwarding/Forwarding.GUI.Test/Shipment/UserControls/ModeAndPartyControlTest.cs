using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using EventArgs = System.EventArgs;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ModeAndPartyControlTest : BaseFreightTest
	{
		#region ACI Zone Visibility

		public void TestACIZoneLabelsVisibility()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			{
				ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
					using (ModeAndPartyControl control = new ModeAndPartyControl())
					{
						form.Controls.Add(control);
						form.Show();
						AssertEquals("ACI Zone labels not visible", false, control.ACIConsignorOriginZoneLabel.Visible);
						AssertEquals("ACI Zone labels not visible", false, control.ACIConsigneeDestinationZoneLabel.Visible);
					}

					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
					using (ModeAndPartyControl control = new ModeAndPartyControl())
					{
						form.Controls.Add(control);
						form.Show();
						AssertEquals("ACI Zone labels visible", true, control.ACIConsignorOriginZoneLabel.Visible);
						AssertEquals("ACI Zone labels visible", true, control.ACIConsigneeDestinationZoneLabel.Visible);
					}

					GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Canada);
					using (ModeAndPartyControl control = new ModeAndPartyControl())
					{
						form.Controls.Add(control);
						form.Show();
						AssertEquals("ACI Zone labels visible", true, control.ACIConsignorOriginZoneLabel.Visible);
						AssertEquals("ACI Zone labels visible", true, control.ACIConsigneeDestinationZoneLabel.Visible);

						shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
						AssertEquals("Hidden for overide", false, control.ACIConsignorOriginZoneLabel.Visible);
						AssertEquals("ACI Zone labels visible", true, control.ACIConsigneeDestinationZoneLabel.Visible);

						shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
						AssertEquals("Hidden for overide", false, control.ACIConsignorOriginZoneLabel.Visible);
						AssertEquals("Hidden for override", false, control.ACIConsigneeDestinationZoneLabel.Visible);

						shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
						shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
						AssertEquals("Shown for non-overide", true, control.ACIConsignorOriginZoneLabel.Visible);
						AssertEquals("Shown for non-override", true, control.ACIConsigneeDestinationZoneLabel.Visible);
					}
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(storedCompany);
				}
			}
		}

		#endregion

		#region Buyer / Supplier Selection

		public void TestBuyerSelectionPopup()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader buyer2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierBuyerLink link1 = supplier.BuyerLinks.AddNew();
			link1.OL_OH_Buyer = buyer1.PK;

			OrgSupplierBuyerLink link2 = supplier.BuyerLinks.AddNew();
			link2.OL_OH_Buyer = buyer2.PK;

			using (ZForm form = new ZForm(shipment))
			using (ModeAndPartyControl control = new ModeAndPartyControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

				control.ConsigneeDocumentaryDocAddressControl.PopupShown = false;
				ConsigneeEnter(control);
				AssertEquals("Popup should be shown if there are more than one buyers", true, control.ConsigneeDocumentaryDocAddressControl.PopupShown);

				link2.Delete();
				control.ConsigneeDocumentaryDocAddressControl.PopupShown = false;
				ConsigneeEnter(control);
				AssertEquals("Popup should not be shown if there is only one buyer", false, control.ConsigneeDocumentaryDocAddressControl.PopupShown);
			}
		}

		void ConsigneeEnter(ModeAndPartyControl control)
		{
			typeof(ModeAndPartyControl).GetMethod(
				"ConsigneeDocumentaryDocAddressControl_Enter",
				BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, new object[] { null, null });
		}

		public void TestSupplierSelectionPopup()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierBuyerLink link1 = buyer.SupplierLinks.AddNew();
			link1.OL_OH_Supplier = supplier1.PK;

			OrgSupplierBuyerLink link2 = buyer.SupplierLinks.AddNew();
			link2.OL_OH_Supplier = supplier2.PK;

			using (ZForm form = new ZForm(shipment))
			using (ModeAndPartyControl control = new ModeAndPartyControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;

				control.ConsignorDocumentaryDocAddressControl.PopupShown = false;
				ConsignorEnter(control);
				AssertEquals("Popup should be shown if there are more than one buyers", true, control.ConsignorDocumentaryDocAddressControl.PopupShown);

				link2.Delete();
				control.ConsignorDocumentaryDocAddressControl.PopupShown = false;
				ConsignorEnter(control);
				AssertEquals("Popup should not be shown if there is only one buyer", false, control.ConsignorDocumentaryDocAddressControl.PopupShown);
			}
		}

		void ConsignorEnter(ModeAndPartyControl control)
		{
			typeof(ModeAndPartyControl).GetMethod(
				"ConsignorDocumentaryDocAddressControl_Enter",
				BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, new object[] { null, null });
		}

		#endregion

		#region Consignor / Consignee Order

		[RequiresSTA]
		public void TestConsignorConsigneeOrder()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AQMCM";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ForwardingShipment importShipment = consol.Shipments.AddNew();

			Env.Registry.SetShipmentScreenLayout(Constants.ShipmentScreenOptions.Auto);
			using (ZForm form = new ZForm(importShipment))
			using (ModeAndPartyControl control = new ModeAndPartyControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(importShipment, "");

				Assert("Import Shipment - Consignee should be before Consignor.", control.ConsigneeDocumentaryDocAddressControl.TabIndex < control.ConsignorDocumentaryDocAddressControl.TabIndex);
			}

			Env.Registry.SetShipmentScreenLayout(Constants.ShipmentScreenOptions.Consignor);
			using (ZForm form = new ZForm(importShipment))
			using (ModeAndPartyControl control = new ModeAndPartyControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(importShipment, "");
				Assert("Registry set to Consignor, Consol is Import. Consignor should be before Consignee", control.ConsignorDocumentaryDocAddressControl.TabIndex < control.ConsigneeDocumentaryDocAddressControl.TabIndex);
			}

			Env.Registry.SetShipmentScreenLayout(Constants.ShipmentScreenOptions.Consignee);
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			consol.JK_RL_NKDischargePort = "AQMCM";
			consol.Shipments.RemoveAndDeleteAll();
			ForwardingShipment exportShipment = consol.Shipments.AddNew();
			using (ZForm form = new ZForm(exportShipment))
			using (ModeAndPartyControl control = new ModeAndPartyControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(exportShipment, "");
				Assert("Registry set to Consignee. Consignee should be before Consignor", control.ConsigneeDocumentaryDocAddressControl.TabIndex < control.ConsignorDocumentaryDocAddressControl.TabIndex);
			}
		}

		#endregion

		#region Documentary Address

		[RequiresSTA]
		public void TestConsignorDocumentaryDocAddressControlText()
		{
			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);
			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.StandardHouse, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestTestTest");
			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.StandardHouse, "TestTestTest");

			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.CoLoadMaster, "Sending Forwarder");
			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.StandardHouse, "TestTestTest");

			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.BlindCoLoadMaster, "Sending Forwarder");
			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.HighVolumeLowValue, "eTailer");
			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.HighVolumeLowValueLegacy, "eTailer");
			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.ThirdPartyOwnershipHouse, "Trader/Supplier");
			AssertControlCaption(ConsignorControl, Constants.ShipmentTypes.StandardHouse, "TestTestTest");
		}

		[RequiresSTA]
		public void TestConsigneeDocumentaryDocAddressControlText()
		{
			AssertControlCaption(ConsigneeControl, Constants.ShipmentTypes.StandardHouse, "Consignee");
			AssertControlCaption(ConsigneeControl, Constants.ShipmentTypes.CoLoadMaster, "Receiving Forwarder");
			AssertControlCaption(ConsigneeControl, Constants.ShipmentTypes.HighVolumeLowValue, "Consignee");
			AssertControlCaption(ConsigneeControl, Constants.ShipmentTypes.HighVolumeLowValueLegacy, "Consignee");
			AssertControlCaption(ConsigneeControl, Constants.ShipmentTypes.BlindCoLoadMaster, "Receiving Forwarder");
		}

		void AssertControlCaption(string labelType, ZString shipmentType, string expectedCaption)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (ZForm form = new ZForm(shipment))
			using (ModeAndPartyControl control = new ModeAndPartyControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(shipment, "");

				shipment.JS_ShipmentType = shipmentType;

				if (labelType == ConsignorControl)
				{
					AssertEquals("Consignor caption should be " + expectedCaption, expectedCaption, control.ConsignorDocumentaryDocAddressControl.Text);
				}
				else if (labelType == ConsigneeControl)
				{
					AssertEquals("Consignee caption should be " + expectedCaption, expectedCaption, control.ConsigneeDocumentaryDocAddressControl.Text);
				}
			}
		}

		const string ConsignorControl = "consignor";
		const string ConsigneeControl = "consignee";

		#endregion

		#region Mode Change

		public void TestChangeCommissionedShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";

			Factory.Save();

			using (ZForm form = new ZForm(shipment))
			{
				using (var control = new ModeAndPartyControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();
					AssertEquals(false, control.ConfirmCalled);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_TransportMode = "AIR";
					AssertEquals(true, control.ConfirmCalled);
				}
			}
		}

		[RequiresSTA]
		public void TestUnbindJS_TransportModeChanged()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";

			Factory.Save();

			using (var form = new ZForm(shipment))
			using (var control = new ModeAndPartyControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				AssertEquals(false, control.ConfirmCalled);

				control.InvokeOnCurrentDataItemChanging(); // Unbing from change event of current data source

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				shipment.JS_TransportMode = "AIR";
				AssertEquals("JS_TransportModeInfo_ValueChanged event should not trigger", false, control.ConfirmCalled);
			}
		}

		class ModeAndPartyControlForTest : ModeAndPartyControl
		{
			public bool ConfirmCalled { get; set; }
			protected override void ConfirmReversal()
			{
				ConfirmCalled = true;
				base.ConfirmReversal();
			}

			public void InvokeOnCurrentDataItemChanging()
			{
				OnCurrentDataItemChanging(EventArgs.Empty);
			}
		}

		#endregion

		#region Label

		[RequiresSTA]
		public void TestCaptionWithCrossTradeAndRegistry()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "ES22E";
			shipment.JS_RL_NKDestination = "FRLYO";
			Factory.Save();

			using (var form = new ShipmentFormWithBasicRegistration(shipment))
			{
				form.Show();

				var modeAndPartyControl = form.ShipmentBasicRegistrationControl.GetModeAndPartyControl();
				modeAndPartyControl.LocalClientOrgControl.Text = "Local Client";
				AssertEquals("Shipment is cross trade but without registry, caption is Local Client", "Local Client", modeAndPartyControl.LocalClientOrgControl.Text);
			}

			using (var form = new ShipmentFormWithBasicRegistration(shipment))
			using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				form.Show();

				var modeAndPartyControl = form.ShipmentBasicRegistrationControl.GetModeAndPartyControl();
				AssertEquals("With registry, when open saved cross trade shipment, caption is Prepaid Bill-To Party", "Prepaid Bill-To Party", modeAndPartyControl.LocalClientOrgControl.Text);
				AssertEquals("With registry, when open saved cross trade shipment, help bubble is Prepaid Bill-To Party", shipment.PrepaidBillToPartyCaption, modeAndPartyControl.LocalClientOrgControl.CaptionResourceString);

				shipment.JS_RL_NKOrigin = "AUSYD";
				AssertEquals("With registry, when change origin to home country, caption is Local Client", "Local Client", modeAndPartyControl.LocalClientOrgControl.Text);
				AssertEquals("With registry, when change origin to home country, help bubble is Local Client", shipment.LocalClientCaption, modeAndPartyControl.LocalClientOrgControl.CaptionResourceString);

				shipment.JS_RL_NKOrigin = "FRLYO";
				shipment.JS_RL_NKDestination = "ES22E";
				AssertEquals("With registry, When change to cross trade shipment, caption is Prepaid Bill-To Party", "Prepaid Bill-To Party", modeAndPartyControl.LocalClientOrgControl.Text);
				AssertEquals("With registry, When change to cross trade shipment, help bubble is Prepaid Bill-To Party", shipment.PrepaidBillToPartyCaption, modeAndPartyControl.LocalClientOrgControl.CaptionResourceString);
			}
		}

		#endregion
	}
}
