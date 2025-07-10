using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class CargoIMPPhase2PlugInTest : TestCaseWithFactory
	{
		public void Test_Visibility()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (FormForTest form = new FormForTest(shipment))
			{
				form.Show();

				AssertEquals(true, form.CargoIMPPhase2PlugIn.Enabled);
				MenuItem cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2");
				AssertNotNull("cargoIMPMenuItem", cargoIMPMenuItem);
				AssertEquals(true, cargoIMPMenuItem.Visible);

				Factory.Save();

				cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Route Map");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				cargoIMPMenuItem.PerformClick();
				AssertEquals(@"Route Map Message Can't be Sent:

'Forwarder Identification' registry item must be set
", UnitTestUserNotification.Instance.LastMessage.Text);

				cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Cancellation");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				cargoIMPMenuItem.PerformClick();
				AssertEquals(@"Cancellation Message Can't be Sent:

CargoIMP Phase 2 Cancellation message can be sent only after previously sent Route Map or Status Update message.
", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertCargoIMPPhase2MenuVisible(Constants.TransportModes.AirSea, true);
			AssertCargoIMPPhase2MenuVisible(Constants.TransportModes.SeaAir, true);
			AssertCargoIMPPhase2MenuVisible(Constants.TransportModes.Sea, false);
			AssertCargoIMPPhase2MenuVisible(string.Empty, false);

			void AssertCargoIMPPhase2MenuVisible(ZString transportMode, bool expectedEnabled)
			{
				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment1.JS_TransportMode = transportMode;
				using (var form = new FormForTest(shipment1))
				{
					form.Show();
					AssertEquals(expectedEnabled, form.CargoIMPPhase2PlugIn.Enabled);

					var menuItem = FindMenuItem(form, "CargoIMP Phase 2");
					if (expectedEnabled)
					{
						AssertNotNull("cargoIMPMenuItem", menuItem);
						AssertEquals(expectedEnabled, menuItem.Visible);
					}
					else
					{
						AssertNull("cargoIMPMenuItem", menuItem);
					}
				}
			}
		}

		public void TestPluginMenuVisibility_AccessIsNotAllowed_DisplayNotAllowedMenuItem()
		{
			var isAllowed = Env.Security.CargoIMPPhase2.IsAllowed;
			Env.Security.CargoIMPPhase2.IsAllowed = false;

			try
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;

				using (FormForTest form = new FormForTest(shipment))
				{
					form.Show();

					var menuItem = FindMenuItem(form, "CargoIMP Phase 2", "Access denied, click this menu for details.");
					AssertEquals("'Access denied' menu item is visible", true, menuItem != null);

					menuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Route Map");
					AssertEquals("'Send Route Map' menu item is visible", false, menuItem != null);

					menuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Cancellation");
					AssertEquals("'Send Route Map' menu item is visible", false, menuItem != null);
				}
			}
			finally
			{
				Env.Security.CargoIMPPhase2.IsAllowed = isAllowed;
			}
		}

		[RequiresSTA]
		public void TestHasChanges_Return()
		{
			var org1 = Factory.New<OrgHeader>();
			var relatedParty = org1.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org2 = Factory.New<OrgHeader>();
			relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			transport.JW_VoyageFlight = "QF1234";

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			shipment.Consols.Add(consol);

			CargoIMPPhase2MessageManager messageManager = CargoIMPPhase2MessageManager.New(shipment);

			using (FormForTest form = new FormForTest(shipment))
			{
				form.Show();
				MenuItem cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Route Map");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				cargoIMPMenuItem.PerformClick();
				AssertEquals("You must save the form before attempting to send any electronic messages. Do you wish to save the Shipment form now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, messageManager.Messages.Count);
			}
		}

		public void TestSendCancel()
		{
			var serviceManagerQuerier = new Mock<IServiceManagerQuerier>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(serviceManagerQuerier.Object))
			{
				serviceManagerQuerier.SetupSequence(m => m.CheckStateOfNamedServiceTask("CI2"))
					.Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily).CallBase();

				SetRequiredRegistryValues();
				ForwardingShipment shipment = CreateValidTestShipment();
				CargoIMPPhase2MessageManager messageManager = CargoIMPPhase2MessageManager.New(shipment);
				Factory.Save();

				var message = messageManager.Messages.AddNew();
				message.EM_MessageText = "MESSAGE1";
				message.EM_MessageType = "RMI";

				using (FormForTest form = new FormForTest(shipment))
				{
					form.Show();
					MenuItem cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Cancellation");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					cargoIMPMenuItem.PerformClick();
					Assert(ZFormModaliser.LastFormShownDialogForTest is CargoIMPPhase2CreateMessageForm);
					AssertEquals(CargoIMPPhase2MessageTypes.RouteMapCancellation, ((CargoIMPPhase2CreateMessageForm)ZFormModaliser.LastFormShownDialogForTest).fMessageType);
				}
			}
		}

		[RequiresSTA]
		public void TestServiceTaskIsNotRunning()
		{
			var serviceManagerQuerier = new Mock<IServiceManagerQuerier>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(serviceManagerQuerier.Object))
			{
				serviceManagerQuerier
					.Setup(m => m.CheckStateOfNamedServiceTask("CI2"))
					.Returns(ServiceTaskStatus.ServiceTaskIsInactive);
				SetRequiredRegistryValues();
				var shipment = CreateValidTestShipment();
				CargoIMPPhase2MessageManager.New(shipment);
				Factory.Save();

				using (var form = new FormForTest(shipment))
				{
					form.Show();
					var cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Route Map");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					cargoIMPMenuItem.PerformClick();
					AssertEquals(false, ZFormModaliser.LastFormShownDialogForTest is CargoIMPPhase2CreateMessageForm);
					AssertEquals(@"The CargoIMP Phase 2 Service Task (code CI2) is not currently running.

Messages can still be created however sending functionality cannot be performed.

Please type ""YES"" to acknowledge that you understand and have read this explanation.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form = new FormForTest(shipment))
				{
					form.Show();
					var cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Route Map");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					cargoIMPMenuItem.PerformClick();
					Assert(ZFormModaliser.LastFormShownDialogForTest is CargoIMPPhase2CreateMessageForm);
					AssertEquals(@"The CargoIMP Phase 2 Service Task (code CI2) is not currently running.

Messages can still be created however sending functionality cannot be performed.

Please type ""YES"" to acknowledge that you understand and have read this explanation.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestServiceTaskEnvironmentIsInvalid()
		{
			var serviceManagerQuerier = new Mock<IServiceManagerQuerier>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(serviceManagerQuerier.Object))
			{
				serviceManagerQuerier
					.Setup(m => m.CheckStateOfNamedServiceTask("CI2"))
					.Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				var shipment = CreateValidTestShipment();
				CargoIMPPhase2MessageManager.New(shipment);
				Factory.Save();

				using (var form = new FormForTest(shipment))
				{
					form.Show();
					var cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Route Map");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					cargoIMPMenuItem.PerformClick();
					AssertEquals(false, ZFormModaliser.LastFormShownDialogForTest is CargoIMPPhase2CreateMessageForm);
					AssertEquals(@"The CargoIMP Phase 2 Service Task (code CI2) environment is not configured correctly.

Messages can still be created however sending functionality cannot be performed due to the following reasons:
Error: 'Sender Identification (PIMA)' registry item must be set
Error: 'FTP Server Address' registry item must be set
Error: 'FTP User Name' registry item must be set
Error: 'FTP Password' registry item must be set

Please type ""YES"" to acknowledge that you understand and have read this explanation.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form = new FormForTest(shipment))
				{
					form.Show();
					var cargoIMPMenuItem = FindMenuItem(form, "CargoIMP Phase 2", "Send Route Map");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					cargoIMPMenuItem.PerformClick();
					Assert(ZFormModaliser.LastFormShownDialogForTest is CargoIMPPhase2CreateMessageForm);
					AssertEquals(@"The CargoIMP Phase 2 Service Task (code CI2) environment is not configured correctly.

Messages can still be created however sending functionality cannot be performed due to the following reasons:
Error: 'Sender Identification (PIMA)' registry item must be set
Error: 'FTP Server Address' registry item must be set
Error: 'FTP User Name' registry item must be set
Error: 'FTP Password' registry item must be set

Please type ""YES"" to acknowledge that you understand and have read this explanation.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		ForwardingShipment CreateValidTestShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 1300.345M;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			transport.JW_VoyageFlight = "QF1234";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			var relatedParty = org1.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			shipment.Consols.Add(consol);
			return shipment;
		}

		void SetRequiredRegistryValues()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ftp.server.com");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "user");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPPasswordEncrypted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Env.CurrentCompany.PK,
					Guid.Empty, Guid.Empty, "SPA");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Env.CurrentCompany.PK,
					Guid.Empty, Guid.Empty, "YUS");
		}

		static MenuItem FindMenuItem(Form form, params string[] path)
		{
			return FindMenuItem(form.Menu.MenuItems, path, 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		static MenuItem FindMenuItem(MenuItem.MenuItemCollection items, string[] path, int index)
		{
			MenuItem next = null;

			foreach (MenuItem item in items)
			{
				string text = item.Text.Replace("&", "");
				if (text == path[index])
				{
					next = item;
					break;
				}
			}

			return (index + 1 == path.Length || next == null) ? next : FindMenuItem(next.MenuItems, path, index + 1);
		}

		class FormForTest : ZTemplateForm
		{
			public FormForTest(ForwardingShipment businessEntity)
				: base(businessEntity)
			{
				PlugIns.Add(ControllerIDs.CargoIMPPhase2);
			}

			public CargoIMPPhase2PlugIn CargoIMPPhase2PlugIn
			{
				get
				{
					return (CargoIMPPhase2PlugIn)PlugIns.GetPlugIn(ControllerIDs.CargoIMPPhase2);
				}
			}
		}
	}
}
