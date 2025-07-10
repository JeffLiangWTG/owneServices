using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner.Test
{
	[TestFixture]
	public class ContentParserFixtures
	{
		[TestCase("tariff.xml")]
		[TestCase("tariffCamelCase.Xml")]
		[TestCase("tariffUppercase.XML")]
		public async Task CaseInsensitiveXML(string fileName)
		{
			using (var xml = GetType().Assembly.GetManifestResourceStream($"CargoWise.RefDbRepo.SourceDataBinaryScanner.Test.files.{fileName}"))
			{
				_dummySourceData.SDA_Filename = fileName;
				_dummySourceData.SDA_Filetype = "XML";
				var memo = new MemoryStream();
				await xml.CopyToAsync(memo);
				_dummySourceData.SDA_Content = memo.ToArray();
				await _contentParser.XmlContent(_dummySourceData, _universalXmlParserMock.Object, _stagingRepositoryMock.Object);
				_universalXmlParserMock.Verify(o => o.ParseAsync(It.IsAny<Stream>(), false), Times.Once);
			}
		}

		[TestCase("tariff.zip")]
		[TestCase("tariffCamelCase.Zip")]
		[TestCase("tariffUppercase.ZIP")]
		public async Task CaseInsensitiveCompressed(string fileName)
		{
			using (var xml = GetType().Assembly.GetManifestResourceStream($"CargoWise.RefDbRepo.SourceDataBinaryScanner.Test.files.{fileName}"))
			{
				_dummySourceData.SDA_Filename = fileName;
				_dummySourceData.SDA_Filetype = "COM";
				var memo = new MemoryStream();
				await xml.CopyToAsync(memo);
				_dummySourceData.SDA_Content = memo.ToArray();
				await _contentParser.CompressedContent(_dummySourceData, _universalXmlParserMock.Object, _stagingRepositoryMock.Object);
				_universalXmlParserMock.Verify(o => o.ParseAsync(It.IsAny<Stream>(), false), Times.Once);
			}
		}

		[TestCase("tariff.zip")]
		[TestCase("tariffCamelCase.Zip")]
		[TestCase("tariffUppercase.ZIP")]
		public async Task CaseCompressedXMLWithFileHash(string fileName)
		{
			using (var xml = GetType().Assembly.GetManifestResourceStream($"CargoWise.RefDbRepo.SourceDataBinaryScanner.Test.files.{fileName}"))
			{
				// Set up the dummy source data
				_dummySourceData.SDA_Filename = fileName;
				_dummySourceData.SDA_Filetype = "COM";
				var memo = new MemoryStream();
				await xml.CopyToAsync(memo);
				_dummySourceData.SDA_Content = memo.ToArray();

				// Instantiate the actual UniversalXmlParser with its dependencies
				var schemaHandler = new Mock<IUniversalXmlSchemaHandler>().Object;
				var stagingRepositoryWrapper = new Mock<IStagingRepositoryWrapper>().Object;
				var sourceDataWriter = new Mock<ISourceDataWriter>().Object;
				var fileTrace = new Mock<IFileTrace>().Object;

				var universalXmlParser = new UniversalXmlParser.UniversalXmlParser(
					schemaHandler,
					stagingRepositoryWrapper,
					sourceDataWriter,
					fileTrace
				);

				// Call the CompressedContent method with the actual UniversalXmlParser
				await _contentParser.CompressedContent(_dummySourceData, universalXmlParser, _stagingRepositoryMock.Object);
			}
		}

		[SetUp]
		public void Setup()
		{
			_dummySourceData = new SourceData
			{
				SDA_Filename = "any",
				SDA_PK = Guid.NewGuid()
			};
			_universalXmlParserMock = new Mock<IUniversalXmlParser>();
			_stagingRepositoryMock = new Mock<IStagingRepository>();
			_contentParser = new ContentParser();
		}

		ContentParser _contentParser;
		SourceData _dummySourceData;
		Mock<IUniversalXmlParser> _universalXmlParserMock;
		Mock<IStagingRepository> _stagingRepositoryMock;
	}
}
