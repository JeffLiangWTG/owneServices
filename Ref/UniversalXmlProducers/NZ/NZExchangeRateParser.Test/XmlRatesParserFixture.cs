using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.NZExchangeRateParser.Test
{
	[TestFixture]
	public class XmlRatesParserFixture
	{
		[Test]
		public void TestParser()
		{
			using (var exchangeXml = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.NZExchangeRateParser.Test.res.currentExchange.xml"))
			{
				_httpClientMock.Setup(x => x.GetAsync("any")).Returns(Task.FromResult(exchangeXml));

				var parser = new XmlRatesParser("any");
				parser.ParseAsync(_httpClientMock.Object).GetAwaiter().GetResult();
				parser.Save(_dumpPath);

				var savedXml = new XmlDocument();
				savedXml.Load(_dumpPath);

				Assert.That(savedXml != null);
				var exchanges = savedXml.GetElementsByTagName("RefExchangeRateZZ");
				Assert.AreEqual(2, exchanges.Cast<XmlNode>().Count(o => o.ChildNodes[2].InnerText == "AUD"));
				var matched1 = exchanges.Cast<XmlNode>().FirstOrDefault(o => o.ChildNodes[2].InnerText == "AUD");
				Assert.IsNotNull(matched1);
				Assert.True(matched1.ChildNodes[0].InnerText.Trim() == "2019-02-10T00:00:00");
				Assert.True(matched1.ChildNodes[1].InnerText.Trim() == "0.92");
				Assert.True(matched1.ChildNodes[3].InnerText.Trim() == "2019-01-28T00:00:00");
			}
		}

		[Test]
		public void TestParserPublicaltionDate()
		{
			using (var exchangeXml = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.NZExchangeRateParser.Test.res.currentExchange.xml"))
			{
				_httpClientMock.Setup(x => x.GetAsync("any")).Returns(Task.FromResult(exchangeXml));

				var parser = new XmlRatesParser("any");
				parser.PublicationTime = new DateTime(2025, 4, 1, 12, 05, 25);
				parser.ParseAsync(_httpClientMock.Object).GetAwaiter().GetResult();
				parser.Save(_dumpPath);

				var savedXml = new XmlDocument();
				savedXml.Load(_dumpPath);

				Assert.That(savedXml != null);
				var publicationDate = savedXml.GetElementsByTagName("PublicationTime");
				Assert.AreEqual(1, publicationDate.Cast<XmlNode>().Count());
				Assert.AreEqual("2025-04-01T12:05:25", publicationDate.Cast<XmlNode>().FirstOrDefault().ChildNodes[0].InnerText);
			}
		}

		[Test]
		public void TestParserWithIncorrectNode()
		{
			using (var exchangeXml = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.NZExchangeRateParser.Test.res.currentExchange_IncorrectNode.xml"))
			{
				_httpClientMock.Setup(x => x.GetAsync("any")).Returns(Task.FromResult(exchangeXml));

				var parser = new XmlRatesParser("any");
				parser.ParseAsync(_httpClientMock.Object).GetAwaiter().GetResult();
				parser.Save(_dumpPath);

				var savedXml = new XmlDocument();
				savedXml.Load(_dumpPath);

				Assert.That(savedXml != null);
				var exchanges = savedXml.GetElementsByTagName("RefExchangeRateZZ");
				Assert.AreEqual(2, exchanges.Cast<XmlNode>().Count(o => o.ChildNodes[2].InnerText == "AUD"));
				var matched1 = exchanges.Cast<XmlNode>().FirstOrDefault(o => o.ChildNodes[2].InnerText == "AUD");
				Assert.IsNotNull(matched1);
				Assert.True(matched1.ChildNodes[0].InnerText.Trim() == "2019-02-10T00:00:00");
				Assert.True(matched1.ChildNodes[1].InnerText.Trim() == "0.92");
				Assert.True(matched1.ChildNodes[3].InnerText.Trim() == "2019-01-28T00:00:00");

				Assert.AreEqual(0, exchanges.Cast<XmlNode>().Count(o => o.ChildNodes[2].InnerText == "CAD"));

				Assert.AreEqual(0, exchanges.Cast<XmlNode>().Count(o => o.ChildNodes[2].InnerText == "CLP"));

				Assert.AreEqual(0, exchanges.Cast<XmlNode>().Count(o => o.ChildNodes[2].InnerText == "CNY"));
			}
		}

		[SetUp]
		public void SetUp()
		{
			_binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_dumpPath = Path.Combine(_binPath, "8CBEF76B-E605-478A-A8DB-701513D93E0C_dump", "exchange.xml");
			_httpClientMock = new Mock<IHttpClientHelper>();
		}

		Mock<IHttpClientHelper> _httpClientMock;
		string _binPath;
		string _dumpPath;
	}
}
