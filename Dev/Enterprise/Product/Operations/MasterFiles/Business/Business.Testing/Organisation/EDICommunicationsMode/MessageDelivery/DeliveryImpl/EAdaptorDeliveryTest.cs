using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Testing
{
	sealed class EAdaptorDeliveryTest : EServicesDeliveryTest
	{
		public void TestCreateEAdaptorUDMMessage()
		{
			var transaction = (ITransactionParticipant)Factory;
			using (Factory.AddDisposableService())
			using (var transactionManager = transaction.BeginTransactionWithManager())
			{
				var dummyBO = Factory.New<DummyBusinessObject>();
				var context =
					new DeliveryContext(Factory)
					{
						ParentInfo = EntityInfo.New(dummyBO),
						ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
						MessageTypeCode = EDIMessageTypeList.Codes.XDC,
						MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
						Notifications = new TestLogger(),
					};

				var messageBody = "<UniversalShipment></UniversalShipment>";

				var expectedInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>DST</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""""></UniversalShipment>
  </Body>
</UniversalInterchange>";

				using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
				{
					mode.Setup(m => m.EK_Filename).Returns("AAAA");
					mode.Setup(m => m.EK_ServerAddressSubject).Returns("TestSubject");
					mode.Setup(m => m.EK_Destination).Returns("DST");
					var result = delivery.BuildEServicesMessage(context, mode.Object, new DeliveryStreamWrapperUXML(stream));
					Factory.Save();

					AssertNotNull(Factory.Load<IXmlEDIInterchange>(result.Identifier));
					CombineAssertions(delegate
					{
						AssertEquals("EI_From", Env.CurrentCompany.GetLicenceCode(), result.EI_From);
						AssertEquals("EI_To", mode.Object.EK_Destination, result.EI_To);
						AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, result.EI_ApplicationCode);
						AssertEquals("EI_InterchangeType", EDIMessageTypeList.Codes.XDC, result.EI_InterchangeType);
						AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, result.EI_ReceiveTransmit);
						AssertEquals("EI_HeaderText", "<EDIDelivery><FileName>AAAA</FileName><EmailSubject>TestSubject</EmailSubject></EDIDelivery>", result.EI_HeaderText);
						AssertEquals("EI_MessageText", expectedInterchange, result.EI_BodyText);
						AssertEquals("EI_GB", GlbBranch.CurrentBranch.PK, result.EI_GB);
						AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.eAdaptorQueued, result.EI_Status);
						AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.eAdaptor, result.EI_TransportType);
					});
					mode.VerifyAll();
				}
				transactionManager.CommitTransaction();
			}
		}

		public void TestCreateEAdaptorXMLMessage()
		{
			var transaction = (ITransactionParticipant)Factory;
			using (Factory.AddDisposableService())
			using (var transactionManager = transaction.BeginTransactionWithManager())
			{
				var dummyBO = Factory.New<DummyBusinessObject>();
				var context =
					new DeliveryContext(Factory)
					{
						ParentInfo = EntityInfo.New(dummyBO),
						ApplicationCode = ApplicationCodeList.Codes.XMS,
						MessageTypeCode = EDIMessageTypeList.Codes.XMS,
						MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
						Notifications = new TestLogger()
					};

				var messageBody = "<UniversalShipment></UniversalShipment>";

				using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
				{
					mode.Setup(m => m.EK_Filename).Returns("AAAA");
					mode.Setup(m => m.EK_ServerAddressSubject).Returns("TestSubject");
					mode.Setup(m => m.EK_Destination).Returns("DST");
					var result = delivery.BuildEServicesMessage(context, mode.Object, new DeliveryStreamWrapperUXML(stream));
					Factory.Save();

					AssertNotNull(Factory.Load<IXmlEDIInterchange>(result.Identifier));
					CombineAssertions(delegate
					{
						AssertEquals("EI_From", Env.CurrentCompany.GetLicenceCode(), result.EI_From);
						AssertEquals("EI_To", mode.Object.EK_Destination, result.EI_To);
						AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.XMS, result.EI_ApplicationCode);
						AssertEquals("EI_InterchangeType", EDIMessageTypeList.Codes.XMS, result.EI_InterchangeType);
						AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, result.EI_ReceiveTransmit);
						AssertEquals("EI_HeaderText", "<EDIDelivery><FileName>AAAA</FileName><EmailSubject>TestSubject</EmailSubject></EDIDelivery>", result.EI_HeaderText);
						AssertEquals("EI_MessageText", messageBody, result.EI_BodyText);
						AssertEquals("EI_GB", GlbBranch.CurrentBranch.PK, result.EI_GB);
						AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.eAdaptorQueued, result.EI_Status);
						AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.eAdaptor, result.EI_TransportType);
					});
					mode.VerifyAll();
				}
				transactionManager.CommitTransaction();
			}
		}

		public void TestEAdaptorDeliveryCorrectlySetsExternalReferenceNumber()
		{
			var transaction = (ITransactionParticipant)Factory;
			using (Factory.AddDisposableService())
			using (var transactionManager = transaction.BeginTransactionWithManager())
			{
				var dummyBO = Factory.New<DummyBusinessObject>();
				var context =
					new DeliveryContext(Factory)
					{
						ParentInfo = EntityInfo.New(dummyBO),
						ApplicationCode = ApplicationCodeList.Codes.XMS,
						MessageTypeCode = EDIMessageTypeList.Codes.XMS,
						MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
						Notifications = new TestLogger()
					};

				var messageBody = "<UniversalEvent></UniversalEvent>";

				using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
				{
					mode.Setup(m => m.EK_Filename).Returns("AAAA");
					mode.Setup(m => m.EK_ServerAddressSubject).Returns("TestSubject");
					mode.Setup(m => m.EK_Destination).Returns("DST");

					var interchange = delivery.BuildEServicesMessage(context, mode.Object, new DeliveryStreamWrapperUXML(stream));

					Factory.Save();

					var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));

					AssertEquals(externalReferenceNumber, message?.EM_ExternalReferenceNumber);

					mode.VerifyAll();
				}
				transactionManager.CommitTransaction();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			delivery = new EAdaptorDelivery()
			{
				ExternalReferenceNumber = externalReferenceNumber
			};
			mode = new Mock<IEDICommunicationsMode>();
			errorNotifier = new Mock<IErrorNotifier<IEDICommunicationsMode>>();
			delivery.ErrorNotifier = errorNotifier.Object;
		}

		class TestLogger : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}

		protected override EServicesDelivery Delivery
		{
			get { return delivery; }
		}

		Mock<IErrorNotifier<IEDICommunicationsMode>> errorNotifier;
		EAdaptorDelivery delivery;
		Mock<IEDICommunicationsMode> mode;
		const string externalReferenceNumber = "123";
	}
}
