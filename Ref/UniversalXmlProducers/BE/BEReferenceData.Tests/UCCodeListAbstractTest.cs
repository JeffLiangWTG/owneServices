using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.BEReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	public abstract class UCCodeListAbstractTest
	{
		string downloadDir;
		string inputDir;

		protected abstract string codeType { get; }
		protected abstract string dataSource { get; }

		[Test]
		public void TestGenerateRefCusCodeListXml()
		{
			var fileName = codeType + ".xml";
			var inputFileName = codeType + ".json";
			var outputFullPath = Path.Combine(inputDir, "AdditionalInformation.json");

			TestHelper.SimulateDownload(outputFullPath, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.UCCCodeListBase.TestFiles.Input." + inputFileName);

			UCCCodeListProgram.Run(new System.DateTime(2020, 09, 01), inputDir, downloadDir);

			var effectiveOutputXmlDocument = new XmlDocument();
			effectiveOutputXmlDocument.Load(Path.Combine(downloadDir, "RefCusCodeListZZ_AI44I.xml"));

			var expectedContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.UCCCodeListBase.TestFiles.Output." + fileName);

			BEReferenceDataTestHelper.AssertEqualXML(effectiveOutputXmlDocument.OuterXml, expectedContent);

		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.GetTempPath();

			downloadDir = TempFolder + "UxmlFiles";
			inputDir = TempFolder + @"UniversalXMLProducers\BE\Content\";

			Directory.CreateDirectory(downloadDir);
			Directory.CreateDirectory(inputDir);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(downloadDir))
			{
				Directory.Delete(downloadDir, true);
			}
			if (Directory.Exists(inputDir))
			{
				Directory.Delete(inputDir, true);
			}
		}

		string TempFolder;
		#endregion
	}
}
