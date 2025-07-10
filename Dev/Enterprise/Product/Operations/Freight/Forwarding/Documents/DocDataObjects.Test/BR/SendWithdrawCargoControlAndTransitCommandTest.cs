using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.Testing.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using INotificationSource = Enterprise.DocumentVisualizer.Core.INotificationSource;
using Notification = Enterprise.DocumentVisualizer.Core.Notification;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR.Testing
{
	sealed class SendWithdrawCargoControlAndTransitCommandTest : TestCaseWithFactory
	{
		public void TestSendWithdrawal()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			Helper.CreateNewCCTPassword();
			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var documentData = CreateDocumentDataAttachedToConsol();
			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();
			notifications.Setup(s => s.QueryUserResponse(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>())).Returns("withdrawal reason");

			var cctHouseManifest = CreateCCTHouseManifest();
			var documentInfo = CreateDocumentInfoForCCTHouseManifest(cctHouseManifest, documentData, notifications);

			var command = new SendWithdrawCargoControlAndTransitCommand();

			if (!InitializeCommand(command, documentInfo))
			{
				Fail("Failed to initialize CancelCCTHouseManifestCommand");
			}

			Assert(command.IsEnabled);

			var commandParameters = new MacroMap(new Dictionary<string, object>());
			Assert("expected command to execute successfully", command.Invoke(commandParameters));

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull("message was created", message);
			AssertContains("message should have WTH purpose", "<Purpose>WTH</Purpose>", message.EM_MessageText);
			Assert(!command.IsEnabled);
		}

		public void TestCheckHasChanges()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			Helper.CreateNewCCTPassword();
			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var documentData = CreateDocumentDataAttachedToConsol();
			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var cctHouseManifest = CreateCCTHouseManifest();
			var documentInfo = CreateDocumentInfoForCCTHouseManifest(cctHouseManifest, documentData, notifications, dataHasChanges: true);

			var command = new SendWithdrawCargoControlAndTransitCommand();

			if (!InitializeCommand(command, documentInfo))
			{
				Fail("Failed to initialize CancelCCTHouseManifestCommand");
			}

			var commandParameters = new MacroMap(new Dictionary<string, object>());
			Assert(!command.Invoke(commandParameters));
			notifications.Verify(s => s.ShowMessage("Please save changes before sending message.", "Unable to send message"), Times.Once);
		}

		public void TestCheckHasMessageErrors()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			Helper.CreateNewCCTPassword();
			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var documentData = CreateDocumentDataAttachedToConsol();
			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var documentNotifications = new Notification[] { new Notification(new Mock<INotificationSource>().Object, NotificationType.MessageError, "Message Error") };
			var cctHouseManifest = CreateCCTHouseManifest();
			var documentInfo = CreateDocumentInfoForCCTHouseManifest(cctHouseManifest, documentData, notifications, documentNotifications);

			var command = new SendWithdrawCargoControlAndTransitCommand();

			if (!InitializeCommand(command, documentInfo))
			{
				Fail("Failed to initialize CancelCCTHouseManifestCommand");
			}

			var commandParameters = new MacroMap(new Dictionary<string, object>());
			Assert(!command.Invoke(commandParameters));
			notifications.Verify(s => s.ShowMessage("This document contains message errors. Please fix all message errors before sending.", "Unable to send message"), Times.Once);
		}

		public void TestCheckHasErrors()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			Helper.CreateNewCCTPassword();
			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var documentData = CreateDocumentDataAttachedToConsol();
			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var documentNotifications = new Notification[] { new Notification(new Mock<INotificationSource>().Object, NotificationType.Error, "Error") };
			var cctHouseManifest = CreateCCTHouseManifest();
			var documentInfo = CreateDocumentInfoForCCTHouseManifest(cctHouseManifest, documentData, notifications, documentNotifications: documentNotifications);

			var command = new SendWithdrawCargoControlAndTransitCommand();

			if (!InitializeCommand(command, documentInfo))
			{
				Fail("Failed to initialize CancelCCTHouseManifestCommand");
			}

			var commandParameters = new MacroMap(new Dictionary<string, object>());
			Assert(!command.Invoke(commandParameters));
			notifications.Verify(s => s.ShowMessage("This document contains errors. Please fix all errors before sending.", "Unable to send message"), Times.Once);
		}

		public void TestSaveCopyToEDocs()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			Helper.CreateNewCCTPassword();
			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var documentData = CreateDocumentDataAttachedToConsol();
			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();
			notifications.Setup(s => s.QueryUserResponse(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>())).Returns("withdrawal reason");

			var cctHouseManifest = CreateCCTHouseManifest();
			var documentInfo = CreateDocumentInfoForCCTHouseManifest(cctHouseManifest, documentData, notifications);

			var command = new SendWithdrawCargoControlAndTransitCommand();

			if (!InitializeCommand(command, documentInfo))
			{
				Fail("Failed to initialize CancelCCTHouseManifestCommand");
			}

			var commandParameters = new MacroMap(new Dictionary<string, object>());
			Assert("expected command to execute successfully", command.Invoke(commandParameters));

			var printJobsQuery = new ZQuery();
			printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobConsolSchema.Constants.TableName);
			printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, documentData.Parent.PK);

			var printJobs = Factory.Load<StmPrintJob>(printJobsQuery);

			AssertEquals("Document was added to eDocs", 1, printJobs.Length);
			AssertEquals("Eagle Datamation International - BN - AUBNE - CCT House Manifest (1)", printJobs[0].SP_EmailSubjectLine);
		}

		#region Implementation

		CargoControlAndTransitHouseManifest CreateCCTHouseManifest()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment1 = consol.Shipments.AddNew();
			PopulateShipment(shipment1, "081001", 12, 25, "goods1");

			var shipment2 = consol.Shipments.AddNew();
			PopulateShipment(shipment2, "081002", 14, 27, "goods2");

			var cctHouseManifest = new CargoControlAndTransitHouseManifestBuilder(consol).Build();

			return cctHouseManifest;
		}

		VisualizerDocumentData CreateDocumentDataAttachedToConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var documentData = Factory.New<VisualizerDocumentData>();

			documentData.Parent = consol;
			documentData.JDD_Name = DataContext.CargoControlAndTransitHouseManifest;

			CreateLogs(documentData, Events.DataExport, Events.MessageSent, Events.MessageAccepted);

			return documentData;
		}

		IDocumentInfo CreateDocumentInfoForCCTHouseManifest(CargoControlAndTransitHouseManifest cctHouseManifest, VisualizerDocumentData documentData, Mock<IUserNotificationService> notifications, Notification[] documentNotifications = null, bool dataHasChanges = false)
		{
			var mockData = new Mock<IDynamicData>();
			mockData.SetupGet(d => d.Value).Returns(cctHouseManifest);
			mockData.SetupGet(d => d.HasChanges).Returns(dataHasChanges);

			var mockDocument = new Mock<IDocument>();
			mockDocument.SetupGet(d => d.Data).Returns(mockData.Object);
			mockDocument.SetupGet(d => d.DataContext).Returns(DataContext.CargoControlAndTransitHouseManifest);
			mockDocument.SetupGet(d => d.Margins).Returns(new Margins());
			mockDocument.SetupGet(d => d.PageDimensions).Returns(new PageDimensions());
			mockDocument.SetupGet(d => d.Rows).Returns(new List<IRow>());
			mockDocument.SetupGet(d => d.Columns).Returns(new List<IColumn>());

			if (documentNotifications != null)
			{
				mockDocument.SetupGet(d => d.Notifications).Returns(documentNotifications);
			}

			var mockMessageInstructions = new Mock<IMessageInstructions>();

			var printInstructions = new Mock<IPrintInstructions>();
			printInstructions.SetupGet(pi => pi.Title).Returns("1");

			var mockEDocsInstructions = new Mock<IEDocsInstructions>();
			mockEDocsInstructions.SetupGet(i => i.SaveCopyToEDocs).Returns(true);
			mockEDocsInstructions.SetupGet(i => i.Parent).Returns(documentData.Parent);

			var mockDocumentDescriptor = new Mock<IDocumentDescriptor>();
			mockDocumentDescriptor.SetupGet(d => d.MessageInstructions).Returns(mockMessageInstructions.Object);
			mockDocumentDescriptor.SetupGet(d => d.EDocsInstructions).Returns(mockEDocsInstructions.Object);
			mockDocumentDescriptor.SetupGet(di => di.Name).Returns("CCT House Manifest");
			mockDocumentDescriptor.SetupGet(di => di.DocumentType).Returns("BKC");
			mockDocumentDescriptor.SetupGet(di => di.PrintInstructions).Returns(printInstructions.Object);
			mockDocumentDescriptor.SetupGet(di => di.EDocsInstructions).Returns(mockEDocsInstructions.Object);

			var mockSecurityService = new Mock<IDocumentSecurityService>();
			mockSecurityService.SetupGet(s => s.CanSendMessage).Returns(true);

			var mockDeliveryService = new Mock<IDocumentDeliveryService>();
			mockDeliveryService
				.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.IsAny<INotifications>()))
				.Returns(CreateEdiCommunicationSettings());

			var services = new ServiceContainer();
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IDocumentSecurityService>(mockSecurityService.Object);
			services.Register<IDocumentDeliveryService>(mockDeliveryService.Object);

			var mockDocumentInfo = new Mock<IDocumentInfo>();
			mockDocumentInfo.SetupGet(d => d.Descriptor).Returns(mockDocumentDescriptor.Object);
			mockDocumentInfo.SetupGet(d => d.Document).Returns(mockDocument.Object);
			mockDocumentInfo.SetupGet(d => d.DocumentData).Returns(documentData);
			mockDocumentInfo.SetupGet(d => d.Services).Returns(services);

			return mockDocumentInfo.Object;
		}

		IEDICommunicationSettings CreateEdiCommunicationSettings()
		{
			var mockEDICommunicationsMode = new Mock<IEDICommunicationsMode>();
			mockEDICommunicationsMode.SetupGet(s => s.EK_CommunicationsTransport).Returns("HUB");
			mockEDICommunicationsMode.SetupGet(s => s.EK_Destination).Returns("Valhalla");

			var mockEDICommunicationSettings = new Mock<IEDICommunicationSettings>();
			mockEDICommunicationSettings
				.SetupGet(s => s.Recipient)
				.Returns(new ZArchitecture.Core.CodeDescriptionPair("foor", "baar"));

			mockEDICommunicationSettings
				.SetupGet(s => s.Purpose)
				.Returns(new ZArchitecture.Core.CodeDescriptionPair("for", "bar"));

			mockEDICommunicationSettings
				.SetupGet(s => s.CommunicationsModes)
				.Returns(new[] { mockEDICommunicationsMode.Object });

			return mockEDICommunicationSettings.Object;
		}

		bool InitializeCommand(SendWithdrawCargoControlAndTransitCommand command, IDocumentInfo documentInfo)
		{
			if (command is INotifiableDocumentInfoCreated notifiableDocumentInfoCreated)
			{
				notifiableDocumentInfoCreated.NotifyDocumentInfoCreated(documentInfo);
				return true;
			}

			return false;
		}

		void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "ReceivingForwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BRSAO";
			receivingForwarder.MainAddress.Address1 = "Av Paulista 291";
			receivingForwarder.MainAddress.Address2 = "Consolacao";
			receivingForwarder.MainAddress.City = "Sao Paulo";
			receivingForwarder.MainAddress.Postcode = "11157802";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SendingForwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BRSAO";
			sendingForwarder.MainAddress.Address1 = "Av Paulista 291";
			sendingForwarder.MainAddress.Address2 = "Consolacao";
			sendingForwarder.MainAddress.City = "Salvador";
			sendingForwarder.MainAddress.Postcode = "11157802";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BR";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "BRAQA";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg2.JW_RL_NKLoadPort = "BRAQA";
			transportLeg2.JW_RL_NKDiscPort = "BRGRU";

			var transportLeg3 = consol.Transports.AddNew();
			transportLeg3.JW_LegOrder = 3;
			transportLeg3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg3.JW_RL_NKLoadPort = "BRGRU";
			transportLeg3.JW_RL_NKDiscPort = "BRSAO";
			transportLeg3.JW_ETD = ZDateTime.Today;
		}

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString desc)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "BRSAO";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "BR";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
		}

		void CreateLogs(IStmALogParent parent, params Event[] events)
		{
			foreach (var @event in events)
			{
				var log = parent.Logs.AddNew(@event);

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					var parameterName = log.GetParameterNameWhichStoresDocumentName();
					log.Parameters.Add(
						new KeyValuePair<string, string>(parameterName, null));
				}
			}

			Factory.Save();
			Thread.Sleep(10);
		}

		CargoControlAndTransitMessagingExtensionsTestHelper Helper => helper ?? (helper = new CargoControlAndTransitMessagingExtensionsTestHelper(Factory));
		CargoControlAndTransitMessagingExtensionsTestHelper helper;

		#endregion
	}
}
