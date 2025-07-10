using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing.Supporters
{
	sealed class DeliveryOceanCarrierMessagingAsPDFCommandTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var consol = CreateConsol(false);

			AssertEquals(1, consol.ShippingLine.Contacts.Count);
			AssertEquals("TEST NAME", consol.ShippingLine.Contacts[0].OC_ContactName);
			AssertEquals("test@test.com", consol.ShippingLine.Contacts[0].OC_Email);

			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			var notificationService = new Mock<IUserNotificationService>();
			var printInstructions = new Mock<IPrintInstructions>();
			var eDocsInstructions = new Mock<IEDocsInstructions>();

			securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
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

			documentData.CreateMessageSentLog("Booking Request");
			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Services).Returns(services);
			documentInfo.SetupGet(di => di.Descriptor).Returns(descriptor.Object);

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns("Booking Request");
			document.Setup(d => d.DataContext).Returns(DataContext.BookingRequest);

			descriptor.SetupGet(di => di.Name).Returns("BookingRequest");
			descriptor.SetupGet(di => di.DocumentType).Returns("ALL");
			descriptor.SetupGet(di => di.PrintInstructions).Returns(printInstructions.Object);
			descriptor.SetupGet(di => di.EDocsInstructions).Returns(eDocsInstructions.Object);

			printInstructions.SetupGet(pi => pi.Title).Returns("Booking Request");

			eDocsInstructions.SetupGet(edoc => edoc.SaveCopyToEDocs).Returns(false);
			eDocsInstructions.SetupGet(edoc => edoc.Parent).Returns(consol);

			var parameters = new DummyDocDataObjectParameters()
			{
				LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
			};

			var bookingRequest = new BookingRequestBuilder(consol, parameters).Build();

			dynamicData.SetupGet(di => di.Value).Returns(bookingRequest);

			var command = new DeliveryOceanCarrierMessagingAsPDFCommand(consol, DataContext.BookingRequest);

			command.NotifyDocumentInfoCreated(documentInfo.Object);

			AssertEquals("test@test.com", bookingRequest.Recipient.Email);
			AssertEquals("TEST NAME", bookingRequest.Recipient.Contact);
			AssertEquals(true, command.IsEnabled);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

			command.SuspendValidation = true;
			AssertEquals(true, command.Invoke());
			AssertEquals("The Booking Request will be sent to Carrier’s contact TEST NAME’s email address test@test.com.\r\nDo you want to continue to send this Booking Request?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		ForwardingConsol CreateConsol(bool isCoload)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_AgentType = isCoload ? Core.Constants.AgentType.CoLoad : Core.Constants.AgentType.Agent;

			var carrier = Factory.New<OrgHeader>();

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "9001";
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

			if (isCoload)
			{
				consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			}
			else
			{
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			}

			Factory.Save();

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = "ALL";

			return consol;
		}
	}
}
