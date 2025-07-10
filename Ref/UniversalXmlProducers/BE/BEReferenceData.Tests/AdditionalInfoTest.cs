using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class AdditionalInfoTest
	{
		Assembly assembly;
		string outputPath;
		string testFilesInputPath;
		string testFilesOutputPath;

		[Test]
		public void TestGenerateRefCusCodeListXml()
		{

			var fileName = "RefCusCodeListZZ_BE_ADDIN.xml";
			var publicationTime = new DateTime(2020, 9, 1);
			var outputFile = Path.Combine(outputPath, fileName);

			var cusCodeList1 = NPOIWordParser.ReadAdditionalInfoDocFileIntoResults(new List<string>() { Path.Combine(testFilesInputPath, "AdditionalInfo6a.doc"), Path.Combine(testFilesInputPath, "AdditionalInfo6a_fr.doc") }, new List<string>() { "Code" }, new List<string>() { "Onderwerp", "Objet" }, null);
			var cusCodeList2 = NPOIWordParser.ReadAdditionalInfoDocFileIntoResults(new List<string>() { Path.Combine(testFilesInputPath, "AdditionalInfo6c.doc"), Path.Combine(testFilesInputPath, "AdditionalInfo6c_fr.doc") }, new List<string>() { "Code" }, new List<string>() { "Champ d’application", "Toepassingsgebied", "Vermeldingen", "Mentions", "Vermelding vak 44 ED", "Vermelding vak 31 ED", "Mention case 31 D.A.U.", "Mention case 44 D.A.U." }, new List<string>() { @"^\d.*-.+", "^ALG.*" });

			XMLGeneration.ExportToXMLFile("BE Additional Info", outputFile, XMLGeneration.GetRefAddCodesWriterConfiguration(Constants.ZZRefCusCodeList.AdditionalInfo), publicationTime, cusCodeList1.Union(cusCodeList2).ToList());

			var fileContent = File.ReadAllText(outputFile);
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, fileName));

			BEReferenceDataTestHelper.AssertEqualXML(fileContent, expectedContent);
		}

		[SetUp]
		public void Setup()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"AdditionalInfo\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"AdditionalInfo\TestFiles\Input");
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"AdditionalInfo\TestFiles\Output");
		}
	}
}
