using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.MessageDelivery.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Codes = Enterprise.MasterFiles.Business.EDICommunicationsModeCommsDirectionList.Codes;

namespace Enterprise.MasterFiles.Business.Workflow.MessageDelivery.Testing
{
	sealed class EHubDeliveryTest : EServicesDeliveryTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSkipXmlDeclarationSample1()
		{
			TestSkipXmlDeclaration("XmlInterchange.xml", "XmlInterchange_NoXmlDeclaration.xml");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSkipXmlDeclarationSample2()
		{
			TestSkipXmlDeclaration("NoNamespaceSample.xml", "NoNamespaceSample_NoXmlDeclaration.xml");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSkipXmlDeclarationSample3()
		{
			TestSkipXmlDeclaration("VerySimpleXML.xml", "VerySimpleXML_NoXmlDeclaration.xml");
		}

		void TestSkipXmlDeclaration(string sourceFile, string expectedFile)
		{
			var delivery = new EHubDelivery();
			using (Factory.AddDisposableService())
			using (var stream = (SubStreamableStream)File.OpenRead(Path.Combine(BaseTestFilePath, sourceFile)))
			{
				mode.Setup(m => m.EK_Filename).Returns("AAAA");
				mode.Setup(m => m.EK_ServerAddressSubject).Returns("TestSubject");
				mode.Setup(m => m.EK_Destination).Returns("DST");
				mode.Setup(m => m.EK_MessagePurpose).Returns("TYP");
				mode.Setup(m => m.EK_FileFormat).Returns("XML");
				var dummyBO = Factory.New<DummyBusinessObject>();
				var result = delivery.BuildEServicesMessage(
					new DeliveryContext(Factory)
					{
						ParentInfo = EntityInfo.New(dummyBO),
						MessageSubTypeCode = EDIMessageSubTypeList.Codes.AgencyBillsOfLading,
						Notifications = new TestLogger()
					}, mode.Object, new DeliveryStreamWrapperUXML(stream));
				Factory.Save();
				using (var expectedStream = File.OpenRead(Path.Combine(BaseTestFilePath, expectedFile)))
				{
					AssertMultilineASCIIEquals("", new StreamReader(expectedStream, Encoding.UTF8).ReadToEnd().Trim(), result.EI_BodyText.Trim());
				}
			}
		}

		string BaseTestFilePath
		{
			get { return Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Testing"); }
		}

		public void TestCreateEHubMessage()
		{
			var transaction = (ITransactionParticipant)Factory;
			using (Factory.AddDisposableService())
			using (var transactionManager = transaction.BeginTransactionWithManager())
			{
				var dummyBO = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
				var dummyTriggerBO = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				((IStmALogParent)dummyTriggerBO).Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				TestCreateEHubMessageRaw(dummyBO, dummyTriggerBO, Factory);

				transactionManager.CommitTransaction();
			}
		}

		void TestCreateEHubMessageRaw(BusinessObject dummyBO, BusinessObject dummyTriggerBO, BusinessObjectFactory factory)
		{
			var context =
				new DeliveryContext(factory)
				{
					ParentInfo = EntityInfo.New(dummyBO),
					TriggerObjectInfo = EntityInfo.New(dummyTriggerBO),
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.AgencyBillsOfLading,
					Notifications = new TestLogger()
				};

			var messageBody = "<Test />";
			using var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody));
			mode.Setup(m => m.EK_Filename).Returns("AAAA");
			mode.Setup(m => m.EK_ServerAddressSubject).Returns("TestSubject");
			mode.Setup(m => m.EK_Destination).Returns("DST");
			mode.Setup(m => m.EK_MessagePurpose).Returns("TYP");
			mode.Setup(m => m.EK_FileFormat).Returns("XML");
			var result = delivery.BuildEServicesMessage(context, mode.Object, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
			factory.Save();

			AssertNotNull(factory.Load<IXmlEDIInterchange>(result.Identifier));
			AssertEquals("EI_From", Env.CurrentCompany.GetLicenceCode(), result.EI_From);
			AssertEquals("EI_To", mode.Object.EK_Destination, result.EI_To);
			AssertEquals("EI_InterchangeType", "XMS", result.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", Codes.Transmit, result.EI_ReceiveTransmit);
			AssertEquals("EI_HeaderText", "<EDIDelivery><FileName>AAAA</FileName><EmailSubject>TestSubject</EmailSubject></EDIDelivery>", result.EI_HeaderText);
			AssertEquals("EI_MessageText", "<Test />", result.EI_BodyText);
			AssertEquals("EI_GB", GlbBranch.CurrentBranch.PK, result.EI_GB);
			AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, result.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, result.EI_TransportType);

			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_EI, result.PK);
			var message = factory.LoadTop1<IXmlEDIMessage>(query);
			AssertNotNull(message);
			AssertPivot(dummyTriggerBO.PK, message);
		}

		public void TestCreateEHubMessageNotThrowConcurrencyError()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dummyBO = (BusinessObject)factory1.New<Forwarding.IForwardingConsol>();
			var dummyTriggerBO = (BusinessObject)factory1.New<Forwarding.IForwardingShipment>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			((IStmALogParent)dummyTriggerBO).Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dummyBOInFactory2 = factory2.LoadTop1<Forwarding.IForwardingConsol>(new ZQuery(JobConsolSchema.PK, dummyBO.PK));
			dummyBOInFactory2.JK_MasterBillNum = "MB222";
			factory2.Save();

			((Forwarding.IForwardingConsol)dummyBO).JK_MasterBillNum = "MB111";

			var context = new DeliveryContext(factory1)
			{
				ParentInfo = EntityInfo.New(dummyBO),
				TriggerObjectInfo = EntityInfo.New(dummyTriggerBO),
				MessageSubTypeCode = EDIMessageSubTypeList.Codes.AgencyBillsOfLading,
				Notifications = new TestLogger()
			};

			var messageBody = "<Test />";
			using (factory1.AddDisposableService())
			using (var stream = (SubStreamableStream)new MemoryStream(ASCIIEncoding.Default.GetBytes(messageBody)))
			{
				AssertNoExceptionThrown(delegate
				{
					mode.Setup(m => m.EK_Filename).Returns("AAAA");
					mode.Setup(m => m.EK_ServerAddressSubject).Returns("TestSubject");
					mode.Setup(m => m.EK_Destination).Returns("DST");
					mode.Setup(m => m.EK_MessagePurpose).Returns("TYP");
					mode.Setup(m => m.EK_FileFormat).Returns("XML");
					delivery.BuildEServicesMessage(context, mode.Object, new DeliveryStreamWrapperUXML(stream));
				});
			}
		}

		void AssertPivot(ZGuid pk, IXmlEDIMessage message)
		{
			var dataExportLog = FindMostRecentDataExportLog(pk);
			(new GenPivotTestHelper()).AssertPivotExists(StmALogSchema.Constants.Prefix, dataExportLog.PK, EDIMessageSchema.Constants.Prefix, message.PK, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);
		}

		StmALog FindMostRecentDataExportLog(ZGuid parentPK)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, parentPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			query.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";
			var result = Factory.LoadTop1<StmALog>(query);
			return result;
		}

		class TestLogger : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			delivery = new EHubDelivery();
			mode = new Mock<IEDICommunicationsMode>();
			errorNotifier = new Mock<IErrorNotifier<IEDICommunicationsMode>>();
			delivery.ErrorNotifier = errorNotifier.Object;
		}

		protected override EServicesDelivery Delivery
		{
			get { return delivery; }
		}

		Mock<IErrorNotifier<IEDICommunicationsMode>> errorNotifier;
		EHubDelivery delivery;
		Mock<IEDICommunicationsMode> mode;
	}
}
