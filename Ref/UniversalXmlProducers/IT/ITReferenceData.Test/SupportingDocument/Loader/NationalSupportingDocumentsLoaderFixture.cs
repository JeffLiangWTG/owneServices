using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class NationalSupportingDocumentsLoaderFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new NationalSupportingDocumentsLoader(dateTimeProvider: null, httpClient: new Mock<IHttpClient>().Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new NationalSupportingDocumentsLoader(dateTimeProvider: new Mock<IDateTimeProvider>().Object, httpClient: null), "When httpClient is null");
		}

		[Test]
		public void GetRawSupportingDocuments()
		{
			int postRequestCount = 0;
			dateTimeProviderMock.Setup(x => x.Now).Returns(DateTime.Now);
			httpClientMock.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns(() =>
			{
				string filename = "";
				switch (postRequestCount)
				{
					case 0:
						filename = "NationalSupportingDocumentList.html";
						break;

					case 1:
						filename = "NationalFilledAttributes_01AO.html";
						break;

					case 2:
						filename = "NationalEmptyAttributes_03YY.html";
						break;
				}
				postRequestCount++;
				return File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\", filename));
			});

			var rawSupportingDocumentsLoader = (IRawSupportingDocumentsLoader)new NationalSupportingDocumentsLoader(dateTimeProviderMock.Object, httpClientMock.Object);
			Assert.AreEqual(2, rawSupportingDocumentsLoader.GetRawSupportingDocuments().Count(), "Number of RawSupportingDocuments found");
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
