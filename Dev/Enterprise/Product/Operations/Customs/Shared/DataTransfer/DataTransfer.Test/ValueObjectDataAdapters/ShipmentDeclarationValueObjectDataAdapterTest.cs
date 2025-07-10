using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(ShipmentDeclarationValueObjectDataAdapter))]
	public class ShipmentDeclarationValueObjectDataAdapterTest : ValueObjectDataAdapterTest<ForwardingShipment, Xsd.Shipment>
	{
		public void TestImportFromXmlFile_NoError()
		{
			var interchange = new Xsd.XmlInterchange();
			var notification = new NotificationBuffer();

			var numOfDeclaration = Factory.GetDatabaseCount(typeof(BaseJobDeclaration));
			var numOfShipment = Factory.GetDatabaseCount(typeof(ForwardingShipment));

			var context = new ValueObjectImportContext(Factory, interchange, notification);
			var shipment = adapter.CreateOrUpdateFromValueObject(XmlShipment, context);

			Factory.Save();

			Assert("Notification Buffer has no error", !notification.HasErrors);
			AssertNotNull("Shipment should not be null", shipment);
			AssertEquals("A New Shipment is created", numOfShipment + 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			Assert("Shipment should have a declaration", shipment.Declarations.Length > 0);
			var jobDeclaration = (BaseJobDeclaration)shipment.Declarations[0];
			AssertEquals("Declaration's Transport Mode", shipment.JS_TransportMode, jobDeclaration.JE_TransportMode);
			AssertEquals("Declaration's Housebill", shipment.JS_HouseBill, jobDeclaration.JE_HouseBill);
			AssertHasDataImportEvent(shipment);
			AssertHasDataImportEvent(jobDeclaration);
		}

		public void TestImportFromXmlFile_ShipmentDeclarationWithoutAConsol()
		{
			var unAttachedShipmentWithJobDec = GetShipment();
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = unAttachedShipmentWithJobDec.PK;

			Factory.Save();

			Assert("PreCondition: UnAttachedShipmentWithJobDec has no changes", !unAttachedShipmentWithJobDec.HasChanges);
			var numOfDeclaration = Factory.GetDatabaseCount(typeof(BaseJobDeclaration));
			var numOfShipment = Factory.GetDatabaseCount(typeof(ForwardingShipment));

			var interchange = new Xsd.XmlInterchange();
			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, interchange, notification);
			var shipment = adapter.CreateOrUpdateFromValueObject(XmlShipment, context);

			Factory.Save();

			AssertEquals("Match to existing Declaration", unAttachedShipmentWithJobDec.PK, shipment.PK);
			Assert("UnAttachedShipmentWithJobDec has no changes", !unAttachedShipmentWithJobDec.HasChanges);
			Assert("Shipment has no changes", !shipment.HasChanges);
			AssertEquals("No Shipment should be created", numOfShipment, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("No Declaration should be created", numOfDeclaration, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			Assert("Notification Buffer should have error", notification.HasErrors);
			AssertBufferHasMessage(notification, "Error: Shipment with HAWB HOUSEBILL1 is already linked to a Consol and/or a Declaration and cannot be imported.\r\n");
		}

		public void TestImportFromXmlFile_ShipmentWithAConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			var attachedShipment = GetShipment();
			consol.Shipments.Add(attachedShipment);

			Factory.Save();

			var numOfDeclaration = Factory.GetDatabaseCount(typeof(BaseJobDeclaration));
			var numOfShipment = Factory.GetDatabaseCount(typeof(ForwardingShipment));

			var interchange = new Xsd.XmlInterchange();
			var notification = new NotificationBuffer();

			var context = new ValueObjectImportContext(Factory, interchange, notification);
			var shipment = adapter.CreateOrUpdateFromValueObject(XmlShipment, context);

			Factory.Save();

			AssertEquals("Notification Buffer has no error", false, notification.HasErrors);
			AssertNotNull("Shipment should not be null", shipment);
			Assert("Not match to existing Declaration", attachedShipment.PK != shipment.PK);
			AssertEquals("A New Shipment should be created", numOfShipment + 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("A New Declaration should be created", numOfDeclaration + 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			Assert("Shipment should have a declaration", shipment.Declarations.Length > 0);
			var jobDeclaration = (BaseJobDeclaration)shipment.Declarations[0];

			AssertEquals("Shipment's TransportMode", Core.Constants.TransportModes.Air, shipment.JS_TransportMode);
			AssertEquals("Shipment's Origin", "AUSYD", shipment.JS_RL_NKOrigin);
			AssertEquals("Shipment's Dest", "MYPKG", shipment.JS_RL_NKDestination);
			AssertEquals("Shipment's Consols", 0, shipment.Consols.Count);

			AssertHasDataImportEvent(shipment);
			AssertHasDataImportEvent(jobDeclaration);
		}

		public void TestImportFromXmlFile_ShipmentWithoutAConsolAndJobDec()
		{
			var unAttachedShipmentWithoutJobDec = GetShipment();
			unAttachedShipmentWithoutJobDec.JS_PackingMode = "BCN";

			Factory.Save();

			var interchange = new Xsd.XmlInterchange();
			var notification = new NotificationBuffer();

			Assert("PreCondition: Shipment has no Declaration", unAttachedShipmentWithoutJobDec.Declarations.Length == 0);
			AssertEquals("PreCondition: Shipment's Packing Mode", "BCN", unAttachedShipmentWithoutJobDec.JS_PackingMode);

			var numOfDeclaration = Factory.GetDatabaseCount(typeof(BaseJobDeclaration));
			var numOfShipment = Factory.GetDatabaseCount(typeof(ForwardingShipment));

			var context = new ValueObjectImportContext(Factory, interchange, notification);
			var shipment = adapter.CreateOrUpdateFromValueObject(XmlShipment, context);

			using (var anotherMutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK))
			{
				Assert("Mutex in DeclarationBeingCreatedForShipmentMutexCreator should not be unlocked after process", !anotherMutex.Lock());

				Factory.Save();
				Assert("Mutex in DeclarationBeingCreatedForShipmentMutexCreator should be unlocked after factory saved", anotherMutex.Lock());
				anotherMutex.Unlock();
			}

			Assert("Notification Buffer has no error", !notification.HasErrors);
			AssertNotNull("Shipment should not be null", shipment);
			AssertEquals("Match to existing Declaration", unAttachedShipmentWithoutJobDec.PK, shipment.PK);
			AssertEquals("No Shipment should be created", numOfShipment, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			Assert("The shipment has a declaration", shipment.Declarations.Length > 0);
			AssertEquals("A New Declaration should be created", numOfDeclaration + 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));

			var jobDeclaration = (BaseJobDeclaration)shipment.Declarations[0];

			AssertEquals("Shipment's packing mode is updated", "LSE", shipment.JS_PackingMode);

			AssertHasDataImportEvent(shipment);
			AssertHasDataImportEvent(jobDeclaration);
		}

		public void TestImportFromXmlFile_ShipmentWithoutAConsolAndWithMutexJobDec()
		{
			var unAttachedShipmentWithoutJobDec = GetShipment();

			using (var fMutex = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, unAttachedShipmentWithoutJobDec.PK.ToString() + GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString()))
			{
				fMutex.Lock();
				Factory.Save();

				Assert("PreCondition: UnAttachedShipmentWithoutJobDec has no changes", !unAttachedShipmentWithoutJobDec.HasChanges);
				var numOfDeclaration = Factory.GetDatabaseCount(typeof(BaseJobDeclaration));
				var numOfShipment = Factory.GetDatabaseCount(typeof(ForwardingShipment));
				var numOfMutex = Factory.GetDatabaseCount(typeof(ForwardingShipment));

				var interchange = new Xsd.XmlInterchange();
				var notification = new NotificationBuffer();
				var context = new ValueObjectImportContext(Factory, interchange, notification);
				var shipment = adapter.CreateOrUpdateFromValueObject(XmlShipment, context);

				Factory.Save();

				AssertEquals("Match to existing Declaration", unAttachedShipmentWithoutJobDec.PK, shipment.PK);
				Assert("UnAttachedShipmentWithoutJobDec has no changes", !unAttachedShipmentWithoutJobDec.HasChanges);
				Assert("Shipment has no changes", !shipment.HasChanges);
				AssertEquals("No Shipment should be created", numOfShipment, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals("No Declaration should be created", numOfDeclaration, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
				Assert("Notification Buffer should have error", notification.HasErrors);
				AssertBufferHasMessage(notification, "Error: Shipment with HAWB HOUSEBILL1 is already linked to a Consol and/or a Declaration and cannot be imported.\r\n");
			}

			Factory.Save();
		}

		public void TestDelagation()
		{
			AssertDelegation();
		}

		public void TestBusinessObjecType()
		{
			AssertEquals("Business Object is a Forwarding Shipment", typeof(ForwardingShipment), adapter.BusinessObjectType);
		}

		protected void AssertHasDataImportEvent(BusinessObject bizObj)
		{
			var dataImportEvents = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to " + bizObj.HumanReadableName, 1, dataImportEvents.Length);
		}

		protected void AssertBufferHasMessage(NotificationBuffer buffer, string errorMesg)
		{
			Assert("Notification buffer should contain error message: " + errorMesg, buffer.AsString.IndexOf(errorMesg) != -1);
		}

		protected virtual void AssertDelegation()
		{
			adapter = ShipmentDeclarationValueObjectDataAdapter.New();
			AssertEquals("Default Adapter Type", typeof(ShipmentDeclarationValueObjectDataAdapter), adapter.GetType());

			TestDeclarationValueObjectDataAdapter.Register();
			adapter = ShipmentDeclarationValueObjectDataAdapter.New();
			AssertEquals("Adapter Overriden New Delegate Type", typeof(TestDeclarationValueObjectDataAdapter), adapter.GetType());
		}

		Xsd.Shipment xmlShipment;
		protected Xsd.Shipment XmlShipment
		{
			get
			{
				if (xmlShipment == null)
				{
					xmlShipment = GetValueObjectFromXmlFile(FileReader.ExtractEmbeddedResourceToFile("Enterprise.Customs.DataTransfer.Testing.Testing", TempDir.DirectoryName, "DeclarationShipmentTestFile.xml"));
				}
				return xmlShipment;
			}
		}

		protected Xsd.Shipment GetValueObjectFromXmlFile(ZString fileName)
		{
			var serializer = new XmlValueObjectSerializer(typeof(Xsd.Shipment));

			var exampleDoc = new XmlDocument();

			using (var reader = new StreamReader(fileName))
			{
				exampleDoc.Load(reader);
			}

			var reader1 = new XmlNodeReader(exampleDoc);
			var xmlShipment = serializer.Deserialize(reader1) as Xsd.Shipment;

			AssertNotNull("PreCondition: Value Object is not null", xmlShipment);

			return xmlShipment;
		}

		protected override ValueObjectDataAdapter<ForwardingShipment, Xsd.Shipment> GetNewBizObjXmlDataAdapter() => TestDeclarationValueObjectDataAdapter.New();

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample() => null;

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => null;

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => null;

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample() => null;

		protected override string ExpectedRootElementName => "Shipment";

		protected override string ExpectedRootCollectionElementName => "Shipments";

		protected override bool IsExportToValueObjectSupported => false;

		protected override bool IsExportToCollectionSupported => false;

		protected ShipmentDeclarationValueObjectDataAdapter adapter;

		protected override void SetUp()
		{
			base.SetUp();
			adapter = ShipmentDeclarationValueObjectDataAdapter.New();
		}

		protected override void TearDown()
		{
			base.TearDown();

			tempDir?.Dispose();
		}

		protected override string TestingCountry => null;

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		ForwardingShipment GetShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HOUSEBILL1";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "MYPKG";
			shipment.JS_RL_NKOrigin = "AUSYD";

			return shipment;
		}

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(ShipmentDeclarationValueObjectDataAdapterTest)));
		TestFileReader fileReader;

		protected class TestDeclarationValueObjectDataAdapter : ShipmentDeclarationValueObjectDataAdapter
		{
			protected TestDeclarationValueObjectDataAdapter()
			{
			}

			protected TestDeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
				: base(triggeredByEvents)
			{
			}

			public static ShipmentDeclarationValueObjectDataAdapter GetInstance() => new TestDeclarationValueObjectDataAdapter();

			public static void Register()
			{
				OverridableNewDelegate.Value = new NewDelegate(GetInstance);
			}
		}
	}
}
