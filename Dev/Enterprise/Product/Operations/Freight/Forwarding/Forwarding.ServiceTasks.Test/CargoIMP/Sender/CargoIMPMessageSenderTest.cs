using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMP.Test
{
	public class CargoIMPMessageSenderTest : TestCaseWithFactory
	{
		public void TestNullLogger()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => CargoIMPMessageSender.New(null));
		}

		public void TestInterchangeHeaderFooterSetForFWBSentFromDirectFWBWebModule()
		{
			var mawbHeader = Factory.New<ExportAWBHeader>();
			mawbHeader.EH_WayBillNumber = "081-11111111";
			mawbHeader.EH_AWBOriginCode = "SYD";
			var interchange = CreateInterchange();
			interchange.EI_HeaderText = "";
			interchange.EI_FooterText = "";
			var message = CreateMessage(interchange);
			message.EM_LinkTable = mawbHeader.TableName;
			message.EM_LinkUniqueID = mawbHeader.PK;
			Factory.Save();
			AssertEquals("Precondition: interchange.EI_HeaderText", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("Precondition: interchange.EI_FooterText", ZString.Empty, interchange.EI_FooterText);
			CargoIMPMessageSender.New(new TestServiceLogger()).Process();
			interchange.Reload();
			AssertEquals("interchange.EI_HeaderText", "FWB\r\nQFSYD\r\n", interchange.EI_HeaderText);
			AssertEquals("interchange.EI_FooterText", "\x04", interchange.EI_FooterText);
			AssertEquals("interchange.HasChanges", false, interchange.HasChanges);
		}

		public void TestInterchangeHeaderFooterSetForFHLSentFromDirectFWBWebModule()
		{
			var mawbHeader = Factory.New<ExportAWBHeader>();
			mawbHeader.EH_By1st = "DL";
			mawbHeader.EH_AWBOriginCode = "AKL";
			var hawbHeader = mawbHeader.ChildBills.AddNew();
			hawbHeader.EH_By1st = "XX"; // Should Get Ignored.
			hawbHeader.EH_AWBOriginCode = "WUU"; // Should Get Ignored.
			var interchange = CreateInterchange();
			interchange.EI_HeaderText = "";
			interchange.EI_FooterText = "";
			var message = CreateMessage(interchange);
			message.EM_MessageType = "FHL";
			message.EM_LinkTable = hawbHeader.TableName;
			message.EM_LinkUniqueID = hawbHeader.PK;
			Factory.Save();
			AssertEquals("Precondition: interchange.EI_HeaderText", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("Precondition: interchange.EI_FooterText", ZString.Empty, interchange.EI_FooterText);
			CargoIMPMessageSender.New(new TestServiceLogger()).Process();
			interchange.Reload();
			AssertEquals("interchange.EI_HeaderText", "FHL\r\nDLAKL\r\n", interchange.EI_HeaderText);
			AssertEquals("interchange.EI_FooterText", "\x04", interchange.EI_FooterText);
			AssertEquals("interchange.HasChanges", false, interchange.HasChanges);
		}

		#region Process

		public void TestSend_Default()
		{
			SendAndAssert("");
		}

		public void TestSend_CCN()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoIMPServiceProvider.CCN.Code);
			SendAndAssert("");
		}

		public void TestSend_Descartes()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoIMPServiceProvider.Descartes.Code);
			SendAndAssert(CargoIMPServiceProvider.Descartes.Code);
		}

		void SendAndAssert(string serviceProvider)
		{
			CIMEDIInterchange interchange = CreateInterchange();
			CIMEDIMessage message = CreateMessage(interchange);
			Factory.Save();
			CargoIMPMessageSender.New(new TestServiceLogger()).Process();
			interchange.Reload();
			message.Reload();
			AssertEquals(EDIInterchange.Status.Sent, interchange.EI_Status);
			AssertEquals(EDIMessage.Status.Sent, message.EM_Status);
			EmailDef emailDef = Env.OutgoingMailManager.EmailsCreated[0];
			string subject = "EAGLE FWB EDI - TEST - FROM";
			if (serviceProvider.Length > 0)
			{
				subject += " - " + serviceProvider;
			}

			AssertEquals(subject, emailDef.Subject);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNoCompanyOrBranch()
		{
			using (Env.SetTemporaryUserContext("branchless user", Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var logger = new TestServiceLogger();
				AssertNoExceptionThrown(() => CargoIMPMessageSender.New(logger).Process());
				const string expectedLog = "Error|No logged in company found.\r\n";
				AssertEquals(logger.ToString(), expectedLog);
			}
		}

		#region Test Move To eHub

		public void TestMoveToeHubChannel_FWB()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes);
			TestMoveToeHubChannel(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FWB);
		}

		public void TestMoveToeHubChannel_FHL()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes);
			TestMoveToeHubChannel(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FHL);
		}

		public void TestMoveToeHubRegistry_FWB_WithRightFallback()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB);
			TestMoveToeHubRegistry(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FWB);
		}

		public void TestMoveToeHubRegistry_FHL_WithRightFallback()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB);
			TestMoveToeHubRegistry(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FHL);
		}

		public void TestMoveToeHubRegistry_FWB()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB);
			TestMoveToeHubRegistry(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FWB);
		}

		public void TestMoveToeHubRegistry_FHL()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB);
			TestMoveToeHubRegistry(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FHL);
		}

		public void TestMoveToeHubRegistry_HVLVFHL()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB);
			var messageType = Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FHL;
			var consol = CreateConsolWithShippingLineHavingSpecificCommunicationMode(messageType, EDICommunicationsModeCommunicationsTransportList.Codes.FTP);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			AssertInterchangeDetails(messageType, "eHubAirService", consignment as BusinessObject);
		}

		void TestMoveToeHubChannel(string messageType)
		{
			var consol = CreateConsolWithShippingLineHavingSpecificCommunicationMode(messageType, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);
			consol.ShippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PIMAAddress, "Pim Address");
			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "A");
			AssertInterchangeDetails(messageType, "EHUBCLIENT", consol);
		}

		void TestMoveToeHubRegistry(string messageType)
		{
			//When CargoIMPServiceProvider = 'HUB', should genetare message based on registry not on communicationMode.
			var consol = CreateConsolWithShippingLineHavingSpecificCommunicationMode(messageType, EDICommunicationsModeCommunicationsTransportList.Codes.FTP);
			AssertInterchangeDetails(messageType, "eHubAirService", consol);
		}

		ForwardingConsol CreateConsolWithShippingLineHavingSpecificCommunicationMode(string messageType, string communicationsTransport)
		{
			var consol = CreateConsol();
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var airline = RefAirline.LoadFromAirline2LetterCode(Factory, consol.AWBHeader.EH_By1st);
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			var communicationsMode = carrier.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode.EK_Destination = "EHUBCLIENT";
			communicationsMode.EK_FileFormat = messageType;
			communicationsMode.EK_CommunicationsTransport = communicationsTransport;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			return consol;
		}

		void AssertInterchangeDetails(string messageType, string expectedRecipient, BusinessObject messageParent)
		{
			var interchange = CreateInterchange();
			var message = CreateMessage(interchange);
			message.EM_LinkUniqueID = messageParent.PK;
			message.EM_LinkTable = messageParent.TableName;
			message.EM_MessageType = messageType;
			Factory.Save();
			var logger = new TestServiceLogger();
			CargoIMPMessageSender.New(logger).Process();
			interchange.Reload();
			message.Reload();
			AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			AssertEquals(expectedRecipient, interchange.EI_To);
			AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals(interchange.PK, interchange.EI_SessionGUID);
			AssertEquals(EDIInterchange.ApplicationCodes.CIM, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchange.ApplicationCodes.CIM, interchange.EI_InterchangeType);
			AssertEquals(EDIMessage.Status.Sent, message.EM_Status);
			AssertEquals(EDIMessage.ApplicationCodes.CIM, message.EM_ApplicationCode);
			AssertEquals(messageType, message.EM_MessageType);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Message with Interchange #1 is queued for eHub.", logger.ToString());
		}

		#endregion Test Move To eHub

		#region Test Move to eAdaptor

		public void TestMoveToAdapterValidLicence_FWB()
		{
			eAdaptorValidLicence(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FWB);
		}

		public void TestMoveToAdapterValidLicence_FHL()
		{
			eAdaptorValidLicence(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FHL);
		}

		public void TestMoveToAdapterInvalidLicence_FWB()
		{
			eAdaptorInvalidLicense(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FWB);
		}

		public void TestMoveToAdapterInvalidLicence_FHL()
		{
			eAdaptorInvalidLicense(Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP.MessageTypes.FHL);
		}

		public void TestMoveToAdapterWithSameInterchangeNumber_FWB()
		{
			eAdaptorSameInterchangeNumber(AWB.Messaging.CargoIMP.MessageTypes.FWB);
		}

		public void TestMoveToAdapterWithSameInterchangeNumber_FHL()
		{
			eAdaptorSameInterchangeNumber(AWB.Messaging.CargoIMP.MessageTypes.FHL);
		}

		#region eAdaptor Tests Setups

		void eAdaptorValidLicence(string messageType)
		{
			var consol = CreateConsol();
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var airline = RefAirline.LoadFromAirline2LetterCode(Factory, consol.AWBHeader.EH_By1st);
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PIMAAddress, "Pim Address");
			var communicationsMode = carrier.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode.EK_Destination = "EHUBCLIENT";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = messageType;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var interchange1 = CreateInterchange();
			var message1 = CreateMessage(interchange1);
			message1.EM_LinkUniqueID = consol.PK;
			message1.EM_LinkTable = consol.TableName;
			message1.EM_MessageType = messageType;
			Factory.Save();
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes);
			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "A");
			var logger = new TestServiceLogger();
			eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CargoIMPMessageSender.New(logger).Process();
			eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			interchange1.Reload();
			message1.Reload();
			AssertEquals("EHUBCLIENT", interchange1.EI_To);
			AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange1.EI_From);
			AssertEquals(interchange1.PK, interchange1.EI_SessionGUID);
			AssertEquals(EDIInterchange.ApplicationCodes.CIM, interchange1.EI_ApplicationCode);
			AssertEquals(EDIInterchange.ApplicationCodes.CIM, interchange1.EI_InterchangeType);
			AssertEquals(EDIMessage.ApplicationCodes.CIM, message1.EM_ApplicationCode);
			AssertEquals(messageType, message1.EM_MessageType);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(EDIInterchange.Status.eAdaptorQueued, interchange1.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eAdaptor, interchange1.EI_TransportType);
			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertContains("Message with Interchange #1 is queued for eAdaptor", logger.ToString());
		}

		void eAdaptorInvalidLicense(string messageType)
		{
			var consol = CreateConsol();
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var airline = RefAirline.LoadFromAirline2LetterCode(Factory, consol.AWBHeader.EH_By1st);
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PIMAAddress, "Pim Address");
			var communicationsMode = carrier.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode.EK_Destination = "EHUBCLIENT";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = messageType;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var interchange1 = CreateInterchange();
			var message1 = CreateMessage(interchange1);
			message1.EM_LinkUniqueID = consol.PK;
			message1.EM_LinkTable = consol.TableName;
			message1.EM_MessageType = messageType;
			Factory.Save();
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes);
			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "A");
			var logger = new TestServiceLogger();
			CargoIMPMessageSender.New(logger).Process();
			interchange1.Reload();
			message1.Reload();
			AssertEquals("EHUBCLIENT", interchange1.EI_To);
			AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange1.EI_From);
			AssertEquals(interchange1.PK, interchange1.EI_SessionGUID);
			AssertEquals(EDIInterchange.ApplicationCodes.CIM, interchange1.EI_ApplicationCode);
			AssertEquals(EDIInterchange.ApplicationCodes.CIM, interchange1.EI_InterchangeType);
			AssertEquals(EDIMessage.ApplicationCodes.CIM, message1.EM_ApplicationCode);
			AssertEquals(messageType, message1.EM_MessageType);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(EDIInterchange.Status.Failed, interchange1.EI_Status);
			AssertEquals(EDIMessage.Status.Failed, message1.EM_Status);
			AssertContains("eAdaptor SDK license does not work with CargoIMP messages. The message with Interchange #1 will be ignored and marked as failed.", logger.ToString());
		}

		void eAdaptorSameInterchangeNumber(string messageType)
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;
			consol.AWBHeader.EH_By1st = "QF";
			consol.AWBHeader.EH_AWBOriginCode = "SYD";
			var carrier = factory.LoadTop1<OrgHeader>(new ZQuery());
			var airline = RefAirline.LoadFromAirline2LetterCode(factory, consol.AWBHeader.EH_By1st);
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PIMAAddress, "Pim Address");
			var communicationsMode = carrier.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode.EK_Destination = "EADAPTORCLIENT";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = messageType;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes);
			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "A");
			eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var interchange1 = CreateInterchangeWithMessageForEAdaptor("19220", consol, messageType, factory);
			factory.Save();
			var logger = new TestServiceLogger();
			CargoIMPMessageSender.New(logger).Process();
			AssertEquals("Interchange should succeed", EDIInterchange.Status.eAdaptorQueued, interchange1.EI_Status);
			AssertEquals("Interchange should succeed", EDIInterchange.TransportType.eAdaptor, interchange1.EI_TransportType);
			AssertContains("Message with Interchange #19220 is queued for eAdaptor.", logger.ToString());
			var interchange2 = CreateInterchangeWithMessageForEAdaptor("19220", consol, messageType, factory);
			factory.Save();
			logger = new TestServiceLogger();
			CargoIMPMessageSender.New(logger).Process();
			AssertEquals("Expected the new interchange to be failed as interchanges with the same number, to and from addresses cannot be saved together", EDIInterchange.Status.Failed, interchange2.EI_Status);
			AssertNotContains("Message with Interchange #19220 is queued for eAdaptor.", logger.ToString());
			AssertContains("Sending of Interchange #19220 has failed. This interchange and all messages attached to it have been failed.", logger.ToString());
		}

		#endregion eAdaptor Tests Setups

		#endregion Test Move to eAdaptor

		#region Testing Batch Processing

		public void TestProcessingValidInterchangeInBatches()
		{
			ForwardingConsol consol1 = CreateConsol();
			ForwardingConsol consol2 = CreateConsol();
			List<CIMEDIInterchange> interchanges = CreateInterchangesWithMessages(consol1, 10);
			interchanges.AddRange(CreateInterchangesWithMessages(consol2, 62));
			Factory.Save();
			TestServiceLogger logger = new TestServiceLogger();
			CargoIMPMessageSender.New(logger).Process();
			AssertContains("Attempting to process Interchange #1", logger.ToString());
			AssertContains("Attempting to process Interchange #50", logger.ToString());
			AssertContains("Attempting to process Interchange #51", logger.ToString());
			AssertContains("Attempting to process Interchange #72", logger.ToString());
			AssertNotContains("has failed", logger.ToString());
		}

		public void TestProcessUsesDifferentBranchForEachInterchange()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "111";
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "1aa";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "1bb";
			Factory.Save();
			using (ForwardingConfigurationRegistry.Instance.SendFWBOrFHLToAirlineBasedOnMAWBPrefix.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, false))
			using (ForwardingConfigurationRegistry.Instance.SendFWBOrFHLToAirlineBasedOnMAWBPrefix.SetTemporaryValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, true))
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var exportAWBHeader = Factory.New<ExportAWBHeader>();
				exportAWBHeader.EH_WayBillNumber = "074-21065380";
				exportAWBHeader.EH_By1st = "LH";
				var interchange1 = CreateInterchange();
				interchange1.EI_GB = branch1.PK;
				interchange1.EI_HeaderText = string.Empty;
				var message1 = CreateMessage(interchange1);
				message1.EM_LinkTable = exportAWBHeader.TableName;
				message1.EM_LinkUniqueID = exportAWBHeader.PK;
				var interchange2 = CreateInterchange();
				interchange2.EI_GB = branch2.PK;
				interchange2.EI_HeaderText = string.Empty;
				var message2 = CreateMessage(interchange2);
				message2.EM_LinkTable = exportAWBHeader.TableName;
				message2.EM_LinkUniqueID = exportAWBHeader.PK;
				Factory.Save();
				var logger = new TestServiceLogger();
				CargoIMPMessageSender.New(logger).Process();
				interchange1.Reload();
				AssertEquals("interchange.EI_HeaderText", "FWB\r\nLH\r\n", interchange1.EI_HeaderText);
				interchange2.Reload();
				AssertEquals("interchange.EI_HeaderText", "FWB\r\nKL\r\n", interchange2.EI_HeaderText);
			}
		}

		public void TestProcessOnlyAffectsMessagesForBranchesOfCurrentCompany()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "111";
			var company1Branch = company1.Branches.AddNew();
			company1Branch.GB_Code = "1aa";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "222";
			var company2Branch = company2.Branches.AddNew();
			company2Branch.GB_Code = "2aa";
			var interchanges = CreateInterchangesWithMessages(CreateConsol(), 2);
			interchanges[0].EI_GB = company1Branch.PK;
			interchanges[1].EI_GB = company2Branch.PK;
			Factory.Save();
			var logger = new TestServiceLogger();
			using (DisposableEnvironment.ForCompany(company1.GC_Code))
			{
				CargoIMPMessageSender.New(logger).Process();
			}

			interchanges.ForEach(interchange => interchange.Reload());
			AssertEquals("Interchange should have been sent, as current company includes interchange's branch.", EDIInterchange.Status.eHubQueued, interchanges[0].EI_Status);
			AssertEquals("Interchange should not have been sent, as current company does not include interchange's branch.", EDIInterchange.Status.Queued, interchanges[1].EI_Status);
		}

		#endregion Testing Batch Processing

		public void TestErrorWhenProcessingInterchangeWithInactiveBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "111";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "1aa";
			branch.GB_IsActive = false;
			var consol = CreateConsol();
			var interchange = CreateInterchange();
			interchange.EI_GB = branch.PK;
			var message = CreateMessage(interchange);
			message.EM_LinkTable = consol.TableName;
			message.EM_LinkUniqueID = consol.PK;
			Factory.Save();
			var logger = new TestServiceLogger();
			using (new DisposableEnvironment(branch.PK.ToGuid(), User.ServiceUserName))
			{
				CargoIMPMessageSender.New(logger).Process();
			}

			interchange.Reload();
			message.Reload();
			AssertEquals(EDIInterchange.Status.Failed, interchange.EI_Status);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			AssertContains("Message with Interchange #1 has an inactive branch and will be ignored and marked as failed.", logger.ToString());
		}

		public void TestProcessLogsBranchSwitching()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "111";
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "1aa";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "1bb";
			var interchanges = CreateInterchangesWithMessages(CreateConsol(), 2);
			interchanges[0].EI_GB = branch1.PK;
			interchanges[1].EI_GB = branch2.PK;
			Factory.Save();
			var logger = new TestServiceLogger();
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				CargoIMPMessageSender.New(logger).Process();
			}

			foreach (var interchange in interchanges)
			{
				AssertContains($"Switched to branch: {interchange.Branch.HumanReadableNameForRegistry}.", logger.ToString());
			}
		}

		public void TestSend_InterchangeFromOtherCompany()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "111";
			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "222";
			CIMEDIInterchange interchange = CreateInterchange();
			interchange.EI_GB = branch1.PK;
			CIMEDIMessage message = CreateMessage(interchange);
			Factory.Save();
			CargoIMPMessageSender.New(new TestServiceLogger()).Process();
			interchange.Reload();
			message.Reload();
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSend_InterchangeFromOtherBranchInSameCompany()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "111";
			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "222";
			GlbBranch branch2 = company1.Branches.AddNew();
			branch2.GB_Code = "333";
			CIMEDIInterchange interchange1 = CreateInterchange();
			interchange1.EI_GB = branch1.PK;
			CIMEDIMessage message1 = CreateMessage(interchange1);
			CIMEDIInterchange interchange2 = CreateInterchange();
			interchange2.EI_GB = branch2.PK;
			CIMEDIMessage message2 = CreateMessage(interchange2);
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				CargoIMPMessageSender.New(new TestServiceLogger()).Process();
				interchange1.Reload();
				message1.Reload();
				interchange2.Reload();
				message2.Reload();
				AssertEquals(EDIInterchange.Status.Sent, interchange1.EI_Status);
				AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
				AssertEquals(EDIInterchange.Status.Sent, interchange2.EI_Status);
				AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);
				AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		#region Test Process Invalid Consol

		public void TestProcessNonAirConsol()
		{
			var consol = CreateConsol();
			consol.JK_UniqueConsignRef = "C00001234";
			var interchange = CreateInterchange();
			var message = CreateMessage(interchange);
			message.EM_LinkTable = consol.TableName;
			message.EM_LinkUniqueID = consol.PK;
			consol.JK_TransportMode = "SEA";
			Factory.Save();
			var logger = new TestServiceLogger();
			CargoIMPMessageSender.New(logger).Process();
			var expectedLog = @"Error|Sending of Interchange #1 has failed.
SEA Consol (C00001234) does not contain an Air Waybill.";
			AssertContains(expectedLog, logger.ToString());
			AssertEquals(EDIInterchange.Status.Failed, interchange.EI_Status);
		}

		#endregion Test Process Invalid Consol

		#endregion Process

		#region Transmission Method

		public void TestInterchangeHeaderFooterSetBasedOnTransmissionMethod()
		{
			using (ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.CargoIMPServiceProviderConstants.CCN))
			{
				var consol = CreateConsol();
				var interchange = CreateInterchange();
				var message = CreateMessage(interchange);
				message.EM_LinkTable = consol.TableName;
				message.EM_LinkUniqueID = consol.PK;
				Factory.Save();
				AssertEquals("Header", interchange.EI_HeaderText);
				CargoIMPMessageSender.New(new TestServiceLogger()).Process();
				interchange.Reload();
				AssertEquals("FWB\r\nQFSYD\r\n", interchange.EI_HeaderText);
				AssertEquals("\x04", interchange.EI_FooterText);
				AssertEquals(false, interchange.HasChanges);
			}
		}

		public void TestInterchangeHeaderSetBasedOnTransmissionMethod_Misconfiguration()
		{
			ForwardingConsol consol = CreateConsol();
			consol.AWBHeader.EH_By1st = "~~";
			CIMEDIInterchange interchange = CreateInterchange();
			CIMEDIMessage message = CreateMessage(interchange);
			message.EM_LinkTable = consol.TableName;
			message.EM_LinkUniqueID = consol.PK;
			Factory.Save();
			TestServiceLogger logger = new TestServiceLogger();
			CargoIMPMessageSender.New(logger).Process();
			interchange.Reload();
			message.Reload();
			AssertEquals(EDIInterchange.Status.Failed, interchange.EI_Status);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Sending of Interchange #1 has failed.\r\nAirline code is not found", logger.ToString());
		}

		#endregion Transmission Method

		#region Implementation

		protected CIMEDIInterchange CreateInterchange()
		{
			CIMEDIInterchange result = Factory.New<CIMEDIInterchange>();
			result.EI_HeaderText = "Header";
			result.EI_BodyText = "Body";
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			result.EI_From = "FROM";
			result.EI_Status = EDIInterchange.Status.Queued;
			result.EI_To = "TO";
			return result;
		}

		protected CIMEDIMessage CreateMessage(CIMEDIInterchange interchange)
		{
			CIMEDIMessage result = Factory.New<CIMEDIMessage>();
			result.EM_MessageText = "MessageText";
			result.EM_MessageType = "FWB";
			result.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			result.EM_ApplicationReference = "AppRef";
			result.EM_EI = interchange.PK;
			return result;
		}

		ForwardingConsol CreateConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;
			consol.AWBHeader.EH_By1st = "QF";
			consol.AWBHeader.EH_AWBOriginCode = "SYD";
			return consol;
		}

		CIMEDIInterchange CreateInterchangeWithMessageForEAdaptor(ZString interchangeNum, ForwardingConsol consol, ZString messageType, BusinessObjectFactory factory)
		{
			CIMEDIInterchange interchange = factory.New<CIMEDIInterchange>();
			interchange.EI_HeaderText = "Header";
			interchange.EI_BodyText = "Body";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = "FROM";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_To = "TO";
			interchange.EI_InterchangeNum = interchangeNum;
			var message = factory.New<CIMEDIMessage>();
			message.EM_MessageText = "MessageText";
			message.EM_MessageType = "FWB";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationReference = "AppRef";
			message.EM_EI = interchange.PK;
			message.EM_LinkUniqueID = consol.PK;
			message.EM_LinkTable = consol.TableName;
			message.EM_MessageType = messageType;
			return interchange;
		}

		List<CIMEDIInterchange> CreateInterchangesWithMessages(ForwardingConsol consol, int numberOfMessage)
		{
			List<CIMEDIInterchange> result = new List<CIMEDIInterchange>();
			for (int i = 0; i < numberOfMessage; i++)
			{
				CIMEDIInterchange interchange = CreateInterchange();
				CIMEDIMessage message = CreateMessage(interchange);
				message.EM_LinkTable = consol.TableName;
				message.EM_LinkUniqueID = consol.PK;
				result.Add(interchange);
			}

			return result;
		}

		#endregion Implementation
	}
}
