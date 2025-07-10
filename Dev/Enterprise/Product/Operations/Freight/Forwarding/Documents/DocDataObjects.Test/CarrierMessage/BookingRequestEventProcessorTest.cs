using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Moq;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class BookingRequestEventProcessorTest : DataObjects.Testing.OCBEventProcessorTest
	{
		#region Test

		public void TestOnResetToOriginal_RecalculateCSR_BookingRequest()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters()
			{
				LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
			};

			var bookingRequestData = new BookingRequestBuilder(consol, parameters).Build();

			var dynamicData = bookingRequestData.MakeDynamic();

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns("Booking Request");
			document.SetupGet(d => d.DataContext).Returns(DataContext.BookingRequest);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnResetToOriginal();

			var csrEntryNumber = consol.Numbers.Find(num => num.CE_EntryType == ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference
									&& num.CE_EntryIsSystemGenerated).FirstOrDefault();
			AssertEquals("CSR number had beed recalculated after reseting to original", "CONSOL0001-V1", csrEntryNumber.CE_EntryNum);
			AssertEquals("SourceID is updated", "CONSOL0001-V1", bookingRequestData.SourceID);
		}

		public void TestOnResetToOriginal_RecalculateCSR_BookingRequest_WithCSRNumberAllocationLock()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters()
			{
				LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
			};

			var bookingRequestData = new BookingRequestBuilder(consol, parameters).Build();

			var dynamicData = bookingRequestData.MakeDynamic();

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns("Booking Request");
			document.SetupGet(d => d.DataContext).Returns(DataContext.BookingRequest);
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

		public void TestAddOCBEvent_SendMessageAsPDF()
		{
			var consol = Consol;
			consol.ShippingLine.ShippingLine.RSL_BookingRequestAvailable = false;
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

		#endregion

		#region Implementation

		protected override DocDataObject DocDataObject
		{
			get
			{
				var parameters = new DummyDocDataObjectParameters()
				{
					LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
				};

				return new BookingRequestBuilder(Consol, parameters).Build();
			}
		}

		protected override string Context => DataContext.BookingRequest;
		protected override string DocumentName => ConsolDocumentNames.BookingRequest;
		protected override string DataStoreName => ConsolDocumentDataStoreNames.SeaBookingRequest2;
		protected override string ExpectedOrgMessage => "|CMP=1234|MAX=10.50|NEW=10.50|OLD=0|QTY=10.50|STA=ORG|TYP=Booking Request";
		protected override string ExpectedAmdMessage => "|CMP=1234|MAX=150.00|NEW=150.00|OLD=10.50|QTY=139.50|STA=AMD|TYP=Booking Request";
		protected override string ExpectedWthMessage => "|CMP=1234|MAX=150.00|NEW=150.00|OLD=150.00|QTY=0.00|STA=WTH|TYP=Booking Request";
		protected override string ExpectedDocumentDeliveryMessage => "|ACT=Document Delivery|CMP=1234|MAX=10.50|NEW=10.50|OLD=0|QTY=10.50|STA=ORG|TYP=Booking Request";

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
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_ReleaseType = ZString.Empty;
			consol.JK_NoCopyBills = 3;
			consol.JK_NoOriginalBills = 4;
			consol.JK_BookingReference = "BOOKINGREF01";
			consol.JK_CoLoadBookingReference = "COLOADREF02";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 2, 19);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_BookingRequestAvailable = true;
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
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BUNGA XYLIMA";
			vessel.RV_LloydsNumber = "8907993";

			mainTransport.JW_Vessel = vessel.RV_Code;
			mainTransport.JW_VoyageFlight = "F9999";

			var refcontainer1 = Factory.New<RefContainer>();
			refcontainer1.RC_Code = "Test";
			refcontainer1.RC_TEU = 5m;

			var refcontainer2 = Factory.New<RefContainer>();
			refcontainer2.RC_Code = "T_st";
			refcontainer2.RC_TEU = 0m;
			refcontainer2.RC_ISOType = "1";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_ContainerCount = 2;
			container1.JC_RC = refcontainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container2.JC_RC = refcontainer2.PK;

			Factory.Save();

			return consol;
		}

		protected override void UpdateConsol()
		{
			Consol.Containers.RemoveAndDeleteAll();

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "Te_t";
			refcontainer.RC_TEU = 5m;

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA00000121";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_ContainerCount = 30;
			container1.JC_RC = refcontainer.PK;

			Factory.Save();
		}

		public void TestAddOCBAndMSNEvent_IsEmailAction()
		{
			var consol = Consol;
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var carrier = Factory.New<OrgHeader>();

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_BookingRequestAvailable = false;
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
			AssertEquals("|ACT=Email|CMP=9001|MAX=10.50|NEW=10.50|OLD=0|QTY=10.50|STA=ORG|TYP=Booking Request", ocb.SL_Reference);
			AssertEquals("|ACT=Email|CMP=9001|DEP=Carrier|MST=Booking Request", msn.SL_Reference);
		}

		public void TestAddOCBAndMSNEvent_IsEmailAction_CoLoader()
		{
			var consol = Consol;
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var creditor = Factory.New<OrgHeader>();

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_BookingRequestAvailable = false;
			shippingLine.RSL_StandardCarrierAlphaCode = "9001";
			shippingLine.RSL_IsNVO = true;

			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			creditor.OH_FullName = "MAERSK";
			creditor.OH_RL_NKClosestPort = "DKAAL";
			creditor.MainAddress.Address1 = "Unit 13";
			creditor.MainAddress.Address2 = "4 Lost Lane";
			creditor.MainAddress.City = "Aalborg";
			creditor.MainAddress.Postcode = "2000";
			creditor.MainAddress.OA_RN_NKCountryCode = "DK";
			creditor.OH_IsShippingProvider = true;
			creditor.OH_IsShippingLine = true;
			creditor.OH_IsSeaWholesaler = false;
			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			Factory.Save();

			var contact = creditor.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = "ALL";

			AddLog(documentData, Events.MessageSent);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(Document).OnMessageSent();

			var ocb = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU);
			var msn = consol.Logs.MostRecentLogByPostedTime(Events.MessageSent);

			AssertNull(ocb);
			AssertNotNull(msn);
			AssertEquals("|ACT=Email|CMP=9001|DEP=Carrier|MST=Booking Request", msn.SL_Reference);
		}

		#endregion
	}
}
