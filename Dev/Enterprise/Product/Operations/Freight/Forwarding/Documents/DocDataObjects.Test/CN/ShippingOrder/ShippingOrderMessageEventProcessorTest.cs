using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Moq;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ShippingOrderMessageEventProcessorTest : DataObjects.Testing.OCBEventProcessorTest
	{
		#region Test

		public void TestOnResetToOriginal_RecalculateCSR()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var dynamicData = shippingOrder.MakeDynamic();

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns("Shipping Order");
			document.SetupGet(d => d.DataContext).Returns(DataContext.ShippingOrder);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnResetToOriginal();

			var csrEntryNumber = consol.Numbers.Find(num => num.CE_EntryType == ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference
									&& num.CE_EntryIsSystemGenerated).FirstOrDefault();

			Assert("CSR is saved", csrEntryNumber.IsInDatabase);
			AssertEquals("CSR number had beed recalculated after reseting to original", "CONSOL0001-V1", csrEntryNumber.CE_EntryNum);
			AssertEquals("SourceID is updated", "CONSOL0001-V1", shippingOrder.SourceID);
		}

		public void TestOnResetToOriginal_RecalculateCSR_WithCSRNumberAllocationLock()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var dynamicData = shippingOrder.MakeDynamic();

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns("Shipping Order");
			document.SetupGet(d => d.DataContext).Returns(DataContext.ShippingOrder);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);

			using (var mutex = new ZGlobalMutex(ZArchitecture.Modules.MutexIDs.CSRNumberAllocation, consol.PK.ToString()))
			{
				mutex.Lock();

				supporter.GetMessageEventsProcessor(document.Object).OnResetToOriginal();

				var csrEntryNumber = consol.Numbers.Find(num => num.CE_EntryType == ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference
					&& num.CE_EntryIsSystemGenerated).FirstOrDefault();
				AssertNull(csrEntryNumber);
			}
		}

		public void TestAddOCBEvent_SendMessageAsPDF_WithOutCarrierBookingAgent()
		{
			var consol = Consol;
			consol.ShippingLine.ShippingLine.RSL_ShippingOrderAvailable = false;
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			var ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNull(ocb);

			AddLog(documentData, Events.MessageSent);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNotNull(ocb);
			AssertEquals(ExpectedOrgMessage, ocb.SL_Reference);
		}

		public void TestAddOCBEvent_SendMessageAsPDF_WithCarrierBookingAgent_ShippingOrderAvailable_False()
		{
			var consol = Consol;

			var agentShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			agentShippingLine.RSL_StandardCarrierAlphaCode = "3333";
			agentShippingLine.RSL_ShippingOrderAvailable = false;
			agentShippingLine.RSL_IsShippingLine = true;
			agentShippingLine.RSL_IsNVO = false;

			var carrierBookingAgent = Factory.New<OrgHeader>();
			carrierBookingAgent.OH_FullName = "NERSK";
			carrierBookingAgent.OH_RL_NKClosestPort = "EKAAL";
			carrierBookingAgent.MainAddress.Address1 = "Unit 14";
			carrierBookingAgent.MainAddress.Address2 = "5 Lost Lane";
			carrierBookingAgent.MainAddress.City = "Balborg";
			carrierBookingAgent.MainAddress.Postcode = "3000";
			carrierBookingAgent.MainAddress.OA_RN_NKCountryCode = "DK";
			carrierBookingAgent.OH_IsShippingProvider = true;
			carrierBookingAgent.OH_IsShippingLine = true;
			carrierBookingAgent.OH_IsSeaWholesaler = false;
			carrierBookingAgent.OH_RSL_ShippingLine = agentShippingLine.PK;

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			var ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNull(ocb);

			AddLog(documentData, Events.MessageSent);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNotNull(ocb);
			AssertEquals(ExpectedOrgMessage, ocb.SL_Reference);
		}

		public void TestAddOCBEvent_SendMessageAsPDF_WithCarrierBookingAgent_ShippingOrderAvailable_True()
		{
			var consol = Consol;

			var agentShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			agentShippingLine.RSL_StandardCarrierAlphaCode = "3333";
			agentShippingLine.RSL_ShippingOrderAvailable = true;
			agentShippingLine.RSL_IsShippingLine = true;
			agentShippingLine.RSL_IsNVO = false;

			var carrierBookingAgent = Factory.New<OrgHeader>();
			carrierBookingAgent.OH_FullName = "NERSK";
			carrierBookingAgent.OH_RL_NKClosestPort = "EKAAL";
			carrierBookingAgent.MainAddress.Address1 = "Unit 14";
			carrierBookingAgent.MainAddress.Address2 = "5 Lost Lane";
			carrierBookingAgent.MainAddress.City = "Balborg";
			carrierBookingAgent.MainAddress.Postcode = "3000";
			carrierBookingAgent.MainAddress.OA_RN_NKCountryCode = "DK";
			carrierBookingAgent.OH_IsShippingProvider = true;
			carrierBookingAgent.OH_IsShippingLine = true;
			carrierBookingAgent.OH_IsSeaWholesaler = false;
			carrierBookingAgent.OH_RSL_ShippingLine = agentShippingLine.PK;

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			var ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNull(ocb);

			AddLog(documentData, Events.MessageSent);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			AssertNull(ocb);
		}
		#endregion

		#region Implementation

		protected override DocDataObject DocDataObject => new ShippingOrderBuilder(Consol).Build();
		protected override string Context => DataContext.ShippingOrder;
		protected override string DocumentName => ConsolDocumentNames.ShippingOrder;
		protected override string DataStoreName => ConsolDocumentDataStoreNames.ShippingOrder;
		protected override string ExpectedOrgMessage => "|CMP=2222|MAX=10.00|NEW=10.00|OLD=0|QTY=10.00|STA=ORG|TYP=Shipping Order";
		protected override string ExpectedAmdMessage => "|CMP=2222|MAX=10.00|NEW=0|OLD=10.00|QTY=0.00|STA=AMD|TYP=Shipping Order";
		protected override string ExpectedWthMessage => "|CMP=2222|MAX=10.00|NEW=0|OLD=0|QTY=0.00|STA=WTH|TYP=Shipping Order";
		protected override string ExpectedDocumentDeliveryMessage => "|ACT=Document Delivery|CMP=2222|MAX=10.00|NEW=10.00|OLD=0|QTY=10.00|STA=ORG|TYP=Shipping Order";

		protected override ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.AgentConsol;
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_AgentsReference = "AgentRef002";
			consol.JK_MasterBillNum = "1112222222";
			consol.JK_CoLoadMasterBill = "COLOAD004";
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USARD";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_ReleaseType = ZString.Empty;
			consol.JK_NoCopyBills = 3;
			consol.JK_NoOriginalBills = 4;
			consol.JK_BookingReference = "BOOKINGREF01";
			consol.JK_CoLoadBookingReference = "COLOADREF02";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 2, 19);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "2222";
			shippingLine.RSL_ShippingOrderAvailable = true;
			shippingLine.RSL_IsShippingLine = true;
			shippingLine.RSL_IsNVO = false;

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
			org.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

			var mainTransport = consol.Transports[0];
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "USARD";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BUNGA XYLIMA";
			vessel.RV_LloydsNumber = "8907993";

			mainTransport.JW_Vessel = vessel.RV_Code;
			mainTransport.JW_VoyageFlight = "F9999";

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "Test";
			refcontainer.RC_TEU = 5m;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_RC = refcontainer.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container2.JC_RC = refcontainer.PK;

			Factory.Save();

			return consol;
		}

		public void TestAddOCBAndMSNEvent_IsEmailAction()
		{
			var consol = Consol;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var carrier = Factory.New<OrgHeader>();

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_ShippingOrderAvailable = false;
			shippingLine.RSL_StandardCarrierAlphaCode = "9001";
			shippingLine.RSL_IsShippingLine = true;
			shippingLine.RSL_IsNVO = false;

			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsSeaWholesaler = false;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			Factory.Save();

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = "ALL";

			AddLog(documentData, Events.MessageSent);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			var ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			var msn = consol.Logs.MostRecentLogByPostedTime(Events.MessageSent);

			AssertNotNull(ocb);
			AssertNotNull(msn);
			AssertEquals("|ACT=Email|CMP=9001|MAX=10.00|NEW=10.00|OLD=0|QTY=10.00|STA=ORG|TYP=Shipping Order", ocb.SL_Reference);
			AssertEquals("|ACT=Email|CMP=9001|DEP=Carrier|MST=Shipping Order", msn.SL_Reference);
		}

		protected override void UpdateConsol()
		{
			Consol.Containers.RemoveAndDeleteAll();

			Factory.Save();
		}

		#endregion
	}
}
