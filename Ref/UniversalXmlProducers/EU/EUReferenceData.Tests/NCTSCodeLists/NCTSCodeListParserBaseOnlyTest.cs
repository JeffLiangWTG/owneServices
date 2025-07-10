using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NCTSCodeListParserBaseOnlyTest
	{
		[Test]
		public void InvalidRecord()
		{
			var codeListDetail = new NctsAdditionalInformation();
			var inputFile = TestHelper.ReadManifestResourceContentAsStream($"CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.TestFiles.Input.RD_NCTS-P5_AdditionalInformation_MissingEnDescription.xml");
			var parser = new NCTSCodeListParser(errorBuilder);
			var result = parser.ParseXML(XDocument.Load(inputFile), codeListDetail);
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(errorBuilder.ToString(), Contains.Substring($"English description missing => Skip record! NctsCodeListXmlProducer: DataSource '{codeListDetail.DataSource}', CodeType '{codeListDetail.CodeType}, dataItem '20100'"));
		}

		[SetUp]
		public void Setup()
		{
			errorBuilder = new StringBuilder();
		}
		StringBuilder errorBuilder;
	}
}
