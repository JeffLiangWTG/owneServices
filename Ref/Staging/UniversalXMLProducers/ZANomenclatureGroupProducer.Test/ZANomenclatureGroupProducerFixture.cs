using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.Staging.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer.Test
{
	[TestFixture]
	public class ZANomenclatureGroupProducerFixture
	{
		[Test]
		public void ParserAndExport2022Version7()
		{
			var fileDownloaderMock = new Mock<IFileDownloader>();
			var responseStream = new Mock<IResponseStream>();
			var dumpPathNomenclature = Path.Combine(_binPath, "D88B9C41-3135-4BA5-BA78-2451190E3355_dumps", "zanomenclature.xml");
			var dumpPathTariff = Path.Combine(_binPath, "F5AB57A3-042B-4F16-BE62-A8F0DFE185EA_dumps", "zatariff.xml");
			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer.Test.Samples.2022modelv7.pdf"))
			{
				fileDownloaderMock.Setup(f => f.GetFileStream(null)).Returns(responseStream.Object);
				responseStream.Setup(f => f.GetResponseStream()).Returns(stream);

				var parser = new ZANomenclatureGroupParser();
				parser.Parse(fileDownloaderMock.Object);

				parser.ExportToXml(dumpPathNomenclature, dumpPathTariff);

				var xmlTariff = new XmlDocument();
				xmlTariff.Load(dumpPathTariff);

				Assert.IsNotNull(xmlTariff);
				var tariffList = xmlTariff.GetElementsByTagName("RefCusTariff");
				Assert.That(tariffList, Has.Count.EqualTo(8394));
				var tariff = tariffList.Item(0);
				Assert.IsNotNull(tariff != null);
				Assert.That(tariff.ChildNodes, Has.Count.EqualTo(2));
				Assert.That(tariff.ChildNodes[0].Name, Is.EqualTo("ZZ1_CompositeKeyOnZZ5"));
				Assert.That(tariff.ChildNodes[1].Name, Is.EqualTo("ZZ1_TariffCode"));

				Assert.That(tariff.ChildNodes[0].InnerText, Is.EqualTo("01.01..01.2.1"));
				Assert.That(tariff.ChildNodes[1].InnerText, Is.EqualTo("010121"));

				var xmlNomenclatureGroup = new XmlDocument();
				xmlNomenclatureGroup.Load(dumpPathNomenclature);

				Assert.That(xmlNomenclatureGroup != null);
				var nomenclatureList = xmlNomenclatureGroup.GetElementsByTagName("RefCusNomenclatureGroup");
				Assert.That(nomenclatureList, Has.Count.EqualTo(3760));
				var nomenclature = nomenclatureList.Item(0);

				Assert.That(nomenclature.ChildNodes[0].Name, Is.EqualTo("ZZ5_CompositeKey"));
				Assert.That(nomenclature.ChildNodes[1].Name, Is.EqualTo("ZZ5_Description"));
				Assert.That(nomenclature.ChildNodes[2].Name, Is.EqualTo("ZZ5_EndDate"));
				Assert.That(nomenclature.ChildNodes[3].Name, Is.EqualTo("ZZ5_StartDate"));
				Assert.That(nomenclature.ChildNodes[4].Name, Is.EqualTo("ZZ5_Value"));

				Assert.That(nomenclature.ChildNodes[0].InnerText, Is.EqualTo("01"));
				Assert.That(nomenclature.ChildNodes[1].InnerText, Is.EqualTo("LIVE ANIMALS; ANIMAL PRODUCTS"));
				Assert.That(nomenclature.ChildNodes[2].InnerText, Is.EqualTo("2079-06-06T23:59:00"));
				Assert.That(nomenclature.ChildNodes[3].InnerText, Is.EqualTo("2022-01-01T00:00:00"));
				Assert.That(nomenclature.ChildNodes[4].InnerText, Is.EqualTo(""));
			}
		}

		[Test]
		public void ExcludeDuplicateHeadingSubHeading()
		{
			var fileDownloaderMock = new Mock<IFileDownloader>();
			var responseStream = new Mock<IResponseStream>();
			var dumpPathNomenclature = Path.Combine(_binPath, "003FCB52-4E81-4348-B542-0A0FDE565989_dumps", "zanomenclature.xml");
			var dumpPathTariff = Path.Combine(_binPath, "24983C38-FD66-49CA-9E03-A61337AD7BD3_dumps", "zatariff.xml");

			// 44.14 and 4414.00 are effectively duplicates
			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer.Test.Samples.2022modelv7.pdf"))
			{
				fileDownloaderMock.Setup(f => f.GetFileStream(null)).Returns(responseStream.Object);
				responseStream.Setup(f => f.GetResponseStream()).Returns(stream);

				var parser = new ZANomenclatureGroupParser();
				parser.Parse(fileDownloaderMock.Object);
				parser.ExportToXml(dumpPathNomenclature, dumpPathTariff);

				var xmlTariff = new XmlDocument();
				xmlTariff.Load(dumpPathTariff);

				Assert.IsNotNull(xmlTariff);
				var tariffList = xmlTariff.GetElementsByTagName("RefCusTariff");
				Assert.That(tariffList, Has.Count.GreaterThan(0));

				var tariffToCheck = tariffList.Cast<XmlNode>().Where(t => t.ChildNodes.Cast<XmlNode>().Any(x => x.Name == "ZZ1_TariffCode" && x.InnerText == "4414")).ToList();
				Assert.That(tariffToCheck, Is.Not.Null);
				Assert.That(tariffToCheck.Count, Is.EqualTo(0), $"Found unexpected Tariff 4414: {string.Join(", ", tariffToCheck.Select(x => x.InnerXml))}");

				var xmlNomenclatureGroup = new XmlDocument();
				xmlNomenclatureGroup.Load(dumpPathNomenclature);

				Assert.That(xmlNomenclatureGroup != null);
				var nomenclatureList = xmlNomenclatureGroup.GetElementsByTagName("RefCusNomenclatureGroup");
				Assert.That(nomenclatureList, Has.Count.GreaterThan(0));

				var nomToCheck = nomenclatureList.Cast<XmlNode>().Where(n => n.ChildNodes.Cast<XmlNode>().Any(x => x.Name == "ZZ5_Value" && x.InnerText == "4414")).ToList();
				Assert.That(nomToCheck, Is.Not.Null);
				Assert.That(nomToCheck.Count, Is.EqualTo(1), $"Found unexpected Nomenclature: {string.Join(", ", nomToCheck.Select(x => x.InnerXml))}");
			}
		}

		[SetUp]
		public void SetUp()
		{
			_binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		}

		string _binPath;

	}
}
