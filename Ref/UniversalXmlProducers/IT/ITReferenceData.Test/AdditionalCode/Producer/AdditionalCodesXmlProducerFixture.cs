using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class AdditionalCodesXmlProducerFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new AdditionalCodesXmlProducer(option: null), "When option is null");

			var xmlProducerOption = new Mock<IXmlProducerOption>();
			xmlProducerOption.Setup(x => x.DataSourceName).Returns(value: null);
			Assert.Throws<ArgumentNullException>(() => new AdditionalCodesXmlProducer(option: xmlProducerOption.Object), "When option.DataSourceName is null");

			xmlProducerOption.Setup(x => x.DataSourceName).Returns("");
			Assert.Throws<ArgumentException>(() => new AdditionalCodesXmlProducer(option: xmlProducerOption.Object), "When option.DataSourceName is empty");

			xmlProducerOption.Setup(x => x.DataSourceName).Returns("XYZ");
			xmlProducerOption.Setup(x => x.FileName).Returns(value: null);
			Assert.Throws<ArgumentNullException>(() => new AdditionalCodesXmlProducer(option: xmlProducerOption.Object), "When option.FileName is null");

			xmlProducerOption.Setup(x => x.FileName).Returns("");
			Assert.Throws<ArgumentException>(() => new AdditionalCodesXmlProducer(option: xmlProducerOption.Object), "When option.FileName is empty");
		}

		[Test]
		public void ExportToXml()
		{
			var xmlProducerOption = new Mock<IXmlProducerOption>();
			xmlProducerOption.Setup(x => x.DataSourceName).Returns("IT Taric AIDA Additional Codes");
			xmlProducerOption.Setup(x => x.FileName).Returns("IT Taric AIDA Additional Codes_20220101112233.xml");
			xmlProducerOption.Setup(x => x.PublicationDateTime).Returns(new DateTime(2022, 03, 25));

			var expectedOutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AdditionalCode\\TestFiles\\ExpectedExportedXml.xml");
			var effectiveOutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AdditionalCode\\TestFiles");
			var producer = new AdditionalCodesXmlProducer(xmlProducerOption.Object);
			producer.ExportToXml(new List<RefCusCodeList>() { GetRefCusCodeList1(), GetRefCusCodeList2() }, effectiveOutputFilePath);

			var effectiveOutputXmlDocument = new XmlDocument();
			effectiveOutputXmlDocument.Load(effectiveOutputFilePath + "\\IT Taric AIDA Additional Codes_20220101112233.xml");

			var expectedOutputXmlDocument = new XmlDocument();
			expectedOutputXmlDocument.Load(expectedOutputFilePath);

			Assert.AreEqual(expectedOutputXmlDocument.InnerXml, effectiveOutputXmlDocument.InnerXml);
		}

		RefCusCodeList GetRefCusCodeList2()
		{
			return new RefCusCodeList()
			{
				ZZD_Code = "Q001",
				ZZD_Description = "Destinati all'alimentazione umana(Tabella A,punto 7,parte 111, DPR 633/72).",
				ZZD_StartDate = new DateTime(2003, 07, 1),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
			};
		}

		RefCusCodeList GetRefCusCodeList1()
		{
			return new RefCusCodeList()
			{
				ZZD_Code = "Z051",
				ZZD_Description = "Manufatti con  singolo impiego (MACSI) esclusi dall'applicazione dell'imposta di cui all'articolo 1, comma da 634 a 650, della legge 27 dicembre 2019, n.160 o per i quali detta imposta non è dovuta",
				ZZD_StartDate = new DateTime(2020, 10, 20),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
			};
		}
	}
}
