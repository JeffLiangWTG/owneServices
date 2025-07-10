using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class CommonDataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\CommonDataUNDG Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\CommonDataUNDG Result.xml");
			var zipFilePath = Path.Combine(binPath, @"TestFiles\IMO Sample.zip");

			var fileDownloaderWrapper = new FileDownloaderWrapper();
			var extractedFiles = fileDownloaderWrapper.ExtractLocalZipFile(zipFilePath);
			var specProvFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.SpecProv, StringComparison.OrdinalIgnoreCase));
			var stowSegFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.StowSeg, StringComparison.OrdinalIgnoreCase));

			var specProvParser = new SpecProvParser();
			var specProvRecords = specProvParser.Parse(specProvFile);

			var stowSegParser = new StowSegParser();
			var stowSegRecords = stowSegParser.Parse(stowSegFile);

			var parser = new CommonDataParser(stowSegRecords, specProvRecords);
			var commonDataRecords = parser.Parse();

			XmlWriterHelper.ExportToXml(CommonDataXmlWriterConfiguration.GetXmlWriter(new DateTime(1601, 1, 1)), commonDataRecords, resultXmlFilePath);

			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}
	}
}
