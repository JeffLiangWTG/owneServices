using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Testing
{
	public abstract class EServicesDeliveryTest : TestCaseWithFactory
	{
		[RootElement(nameof(TestEvent))]
		class TestEvent : TopLevelDataObject
		{
			[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(UniversalDataBuss.DataObjects.Universal._2011_11.DataContext))]
			[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(UniversalDataBuss.DataObjects.Universal._2012_11.DataContext))]
			[ReferenceProperty]
			public override IDataContextDataObject DataContext { get; set; }
		}

		IEDICommunicationsMode CreateMockCommunicationsMode()
		{
			var mode = new Mock<IEDICommunicationsMode>();
			mode.Setup(m => m.EK_Filename).Returns("AAAA");
			mode.Setup(m => m.EK_ServerAddressSubject).Returns("TestSubject");
			mode.Setup(m => m.EK_Destination).Returns("DST");
			mode.Setup(m => m.EK_MessagePurpose).Returns("TYP");
			mode.Setup(m => m.EK_FileFormat).Returns("XUS");

			return mode.Object;
		}

		public void TestMessageNumberCollectionIsCorrectlyPopulated()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			Factory.Save();

			var context =
				new DeliveryContext(Factory)
				{
					ParentInfo = EntityInfo.New(dummy),
					ApplicationCode = ApplicationCodeList.Codes.XMS,
					MessageTypeCode = EDIMessageTypeList.Codes.XMS,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
					Notifications = new NotificationsForTest(),
				};

			var mode = CreateMockCommunicationsMode();

			using (Factory.AddDisposableService())
			{
				var testEvent = new TestEvent();
				var interchange = Delivery.BuildEServicesMessage(context, mode, new DeliveryStreamWrapperUXML(context.ParentInfo, testEvent, new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter(), UniversalXmlInfo.Namespace_2012_11));

				Factory.Save();

				var message = Factory.Load<IXmlEDIMessage>(new ZQuery()).FirstOrDefault();

				AssertNotNull(nameof(message), message);

				var interchangeMessageNumbers = XElement.Parse(interchange.EI_BodyText)?.Element(XName.Get(nameof(TestEvent), UniversalXmlInfo.Namespace_2012_11))?.Element(XName.Get(nameof(ITopLevelDataObject.MessageNumberCollection), UniversalXmlInfo.Namespace_2012_11))?.Elements();
				var messageMessageNumbers = message.Content.Element(XName.Get(nameof(TestEvent), UniversalXmlInfo.Namespace_2012_11))?.Element(XName.Get(nameof(ITopLevelDataObject.MessageNumberCollection), UniversalXmlInfo.Namespace_2012_11))?.Elements();

				Action<IEnumerable<XElement>, string> validateNumberCollection = (messageNumbers, ownerName) =>
				{
					AssertNotNull($"{ownerName}.{nameof(messageNumbers)}", messageNumbers);
					AssertNotNull($"{ownerName}.trackingID", messageNumbers.FirstOrDefault(mn => mn.Attribute(nameof(MessageNumber.Type))?.Value == nameof(MessageNumberType.TrackingID) && mn.Value == interchange.EI_SessionGUID.ToString()));
					AssertNotNull($"{ownerName}.messageNumber", messageNumbers.FirstOrDefault(mn => mn.Attribute(nameof(MessageNumber.Type))?.Value == nameof(MessageNumberType.MessageNumber) && mn.Value == message.EM_MessageNum));
					AssertNotNull($"{ownerName}.interchangeNumber", messageNumbers.FirstOrDefault(mn => mn.Attribute(nameof(MessageNumber.Type))?.Value == nameof(MessageNumberType.InterchangeNumber) && mn.Value == interchange.EI_InterchangeNum));
				};

				validateNumberCollection(interchangeMessageNumbers, nameof(interchange));
				validateNumberCollection(messageMessageNumbers, nameof(message));
			}
		}

		public void TestLoadingLogParentUsesLoadStrategy()
		{
			var nonPersistentDummy = new NonPersistentBusinessObjectForTest(Factory);
			var persistentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var notifications = new NotificationsForTest();

			var context =
				new DeliveryContext(otherFactory)
				{
					ParentInfo = EntityInfo.New(nonPersistentDummy),
					ApplicationCode = ApplicationCodeList.Codes.XMS,
					MessageTypeCode = EDIMessageTypeList.Codes.XMS,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
					Notifications = notifications
				};

			const string messageBody = "<UniversalShipment></UniversalShipment>";

			using var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody));
			var mode = CreateMockCommunicationsMode();
			var mockLoadStrategy = new Mock<IBusinessObjectLoadStrategy>();

			mockLoadStrategy.Setup(m => m.Load(otherFactory, nonPersistentDummy.PK)).Returns(persistentDummy);

			var loadStrategies = new Hashtable
			{
				{ typeof(NonPersistentBusinessObjectForTest).FullName, new TestObjectHandle(mockLoadStrategy) }
			};

			using (ObjectFactory.Substitute("BusinessObjectLoadStrategyList", loadStrategies))
			using (otherFactory.AddDisposableService())
			{
				var result = Delivery.BuildEServicesMessage(context, mode, new DeliveryStreamWrapperUXML(stream));
				Factory.Save();

				AssertNotNull("message was created", result);
				AssertNotContains("triggering business object was provided by the load strategy", "Cannot find triggering business object.", notifications.ToString());
			}
		}

		public void TestGetMessageDataLogLinker()
		{
			var nonPersistentDummy = new NonPersistentBusinessObjectForTest(Factory);
			var persistentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var notifications = new NotificationsForTest();

			var context =
				new DeliveryContext(otherFactory)
				{
					ParentInfo = EntityInfo.New(persistentDummy),
					ApplicationCode = ApplicationCodeList.Codes.XMS,
					MessageTypeCode = EDIMessageTypeList.Codes.XMS,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
					Notifications = notifications
				};

			const string messageBody = "<UniversalShipment></UniversalShipment>";

			using (otherFactory.AddDisposableService())
			using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
			{
				var mode = new NonPersistentEDICommunicationMode();
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_Destination = "DST";
				var result = Delivery.BuildEServicesMessage(context, mode, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
				otherFactory.Save();

				var logs = persistentDummy.GetLogs();

				AssertEquals(1, logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, MessageDataLogLinkerEventCode())).Length);
			}
		}

		public void TestInterchangeWithUNKNotCreated()
		{
			var persistentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var notifications = new NotificationsForTest();

			var context =
				new DeliveryContext(otherFactory)
				{
					ParentInfo = EntityInfo.New(persistentDummy),
					ApplicationCode = ApplicationCodeList.Codes.XMS,
					MessageTypeCode = EDIMessageTypeList.Codes.XMS,
					Notifications = notifications,
					MessageSubTypeCode = string.Empty
				};

			const string messageBody = "<Payload><UnknownType></UnknownType></Payload>";

			using (otherFactory.AddDisposableService())
			using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
			{
				var mode = new NonPersistentEDICommunicationMode();
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_Destination = "DST";
				var result = Delivery.BuildEServicesMessage(context, mode, new DeliveryStreamWrapperUXML(stream));
				otherFactory.Save();
				AssertEquals(@"We should not be creating unknown messages.

Delivery mode details:
Transport: 
File format: XML
Purpose: 
Destination: DST
Organization: 
File name: 
ApplicationCode: XMS
Stream Data: <Payload><UnknownType></UnknownType></Payload>", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				AssertContains("Should have notification that unknown Edi messages should not be saved.", "Edi Messages with unknown Sub Type can't be created", context.Notifications.ToString());

				var logs = persistentDummy.GetLogs();
				AssertEquals(0, logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, MessageDataLogLinkerEventCode())).Length);
			}
		}

		public void TestInterchangeWithORGCreated()
		{
			var persistentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var notifications = new NotificationsForTest();

			var context =
				new DeliveryContext(otherFactory)
				{
					ParentInfo = EntityInfo.New(persistentDummy),
					ApplicationCode = ApplicationCodeList.Codes.XMS,
					MessageTypeCode = EDIMessageTypeList.Codes.XMS,
					Notifications = notifications,
					MessageSubTypeCode = string.Empty
				};

			const string messageBody = "<Payload><Organisations></Organisations></Payload>";

			using (otherFactory.AddDisposableService())
			using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
			{
				var mode = new NonPersistentEDICommunicationMode();
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_Destination = "DST";
				var result = Delivery.BuildEServicesMessage(context, mode, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
				otherFactory.Save();
				AssertNotContains("Should NOT have notification that unknown Edi messages should not be saved.", "Edi Messages with unknown Sub Type can't be created", context.Notifications.ToString());
				var logs = persistentDummy.GetLogs();
				AssertEquals(1, logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, MessageDataLogLinkerEventCode())).Length);
			}
		}

		public void TestDoNotLoadMessageContentAsString()
		{
			var persistentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var notifications = new NotificationsForTest();

			var context =
				new DeliveryContext(otherFactory)
				{
					ParentInfo = EntityInfo.New(persistentDummy),
					ApplicationCode = ApplicationCodeList.Codes.XMS,
					MessageTypeCode = EDIMessageTypeList.Codes.XMS,
					Notifications = notifications,
					MessageSubTypeCode = string.Empty
				};

			const string messageBody = "<Payload><Organisations></Organisations></Payload>";

			using (otherFactory.AddDisposableService())
			using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
			{
				var mode = new NonPersistentEDICommunicationMode();
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_Destination = "DST";
				var result = Delivery.BuildEServicesMessage(context, mode, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
				var interchangeRow = ((INeedRow)result).Row;
				var messages = otherFactory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, result.PK));
				AssertEquals(1, messages.Length);
				var messageRow = ((INeedRow)messages[0]).Row;

				CombineAssertions("Interchange and Message should have their content set with a stream, this should not be accessed until saving for memory reasons", () =>
				{
					AssertEquals(2, (interchangeRow[EDIInterchangeSchema.EI_BodyData.Name] as byte[]).Length);
					AssertEquals(0, (interchangeRow[EDIInterchangeSchema.EI_BodyNText.Name] as string).Length);
					AssertEquals(0, (interchangeRow[EDIInterchangeSchema.EI_BodyText.Name] as string).Length);
					AssertEquals(2, (messageRow[EDIMessageSchema.EM_MessageData.Name] as byte[]).Length);
					AssertEquals(0, (messageRow[EDIMessageSchema.EM_MessageText.Name] as string).Length);
					AssertEquals(0, (messageRow[EDIMessageSchema.EM_MessageNText.Name] as string).Length);
					AssertNoExceptionThrown(otherFactory.Save);
				});
			}
		}

		public void TestInterchangeWithValidMessageIsCreated()
		{
			var persistentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var notifications = new NotificationsForTest();

			var context =
				new DeliveryContext(otherFactory)
				{
					ParentInfo = EntityInfo.New(persistentDummy),
					ApplicationCode = ApplicationCodeList.Codes.XMS,
					MessageTypeCode = EDIMessageTypeList.Codes.XMS,
					Notifications = notifications,
					MessageSubTypeCode = string.Empty
				};

			const string messageBody = "<Payload><Orders></Orders></Payload>";

			using (otherFactory.AddDisposableService())
			using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
			{
				var mode = new NonPersistentEDICommunicationMode();
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_Destination = "DST";
				var result = Delivery.BuildEServicesMessage(context, mode, new DeliveryStreamWrapperUXML(stream));
				otherFactory.Save();
				AssertNotContains("Should NOT have notification that unknown Edi messages should not be saved.", "Edi Messages with unknown Sub Type can't be created", context.Notifications.ToString());
			}
		}

		protected virtual ZString MessageDataLogLinkerEventCode()
		{
			return Events.DataExportCode;
		}

		protected abstract EServicesDelivery Delivery { get; }
	}
}
