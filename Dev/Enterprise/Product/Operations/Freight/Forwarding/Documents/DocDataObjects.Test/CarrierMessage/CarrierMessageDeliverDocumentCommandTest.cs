using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class CarrierMessageDeliverDocumentCommandTest : TestCaseWithFactory
	{
		public void TestNoAction()
		{
			var consol = CreateConsol();
			var notificationService = new Mock<IUserNotificationService>();
			AssertDeliverDocumentPopupForm(consol, notificationService, DeliverDocumentPopupAction.NoAction, result: false);
		}

		public void TestSendMessage()
		{
			var consol = CreateConsol();
			var notificationService = new Mock<IUserNotificationService>();
			AssertDeliverDocumentPopupForm(consol, notificationService, DeliverDocumentPopupAction.SendMessage);
			notificationService.Verify(s => s.ShowMessage("Message has been sent.", "Sending Message"), Times.Once);
		}

		public void TestOnlyDeliverDocument()
		{
			var consol = CreateConsol();
			AssertDeliverDocumentPopupForm(consol, new Mock<IUserNotificationService>(), DeliverDocumentPopupAction.DeliverDocument);

			var ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNull(ocb);
		}

		void AssertDeliverDocumentPopupForm(ForwardingConsol consol, Mock<IUserNotificationService> notificationService, DeliverDocumentPopupAction action, bool result = true)
		{
			var documentInfo = PrepareDocumentData(consol, notificationService);
			var command = new CarrierMessageDeliverDocumentCommand(consol);
			((INotifiableDocumentInfoCreated)command).NotifyDocumentInfoCreated(documentInfo.Object);

			using (FreightDataRegistry.Instance.EnableCarrierMessagingConnectionValidationTestUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var mockService = new Mock<IRoutingRuleValidatorService>();
				mockService
					.Setup(m => m.PerformValidationCheck(It.Is<RoutingRuleValidatorRequest>(request =>
						request.ServiceUrl == "https://ehub-routingws-test.wisegrid.net/RoutingRuleValidationWebService.svc"
						&& request.ClientId == "HYETSTTST"
						&& request.Password == "test"
						&& !string.IsNullOrWhiteSpace(request.Interchange))))
					.Returns(() => new RoutingRuleValidatorResponse { ValidationResult = true });

				AssertEquals(command.DeliverDocumentPopupFormAction(null, new DeliverDocumentCommand(consol), new HashSet<IDocumentInfo>() { documentInfo.Object }, action), result);
				Assert(command.IsEnabled);
			}
		}

		ForwardingConsol CreateConsol(bool isCoload = false)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001001";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_AgentType = isCoload ? Core.Constants.AgentType.CoLoad : Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_BookingReference = "1001";
			consol.JK_NoOriginalBills = 1;
			consol.JK_NoCopyBills = 3;
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);
			consol.JK_CarrierContractNumber = "11111";

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAERSK";
			org.OH_RL_NKClosestPort = "DKAAL";
			org.MainAddress.Address1 = "Unit 13";
			org.MainAddress.Address2 = "4 Lost Lane";
			org.MainAddress.City = "Aalborg";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.OA_RN_NKCountryCode = "DK";
			org.OH_IsShippingProvider = true;
			org.OH_IsShippingLine = true;
			org.OH_IsSeaWholesaler = false;

			var customscode = org.CustomsCodes.AddNew();
			customscode.OK_RN_NKCodeCountry = "US";
			customscode.OK_CodeType = "CCC";
			customscode.OK_CustomsRegNo = "9876";

			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "9876";
			shippingLine.RSL_CargoWiseOneCode = "AAAA";
			shippingLine.RSL_BookingRequestAvailable = false;
			shippingLine.RSL_ShippingInstructionAvailable = false;
			shippingLine.RSL_ShippingOrderAvailable = false;
			shippingLine.RSL_IsNVO = false;

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "BLOOP";
			unpackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unpackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unpackDepotAddress.MainAddress.Address2 = "Crabby";
			unpackDepotAddress.MainAddress.City = "New York";
			unpackDepotAddress.MainAddress.Postcode = "10005";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";
			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var dangerousGood1 = consol.ConsolDGRestrictionCollection.AddNew();
			dangerousGood1.JKD_Class = "1.1D";
			dangerousGood1.JKD_UNNO = "0004";
			dangerousGood1.JKD_Variant = "a";
			var dangerousGood2 = consol.ConsolDGRestrictionCollection.AddNew();
			dangerousGood2.JKD_Class = "6.1";
			dangerousGood2.JKD_UNNO = "1695";
			dangerousGood2.JKD_Variant = "";

			var letterOfCredit = consol.Numbers.AddNew();
			letterOfCredit.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.LetterOfCreditNumber;
			letterOfCredit.CE_EntryNum = "22222";

			var sldNumber = consol.Numbers.AddNew();
			sldNumber.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
			sldNumber.CE_EntryNum = "12345";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Steven";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "CNSHA";
			transport3.JW_Vessel = "Miranda";
			transport3.JW_VoyageFlight = "333";

			PopulateAddresses(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 2;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;
			FillWithContactInformation(consignorPickupAddress.MainAddress);

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "LCMSIN";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "SGSIN";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 155";
			consigneeDeliveryAddress.MainAddress.Address2 = "55 Lost Lane";
			consigneeDeliveryAddress.MainAddress.City = "SINGAPORE";
			consigneeDeliveryAddress.MainAddress.Postcode = "2215";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;
			FillWithContactInformation(consigneeDeliveryAddress.MainAddress);

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 200;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "ABCDE";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";
			packline1.JL_ContainerPackingOrder = 1;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "6666", "E", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "6666";
				subs.DG_Variant = "E";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg = packline1.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			var hc = Factory.New<JobPackLineHarmonisedCode>();
			hc.JLH_RN_NKCountry = "CN";
			hc.JLH_Code = "1234";

			packline1.HarmonisedCodes.Add(hc);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ExportRefNumber = "BBB";
			packline2.JL_PackageCount = 5;
			packline2.JL_ExportRefNumber = "REF001";
			packline2.JL_DetailedDescription = "pack2";
			packline2.JL_ContainerPackingOrder = 2;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 20000;
			packline3.JL_ActualWeightUQ = "G";
			packline3.JL_ActualVolume = 300;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_HarmonisedCode = "EEEEE";
			packline3.JL_ExportRefNumber = "REF002";
			packline3.JL_DetailedDescription = "pack3";
			packline3.JL_ContainerPackingOrder = 3;

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);
			container.PackLines.Add(packline3);

			Factory.Save();

			return consol;
		}

		void FillWithContactInformation(OrgAddress address)
		{
			address.OA_Phone = "12344";
			address.OA_Fax = "222";
			address.OA_Email = "aaa@test.com";
		}

		void PopulateAddresses(ForwardingConsol consol, bool isCoload = false)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "9001";
			shippingLine.RSL_IsNVO = false;
			shippingLine.RSL_BookingRequestAvailable = true;

			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			if (isCoload)
			{
				consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			}
			else
			{
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			}

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKLoadPort.Left(2);

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_OH = sendingForwarder.PK;
			sendingForwarderContact.OC_ContactName = "Sender Name";
			sendingForwarderContact.OC_Email = "name@sender.com";
			sendingForwarderContact.OC_Phone = "1111111";
			sendingForwarderContact.OC_Fax = "2222222";

			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_OH = receivingForwarder.PK;
			receivingForwarderContact.OC_ContactName = "Receiver Name";
			receivingForwarderContact.OC_Email = "name@receiver.com";
			receivingForwarderContact.OC_Phone = "3333333";
			receivingForwarderContact.OC_Fax = "4444444";

			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;

			var handlingAgent = Factory.New<OrgHeader>();
			handlingAgent.OH_FullName = "I'm Handling Stuff";
			handlingAgent.OH_RL_NKClosestPort = "AUSYD";
			handlingAgent.MainAddress.Address1 = "Unit 2";
			handlingAgent.MainAddress.Address2 = "60 What Lane";
			handlingAgent.MainAddress.City = "Sydney";
			handlingAgent.MainAddress.Postcode = "2023";
			handlingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = handlingAgent.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Booking Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Stay in touch";
			notifyParty.OH_RL_NKClosestPort = "AUSYD";
			notifyParty.MainAddress.Address1 = "Unit 205";
			notifyParty.MainAddress.Address2 = "128 Why Lane";
			notifyParty.MainAddress.City = "Sydney";
			notifyParty.MainAddress.Postcode = "2000";
			notifyParty.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			notifyParty.MainAddress.OA_Email = "stayintouch@test.com";

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "I'm Notifying About Stuff";
			notifyParty2.OH_RL_NKClosestPort = "AUSYD";
			notifyParty2.MainAddress.Address1 = "Unit 2";
			notifyParty2.MainAddress.Address2 = "60 What Lane";
			notifyParty2.MainAddress.City = "Sydney";
			notifyParty2.MainAddress.Postcode = "2023";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "Notifying 3";
			notifyParty3.OH_RL_NKClosestPort = "AUSYD";
			notifyParty3.MainAddress.Address1 = "Unit 2";
			notifyParty3.MainAddress.Address2 = "60 What Kine";
			notifyParty3.MainAddress.City = "Sydney";
			notifyParty3.MainAddress.Postcode = "2029";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);

			consol.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_PortName = "Test";
			unloco.RL_Code = "12345";

			consol.JK_RL_NKCarrierBookingOffice = unloco.RL_Code;
		}

		Mock<IDocumentInfo> PrepareDocumentData(ForwardingConsol consol, Mock<IUserNotificationService> notificationService)
		{
			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			var messageInstructions = new Mock<IMessageInstructions>();
			var eDocsInstructions = new Mock<IEDocsInstructions>();
			var printInstructions = new DummyPrintInstructions()
			{
				Title = "Test Document",
				DeliveryModes = new[]
				{
						nameof(PrintCopyType.ALL)
				}
			};

			securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
			securityService.Setup(s => s.CanDeliver).Returns(true);
			notificationService.Setup(ns => ns.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IDocumentSecurityService>(securityService.Object);
			services.Register<IUserNotificationService>(notificationService.Object);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = Business.ConsolDocumentDataStoreNames.SeaBookingRequest2;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Services).Returns(services);
			documentInfo.SetupGet(di => di.Descriptor).Returns(descriptor.Object);

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns("Booking Request");
			document.Setup(d => d.DataContext).Returns(DataContext.BookingRequest);

			eDocsInstructions.SetupGet(edoc => edoc.SaveCopyToEDocs).Returns(false);
			eDocsInstructions.SetupGet(edoc => edoc.Parent).Returns(consol);

			descriptor.SetupGet(di => di.Name).Returns("BookingRequest");
			descriptor.SetupGet(di => di.DocumentType).Returns("ALL");
			descriptor.SetupGet(di => di.MessageInstructions).Returns(messageInstructions.Object);
			descriptor.SetupGet(di => di.EDocsInstructions).Returns(eDocsInstructions.Object);
			descriptor.SetupGet(di => di.PrintInstructions).Returns(printInstructions);

			messageInstructions.SetupGet(mi => mi.DocumentName).Returns("Booking Request");
			messageInstructions.SetupGet(mi => mi.XmlNamespace).Returns("BookingRequest/1");
			messageInstructions.SetupGet(mi => mi.EHubClientID).Returns("EDI");
			var parameters = new DummyDocDataObjectParameters()
			{
				LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
			};

			var bookingRequest = new BookingRequestBuilder(consol, parameters).Build();
			dynamicData.SetupGet(di => di.Value).Returns(bookingRequest);
			Factory.Save();

			return documentInfo;
		}
	}
}
