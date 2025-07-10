using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class XmlProducerOptionFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new XmlProducerOption(dateTimeProvider: null, publicationTimeLoader: new Mock<IPublicationTimeLoader>().Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new XmlProducerOption(dateTimeProvider: new Mock<IDateTimeProvider>().Object, publicationTimeLoader: null), "When publicationTimeLoader is null");
		}

		[Test]
		public void DataSourceName()
		{
			IXmlProducerOption option = new XmlProducerOption(dateTimeProviderMock.Object, publicationTimeLoaderMock.Object);
			Assert.AreEqual("IT Taric AIDA Supporting Documents", option.DataSourceName);
		}

		[Test]
		public void FileName()
		{
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 01, 01, 11, 22, 33));

			IXmlProducerOption option = new XmlProducerOption(dateTimeProviderMock.Object, publicationTimeLoaderMock.Object);
			Assert.AreEqual("IT Taric AIDA Supporting Documents_20220101112233.xml", option.FileName);
		}

		[Test]
		public void PublicationDateTime()
		{
			publicationTimeLoaderMock.Setup(x => x.GetDateTime()).Returns(new DateTime(2022, 06, 13));

			IXmlProducerOption option = new XmlProducerOption(dateTimeProviderMock.Object, publicationTimeLoaderMock.Object);
			Assert.AreEqual(new DateTime(2022, 06, 13), option.PublicationDateTime);
		}

		[SetUp]
		protected void SetUp()
		{
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			publicationTimeLoaderMock = new Mock<IPublicationTimeLoader>();
		}

		Mock<IDateTimeProvider> dateTimeProviderMock;
		Mock<IPublicationTimeLoader> publicationTimeLoaderMock;
	}
}
