using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class SupportingDocumentsXmlProducerFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new SupportingDocumentsXmlProducer(option: null), "When option is null");

			var xmlProducerOption = new Mock<IXmlProducerOption>();
			xmlProducerOption.Setup(x => x.DataSourceName).Returns(value: null);
			Assert.Throws<ArgumentNullException>(() => new SupportingDocumentsXmlProducer(option: xmlProducerOption.Object), "When option.DataSourceName is null");

			xmlProducerOption.Setup(x => x.DataSourceName).Returns("");
			Assert.Throws<ArgumentException>(() => new SupportingDocumentsXmlProducer(option: xmlProducerOption.Object), "When option.DataSourceName is empty");

			xmlProducerOption.Setup(x => x.DataSourceName).Returns("XYZ");
			xmlProducerOption.Setup(x => x.FileName).Returns(value: null);
			Assert.Throws<ArgumentNullException>(() => new SupportingDocumentsXmlProducer(option: xmlProducerOption.Object), "When option.FileName is null");

			xmlProducerOption.Setup(x => x.FileName).Returns("");
			Assert.Throws<ArgumentException>(() => new SupportingDocumentsXmlProducer(option: xmlProducerOption.Object), "When option.FileName is empty");
		}

		[Test]
		public void ExportToXml()
		{
			var xmlProducerOption = new Mock<IXmlProducerOption>();
			xmlProducerOption.Setup(x => x.DataSourceName).Returns("IT Taric AIDA Supporting Documents");
			xmlProducerOption.Setup(x => x.FileName).Returns("IT Taric AIDA Supporting Documents_20220101112233.xml");
			xmlProducerOption.Setup(x => x.PublicationDateTime).Returns(new DateTime(2022, 03, 25));

			var expectedOutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SupportingDocument\\TestFiles\\ExpectedExportedXml.xml");
			var effectiveOutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SupportingDocument\\TestFiles");
			var producer = new SupportingDocumentsXmlProducer(xmlProducerOption.Object);
			producer.ExportToXml(new List<RefCusCodeList>() { GetRefCusCodeList1(), GetRefCusCodeList2() }, effectiveOutputFilePath);

			var effectiveOutputXmlDocument = new XmlDocument();
			effectiveOutputXmlDocument.Load(effectiveOutputFilePath + "\\IT Taric AIDA Supporting Documents_20220101112233.xml");

			var expectedOutputXmlDocument = new XmlDocument();
			expectedOutputXmlDocument.Load(expectedOutputFilePath);

			Assert.AreEqual(expectedOutputXmlDocument.InnerXml, expectedOutputXmlDocument.InnerXml);
		}

		RefCusCodeList GetRefCusCodeList1()
		{
			return new RefCusCodeList()
			{
				ZZD_ZZK_NKCodeType = "DC44I",
				ZZD_Code = "01AO",
				ZZD_Description = "Autorizzazione Ministero dello Sviluppo Economico ai fini del rispetto dell'obbligo di cui all'art. 2-quater del D.L. 10 gennaio 2006, n. 2 e successive modifiche.",
				ZZD_StartDate = new DateTime(2013, 05, 31),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[2]
				{
					new RefCusCodeListAttribute()
					{
						ZZE_Value = "Y",
						ZZE_ZXE_NKName = "Retroactive"
					},
					new RefCusCodeListAttribute()
					{
						ZZE_Value = "Y",
						ZZE_ZXE_NKName = "ElectronicFolder"
					}
				}
			};
		}

		RefCusCodeList GetRefCusCodeList2()
		{
			return new RefCusCodeList()
			{
				ZZD_ZZK_NKCodeType = "DC44I",
				ZZD_Code = "N002",
				ZZD_Description = "Certificato di conformità alle norme di commercializzazione dell'Unione europea applicabili agli ortofrutticoli freschi.",
				ZZD_StartDate = new DateTime(2004, 07, 01),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[2]
				{
					new RefCusCodeListAttribute()
					{
						ZZE_Value = "Y",
						ZZE_ZXE_NKName = "Year"
					},
					new RefCusCodeListAttribute()
					{
						ZZE_Value = "Y",
						ZZE_ZXE_NKName = "Quantity"
					}
				}
			};
		}
	}
}
