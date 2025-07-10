using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class DeclarationXmlDataImporterTest : TestCaseWithFactory
	{
		public void TestImport_ValidDocument()
		{
			var importer = new TestDeclarationXmlDataImporter(DeclarationValueObjectDataAdapter.New());

			AssertEquals("Precondition: Database should not contain any Declarations", 0, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));

			ImportDeclarationXmlData(importer, TestFileHelper.GetPathForTesting("ValidDeclaration.xml"), new NotificationBuffer());
			AssertEquals("Declaration should be imported", 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			Assert(!importer.IsImportedDeclarationRejected);

			ImportDeclarationXmlData(importer, TestFileHelper.GetPathForTesting("ValidDeclaration.xml"), new NotificationBuffer());
			AssertEquals("Declaration should be rejected", 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			Assert(importer.IsImportedDeclarationRejected);

			SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ImportDeclarationXmlData(importer, TestFileHelper.GetPathForTesting("ValidDeclaration.xml"), new NotificationBuffer());
			AssertEquals("Declaration should not be upgrated", 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
			Assert(!importer.IsImportedDeclarationRejected);
		}

		public void TestImport_InvalidDocument()
		{
			var importer = new TestDeclarationXmlDataImporter(DeclarationValueObjectDataAdapter.New());
			var notify = new NotificationBuffer();

			ImportDeclarationXmlData(importer, TestFileHelper.GetPathForTesting("InvalidXmlDocument.xml"), notify);
			AssertEquals("The user should be notified of an error", true, notify.AsString.IndexOf("There is an error in the XML document") != -1);
		}

		void ImportDeclarationXmlData(DeclarationXmlDataImporter importer, string fileName, NotificationBuffer notify)
		{
			using (var reader = new StreamReader(fileName))
			{
				importer.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
			}
			Factory.Save();
		}

		sealed class TestDeclarationXmlDataImporter : DeclarationXmlDataImporter
		{
			public TestDeclarationXmlDataImporter(DeclarationValueObjectDataAdapter adapter) : base(adapter)
			{
			}

			public new XmlValueObjectSerializer GetSerializer() => base.GetSerializer();
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
