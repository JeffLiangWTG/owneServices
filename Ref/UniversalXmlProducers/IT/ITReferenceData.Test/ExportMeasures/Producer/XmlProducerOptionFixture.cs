using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExportMeasures
{
	[TestFixture]
	sealed class XmlProducerOptionFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new XmlProducerOption(dateTimeProvider: null, publicationTimeLoader: publicationTimeLoaderMock.Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new XmlProducerOption(dateTimeProvider: dateTimeProviderMock.Object, publicationTimeLoader: null), "When publicationTimeLoader is null");
		}

		[Test]
		public void DataSourceName()
		{
			IXmlProducerOption option = new XmlProducerOption(dateTimeProviderMock.Object, publicationTimeLoaderMock.Object);
			Assert.AreEqual("IT Export Measures", option.DataSourceName);
		}

		[Test]
		public void FileName()
		{
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2025, 1, 25, 11, 22, 33));

			IXmlProducerOption option = new XmlProducerOption(dateTimeProviderMock.Object, publicationTimeLoaderMock.Object);
			Assert.AreEqual("IT Export Measures_20250125112233.xml", option.FileName);
		}

		[Test]
		public void PublicationDateTime()
		{
			publicationTimeLoaderMock.Setup(x => x.GetDateTime()).Returns(new DateTime(2025, 1, 24));

			IXmlProducerOption option = new XmlProducerOption(dateTimeProviderMock.Object, publicationTimeLoaderMock.Object);
			Assert.AreEqual(new DateTime(2025, 1, 24), option.PublicationDateTime);
		}

		[SetUp]
		public void SetUp()
		{
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			publicationTimeLoaderMock = new Mock<IPublicationTimeLoader>();
		}

		Mock<IDateTimeProvider> dateTimeProviderMock;
		Mock<IPublicationTimeLoader> publicationTimeLoaderMock;
	}
}
