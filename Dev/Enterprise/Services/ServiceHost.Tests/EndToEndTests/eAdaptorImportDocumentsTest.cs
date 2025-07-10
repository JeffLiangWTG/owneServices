using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class eAdaptorImportDocumentsTest : eAdaptorEndToEndTest
	{
		const string imageData = "U2lubmUgRmlhbm5hIEbDoWlsLA0KYXTDoSBmYW9pIGdoZWFsbCBhZyDDiWlyaW5uLA0KQnXDrW9uIGTDoXIgc2x1YQ0KdGhhciB0b2lubiBkbyByw6FpbmlnIGNodWdhaW5uLA0KRmFvaSBtaMOzaWQgYmhlaXRoIHNhb3INClNlYW50w61yIMOhciBzaW5zZWFyIGZlYXN0YSwNCk7DrSBmaMOhZ2ZhciBmYW9pbiB0w61vcsOhbiBuw6EgZmFvaW4gdHLDoWlsbC4NCkFub2NodCBhIHRow6lhbSBzYSBiaGVhcm5hIGJoYW9pbCwNCkxlIGdlYW4gYXIgR2hhZWlsLCBjaHVuIGLDoWlzIG7DsyBzYW9pbCwNCkxlIGd1bm5hLXNjcsOpYWNoIGZhb2kgbMOhbWhhY2ggbmEgYnBpbMOpYXIsDQpTZW8gbGliaCBjYW5haWcgYW1ocsOhbiBuYSBiaEZpYW5uLg==";

		List<AttachedDocument> GetExampleAttachedDocumentCollection()
		{
			var doc1 = AttachedDocumentCreator.Create(
				fileName: "CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf",
				base64ImageData: imageData,
				documentType: "CAD"
			);
			var doc2 = AttachedDocumentCreator.Create(
				fileName: "CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf",
				base64ImageData: imageData,
				documentType: "PDF"
			);

			return new List<AttachedDocument> { doc1, doc2 };
		}

		public void TestImportAttachmentDocumentCollection_UniversalEvent()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, bizo.JS_UniqueConsignRef, companyCode: "EDI");
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.DocumentImportedCode);
			universalEvent.AttachedDocumentCollection = GetExampleAttachedDocumentCollection();

			AssertImportAttachedDocumentCollection(bizo as BusinessObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent, universalEvent);
		}

		public void TestImportAttachmentDocumentCollection_XUS_QuotedBooking()
		{
			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			bizo.JS_BookingReference = "BLATTICUS";
			bizo.JS_IsBooking = true;
			bizo.JS_IsForwardRegistered = false;
			Factory.Save();
			var booking = Factory.Load<IQuotedBooking>(bizo.PK);

			ImportDocumentsViaXUS(booking as BusinessObject, DataContextType.ForwardingBooking, bizo.JS_UniqueConsignRef);
		}

		public void TestImportAttachmentDocumentCollection_XUS_Shipment()
		{
			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			ImportDocumentsViaXUS(bizo as BusinessObject, DataContextType.ForwardingShipment, bizo.JS_UniqueConsignRef);
		}

		public void TestImportAttachmentDocumentCollection_XUS_Consol()
		{
			var bizo = Factory.New<Forwarding.IForwardingConsol>();
			((BusinessObject)bizo).FillWithValidTestData();
			bizo.JK_UniqueConsignRef = "C00001010";
			Factory.Save();

			ImportDocumentsViaXUS(bizo as BusinessObject, DataContextType.ForwardingConsol, bizo.JK_UniqueConsignRef);
		}

		public void TestImportAttachmentDocumentCollection_XUS_Declaration()
		{
			var bizo = Factory.New<Integration.Customs.IBaseJobDeclaration>();
			((BusinessObject)bizo).FillWithValidTestData();
			Factory.Save();

			ImportDocumentsViaXUS(bizo as BusinessObject, DataContextType.CustomsDeclaration, bizo.JE_DeclarationReference);
		}

		void ImportDocumentsViaXUS(BusinessObject targetBizo, DataContextType dataTargetType, string dataTargetKey)
		{
			var dataContext = DataContextCreator.Create(dataTargetType, dataTargetKey, companyCode: "EDI");
			var shipment = UniversalShipmentCreator.Create(dataContext);
			shipment.SetAttachedDocumentCollection(GetExampleAttachedDocumentCollection);

			AssertImportAttachedDocumentCollection(targetBizo, EDIMessageSubTypeList.Codes.XmlUniversalShipment, shipment);
		}

		public void TestImportAttachmentDocumentFileSize_UniversalEvent()
		{
			var testOrgHeader = Factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
			var testAddress = testOrgHeader.Addresses.AddNew();
			testAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			testAddress.OA_Address1 = "Address1";
			testAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

			Factory.Save();

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentBizo = (BusinessObject)shipment;
			shipment.JS_UniqueConsignRef = "S00001001";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = testOrgHeader.MainAddress.PK;
			shipmentBizo.FillWithValidTestData();

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "DEEZ";
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
			triggerAction.PQ_Calc_TriggerParty = "OTH";
			triggerAction.PQ_OH_Recipient = testOrgHeader.PK;
			triggerAction.PQ_TriggerParty = "ACR";

			Factory.Save();

			//Add EdiCommunication Module
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://");

			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_Module = "SHP";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			communicationsMode.EK_Destination = "UNVRSLVNT";
			communicationsMode.EK_PublishInternalMilestones = true;
			testOrgHeader.EDICommunicationsModes.Add(communicationsMode);

			Factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, companyCode: "EDI");
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.DocumentImportedCode);
			universalEvent.SetAttachedDocumentCollection(GetExampleAttachedDocumentCollection);
			var responseBody = PostRequest(universalEvent);
			AssertContains("Successfully Added eDoc", responseBody);

			using (Env.Instance.SuppressSwitchContextCheck())
			{
				//Run Logwalker
				var res = MasterFilesTestHelper.RunLogWalker();

				//Check stmALog for DEX
				shipment = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
				var logs = ((IWorkflowProvider)shipment).Logs.Find(l => l.SL_SE_NKEvent == "DEX").ToList();
				AssertEquals(2, logs.Count);

				for (int i = 0; i < 2; i++)
				{
					using (var relatedEDIMessage = logs[i].RelatedEDIMessage)
					{
						var messageText = relatedEDIMessage.Message.EM_MessageText;
						AssertContains(imageData, messageText);
						AssertContains("<FileSizeInBytes>364</FileSizeInBytes>", messageText);
					}
				}
			}
		}

		public void TestVisibleCodeFields()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";

			var company = factory.New<GlbCompany>();
			company.GC_Code = "DEC";

			var branch = factory.New<GlbBranch>();
			branch.GB_Code = "DEB";
			branch.GB_GC = company.PK;

			var department = factory.New<GlbDepartment>();
			department.GE_Code = "DED";

			factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, bizo.JS_UniqueConsignRef, companyCode: "GB1");
			var attachedDocument = AttachedDocumentCreator.Create(
				fileName: "DOCPACK CONFIRMATION - 01680415.txt",
				base64ImageData: imageData,
				documentType: "DP2",
				isPublished: false
			);
			attachedDocument.VisibleBranchCode = branch.GB_Code;
			attachedDocument.VisibleCompanyCode = company.GC_Code;
			attachedDocument.VisibleDepartmentCode = department.GE_Code;
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.DocumentImportedCode);
			UniversalEventCreator.SetAttachedDocumentCollection(universalEvent, attachedDocument);

			var responseBody = PostRequest(universalEvent);
			AssertContains("Successfully Added eDoc", responseBody);

			dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, bizo.JS_UniqueConsignRef, companyCode: GlbCompany.CurrentCompany.GC_Code);
			var documentRequest = UniversalDocumentRequestCreator.Create(dataContext);
			var universalResponse = eAdaptorTestHelper.PostRequestWithUniversalResponse<UniversalDataBuss.DataObjects.Universal.Event>(documentRequest);

			AssertEquals(1, universalResponse.ResponseDataObject?.AttachedDocumentCollection?.Count);
			using (attachedDocument = universalResponse.ResponseDataObject.AttachedDocumentCollection.First())
			{
				AssertEquals(branch.GB_Code, attachedDocument.VisibleBranchCode);
				AssertEquals(company.GC_Code, attachedDocument.VisibleCompanyCode);
				AssertEquals(department.GE_Code, attachedDocument.VisibleDepartmentCode);
			}
		}

		public void TestImportValidatesFileExtensions()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";

			factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, bizo.JS_UniqueConsignRef);
			var attachedDocument1 = AttachedDocumentCreator.Create(
				fileName: "DELETE SYSTEM32.exe",
				base64ImageData: imageData,
				documentType: "PDF",
				isPublished: false
			);
			var attachedDocument2 = AttachedDocumentCreator.Create(
							fileName: "DONT DELETE SYSTEM32.txt",
							base64ImageData: imageData,
							documentType: "PDF",
							isPublished: false
						);
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.DocumentImportedCode);
			UniversalEventCreator.SetAttachedDocumentCollection(universalEvent, attachedDocument1, attachedDocument2);

			var responseBody = eAdaptorTestHelper.PostRequestWithUniversalResponse<UniversalDataBuss.DataObjects.Universal.Event>(universalEvent);

			AssertContains("dangerous file type", responseBody.ProcessingLog);

			var reloadBizo = new BusinessObjectFactory().Load(bizo.GetType(), bizo.PK);
			var savedEDocs = ((IDocManagerSupport)reloadBizo).DocManagerInfo.Files.Cast<IeDoc>();
			AssertEquals(0, savedEDocs.Count());
		}

		void AssertImportAttachedDocumentCollection(BusinessObject bizo, string messageType, ITopLevelDataObject dataObject)
		{
			var responseBody = PostRequest(dataObject);

			AssertContains("Successfully Added eDoc", responseBody);

			var reloadBizo = new BusinessObjectFactory().Load(bizo.GetType(), bizo.PK);
			var savedEDocs = ((IDocManagerSupport)reloadBizo).DocManagerInfo.Files.Cast<IeDoc>();
			AssertEquals(2, savedEDocs.Count());

			var eDoc1 = savedEDocs.FirstOrDefault(e => e.FileName == "CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf");
			var eDoc2 = savedEDocs.FirstOrDefault(e => e.FileName == "CIV-Commercial_Invoice-DOUCMENT_NUMBER[2].pdf");
			AssertNotNull(eDoc1);
			AssertNotNull(eDoc2);

			var ddiLogs = reloadBizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.DocumentImportedCode);
			var ddi1 = ddiLogs.FirstOrDefault(l => l.SL_Reference == eDoc1.DocType + "|" + eDoc1.UniqueKey.ToString());
			var ddi2 = ddiLogs.FirstOrDefault(l => l.SL_Reference == eDoc2.DocType + "|" + eDoc2.UniqueKey.ToString());
			AssertNotNull("Expecting one DDI event per document", ddi1);
			AssertNotNull("Expecting one DDI event per document", ddi2);

			using (var linkedMessage1 = ddi1.RelatedEDIMessage)
			using (var linkedMessage2 = ddi2.RelatedEDIMessage)
			{
				AssertEquals("DDI event should be linked to message", messageType, linkedMessage1.Message.EM_MessageSubType);
				AssertEquals("DDI event should be linked to message", messageType, linkedMessage2.Message.EM_MessageSubType);
			}
		}
	}
}
