using System;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	[TestFixture]
	abstract class CMRReferenceDataParserAbstractTest : CommonCMRDataParserAbstractTest
	{
		protected abstract DateTime PublishedDate { get; }

		protected abstract ICMRDataParser Parser { get; }

		[Test]
		public void TestCMRRefDataParser()
		{
			var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", TestFileFolderName);
			using (var txtStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, TextFileName)))
			using (var txtReader = new StreamReader(txtStream))
			using (var expectedXmlStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, XMLFileName)))
			using (var expectedXmlReader = new StreamReader(expectedXmlStream))
			{
				var outputFolderPath = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "TestFiles");
				Directory.CreateDirectory(outputFolderPath);

				Parser.Parse(txtReader.ReadToEnd(), PublishedDate, outputFolderPath);

				var outputFilePath = Path.Combine(outputFolderPath, XMLFileName);
				using (var actualXmlStream = new FileStream(outputFilePath, FileMode.Open))
				using (var actualXmlReader = new StreamReader(actualXmlStream))
				{
					var expected = expectedXmlReader.ReadToEnd();
					var actual = actualXmlReader.ReadToEnd();
					Assert.AreEqual(expected, actual);
				}
				File.Delete(outputFilePath);
			}
		}
	}
}
