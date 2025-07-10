using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

[TestFixture]
class DownloadExchangeRatesTests
{
	[Test]
	public void TestDownloadXML()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var exchangeRatesURL = Path.Combine(Path.GetDirectoryName(assembly.Location), @"Business\ExchangeRates\TestFiles\Input\eurofxref-hist-90d-download.xml");
		var ratesXML = DownloadExchangeRates.Download(exchangeRatesURL);
		var xmlNamespaceManager = new XmlNamespaceManager(ratesXML.NameTable);
		xmlNamespaceManager.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");
		xmlNamespaceManager.AddNamespace("lo", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
		Assert.That(ratesXML.SelectNodes("//lo:Cube[@currency]", xmlNamespaceManager).Count, Is.EqualTo(64));
	}

	[Test]
	public void TestDownloadInvalidXML()
	{
		var exception = Assert.Throws<ExchangeRatesException>(() => DownloadExchangeRates.Download("InvalidURL"));
		Assert.That(exception.Message, Is.EqualTo("Unable to Load ES Exchange Rate XML from the following URL: InvalidURL"));
	}
}
