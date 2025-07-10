using System.Xml;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public class XmlDeclarationDataImporterTest : DeclarationDataImporterTest
	{
		public void TestConstructor()
		{
			AssertNotNull(importer);
		}

		protected override void SetUp()
		{
			base.SetUp();
			XmlDocument xmlDoc = new XmlDocument();
			importer = new XmlDataImporterTestClass(xmlDoc, declaration);
		}

		XmlDataImporterTestClass importer;

		sealed class XmlDataImporterTestClass : XmlDeclarationDataImporter
		{
			public XmlDataImporterTestClass(XmlDocument xmlDoc, BaseJobDeclaration toJobDec) : base(xmlDoc, toJobDec)
			{
			}

			public override void Import()
			{
			}

			public override int FailedRecordCount => 0;

			public override int ProcessedRecordCount => 0;

			public override int TotalRecordCount => 0;
		}
	}
}
