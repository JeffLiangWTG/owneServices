using System;
using System.IO;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test
{
	[TestFixture]
	class PublicationTimeLoaderFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new PublicationTimeLoader(dateTimeProvider: null, httpClient: new Mock<IHttpClient>().Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new PublicationTimeLoader(dateTimeProvider: new Mock<IDateTimeProvider>().Object, httpClient: null), "When httpClient is null");
		}

		[Test]
		public void GetDateTime()
		{
			var taricServletHomePageHtml = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Shared\TestFiles\TaricServletHomePageWithValidPublicationDate.html"));
			httpClientMock.Setup(x => x.Get(It.IsAny<string>())).Returns(taricServletHomePageHtml);

			IPublicationTimeLoader loader = new PublicationTimeLoader(dateTimeProviderMock.Object, httpClientMock.Object);
			Assert.AreEqual(new DateTime(2022, 06, 13), loader.GetDateTime(), "Publication DateTime");
		}

		[Test]
		public void GetDateTimeDefaultIfNotAvailableFromCustomsSite()
		{
			var taricServletHomePageHtml = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Shared\TestFiles\TaricServletHomePageWithMissingPublicationDate.html"));
			httpClientMock.Setup(x => x.Get(It.IsAny<string>())).Returns(taricServletHomePageHtml);
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 01, 01));
			IPublicationTimeLoader loader = new PublicationTimeLoader(dateTimeProviderMock.Object, httpClientMock.Object);
			Assert.AreEqual(new DateTime(2022, 01, 01), loader.GetDateTime(), "Publication DateTime");
		}

		[SetUp]
		protected void SetUp()
		{
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			httpClientMock = new Mock<IHttpClient>();
		}

		Mock<IDateTimeProvider> dateTimeProviderMock;
		Mock<IHttpClient> httpClientMock;
	}
}
