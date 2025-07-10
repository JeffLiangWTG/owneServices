using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	abstract class NCTSCodeListParserAbstractTest
	{
		protected NCTSCodeListParserAbstractTest(string inputFileName, IUCCExportCodeListDetail codeListDetail, int resultCount, string inputFileDir = null)
		{
			InputFileName = inputFileName;
			CodeListDetail = codeListDetail;
			ResultCount = resultCount;
			if (!string.IsNullOrEmpty(inputFileDir))
			{
				InputFileDir = inputFileDir;
			}
		}

		[Test]
		public void ParseXML()
		{
			var inputFile = TestHelper.ReadManifestResourceContentAsStream($"{InputFileDir}.{InputFileName}");
			var parser = new NCTSCodeListParser(new StringBuilder());
			var result = parser.ParseXML(XDocument.Load(inputFile), CodeListDetail);
			Assert.That(result.Count(), Is.EqualTo(ResultCount));
		}

		[SetUp]
		public void Setup()
		{
		}

		readonly string InputFileName;
		readonly IUCCExportCodeListDetail CodeListDetail;
		readonly int ResultCount;
		readonly string InputFileDir = "CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.TestFiles.Input";
	}
}
