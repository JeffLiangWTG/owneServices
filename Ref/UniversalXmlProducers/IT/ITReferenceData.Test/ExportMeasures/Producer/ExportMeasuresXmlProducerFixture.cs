using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExportMeasures
{
	[TestFixture]
	sealed class ExportMeasuresXmlProducerFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresXmlProducer(option: null), "When option is null");

			xmlProducerOption.Setup(x => x.DataSourceName).Returns(value: null);
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresXmlProducer(option: xmlProducerOption.Object), "When option.DataSourceName is null");

			xmlProducerOption.Setup(x => x.DataSourceName).Returns("");
			Assert.Throws<ArgumentException>(() => new ExportMeasuresXmlProducer(option: xmlProducerOption.Object), "When option.DataSourceName is empty");

			xmlProducerOption.Setup(x => x.DataSourceName).Returns("XYZ");
			xmlProducerOption.Setup(x => x.FileName).Returns(value: null);
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresXmlProducer(option: xmlProducerOption.Object), "When option.FileName is null");

			xmlProducerOption.Setup(x => x.FileName).Returns("");
			Assert.Throws<ArgumentException>(() => new ExportMeasuresXmlProducer(option: xmlProducerOption.Object), "When option.FileName is empty");
		}

		[Test]
		public void ExportToXml()
		{
			xmlProducerOption.Setup(x => x.DataSourceName).Returns("IT Export Measures");
			xmlProducerOption.Setup(x => x.FileName).Returns("IT Export Measures_20250125112233.xml");
			xmlProducerOption.Setup(x => x.PublicationDateTime).Returns(new DateTime(2025, 1, 25));

			var expectedOutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportMeasures\\TestFiles\\ExpectedExportedXml.xml");
			var effectiveOutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportMeasures\\TestFiles");
			var producer = new ExportMeasuresXmlProducer(xmlProducerOption.Object);
			producer.ExportToXml(Tariffs, effectiveOutputFilePath);

			var effectiveOutputXmlDocument = new XmlDocument();
			effectiveOutputXmlDocument.Load(Path.Combine(effectiveOutputFilePath, "IT Export Measures_20250125112233.xml"));

			var expectedOutputXmlDocument = new XmlDocument();
			expectedOutputXmlDocument.Load(expectedOutputFilePath);

			Assert.AreEqual(expectedOutputXmlDocument.InnerXml, effectiveOutputXmlDocument.InnerXml);
		}

		[SetUp]
		public void SetUp()
		{
			xmlProducerOption = new Mock<IXmlProducerOption>();
		}

		RefCusTariff[] Tariffs =>
		[
			new RefCusTariff
			{
				ZZ1_TariffCode = "22082086",
				RefCusTariffAdditionalCodes =
				[
					new RefCusTariffAdditionalCode
					{
						ZY2_AdditionalCode = "U001",
						ZY2_Description = "Grappa piemontese o Grappa del Piemonte IG",
						ZY2_ZY3_NKCategory = "ESM",
						RefCusApplicabilities =
						[
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTime(2025, 1, 10),
								ZZT_ZZA_NKTradeGroup = "1011",
							},
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTime(2025, 1, 10),
								ZZT_ZZA_NKTradeGroup = "1014",
							},
						],
					},
					new RefCusTariffAdditionalCode
					{
						ZY2_AdditionalCode = "U002",
						ZY2_Description = "Grappa friulana o Grappa del Friuli IG",
						ZY2_ZY3_NKCategory = "ESM",
						RefCusApplicabilities =
						[
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTime(2025, 1, 10),
								ZZT_ZZA_NKTradeGroup = "1014",
							},
						],
					},
				],
			},
			new RefCusTariff
			{
				ZZ1_TariffCode = "04069063",
				RefCusTariffAdditionalCodes =
				[
					new RefCusTariffAdditionalCode
					{
						ZY2_AdditionalCode = "U017",
						ZY2_Description = "Pecorino romano DOP",
						ZY2_ZY3_NKCategory = "ESM",
						RefCusApplicabilities =
						[
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTime(2024, 1, 15),
								ZZT_ZZA_NKTradeGroup = "1011",
							},
						],
					},
				],
			},
		];

		Mock<IXmlProducerOption> xmlProducerOption;
	}
}
