using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class CarrierMessagingPlugInTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest()
		{
			return new UniversalDataCarrierMessagingPlugIn(Factory.New<ForwardingConsol>());
		}

		public void TestLoadZPlugIn()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";

			using (var plugIn = new UniversalDataCarrierMessagingPlugIn(consol))
			{
				AssertNull(plugIn.UserControl);
				AssertNotNull(plugIn.TopLevelMenu);
				AssertEquals("Send Forward Air Booking Request", plugIn.TopLevelMenu.Text);
			}
		}

		public void TestPluginEnabled()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (var plugIn = new UniversalDataCarrierMessagingPlugIn(consol))
			{
				consol.JK_RL_NKLoadPort = "US111";

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
				{
					consol.JK_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("Consol Case: Plugin is enabled for Road consol when is US port", true, plugIn.Enabled);

					consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("Consol Case: Plugin is disabled for Sea consol", false, plugIn.Enabled);

					consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
					AssertEquals("Consol Case: Plugin is disabled for Rail consol", false, plugIn.Enabled);

					consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("Consol Case: Plugin is disabled for Air consol", false, plugIn.Enabled);
				}

				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("Consol Case: Plugin is disabled when not log in with US or CA account", false, plugIn.Enabled);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
				{
					consol.JK_RL_NKLoadPort = "CA111";
					AssertEquals("Consol Case: Plugin is enabled for Road consol when is CA port", true, plugIn.Enabled);

					consol.JK_RL_NKLoadPort = "CN111";
					AssertEquals("Consol Case: Plugin is disabled for Road consol when is not US or CA port", false, plugIn.Enabled);
				}

				consol.JK_RL_NKLoadPort = "US111";
				AssertEquals("Consol Case: Plugin is disabled when not log in with US or CA account", false, plugIn.Enabled);

				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "CN111";
				AssertEquals("Finished test Consol Case, prepare disabled plugin for testing Transport Case next", false, plugIn.Enabled);

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "US111";

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
				{
					transport.JW_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("Transport Case: Plugin is enabled for Road transport when is US port", true, plugIn.Enabled);

					transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("Transport Case: Plugin is disabled for Sea transport", false, plugIn.Enabled);

					transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
					AssertEquals("Transport Case: Plugin is disabled for Rail transport", false, plugIn.Enabled);

					transport.JW_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("Transport Case: Plugin is disabled for Air transport", false, plugIn.Enabled);
				}

				transport.JW_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("Transport Case: Plugin is disabled when not log in with US or CA account", false, plugIn.Enabled);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
				{
					transport.JW_RL_NKLoadPort = "CA111";
					AssertEquals("Transport Case: Plugin is enabled for Road transport when is CA port", true, plugIn.Enabled);

					transport.JW_RL_NKLoadPort = "CN111";
					AssertEquals("Transport Case: Plugin is disabled for Road transport when is not US or CA port", false, plugIn.Enabled);
				}

				transport.JW_RL_NKLoadPort = "US111";
				AssertEquals("Transport Case: Plugin is disabled when not log in with US or CA account", false, plugIn.Enabled);
			}
		}

		public void TestMessage()
		{
			var haulageOrg = Factory.New<OrgHeader>();
			haulageOrg.OH_Code = "BUMPER";

			var communicationsMode = haulageOrg.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "PANDORA";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_OA_ShippingLineAddress = haulageOrg.MainAddress.PK;
			consol.JK_MasterBillNum = "46719452";
			consol.JK_RL_NKLoadPort = "US111";
			consol.Shipments.AddNew();
			consol.ShippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.EHubOrganisationID, Messaging.Integration.ApplicationCodeList.Codes.ForwardAir);
			consol.Transports.Cast<Transport>().Last().JW_ETA = ZDateTime.Today.AddDays(10);
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.JK_AWBServiceLevel = "PUC";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			using (var plugIn = new UniversalDataCarrierMessagingPlugIn(consol))
			{
				IManualDataExportProgressForm progressForm = null;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					progressForm = (IManualDataExportProgressForm)form;
					progressForm.Form.Shown += (s, e) => progressForm.SendButton.PerformClick();
				});

				plugIn.TopLevelMenu.PerformClick();

				AssertNotNull("Progress form was shown", progressForm);
				AssertMultilineASCIIEquals("notifications",
@"Processing Consol C00001000 (Master Bill='46719452')
Universal Shipment queued for sending to Organization [BUMPER].

Delivery succeeded.
", progressForm.NotificationsTextBox.Text);
			}
		}

		public void TestPluginWhenConsolTransportModeChanges()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (var plugIn = new UniversalDataCarrierMessagingPlugIn(consol))
				{
					AssertEquals("Transport mode is Air.", false, plugIn.Enabled);

					consol.JK_RL_NKLoadPort = "USCHI";
					AssertEquals("Transport mode is Air.", false, plugIn.Enabled);

					consol.JK_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("Transport mode is Road.", true, plugIn.Enabled);
				}
			}
		}

		public void TestPluginDispose_ShoundNotCauseNRE_WhenTransportIsDetached()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "NZAKL";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (var plugIn = new UniversalDataCarrierMessagingPlugIn(consol))
				{
					transport1.JW_RL_NKLoadPort = "USCHI";
					transport1.JW_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("Transport mode is Road.", true, plugIn.Enabled);

					consol.Transports.Remove(transport1);
					plugIn.Dispose();

					AssertNoExceptionThrown(() => transport1.JW_TransportMode = Core.Constants.TransportModes.Air);
				}
			}
		}

		public void TestPluginWhenConsolRoutingChanges()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "NZAKL";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (var plugIn = new UniversalDataCarrierMessagingPlugIn(consol))
				{
					AssertEquals("Transport mode is Air.", false, plugIn.Enabled);

					transport1.JW_RL_NKLoadPort = "USCHI";
					AssertEquals("Transport mode is Air.", false, plugIn.Enabled);

					transport1.JW_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("Transport mode is Road.", true, plugIn.Enabled);

					consol.Transports.RemoveAndDelete(transport1);
					AssertEquals("Transport mode is not Road.", false, plugIn.Enabled);

					var transport3 = consol.Transports.AddNew();
					transport3.JW_TransportMode = Core.Constants.TransportModes.Road;
					transport3.JW_RL_NKLoadPort = "CAARV";

					AssertEquals("Transport mode is Road.", true, plugIn.Enabled);
				}
			}
		}

		public void TestConsolHasChanges()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";

			Factory.Save();

			consol.JK_MasterBillNum = "3434444";

			using (var plugIn = new UniversalDataCarrierMessagingPlugIn(consol))
			{
				IManualDataExportProgressForm progressForm = null;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					progressForm = (IManualDataExportProgressForm)form;
					progressForm.Form.Shown += (s, e) => progressForm.SendButton.PerformClick();
				});

				plugIn.TopLevelMenu.PerformClick();

				AssertEquals("Please save your changes before you continue.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Progress form was not shown", progressForm);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				plugIn.TopLevelMenu.PerformClick();
				AssertNotEquals("Please save your changes before you continue.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Progress form was shown", progressForm);
			}
		}
	}
}
