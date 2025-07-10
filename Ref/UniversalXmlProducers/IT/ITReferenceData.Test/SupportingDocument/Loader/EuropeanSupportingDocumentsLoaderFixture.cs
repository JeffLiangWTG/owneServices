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
	class EuropeanSupportingDocumentsLoaderFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new EuropeanSupportingDocumentsLoader(dateTimeProvider: null, httpClient: new Mock<IHttpClient>().Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new EuropeanSupportingDocumentsLoader(dateTimeProvider: new Mock<IDateTimeProvider>().Object, httpClient: null), "When httpClient is null");
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
						filename = "EuropeanSupportingDocumentTypesList.html";
						break;

					case 1:
						filename = "EuropeanSupportingDocumentListOfTypeA.html";
						break;

					case 2:
						filename = "EuropeanSupportingDocumentListOfTypeD.html";
						break;

					case 3:
						filename = "EuropeanSupportingDocumentListOfTypeI.html";
						break;

					case 4:
						filename = "EuropeanSupportingDocument_A001.html";
						break;

					case 5:
						filename = "EuropeanSupportingDocument_D005.html";
						break;
				}
				postRequestCount++;
				return File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\", filename));
			});

			var rawSupportingDocumentsLoader = (IRawSupportingDocumentsLoader)new EuropeanSupportingDocumentsLoader(dateTimeProviderMock.Object, httpClientMock.Object);
			Assert.AreEqual(3, rawSupportingDocumentsLoader.GetRawSupportingDocuments().Count(), "Number of RawSupportingDocuments found");
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
