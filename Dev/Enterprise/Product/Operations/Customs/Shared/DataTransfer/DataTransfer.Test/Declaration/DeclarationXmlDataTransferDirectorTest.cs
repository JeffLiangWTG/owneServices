using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class DeclarationXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestNewXmlDataImporter_ReturnsCorrectImporterType()
		{
			AssertType<DeclarationXmlDataImporter>(director.NewXmlDataImporter());
		}

		TestDeclarationXmlDataTransferDirector director;

		protected override void SetUp()
		{
			base.SetUp();
			director = new TestDeclarationXmlDataTransferDirector(DeclarationValueObjectDataAdapter.New(), false);
		}

		sealed class TestDeclarationXmlDataTransferDirector : DeclarationXmlDataTransferDirector
		{
			public TestDeclarationXmlDataTransferDirector(DeclarationValueObjectDataAdapter adapter, bool hasLicence)
				: base(adapter, hasLicence)
			{
			}

			public new XmlDataImporter NewXmlDataImporter() => base.NewXmlDataImporter();
		}
	}
}
