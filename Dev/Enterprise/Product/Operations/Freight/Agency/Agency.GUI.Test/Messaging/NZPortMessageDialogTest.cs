using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using MessageTypes = Enterprise.Core.Constants.EventReferenceMessageTypes;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	class NZPortMessageDialogTest : BaseAgencyTest
	{
		public void TestSendButtonClick_MessageInfoHasErrors_ShowErrorMessage()
		{
			var voyage = Factory.New<JobVoyage>();
			var message = new NZPortMessage(voyage);
			message.Direction = ZString.Empty;
			using (var dialog = new NZPortMessageDialogForTest(message))
			{
				dialog.Show();
				dialog.PerformClickSend();
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSendButtonClick_ShipmentHasErrors_ShowErrorMessage()
		{
			var voyage = CreateVoyage();
			CreateBillOfLading(voyage, "NZAKL", "NZLYT", OrgHeader1, "billofLading1");
			CreateBillOfLading(voyage, "NZAKL", "NZLYT", OrgHeader1, "billofLading1");

			var portManifestPorts = new PortManifestPortCollection();
			CreatePortManifestPort(portManifestPorts, "NZAKL", OrgHeader1.PK, "NZAKL_OH1");

			var message = new NZPortMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portManifestPorts))
			{
				using (var dialog = new NZPortMessageDialogForTest(message))
				{
					message.Port = "NZAKL";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = "ORG";

					dialog.Show();
					dialog.PerformClickSend();

					AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		public void TestSendButtonClick_NoErrorMessage_SingleBillOfLadings()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "NZAKL", "NZLYT", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "NZLYT", "NZNPE", OrgHeader1, "billofLading2");
			var billOfLading3 = CreateBillOfLading(voyage, "NZAKL", "NZLYT", OrgHeader2, "billofLading3");
			var billOfLading4 = CreateBillOfLading(voyage, "NZAKL", "NZNPE", OrgHeader1, "billofLading4");

			var portManifestPorts = new PortManifestPortCollection();
			CreatePortManifestPort(portManifestPorts, "NZLYT", OrgHeader1.PK, "NZLYT_OH1");
			CreatePortManifestPort(portManifestPorts, "NZLYT", OrgHeader2.PK, "NZLYT_OH2");
			CreatePortManifestPort(portManifestPorts, "NZAKL", OrgHeader1.PK, "NZAKL_OH1");
			CreatePortManifestPort(portManifestPorts, "NZAKL", OrgHeader2.PK, "NZAKL_OH2");
			CreatePortManifestPort(portManifestPorts, "NZNPE", OrgHeader1.PK, "NZNPE_OH1");

			var message = new NZPortMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, CurrentBranchForTest.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portManifestPorts))
			{
				using (var dialog = new NZPortMessageDialogForTest(message))
				{
					message.Port = "NZLYT";
					message.Direction = Constants.PortDirection.Discharge;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = "ORG";

					dialog.Show();
					dialog.PerformClickSend();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, MessageTypes.DischargeManifest, "NZLYT");

					AssertNull(billOfLading2.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertNull(billOfLading3.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertNull(billOfLading4.Logs.MostRecentLogByEventTime(Events.DataExport));
				}
			}
		}

		public void TestSendButtonClick_NoErrorMessage_MultipleBillOfLadings()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "NZAKL", "NZLYT", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "NZAKL", "NZNPE", OrgHeader1, "billofLading2");
			var billOfLading3 = CreateBillOfLading(voyage, "NZAKL", "NZLYT", OrgHeader2, "billofLading3");

			var portManifestPorts = new PortManifestPortCollection();
			CreatePortManifestPort(portManifestPorts, "NZAKL", OrgHeader1.PK, "NZAKL_OH1");
			OrgHeader3.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "OrgHeader3", Constants.CountryCodes.Spain);

			var message = new NZPortMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, CurrentBranchForTest.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portManifestPorts))
			{
				using (var dialog = new NZPortMessageDialogForTest(message))
				{
					message.Port = "NZAKL";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = "ORG";

					dialog.Show();
					dialog.PerformClickSend();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, MessageTypes.LoadManifest, "NZAKL");
					AssertMessageHasBeenSent(billOfLading2, Events.MessageSent, MessageTypes.LoadManifest, "NZAKL");

					AssertNull(billOfLading3.Logs.MostRecentLogByEventTime(Events.DataExport));
				}
			}
		}

		public void TestSendButtonClick_RecipientIdIsEqualToShippingPortsMessagingEHubIDRegistryOfSpanishPort()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, SpainBranchForTest.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = CreateVoyage();

				Carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "ESCARNO", Constants.CountryCodes.Spain);
				var billOfLading1 = CreateBillOfLading(voyage, "NZAKL", "ESVLC", OrgHeader3, "billofLading1");
				OrgHeader3.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "OrgHeader3", Constants.CountryCodes.Spain);

				var portManifestPorts = new PortManifestPortCollection();
				CreatePortManifestPort(portManifestPorts, "ESVLC", OrgHeader3.PK, "ESVLC_SENDER");

				var message = new NZPortMessage(voyage);
				message.MessageType = PortMessageTypeList.Codes.Original;

				var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();
				var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDCollection.AddNew();
				shippingPortsMessagingEHubID.Port = "ESVLC";
				shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.xHub;
				shippingPortsMessagingEHubID.RecipientID = "ESVLC_RECIPIENT";

				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shippingPortsMessagingEHubIDCollection))
				using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portManifestPorts))
				{
					using (var dialog = new NZPortMessageDialogForTest(message))
					{
						message.Port = "ESVLC";
						message.Direction = Constants.PortDirection.Discharge;
						message.PrincipalPK = OrgHeader3.PK;
						message.MessageType = "ORG";

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals("Information Message successfully created and queued for delivery.",
							UnitTestUserNotification.Instance.LastMessage.ToString());
						AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, MessageTypes.DischargeManifest, "ESVLC");

						AssertRecipient(billOfLading1, "ESVLC_RECIPIENT");
					}
				}
			}
		}

		public void TestSendButtonClick_RecipientIdIsEqualToShippingPortsMessagingEHubIDRegistryWithEmptyPort()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, SpainBranchForTest.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = CreateVoyage();
				Carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "ESCARNO", Constants.CountryCodes.Spain);
				var billOfLading1 = CreateBillOfLading(voyage, "NZAKL", "ESVLC", OrgHeader3, "billofLading1");
				OrgHeader3.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "OrgHeader3", Constants.CountryCodes.Spain);

				var portManifestPorts = new PortManifestPortCollection();
				CreatePortManifestPort(portManifestPorts, "ESVLC", OrgHeader3.PK, "ESVLC_SENDER");

				var message = new NZPortMessage(voyage);
				message.MessageType = PortMessageTypeList.Codes.Original;

				var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();

				var shippingPortsMessagingEHubID2 = shippingPortsMessagingEHubIDCollection.AddNew();
				shippingPortsMessagingEHubID2.Port = ZString.Empty;
				shippingPortsMessagingEHubID2.Module = ModuleTypes.Codes.eHub;
				shippingPortsMessagingEHubID2.RecipientID = "DEF_RECIPIENT";

				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shippingPortsMessagingEHubIDCollection))
				using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portManifestPorts))
				{
					using (var dialog = new NZPortMessageDialogForTest(message))
					{
						message.Port = "ESVLC";
						message.Direction = Constants.PortDirection.Discharge;
						message.PrincipalPK = OrgHeader3.PK;
						message.MessageType = "ORG";

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals("Information Message successfully created and queued for delivery.",
							UnitTestUserNotification.Instance.LastMessage.ToString());
						AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, MessageTypes.DischargeManifest, "ESVLC");

						AssertRecipient(billOfLading1, "DEF_RECIPIENT");
					}
				}
			}
		}

		public void TestValidateNIFCustomsRegNoForSpainPort_Load()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, SpainBranchForTest.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = CreateVoyage();
				Carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "ESCARNO", Constants.CountryCodes.Spain);
				var billOfLading1 = CreateBillOfLading(voyage, "ESBCN", "ESVLC", OrgHeader3, "billofLading1");

				var portManifestPorts = new PortManifestPortCollection();
				CreatePortManifestPort(portManifestPorts, "ESBCN", OrgHeader3.PK, "ESBCN_SENDER");

				var message = new NZPortMessage(voyage);
				message.MessageType = PortMessageTypeList.Codes.Original;

				var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();

				var shippingPortsMessagingEHubID2 = shippingPortsMessagingEHubIDCollection.AddNew();
				shippingPortsMessagingEHubID2.Port = ZString.Empty;
				shippingPortsMessagingEHubID2.Module = ModuleTypes.Codes.eHub;
				shippingPortsMessagingEHubID2.RecipientID = "DEF_RECIPIENT";

				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shippingPortsMessagingEHubIDCollection))
				using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portManifestPorts))
				{
					using (var dialog = new NZPortMessageDialogForTest(message))
					{
						message.Port = "ESBCN";
						message.Direction = Constants.PortDirection.Load;
						message.PrincipalPK = OrgHeader3.PK;
						message.MessageType = "ORG";

						OrgHeader3.CustomsCodes.RemoveAll();
						OrgHeader3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "ESCARNO", Constants.CountryCodes.Spain);

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
						Assert(message.Issues.Cast<PortMessageIssue>().Any(x => x.Text == "V00001000's Principal does not have a NIF code for Spain."));

						OrgHeader3.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "OrgHeader3", Constants.CountryCodes.Spain);

						billOfLading1.ConsignorPK = Consingor.PK;
						Consingor.CustomsCodes.RemoveAll();
						message.Issues.RemoveAll();

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
						Assert(message.Issues.Cast<PortMessageIssue>().Any(x => x.Text == "V00001000's Consignor does not have a NIF code for Spain."));

						Consingor.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "Consingor", Constants.CountryCodes.Spain);
						var departureCTO = voyage.Origins.Cast<VoyageOrigin>().First(x => x.JA_RL_NKPortOfLoading == "ESBCN").DepartureCTOAddress.Header;
						departureCTO.CustomsCodes.RemoveAll();
						message.Issues.RemoveAll();

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
						Assert(message.Issues.Cast<PortMessageIssue>().Any(x => x.Text == "The CTO does not have a NIF code for Spain."));

						departureCTO.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "departureCTO", Constants.CountryCodes.Spain);
						message.Issues.RemoveAll();

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
						AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, MessageTypes.LoadManifest, "ESBCN");
						AssertRecipient(billOfLading1, "DEF_RECIPIENT");
					}
				}
			}
		}

		public void TestValidateNIFCustomsRegNoForSpainPort_Discharge()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, SpainBranchForTest.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = CreateVoyage();
				Carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "ESCARNO", Constants.CountryCodes.Spain);
				var billOfLading1 = CreateBillOfLading(voyage, "NZAKL", "ESVLC", OrgHeader3, "billofLading1");

				var portManifestPorts = new PortManifestPortCollection();
				CreatePortManifestPort(portManifestPorts, "ESVLC", OrgHeader3.PK, "ESVLC_SENDER");

				var message = new NZPortMessage(voyage);
				message.MessageType = PortMessageTypeList.Codes.Original;

				var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();

				var shippingPortsMessagingEHubID2 = shippingPortsMessagingEHubIDCollection.AddNew();
				shippingPortsMessagingEHubID2.Port = ZString.Empty;
				shippingPortsMessagingEHubID2.Module = ModuleTypes.Codes.eHub;
				shippingPortsMessagingEHubID2.RecipientID = "DEF_RECIPIENT";

				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shippingPortsMessagingEHubIDCollection))
				using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portManifestPorts))
				{
					using (var dialog = new NZPortMessageDialogForTest(message))
					{
						message.Port = "ESVLC";
						message.Direction = Constants.PortDirection.Discharge;
						message.PrincipalPK = OrgHeader3.PK;
						message.MessageType = "ORG";

						OrgHeader3.CustomsCodes.RemoveAll();
						OrgHeader3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "ESCARNO", Constants.CountryCodes.Spain);

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
						Assert(message.Issues.Cast<PortMessageIssue>().Any(x => x.Text == "V00001000's Principal does not have a NIF code for Spain."));

						OrgHeader3.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "OrgHeader3", Constants.CountryCodes.Spain);

						billOfLading1.ConsigneePK = Consingee.PK;
						Consingee.CustomsCodes.RemoveAll();
						message.Issues.RemoveAll();

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
						Assert(message.Issues.Cast<PortMessageIssue>().Any(x => x.Text == "V00001000's Consignee does not have a NIF code for Spain."));

						Consingee.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "Consingee", Constants.CountryCodes.Spain);
						var arrivalCTO = voyage.Destinations.Cast<VoyageDestination>().First(x => x.JB_RL_NKPortOfDischarge == "ESVLC").ArrivalCTOAddress.Header;
						arrivalCTO.CustomsCodes.RemoveAll();
						message.Issues.RemoveAll();

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
						Assert(message.Issues.Cast<PortMessageIssue>().Any(x => x.Text == "The CTO does not have a NIF code for Spain."));

						arrivalCTO.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "departureCTO", Constants.CountryCodes.Spain);
						message.Issues.RemoveAll();

						dialog.Show();
						dialog.PerformClickSend();

						AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
						AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, MessageTypes.DischargeManifest, "ESVLC");
						AssertRecipient(billOfLading1, "DEF_RECIPIENT");
					}
				}
			}
		}

		void AssertMessageHasBeenSent(BillOfLading shipment, Event evnt, string messageType, string port)
		{
			var expectedReference = StmALog.GenerateEventReference("", new[] { Params.MessageType.AsKeyFor(messageType), Params.Department.AsKeyFor("Terminal"), Params.Location.AsKeyFor(port) });
			var generatedLogEvent = shipment.Sailing.Voyage.Logs.MostRecentLogByEventTime(evnt, expectedReference);
			var generatedDexEvent = shipment.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertNotNull(string.Format("{0} event", evnt.Code), generatedLogEvent);
			AssertNotNull(string.Format("{0} event", Events.DataExport.Code), generatedDexEvent);
			var universalXml = new XPathDocument(new StringReader(generatedDexEvent.RelatedEDIMessage.Message.EM_MessageText)).CreateNavigator();
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("u", "http://www.cargowise.com/Schemas/Universal/2012/11");
			AssertEquals("EventType in the generated XML", evnt.Code, universalXml.SelectSingleNode("/u:UniversalShipment/u:Shipment/u:DataContext/u:Workflow/u:EventType", namespaceManager).Value);
			AssertEquals("EventReference in the generated XML", expectedReference, universalXml.SelectSingleNode("/u:UniversalShipment/u:Shipment/u:DataContext/u:Workflow/u:TriggerReference", namespaceManager).Value);
		}

		void AssertRecipient(BillOfLading shipment, string recipient)
		{
			var generatedDexEvent = shipment.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertEquals(recipient, generatedDexEvent.RelatedEDIMessage.Recipient);
		}

		GlbBranch CurrentBranchForTest
		{
			get
			{
				if (currentBranchForTest == null)
				{
					var orgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.NewZealand);
					currentBranchForTest = Factory.NewWithValidTestData<GlbBranch>();
					currentBranchForTest.GB_OH_OrgProxy = orgProxy.PK;
					currentBranchForTest.GB_RL_NKHomePort = "NZAKL";
					currentBranchForTest.Company.GC_RN_NKCountryCode = "NZ";
					Factory.Save();
				}

				return currentBranchForTest;
			}
		}

		GlbBranch currentBranchForTest;

		GlbBranch SpainBranchForTest
		{
			get
			{
				if (spainBranchForTest == null)
				{
					var orgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Spain);
					spainBranchForTest = Factory.NewWithValidTestData<GlbBranch>();
					spainBranchForTest.GB_OH_OrgProxy = orgProxy.PK;
					spainBranchForTest.GB_RL_NKHomePort = "ESMAD";
					spainBranchForTest.Company.GC_RN_NKCountryCode = "ES";
					Factory.Save();
				}

				return spainBranchForTest;
			}
		}

		GlbBranch spainBranchForTest;

		#region Types

		class NZPortMessageDialogForTest : NZPortMessageDialog
		{
			public NZPortMessageDialogForTest(NZPortMessage portMessage) : base(portMessage)
			{
			}

			public void PerformClickSend()
			{
				sendButton.PerformClick();
			}
		}
		#endregion

		#region Implementation

		JobVoyage CreateVoyage()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();

			voyage.JV_RV_NKVessel = vessel.RV_Code;
			voyage.JV_VoyageFlight = "ABCWTG";
			vessel.RV_LloydsNumber = "WTG";

			var origin1 = voyage.Origins.AddNew();
			origin1.FillWithValidTestData();
			origin1.JA_RL_NKPortOfLoading = "NZAKL";
			origin1.JA_E_DEP = new DateTime(2022, 11, 29);
			origin1.JA_OA_DepartureCTOAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var origin2 = voyage.Origins.AddNew();
			origin2.FillWithValidTestData();
			origin2.JA_RL_NKPortOfLoading = "NZLYT";
			origin2.JA_E_DEP = new DateTime(2022, 12, 01);
			origin2.JA_OA_DepartureCTOAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var origin3 = voyage.Origins.AddNew();
			origin3.FillWithValidTestData();
			origin3.JA_RL_NKPortOfLoading = "ESBCN";
			origin3.JA_E_DEP = new DateTime(2022, 12, 01);
			var origin3DepartureCTOAddress = Factory.NewWithValidTestData<OrgHeader>();
			origin3DepartureCTOAddress.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "N3FO1D", Constants.CountryCodes.Spain);
			origin3.JA_OA_DepartureCTOAddress = origin3DepartureCTOAddress.MainAddress.PK;

			var destinations1 = voyage.Destinations.AddNew();
			destinations1.FillWithValidTestData();
			destinations1.JB_RL_NKPortOfDischarge = "NZLYT";
			destinations1.JB_E_ARV = new DateTime(2022, 11, 29);
			destinations1.JB_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var destinations2 = voyage.Destinations.AddNew();
			destinations2.FillWithValidTestData();
			destinations2.JB_RL_NKPortOfDischarge = "NZNPE";
			destinations2.JB_E_ARV = new DateTime(2022, 12, 01);
			destinations2.JB_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var destinations3 = voyage.Destinations.AddNew();
			destinations3.FillWithValidTestData();
			destinations3.JB_RL_NKPortOfDischarge = "ESVLC";
			destinations3.JB_E_ARV = new DateTime(2022, 12, 02);
			var destinations3ArrivalCTOAddress = Factory.NewWithValidTestData<OrgHeader>();
			destinations3ArrivalCTOAddress.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIFD3A", Constants.CountryCodes.Spain);
			destinations3.JB_OA_ArrivalCTOAddress = destinations3ArrivalCTOAddress.MainAddress.PK;

			voyage.GenerateSailings();

			foreach (JobSailing sailing in voyage.Sailings)
			{
				sailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			}

			Factory.Save();

			return voyage;
		}

		BillOfLading CreateBillOfLading(JobVoyage voyage, ZString loadingPort, ZString dischargePort, OrgHeader orgHeader, ZString houseBill, bool createUndg = true)
		{
			var sailingPK = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == loadingPort && x.JX_JB_RL_NKPortOfDischarge == dischargePort).PK;

			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_PackingMode = Constants.ContainerModes.Bulk;
			billOfLading.JS_OH_DeliveryAgent = orgHeader.PK;
			billOfLading.JS_JX = sailingPK;
			billOfLading.JS_GoodsDescription = "WTG";
			billOfLading.JS_HouseBill = houseBill;

			billOfLading.ConsigneePK = Consingee.PK;
			billOfLading.ConsignorPK = Consingor.PK;

			billOfLading.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			billOfLading.ConsigneeDocumentaryAddress.E2_CompanyName = "CONSIGNEE NAME";
			billOfLading.ConsigneeDocumentaryAddress.E2_Address1 = "CONSIGNEE ADDRESS 1";
			billOfLading.ConsigneeDocumentaryAddress.E2_Address2 = "CONSIGNEE ADDRESS 2";
			billOfLading.ConsigneeDocumentaryAddress.E2_Postcode = "65432";
			billOfLading.ConsigneeDocumentaryAddress.E2_City = "LOS ANGELES";
			billOfLading.ConsigneeDocumentaryAddress.E2_State = "CA";
			billOfLading.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "US";
			billOfLading.ConsigneeDocumentaryAddress.E2_Contact = "BROWN SMITH";
			billOfLading.ConsigneeDocumentaryAddress.E2_Phone = "+61 2 6958 6543";
			billOfLading.ConsigneeDocumentaryAddress.E2_Fax = "+61 2 6958 6544";
			billOfLading.ConsigneeDocumentaryAddress.E2_Email = "consingee@where.com";

			billOfLading.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			billOfLading.ConsignorDocumentaryAddress.E2_CompanyName = "CONSIGNOR NAME";
			billOfLading.ConsignorDocumentaryAddress.E2_Address1 = "CONSIGNOR ADDRESS 1";
			billOfLading.ConsignorDocumentaryAddress.E2_Address2 = "CONSIGNOR ADDRESS 2";
			billOfLading.ConsignorDocumentaryAddress.E2_Postcode = "2215";
			billOfLading.ConsignorDocumentaryAddress.E2_City = "Aukland";
			billOfLading.ConsignorDocumentaryAddress.E2_State = "AUK";
			billOfLading.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "NZ";
			billOfLading.ConsignorDocumentaryAddress.E2_Contact = "BOB SMITH";
			billOfLading.ConsignorDocumentaryAddress.E2_Phone = "+61 2 5684 6543";
			billOfLading.ConsignorDocumentaryAddress.E2_Fax = "+61 2 5684 6544";
			billOfLading.ConsignorDocumentaryAddress.E2_Email = "consingor@where.com";

			var container = billOfLading.RealContainers[0];
			container.JC_ContainerNum = "ABC";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_GrossWeight = 100.0;

			billOfLading.OuterPackLines.RemoveAndDeleteAll();
			var packLine = billOfLading.OuterPackLines.AddNew();

			if (createUndg)
			{
				var substance = Factory.NewWithValidTestData<UNDGSubstance>();

				var undg = packLine.UNDGs.AddNew();
				undg.DI_DG = substance.PK;
				undg.DI_DGFlashPoint = 2;
				undg.DI_PackageCount = 1;
				undg.DI_TechnicalName = "WTG";
				undg.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				undg.DI_DGWeight = 10;
				undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			}

			packLine.JL_JC = container.PK;
			packLine.JL_PackageCount = 1;
			packLine.JL_DetailedDescription = "WTG";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_ActualWeight = 23630m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 23m;

			foreach (Transport transport in billOfLading.Transports)
			{
				transport.CarrierPK = Carrier.PK;
			}

			billOfLading.JS_JX = sailingPK;
			billOfLading.JS_OA_BookedShippingLineAddress = Carrier.MainAddress.PK;

			foreach (VoyageDestination destination in voyage.Destinations)
			{
				destination.JB_E_ARV = new DateTime(2022, 12, 01);
			}

			Factory.Save();

			return billOfLading;
		}

		protected override void SetUp()
		{
			base.SetUp();

			OrgHeader1 = CreateOrganisation("OrgHeader1", "USLAX", "OrgHeader1 ADDRESS 1", "LOS ANGELES", "+1 801 120 234");
			OrgHeader1.OH_Code = "OH1";
			OrgHeader1.OH_IsShippingProvider = true;
			OrgHeader1.OH_IsShippingLine = true;
			OrgHeader1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			OrgHeader2 = CreateOrganisation("OrgHeader2", "USLAX", "OrgHeader2 ADDRESS 1", "LOS ANGELES", "+1 801 120 234");
			OrgHeader2.OH_Code = "OH2";
			OrgHeader2.OH_IsShippingProvider = true;
			OrgHeader2.OH_IsShippingLine = true;
			OrgHeader2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			OrgHeader3 = CreateOrganisation("OrgHeader3", "USLAX", "OrgHeader3 ADDRESS 1", "LOS ANGELES", "+1 801 120 234", Constants.CountryCodes.Spain);
			OrgHeader3.OH_Code = "OH3";
			OrgHeader3.OH_IsShippingProvider = true;
			OrgHeader3.OH_IsShippingLine = true;
			OrgHeader3.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Carrier = CreateOrganisation("CARRIER", "USLAX", "CARRIER ADDRESS 1", "LOS ANGELES", "+1 801 120 456");
			Carrier.OH_IsShippingLine = true;
			Carrier.OH_IsShippingProvider = true;
			Carrier.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Consingee = CreateOrganisation("CONSINGEE", "USLAX", "CONSINGEE ADDRESS 1", "LOS ANGELES", "+1 801 120 456");
			Consingee.OH_IsConsignee = true;
			Consingee.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "Consingee", Constants.CountryCodes.Spain);

			Consingor = CreateOrganisation("CONSIGNOR", "NZAKL", "CONSIGNOR ADDRESS 1", "SYDNEY", "+61 2 9156 4568");
			Consingor.OH_IsConsignor = true;
			Consingor.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "Consingor", Constants.CountryCodes.Spain);

			Factory.Save();

			Env.Security.AgencyPrincipalAccess.IsAllowed = true;
		}
		OrgHeader OrgHeader1;
		OrgHeader OrgHeader2;
		OrgHeader OrgHeader3;

		OrgHeader Consingee;
		OrgHeader Consingor;
		OrgHeader Carrier;

		PortManifestPort CreatePortManifestPort(PortManifestPortCollection collection, ZString portCode, ZGuid principalPK, ZString senderID)
		{
			var port = collection.AddNew();

			port.Port = portCode;
			port.PrincipalPK = principalPK;
			port.SenderID = senderID;
			port.Enabled = true;

			return port;
		}

		public OrgHeader CreateOrganisation(ZString fullName, ZString closestPort, ZString address1, ZString city, ZString phone, string countryCode = Constants.CountryCodes.NewZealand)
		{
			OrgHeader orgHeader = base.Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", countryCode);
			orgHeader.OH_FullName = fullName;
			orgHeader.OH_RL_NKClosestPort = closestPort;
			orgHeader.MainAddress.OA_Address1 = address1;
			orgHeader.MainAddress.OA_City = city;
			orgHeader.MainAddress.OA_Phone = phone;
			return orgHeader;
		}

		#endregion
	}
}
