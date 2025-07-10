using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters;
using Enterprise.Freight.Forwarding.Documents.Testing.GUI.DocSending;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using Money = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Money;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.Testing.Supporters
{
	sealed class OpenCOOrderFormCommandTest : TestCaseWithFactory
	{
		const string CertificateOfOriginNumber = "62.000000000.2024.000111";

		public void Test_CommandId()
		{
			var command = new OpenCOOrderFormCommand();

			AssertEquals(CommandIds.SendMessage, command.Id);
		}

		public void Test_Caption()
		{
			var command = new OpenCOOrderFormCommand();

			AssertEquals("Open CO Order Form", command.Caption);
		}

		public void Test_IsEnabled()
		{
			var command = new OpenCOOrderFormCommand();

			AssertEquals(true, command.IsEnabled);
		}

		public void Test_OpenCOOrderFormOnInvoke_HasMessageErrors_DoNotOpenDoc_ShowMessage()
		{
			var mockedDocumentInfo = new Mock<IDocumentInfo>();
			var mockedDocument = new Mock<IDocument>();
			var mockedDynamicData = new Mock<IDynamicData>();
			var mockedServiceContainer = new Mock<IServiceContainer>();
			var mockedUserNotificationService = new Mock<IUserNotificationService>();

			var nzcfta = new NZCFTA("ForwardingShipment", "C00001015");
			nzcfta.CurrentUserErrorInfo.AddMessageErrorIfEmpty("Test message 1");
			nzcfta.RemarksInfo.AddMessageErrorIfEmpty("Test message 2");
			nzcfta.RemarksInfo.AddMessageErrorIfEmpty("Test message 2"); // Check to see duplicate message errors are not logged

			mockedDocumentInfo
				.SetupGet(i => i.Document)
				.Returns(mockedDocument.Object);

			mockedDocumentInfo
				.SetupGet(i => i.Services)
				.Returns(mockedServiceContainer.Object);

			mockedDocument
				.SetupGet(i => i.Data)
				.Returns(mockedDynamicData.Object);

			mockedDynamicData
				.SetupGet(i => i.Value)
				.Returns(nzcfta);

			mockedServiceContainer
				.Setup(i => i.Resolve<IUserNotificationService>())
				.Returns(mockedUserNotificationService.Object);

			var command = new OpenCOOrderFormCommand();
			command.NotifyDocumentInfoCreated(mockedDocumentInfo.Object);
			command.Invoke();

			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);

			mockedUserNotificationService.Verify(i => i.ShowMessage("This document contains message errors. Please fix all message errors before sending.\r\n\r\n Test message 1\r\n Test message 2", null));
		}

		public void Test_OpenCOOrderFormOnInvoke_HasErrors_DoNotOpenDoc_ShowMessage()
		{
			var mockedDocumentInfo = new Mock<IDocumentInfo>();
			var mockedDocument = new Mock<IDocument>();
			var mockedDynamicData = new Mock<IDynamicData>();
			var mockedServiceContainer = new Mock<IServiceContainer>();
			var mockedUserNotificationService = new Mock<IUserNotificationService>();

			var nzcfta = new NZCFTA("ForwardingShipment", "C00001015");
			nzcfta.CurrentUserErrorInfo.AddErrorIfEmpty("Test message 1");
			nzcfta.RemarksInfo.AddErrorIfEmpty("Test message 2");
			nzcfta.RemarksInfo.AddErrorIfEmpty("Test message 2"); // Check to see duplicate errors are not logged

			mockedDocumentInfo
				.SetupGet(i => i.Document)
				.Returns(mockedDocument.Object);

			mockedDocumentInfo
				.SetupGet(i => i.Services)
				.Returns(mockedServiceContainer.Object);

			mockedDocument
				.SetupGet(i => i.Data)
				.Returns(mockedDynamicData.Object);

			mockedDynamicData
				.SetupGet(i => i.Value)
				.Returns(nzcfta);

			mockedServiceContainer
				.Setup(i => i.Resolve<IUserNotificationService>())
				.Returns(mockedUserNotificationService.Object);

			var command = new OpenCOOrderFormCommand();
			command.NotifyDocumentInfoCreated(mockedDocumentInfo.Object);
			command.Invoke();

			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);

			mockedUserNotificationService.Verify(i => i.ShowMessage("This document contains errors. Please fix all errors before sending.\r\n\r\n Test message 1\r\n Test message 2", null));
		}

		public void Test_OpenCOOrderFormOnInvoke_HasUnsavedChanges_SaveChanges_OpenDocPopup()
		{
			Assert_HasUnsavedChanges_SaveChanges_OpenDocPopup(documentDataHasChanges: false, dynamicDataHasChanges: true);
			Assert_HasUnsavedChanges_SaveChanges_OpenDocPopup(documentDataHasChanges: true, dynamicDataHasChanges: false);
		}

		public void Assert_HasUnsavedChanges_SaveChanges_OpenDocPopup(bool documentDataHasChanges, bool dynamicDataHasChanges)
		{
			var mockedDocumentInfo = new Mock<IDocumentInfo>();
			var mockedDocument = new Mock<IDocument>();
			var mockedDocumentData = new Mock<IVisualizerDocumentData>();
			var mockedDynamicData = new Mock<IDynamicData>();
			var mockedServiceContainer = new Mock<IServiceContainer>();

			var nzcfta = new NZCFTA("ForwardingShipment", "C00001015");

			mockedDocumentInfo
				.SetupGet(i => i.Document)
				.Returns(mockedDocument.Object);

			mockedDocumentInfo
				.SetupGet(i => i.Services)
				.Returns(mockedServiceContainer.Object);

			mockedDocumentInfo
			.SetupGet(i => i.DocumentData)
			.Returns(mockedDocumentData.Object);

			mockedDocument
				.SetupGet(i => i.Data)
				.Returns(mockedDynamicData.Object);

			mockedDocumentData
				.SetupGet(i => i.HasChanges)
				.Returns(documentDataHasChanges);

			mockedDynamicData
				.SetupGet(i => i.Value)
				.Returns(nzcfta);

			mockedDynamicData
				.SetupGet(i => i.HasChanges)
				.Returns(dynamicDataHasChanges);

			var command = new OpenCOOrderFormCommandForTests();
			command.NotifyDocumentInfoCreated(mockedDocumentInfo.Object);
			command.Invoke();

			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);

			Assert("SaveUnSavedChanges should be called exactly once.", command.CapturedSaveUnSavedChangesCalls.Count == 1);
		}

		public void Test_OpenCOOrderFormOnInvoke_HasNoErrors_DoesOpenDocAndSendMessage()
		{
			// arrange
			var consol = CreateConsol();
			var shipment = CreateShipment();
			consol.Shipments.Add(shipment);
			var documentData = CreateDocumentData(shipment);
			var builder = new NZCFTABuilder(shipment);
			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 1);
			GlbStaff.CurrentUser.GS_EmailAddress = "abc@xyz.com";
			var nzcfta = builder.Build();
			nzcfta.CurrentUser.City = "Galway";
			CreateLineItems(nzcfta);
			var documentInfo = CreateDocumentInfo(nzcfta, documentData);

			var command = new OpenCOOrderFormCommandForTests();

			var notifiableDocumentInfoCreated = command as INotifiableDocumentInfoCreated;
			AssertNotNull("Failed to initialize OpenCOOOrderFormSendMessageCommand", notifiableDocumentInfoCreated);
			notifiableDocumentInfoCreated.NotifyDocumentInfoCreated(documentInfo);

			Factory.Save();

			// act
			Assert("expected command to execute successfully", command.Invoke());

			// assert
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull("message was created", message);
		}

		public void Test_OpenCOOrderFormOnInvoke_HasNoErrors_DoesOpenDocAndSendMessageAmendment()
		{
			// arrange
			var consol = CreateConsol();
			var shipment = CreateShipment();
			consol.Shipments.Add(shipment);
			var documentData = CreateDocumentData(shipment);
			var builder = new NZCFTABuilder(shipment);
			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 1);
			GlbStaff.CurrentUser.GS_EmailAddress = "abc@xyz.com";
			var nzcfta = builder.Build();
			nzcfta.CurrentUser.City = "Galway";
			CreateLineItems(nzcfta);
			var documentInfo = CreateDocumentInfo(nzcfta, documentData);

			// act: initial send
			var command = new OpenCOOrderFormCommandForTests();
			command.NotifyDocumentInfoCreated(documentInfo);
			Factory.Save();

			// assert: initial send
			Assert("expected command to execute successfully", command.Invoke());

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull("message was created", message);

			using (var messageReader = message.GetEM_MessageTextReader())
			{
				using (var usxml = messageReader.Parse<UniversalShipment>())
				{
					AssertEquals("ORG", usxml.DataContext.DocumentaryOverride.Purpose.Code);
					AssertEquals("COO Number must not be set on an original submission", "", usxml.AddInfoCollection.FirstOrDefault(x => x.Key == (ZString?)AddinfoTypes.CertificateOfOriginId)?.Value);
				}
			}

			// act: resend
			foreach (var stmALogParent in new IStmALogParent[] { documentData, shipment })
			{
				stmALogParent.Logs.CreateOrRecreateEventLog(
					Events.MessageAccepted,
					EstimateActual.Actual,
					ZDateTimeOffset.Now,
					string.Empty,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, CertificateOfOriginNumber));
			}

			builder = new NZCFTABuilder(shipment);
			nzcfta = builder.Build();
			nzcfta.CurrentUser.City = "Galway";
			CreateLineItems(nzcfta);
			documentInfo = CreateDocumentInfo(nzcfta, documentData);
			command.NotifyDocumentInfoCreated(documentInfo);
			Factory.Save();

			// assert: resend
			Assert("expected command to execute successfully", command.Invoke());

			message = Factory.Load<EDIMessage>(new ZQuery()).OrderByDescending(x => x.EM_DateTimeInterchangeSent).FirstOrDefault();
			AssertNotNull("message was created", message);

			using (var messageReader = message!.GetEM_MessageTextReader())
			{
				using (var usxml = messageReader.Parse<UniversalShipment>())
				{
					AssertEquals("AMD", usxml.DataContext.DocumentaryOverride.Purpose.Code);
					AssertEquals("COO Number must be set on an amendment", CertificateOfOriginNumber, usxml.AddInfoCollection.FirstOrDefault(x => x.Key == (ZString?)AddinfoTypes.CertificateOfOriginId)?.Value);
				}
			}
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "CNCHA";
			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today.AddDays(1);
			transport.JW_ETA = ZDate.Today.AddDays(5);
			transport.JW_Vessel = "VesselName";

			var leg = consol.Transports.FirstTransportWithTransportModeAndExportVessel(consol.TransportMode);
			leg.JW_VoyageFlight = "ABC";

			return consol;
		}

		public void CreateLineItems(NZCFTA nzcfta)
		{
			var criterionList = new OriginCriterionListNZCFTA();
			var quantityUnitList = new NZCustomsTariffQuantityUnitList();

			nzcfta.LineItems = new[]
			{
				new NZCFTALineItem("b194952d-4732-4d79-a31f-a5eee60f8678")
				{
					Quantity =
						new Measurement
						{
							Value = 1,
							Unit = new DummyCodeDescription
							{
								Code = Core.Constants.Weight.Kilograms
							}
					},
					Invoice = new Invoice()
					{
						Number = "INV0001",
						Date = new ZDateTime(2023, 07, 11),
						Amount = new Money
						{
							Amount = 10.00m,
							Currency = new DummyCodeDescription
							{
								Code = "AUD"
							}
						}
					},
					ItemNumber = 100,
					MarksAndNumbers = "marks & nums (1)",
					GoodsDescription = "goods description (1)",
					QuantityNumber = 10,
					QuantityUnit = new CodeDescription(quantityUnitList)
					{
						Code = NZCustomsTariffQuantityUnitList.Codes.NMP
					},
					OriginCriterion = new CodeDescription(criterionList)
					{
						Code = OriginCriterionListNZCFTA.Codes.WO
					},
					PackageCount = 20
				},
				new NZCFTALineItem("d1893ae0-b09b-436a-adc1-6372291db6b1")
				{
					Quantity =
						new Measurement
						{
							Value = 2,
							Unit = new DummyCodeDescription
							{
								Code = Core.Constants.Weight.Kilograms
							}
					},
					Invoice = new Invoice()
					{
						Number = "INV0002",
						Date = new ZDateTime(2023, 07, 11),
						Amount = new Money
						{
							Amount = 13.37m,
							Currency = new DummyCodeDescription
							{
								Code = "AUD"
							}
						}
					},
					ItemNumber = 200,
					MarksAndNumbers = "marks & nums (2)",
					GoodsDescription = "goods description (2)",
					QuantityNumber = 10,
					QuantityUnit = new CodeDescription(quantityUnitList)
					{
						Code = NZCustomsTariffQuantityUnitList.Codes.NMP
					},
					OriginCriterion = new CodeDescription(criterionList)
					{
						Code = OriginCriterionListNZCFTA.Codes.PSR
					},
					PackageCount = 10,
					PackageType = new DummyCodeDescription
					{
						Code = "PKG",
						Description = "Package"
					}
				}
			};
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

		IDocumentInfo CreateDocumentInfo(NZCFTA nzfta, VisualizerDocumentData documentData)
		{
			var mockData = new Mock<IDynamicData>();
			mockData.SetupGet(d => d.Value).Returns(nzfta);

			var mockDocument = new Mock<IDocument>();
			mockDocument.SetupGet(d => d.Name).Returns(ShipmentDocumentNames.NZCFTACertificateOfOrigin);
			mockDocument.SetupGet(d => d.Data).Returns(mockData.Object);
			mockDocument.SetupGet(d => d.DataContext).Returns(DataContext.CertificateOfOriginNZCFTA);
			mockDocument.SetupGet(d => d.Margins).Returns(new Margins());
			mockDocument.SetupGet(d => d.PageDimensions).Returns(new PageDimensions { PaperName = "A4" });
			mockDocument.SetupGet(d => d.Rows).Returns(new List<Row>());
			mockDocument.SetupGet(d => d.Columns).Returns(new List<Column>());

			var mockMessageInstructions = new Mock<IMessageInstructions>();
			mockMessageInstructions.SetupGet(mi => mi.DocumentName).Returns(ShipmentDocumentNames.NZCFTACertificateOfOrigin);

			var mockEDocsInstructions = new Mock<IEDocsInstructions>();
			mockEDocsInstructions.SetupGet(i => i.SaveCopyToEDocs).Returns(false);

			var mockDocumentDescriptor = new Mock<IDocumentDescriptor>();
			mockDocumentDescriptor.SetupGet(d => d.MessageInstructions).Returns(mockMessageInstructions.Object);
			mockDocumentDescriptor.SetupGet(d => d.EDocsInstructions).Returns(mockEDocsInstructions.Object);

			var mockSecurityService = new Mock<IDocumentSecurityService>();
			mockSecurityService.SetupGet(s => s.CanSendMessage).Returns(true);

			var mockDeliveryService = new Mock<IDocumentDeliveryService>();
			mockDeliveryService
				.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.IsAny<INotifications>()))
				.Returns(CreateEdiCommunicationSettings());

			var mockedUserNotificationService = new Mock<IUserNotificationService>();
			mockedUserNotificationService
				.Setup(x => x.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);

			var services = new ServiceContainer();
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IDocumentSecurityService>(mockSecurityService.Object);
			services.Register<IDocumentDeliveryService>(mockDeliveryService.Object);
			services.Register<IUserNotificationService>(mockedUserNotificationService.Object);

			var mockDocumentInfo = new Mock<IDocumentInfo>();
			mockDocumentInfo.SetupGet(d => d.Descriptor).Returns(mockDocumentDescriptor.Object);
			mockDocumentInfo.SetupGet(d => d.Document).Returns(mockDocument.Object);
			mockDocumentInfo.SetupGet(d => d.DocumentData).Returns(documentData);
			mockDocumentInfo.SetupGet(d => d.Services).Returns(services);

			return mockDocumentInfo.Object;
		}

		VisualizerDocumentData CreateDocumentData(ForwardingShipment shipment)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_Name = ShipmentDocumentDataStoreNames.NZCFTACertificateOfOrigin;

			return documentData;
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001007";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKDestination = "CNCHA";
			shipment.JS_RL_NKLoadPort = "CNCHA";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "Lots of Widgets";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";

			var deliveryOrderReceiptNote = shipment.Notes.AddNew();
			deliveryOrderReceiptNote.ST_Description = PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description;
			deliveryOrderReceiptNote.ST_NoteText = "Here are some COO Notes";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			PopulateShipmentAddresses(shipment);

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "I'm manufacturer";
			manufacturer.OH_RL_NKClosestPort = "NZAKL";
			manufacturer.MainAddress.Address1 = "83 Ulster road";
			manufacturer.MainAddress.Address2 = "Unit52";
			manufacturer.MainAddress.City = "New Zealand";
			manufacturer.MainAddress.Postcode = "1010";
			manufacturer.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.ManufacturerDocAddress.E2_OA_Address = manufacturer.MainAddress.PK;
		}

		public void Test_Image()
		{
			var command = new OpenCOOrderFormCommand();

			AssertType<Bitmap>(command.Image);
		}
	}

	// TODO: Does this need to be run Winzor as well?
	class OpenCOOrderFormCommandForTests : OpenCOOrderFormCommand
	{
		public List<(IDocument Document, IVisualizerDocumentData DocumentData)> CapturedSaveUnSavedChangesCalls { get; } = new();
		public override bool ShowSupportingDocPopupForm(ISupportingDocDataObject supportingDocDataObject)
		{
			using (var form = new SupportingDocFormForTests(supportingDocDataObject))
			{
				form.Show();
				form.OkButton.PerformClick();
			}
			return true;
		}

		protected override void DocumentDataSave(IServiceContainer services, IDocument document, IVisualizerDocumentData documentData)
		{
			CapturedSaveUnSavedChangesCalls.Add((document, documentData));
		}
	}
}
