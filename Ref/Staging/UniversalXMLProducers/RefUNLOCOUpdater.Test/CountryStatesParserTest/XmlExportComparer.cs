using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test
{
	[TestFixture]
	public class XmlExportComparer
	{
		[Test]
		[Platform("64-bit", Reason = "Only support 64 bit ODBC driver")]
		[Property("DAT:CapabilityRequirements", (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc)]
		public void XmlFileComparerTest()
		{
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

			var helperMock = new Mock<IHttpClientHelper>();
			var htmlContent = string.Empty;

			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.unece_codes.html"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				htmlContent = reader.ReadToEnd();
			}

			helperMock.Setup(x => x.GetWebPageAsync(MdbTestHelper.Url)).Returns(Task.FromResult(htmlContent));
			var scrapeWebPage = new ScrapePageHTML(helperMock.Object, MdbTestHelper.Url);

			var mdbFile = MdbTestHelper.DownloadMdbFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.UNCountryStatesExpectedMdb.zip", _mdbDownloadPath, GetType());

			var parser = new CountryStatesParser(scrapeWebPage.PublicationTime, _dumpPath, mdbFile);
			parser.GenerateXml();
			var expectedExportFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ref\UNCountryStatesExpectedResultXml.xml");

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(_dumpPath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Directory.Delete(_mdbDownloadPath, true);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[SetUp]
		public void SetUp()
		{
			_binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_dumpPath = Path.Combine(_binPath, "8CBEF76B-E605-478A-A8DB-701513D93E0C_dump", "countryStates.xml");
			_mdbDownloadPath = Path.Combine(_binPath, "TestDownloads_29C9C64E-FB32-401E-89C5-8A8B4975BCBA");
		}

		string _binPath;
		string _dumpPath;
		string _mdbDownloadPath;
	}
}

