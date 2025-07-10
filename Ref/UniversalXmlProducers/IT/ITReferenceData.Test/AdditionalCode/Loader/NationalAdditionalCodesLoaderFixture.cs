using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class NationalAdditionalCodesLoaderFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new NationalAdditionalCodesLoader(dateTimeProvider: null, httpClient: new Mock<IHttpClient>().Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new NationalAdditionalCodesLoader(dateTimeProvider: new Mock<IDateTimeProvider>().Object, httpClient: null), "When httpClient is null");
		}

		[Test]
		public void GetRawAdditionalCodes()
		{
			int postRequestCount = 0;
			dateTimeProviderMock.Setup(x => x.Now).Returns(DateTime.Now);
			httpClientMock.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns(() =>
			{
				string filename = "";
				switch (postRequestCount)
				{
					case 0:
						filename = "NationalAdditionalCodeTypesList.html";
						break;

					case 1:
						filename = "NationalAdditionalCodeListOfTypeQ.html";
						break;

					case 2:
						filename = "NationalAdditionalCodeListOfTypeZ.html";
						break;

					case 3:
						filename = "NationalAdditionalCode_Q001.html";
						break;

					case 4:
						filename = "NationalAdditionalCode_Z051.html";
						break;
				}
				postRequestCount++;
				return File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AdditionalCode\TestFiles\", filename));
			});

			var rawAdditionalCodesLoader = (IRawAdditionalCodesLoader)new NationalAdditionalCodesLoader(dateTimeProviderMock.Object, httpClientMock.Object);
			Assert.AreEqual(2, rawAdditionalCodesLoader.GetRawAdditionalCodes().Count(), "Number of RawAdditionalCodes found");
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
