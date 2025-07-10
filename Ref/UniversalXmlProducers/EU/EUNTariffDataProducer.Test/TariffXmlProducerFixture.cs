using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class TariffXmlProducerFixture
	{
		[Test]
		public void XmlProducerOutputTest()
		{
			var tariff = new RefCusTariff[]
				{ new RefCusTariff() {
					ZZ1_TariffCode = "10101010",
					ZZ1_StartDate = DateTime.MinValue,
					ZZ1_EndDate = DateTime.MaxValue,
					ZZ1_Description = "Description",
					ZZ1_CompositeKeyOnZZ5 = "1010",
					RefCusTariffUOMs = new RefCusTariffUOM[]
					{ new RefCusTariffUOM() { ZZ8_Type = "CU1", ZZ8_UOM = "KG", ZZ8_ZZZ_NKDataGrouping = "EUN" } }
				} };
			IXmlProducer<RefCusTariff> xmlProducer;
			xmlProducer = new ImportTariffXmlProducer();
			var expectedResultXml = "ExpectedXML.xml";
			var outputFile = "OutputXML.xml";
			var exportFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\" + outputFile + "");
			var expectedExportFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\" + expectedResultXml + "");
			xmlProducer.InitializeWriter(DateTime.MinValue, "EUN Test Fixture");
			xmlProducer.ExportToXml(tariff.ToArray(), exportFilepath);
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}
	}
}
