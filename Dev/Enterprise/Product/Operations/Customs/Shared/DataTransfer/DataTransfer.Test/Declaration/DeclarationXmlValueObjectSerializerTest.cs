using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class DeclarationXmlValueObjectSerializerTest : TestCaseWithFactory
	{
		public void TestDontImportIfFoundExistingDeclarationOrUpdateIfAllowed()
		{
			var numberOfDecsBeforeImport = Factory.GetDatabaseCount(typeof(BaseJobDeclaration));
			var notify = new NotificationBuffer();

			ImportDeclarationXmlData(TestImportFileName, notify);
			AssertEquals("1 declaration should be imported when no existing declaration is found", numberOfDecsBeforeImport + 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			AssertEquals("The user should not be warned of anything for this import", false, notify.HasWarnings);
			AssertEquals(System.Environment.NewLine + "Declaration (Master Bill='Masterbill' House Bill='Housebill') created", serializer.GetDeclarationImportStatuses());

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "Masterbill";
			declaration.JE_HouseBill = "Housebill";
			Factory.Save();

			ImportDeclarationXmlData(TestImportFileName, notify);
			AssertEquals("No new declarations should be imported if an existing declaration is found", numberOfDecsBeforeImport + 2, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			AssertEquals("The user should be notified if an existing declaration is found and no import occurred", true, notify.AsString.IndexOf("However, it will not be updated") != -1);
			AssertEquals("The user should not be warned of anything for this import (some client dlls treat warnings as errors)", false, notify.HasWarnings);
			Assert(serializer.GetDeclarationImportStatuses().Contains("update rejected, update is not allowed"));

			SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ImportDeclarationXmlData(TestImportFileName, notify);
			AssertEquals("No new declarations should be imported if an existing declaration is found", numberOfDecsBeforeImport + 2, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			AssertEquals("An existing declaration should be updated", true, notify.AsString.IndexOf("Declaration " + declaration.JE_DeclarationReference + " updated") != -1);
			AssertEquals("The user should not be warned of anything for this import (some client dlls treat warnings as errors)", false, notify.HasWarnings);
			Assert(serializer.GetDeclarationImportStatuses().Contains("updated"));

			var log = declaration.Logs.AddNew(Events.CustomsEntryStatus);
			ImportDeclarationXmlData(TestImportFileName, notify);
			AssertEquals("No new declarations should be imported if an existing declaration is found", numberOfDecsBeforeImport + 2, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			AssertEquals("An existing declaration should be rejected, CES events found", true, notify.AsString.IndexOf("has CES Event(s) and therefore it will not be updated") != -1);
			AssertEquals("The user should not be warned of anything for this import (some client dlls treat warnings as errors)", false, notify.HasWarnings);
			Assert(serializer.GetDeclarationImportStatuses().Contains("update rejected, Customs messaging has commenced"));

			log.Cancel();
			ImportDeclarationXmlData(TestImportFileName, notify);
			AssertEquals("No new declarations should be imported if an existing declaration is found", numberOfDecsBeforeImport + 2, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			AssertEquals("An existing declaration should be updated", true, notify.AsString.IndexOf("Declaration " + declaration.JE_DeclarationReference + " updated") != -1);
			AssertEquals("The user should not be warned of anything for this import (some client dlls treat warnings as errors)", false, notify.HasWarnings);
			Assert(serializer.GetDeclarationImportStatuses().Contains("updated"));
		}

		public void TestMatchingByAgentReferenceAndUpdateExistingDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			declaration.JE_DeclarationReference = "B00155272";//Set in the SampleAgentReference.Xml
			Factory.Save();

			SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.ImportDeclarationNoFromXml.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var notify = new NotificationBuffer();
			ImportDeclarationXmlData(TestFileHelper.GetPathForTestFiles("SampleAgentReference.xml"), notify);

			AssertEquals("Should not have created another one", 1, Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00155272")).Length);
		}

		public void TestMatchingByAgentReferenceAndErrorIfExistingOneBelongsToDifferentCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "~1";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = branch.PK;
			Factory.Save();
			declaration.JE_DeclarationReference = "B00155272";//Specified in the SampleAgentReference.Xml
			Factory.Save();

			SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.ImportDeclarationNoFromXml.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var notify = new NotificationBuffer();

			using (var stream = File.OpenRead(TestFileHelper.GetPathForTestFiles("SampleAgentReference.xml")))
			{
				var collection = new ImportedBusinessObjectCollection(Factory);
				serializer.ImportXmlData(stream, DeclarationValueObjectDataAdapter.New(), collection, null, notify);
			}

			Assert(notify.HasErrors);
			Assert(notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));
			AssertContains("A declaration exists and it belongs to a different company, ", notify.AsString);
		}

		public void TestMatchingByAgentReferenceIfExistingOneBelongsToDifferentCompanyButSharingSameShipment()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "~1";

			var shipment = Factory.New<ForwardingShipment>();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			shipment.JS_UniqueConsignRef = "B00155272";//Specified in the SampleAgentReference.Xml
			declaration.JE_DeclarationReference = "B00155272";//Specified in the SampleAgentReference.Xml
			Factory.Save();

			SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.ImportDeclarationNoFromXml.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var notify = new NotificationBuffer();
			using (var stream = File.OpenRead(TestFileHelper.GetPathForTestFiles("SampleAgentReference.xml")))
			{
				var collection = new ImportedBusinessObjectCollection(Factory);
				serializer.ImportXmlData(stream, DeclarationValueObjectDataAdapter.New(), collection, null, notify);
			}

			Assert(notify.HasErrors);
		}

		public void TestCancelledDeclarationDoesNotGetUpdated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			declaration.JE_DeclarationReference = "B00155272";//Set in the SampleAgentReference.Xml
			declaration.JE_IsCancelled = true;
			Factory.Save();

			SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.ImportDeclarationNoFromXml.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var notify = new NotificationBuffer();
			ImportDeclarationXmlData(TestFileHelper.GetPathForTestFiles("SampleAgentReference.xml"), notify);

			AssertContains("Declaration " + declaration.JE_DeclarationReference + " cannot be updated as it has been inactivated.", notify.AsString);
		}

		public void TestDeclarationUpdatedIfExistsAndUpdateAllowed()
		{
			SystemDataRegistry.Instance.ImportDeclarationNoFromXml.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var existstingDeclaration = Factory.New<BaseJobDeclaration>();
			existstingDeclaration.JE_DeclarationReference = "B00001001";
			existstingDeclaration.JE_ContainerMode = "LCL";
			existstingDeclaration.JE_MasterBill = "MBL";
			existstingDeclaration.JE_HouseBill = "HBL";

			Factory.Save();

			var serialiser = new DeclarationXmlValueObjectSerializerForTest();

			var collection = new BaseJobDeclarationCollection(Factory);

			var notify = new NotificationBuffer();
			Xsd.Consol xmlConsol = new Xsd.Consol();
			xmlConsol.ConsolDetail.AgentReference = "B00001001";
			xmlConsol.ConsolDetail.ContainerMode = Xsd.ContainerMode.FCL;
			xmlConsol.ConsolDetail.ContainerModeSpecified = true;

			Xsd.ConsolIdentifier masterBill = xmlConsol.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.Value = "MBL";

			Xsd.Shipment xmlShipment = xmlConsol.Shipments.AddNew();

			Xsd.ShipmentIdentifier houseBill = xmlShipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "HBL";

			var context = new ValueObjectImportContext(Factory, notify);

			serialiser.CreateOrUpdateFromValueObject(DeclarationValueObjectDataAdapter.New(), collection, xmlConsol, context);

			AssertEquals("Should not have Data Prevent Save Error", false, notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));
			AssertEquals("Declaration is imported", 1, collection.Count);
			AssertEquals("Declaration is updated", collection[0].PK, existstingDeclaration.PK);
			AssertEquals("Declaration Field is updated", "FCL", collection[0].JE_ContainerMode);
		}

		public void TestImport_ValidDocument()
		{
			int numberOfDecsBeforeImport = Factory.GetDatabaseCount(typeof(BaseJobDeclaration));

			var notify = new NotificationBuffer();
			ImportDeclarationXmlData(TestFileHelper.GetPathForTesting("ValidDeclaration.xml"), notify);

			AssertEquals("There should be no warnings importing the file", false, notify.HasWarnings);
			AssertEquals("There should be no errors importing the file", false, notify.HasErrors);
			AssertEquals(numberOfDecsBeforeImport + 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
		}

		public void TestImport_InvalidDocument()
		{
			var notify = new NotificationBuffer();
			ImportDeclarationXmlData(TestFileHelper.GetPathForTesting("InvalidXmlDocument.xml"), notify);

			AssertEquals("The user should be notified of an error", true, notify.HasErrors);
			AssertEquals("The user should be notified of an error", true, notify.AsString.IndexOf("There is an error in the XML document") != -1);
		}

		string TestImportFileName => TestFileHelper.GetPathForTesting("ValidDeclaration.xml");

		DeclarationXmlValueObjectSerializer serializer;

		protected override void SetUp()
		{
			base.SetUp();
			serializer = new DeclarationXmlValueObjectSerializer();
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;

		void ImportDeclarationXmlData(string fileName, INotifications notify)
		{
			using (Stream stream = File.OpenRead(fileName))
			{
				ImportedBusinessObjectCollection collection = new ImportedBusinessObjectCollection(Factory);
				serializer.ImportXmlData(stream, DeclarationValueObjectDataAdapter.New(), collection, null, notify);
			}

			Factory.Save();
		}

		sealed class ImportedBusinessObjectCollection : BusinessObjectCollection<BusinessObject>
		{
			public ImportedBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		sealed class DeclarationXmlValueObjectSerializerForTest : DeclarationXmlValueObjectSerializer
		{
			public new void CreateOrUpdateFromValueObject(IValueObjectDataAdapter iDataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
			{
				base.CreateOrUpdateFromValueObject(iDataAdapter, collection, valueObject, context);
			}
		}
	}
}
