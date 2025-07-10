using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	public class PublishHouseBillCommandTest : TestCaseWithFactory
	{
		public void TestPublishHouseBillCommand()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.IsEditingElectronicBOL = false;
			shipment.JS_ElectronicBillOfLadingStatus = ZString.Empty;

			var publishHouseBillCommand = new PublishHouseBillCommand(shipment);

			AssertEquals(CommandIds.SendMessage, publishHouseBillCommand.Id);
			AssertEquals("Publish", publishHouseBillCommand.Caption);
			Assert(!publishHouseBillCommand.IsVisible);
			Assert(publishHouseBillCommand.IsEnabled);

			foreach (var code in new[] {
				FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillPublished,
				FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication,
				FreightConstants.BillOfLadingBillStatus.Codes.Surrendered,
				FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper,
				FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred
			})
			{
				shipment.IsEditingElectronicBOL = true;
				shipment.JS_ElectronicBillOfLadingStatus = code;

				publishHouseBillCommand = new PublishHouseBillCommand(shipment);

				Assert(publishHouseBillCommand.IsVisible);
				Assert(!publishHouseBillCommand.IsEnabled);
			}

			shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
			publishHouseBillCommand = new PublishHouseBillCommand(shipment);

			Assert(publishHouseBillCommand.IsVisible);
			Assert(publishHouseBillCommand.IsEnabled);
		}

		public void TestInvoke_SendMessage_Original()
		{
			var query = new ZQuery();
			query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.SupplyChainLogistics);
			query.AddToFilter(RefDocTypeSchema.RT_DocType, "SEHB");

			var sEHBDocType = Factory.LoadTop1<RefDocType>(query);
			if (sEHBDocType != null)
			{
				sEHBDocType.RT_LogSystemCreatedDocsToEDocs = false;
				Factory.Save();
			}

			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.ConsigneePK = org.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;

				shipment.HolderDocAddress.OrganisationPK = org.PK;
				shipment.SurrenderPartyDocAddress.OrganisationPK = org.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;

				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingTerms = Core.Constants.BillOfLadingBillTerms.Codes.Transferable;
				shipment.JS_ElectronicBillOfLadingVersion = (ZShort)0;

				Factory.Save();

				(var documentInfo, var notificationService) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);

				command.NotifyDocumentInfoCreated(documentInfo.Object);
				var result = command.Invoke();

				AssertEquals("result", true, result);

				var messages = GetEDIMessages();
				foreach (var message in messages)
				{
					AssertNotNull("Message was created", message);
					AssertEquals("EM_ApplicationCode", "UDM", message.EM_ApplicationCode);
					AssertEquals("EM_MessageType", "XDC", message.EM_MessageType);
					AssertEquals("EM_MessageSubType", "XUS", message.EM_MessageSubType);
					AssertEquals("EM_Status", "SNT", message.EM_Status);
					AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);

					var messageXml = XDocument.Parse(message.EM_MessageText);
					var xmlNamespace = messageXml.Root.GetDefaultNamespace();

					if (xmlNamespace.NamespaceName.Contains(DocDataConstants.XmlNamespaces.EHBL))
					{
						Assert("Purpose", message.EM_MessageText.Contains("<Purpose>ORG</Purpose>"));
						Assert("DocumentName", message.EM_MessageText.Contains("<DocumentName>Electronic House Bill</DocumentName>"));
					}
					else
					{
						Assert(xmlNamespace.NamespaceName.Contains(DocDataConstants.XmlNamespaces.BLData));
					}
				}

				AssertEquals("command is disabled after success", false, command.IsEnabled);
				AssertEquals("command is visible after success", true, command.IsVisible);

				AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication, shipment.JS_ElectronicBillOfLadingStatus);
				AssertEquals((ZShort)1, shipment.JS_ElectronicBillOfLadingVersion);

				var recentMessageSentLog = documentData.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.MessageSentCode).OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();
				AssertEquals("|ACT=Original|DEP=Title Registry|MST=Electronic House Bill|TYP=Original Bill Sent for Publication", recentMessageSentLog.SL_Reference);
			}
		}

		public void TestInvoke_SendMessage_Amendment()
		{
			var query = new ZQuery();
			query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.SupplyChainLogistics);
			query.AddToFilter(RefDocTypeSchema.RT_DocType, "SEHB");

			var sEHBDocType = Factory.LoadTop1<RefDocType>(query);
			if (sEHBDocType != null)
			{
				sEHBDocType.RT_LogSystemCreatedDocsToEDocs = false;
				Factory.Save();
			}

			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.ConsigneePK = org.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;

				shipment.HolderDocAddress.OrganisationPK = org.PK;
				shipment.SurrenderPartyDocAddress.OrganisationPK = org.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;

				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingTerms = Core.Constants.BillOfLadingBillTerms.Codes.Transferable;
				shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				shipment.JS_ElectronicBillOfLadingVersion = (ZShort)1;

				Factory.Save();

				(var documentInfo, var notificationService) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);

				command.NotifyDocumentInfoCreated(documentInfo.Object);
				var result = command.Invoke();

				AssertEquals("result", true, result);

				var messages = GetEDIMessages();

				foreach (var message in messages)
				{
					AssertNotNull("Message was created", message);
					AssertEquals("EM_ApplicationCode", "UDM", message.EM_ApplicationCode);
					AssertEquals("EM_MessageType", "XDC", message.EM_MessageType);
					AssertEquals("EM_MessageSubType", "XUS", message.EM_MessageSubType);
					AssertEquals("EM_Status", "SNT", message.EM_Status);
					AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);

					var messageXml = XDocument.Parse(message.EM_MessageText);
					var xmlNamespace = messageXml.Root.GetDefaultNamespace();

					if (xmlNamespace.NamespaceName.Contains(DocDataConstants.XmlNamespaces.EHBL))
					{
						Assert("Purpose", message.EM_MessageText.Contains("<Purpose>AMD</Purpose>"));
						Assert("DocumentName", message.EM_MessageText.Contains("<DocumentName>Electronic House Bill</DocumentName>"));
					}
					else
					{
						Assert(xmlNamespace.NamespaceName.Contains(DocDataConstants.XmlNamespaces.BLData));
					}
				}

				AssertEquals("command is disabled after success", false, command.IsEnabled);
				AssertEquals("command is visible after success", true, command.IsVisible);

				AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication, shipment.JS_ElectronicBillOfLadingStatus);
				AssertEquals((ZShort)2, shipment.JS_ElectronicBillOfLadingVersion);

				var recentMessageSentLog = documentData.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.MessageSentCode).OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();
				AssertEquals("|ACT=Amendment|DEP=Title Registry|MST=Electronic House Bill|TYP=Original Bill Sent for Publication", recentMessageSentLog.SL_Reference);
			}
		}

		public void TestInvoke_WhenCanNotSendMessage()
		{
			(var documentInfo, var notificationService) = CreateDocumentInfo(canSendMessage: false, hasChanges: false, hasErrors: false, hasMessageErrors: false);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			var result = command.Invoke();

			AssertMessageNotCreated(notificationService, result, title: "When Can Not Send Message", messageNotification: null);
			AssertNullOrEmpty(shipment.JS_ElectronicBillOfLadingStatus);
		}

		public void TestInvoke_WhenHasChanged()
		{
			(var documentInfo, var notificationService) = CreateDocumentInfo(canSendMessage: true, hasChanges: true, hasErrors: false, hasMessageErrors: false);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			var result = command.Invoke();

			AssertMessageNotCreated(notificationService, result, title: "When Has Message Errors", messageNotification: "Please save all changes before sending.");
			AssertNullOrEmpty(shipment.JS_ElectronicBillOfLadingStatus);
		}

		public void TestInvoke_CheckElectronicBOLMinimumRequirements()
		{
			(var documentInfo, var notificationService) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: true);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			var result = command.Invoke();

			AssertMessageNotCreated(notificationService, result, title: "CheckElectronicBOLMinimumRequirements", messageNotification: "There are validation errors on this form due to missing mandatory information. Please correct these errors before publishing.");
			AssertNullOrEmpty(shipment.JS_ElectronicBillOfLadingStatus);
		}

		public void TestCopyToEDocs()
		{
			var query = new ZQuery();
			query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.SupplyChainLogistics);
			query.AddToFilter(RefDocTypeSchema.RT_DocType, "SEHB");

			var sEHBDocType = shipment.Factory.LoadTop1<RefDocType>(query);
			if (sEHBDocType == null)
			{
				sEHBDocType = Factory.NewWithValidTestData<RefDocType>();
				sEHBDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
				sEHBDocType.RT_DocType = "SEHB";
			}

			sEHBDocType.RT_LogSystemCreatedDocsToEDocs = true;

			Factory.Save();

			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.ConsigneePK = org.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;

				shipment.HolderDocAddress.OrganisationPK = org.PK;
				shipment.SurrenderPartyDocAddress.OrganisationPK = org.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;

				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingTerms = Core.Constants.BillOfLadingBillTerms.Codes.Transferable;
				shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				shipment.JS_ElectronicBillOfLadingVersion = (ZShort)1;

				shipment.JS_HouseBill = "S00024121601";
				shipment.JS_ElectronicBillOfLadingHouseBill = "S000241216011";

				Factory.Save();

				(var documentInfo, var notificationService) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);

				command.NotifyDocumentInfoCreated(documentInfo.Object);
				var result = command.Invoke();

				AssertEquals("result", true, result);

				var printJobsQuery = new ZQuery();
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobShipmentSchema.Constants.TableName);
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, shipment.PK);

				var printJobs = Factory.Load<StmPrintJob>(printJobsQuery);

				AssertEquals("Document was added to eDocs", 1, printJobs.Length);
				AssertEquals("S000241216011_2.XLS", printJobs[0].SP_EmailAttachments);
				AssertEquals("SEHB", printJobs[0].SP_DocumentType);
			}
		}

		public void TestShowMessageErrorsBeforePublishing()
		{
			var query = new ZQuery();
			query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.SupplyChainLogistics);
			query.AddToFilter(RefDocTypeSchema.RT_DocType, "SEHB");

			var sEHBDocType = shipment.Factory.LoadTop1<RefDocType>(query);
			if (sEHBDocType == null)
			{
				sEHBDocType = Factory.NewWithValidTestData<RefDocType>();
				sEHBDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
				sEHBDocType.RT_DocType = "SEHB";
			}

			sEHBDocType.RT_LogSystemCreatedDocsToEDocs = true;

			Factory.Save();

			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.HolderDocAddress.OrganisationPK = org.PK;
				shipment.SurrenderPartyDocAddress.OrganisationPK = org.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;

				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingTerms = Core.Constants.BillOfLadingBillTerms.Codes.Transferable;
				shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				shipment.JS_ElectronicBillOfLadingVersion = (ZShort)1;

				shipment.JS_HouseBill = "S00024121601";
				shipment.JS_ElectronicBillOfLadingHouseBill = "S000241216011";

				Factory.Save();

				(var documentInfo, var notificationService) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false, hasPublishingMessageErrors: true);

				command.NotifyDocumentInfoCreated(documentInfo.Object);
				var result = command.Invoke();

				AssertEquals("result", false, result);
				notificationService.Verify(s => s.ShowMessage("This document contains message errors. Please fix all message errors before publishing.", "Publishing"), Times.Once);
			}
		}

		void AssertMessageNotCreated(Mock<IUserNotificationService> notificationService, bool result, string title, string messageNotification)
		{
			CombineAssertions(title, () =>
			{
				AssertEquals("result", false, result);
				if (messageNotification != null)
				{
					notificationService.Verify(s => s.ShowMessage(messageNotification, "Sending Message"), Times.Once);
				}
				var messages = GetEDIMessages();

				Assert("Message was not created", !messages.Any());
			});
		}

		(Mock<IDocumentInfo>, Mock<IUserNotificationService>) CreateDocumentInfo(bool canSendMessage, bool hasChanges, bool hasErrors, bool hasMessageErrors, bool hasPublishingMessageErrors = false)
		{
			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			var notificationService = new Mock<IUserNotificationService>();

			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			document.SetupGet(d => d.DataContext).Returns(DataContext.CargoControlAndTransitHouseManifest);
			document.SetupGet(d => d.Margins).Returns(new Margins());
			document.SetupGet(d => d.PageDimensions).Returns(new PageDimensions());
			document.SetupGet(d => d.Rows).Returns(new List<IRow>());
			document.SetupGet(d => d.Columns).Returns(new List<IColumn>());

			securityService.SetupGet(ss => ss.CanSendMessage).Returns(canSendMessage);

			dynamicData.SetupGet(d => d.HasChanges).Returns(hasChanges);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IDocumentSecurityService>(securityService.Object);
			services.Register<IUserNotificationService>(notificationService.Object);

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var dataObject = new HouseBillBuilder(shipment, parameters);

			document.SetupGet(di => di.Name).Returns(ShipmentDocumentNames.BillOfLading);
			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			if (hasErrors)
			{
				var documentNotifications = new Notification[] { new Notification(new Mock<INotificationSource>().Object, DocumentVisualizer.Core.NotificationType.Error, "Error") };
				document.SetupGet(d => d.Notifications).Returns(documentNotifications);
			}

			if (hasMessageErrors)
			{
				var documentNotifications = new Notification[] { new Notification(new Mock<INotificationSource>().Object, DocumentVisualizer.Core.NotificationType.MessageError, "Message Error") };
				document.SetupGet(d => d.Notifications).Returns(documentNotifications);
			}

			if (hasPublishingMessageErrors)
			{
				var documentNotifications = new Notification[] { new Notification(new Mock<INotificationSource>().Object, DocumentVisualizer.Core.NotificationType.MessageError, "Publishing") };
				document.SetupGet(d => d.Notifications).Returns(documentNotifications);
			}

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);

			dynamicData.SetupGet(di => di.Value).Returns(dataObject);

			return (documentInfo, notificationService);
		}

		EDIMessage[] GetEDIMessages()
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, VisualizerDocumentData.Schema.TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, documentData.PK);
			return Factory.Load<EDIMessage>(query);
		}

		protected override void SetUp()
		{
			base.SetUp();

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "S0001";
			shipment.IsEditingElectronicBOL = true;

			documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = ShipmentDocumentDataStoreNames.BillOfLading;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_ParentTableCode = shipment.TablePrefix;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "vesselCode";
			vessel.RV_Name = "vesselName";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";
			transport.JW_Vessel = "vesselName";
			transport.JW_VoyageFlight = "voyage1";

			Factory.Save();

			command = new PublishHouseBillCommand(shipment);
		}

		protected ForwardingShipment shipment;
		protected VisualizerDocumentData documentData;
		protected PublishHouseBillCommand command;
	}
}
