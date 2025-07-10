using System;
using System.Linq;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;
using Types = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListCodeTypes;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	sealed class RefDataRepoEuropeanSupportingDocumentCachedLoaderFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Multiple(() =>
			{
				Assert.Throws<ArgumentNullException>(() => new RefDataRepoEuropeanSupportingDocumentCachedLoader(dateTimeProvider: null, httpClient: new Mock<IHttpClient>().Object, Types.SupportingDocumentNcts), "When dateTimeProvider is null");
				Assert.Throws<ArgumentNullException>(() => new RefDataRepoEuropeanSupportingDocumentCachedLoader(dateTimeProvider: new Mock<IDateTimeProvider>().Object, httpClient: null, Types.SupportingDocumentNcts), "When httpClient is null");
				Assert.Throws<ArgumentNullException>(() => new RefDataRepoEuropeanSupportingDocumentCachedLoader(dateTimeProvider: new Mock<IDateTimeProvider>().Object, httpClient: new Mock<IHttpClient>().Object, null), "When codeType is null");
				Assert.Throws<ArgumentException>(() => new RefDataRepoEuropeanSupportingDocumentCachedLoader(dateTimeProvider: new Mock<IDateTimeProvider>().Object, httpClient: new Mock<IHttpClient>().Object, ""), "When codeType is empty");
			});
		}

		[Test]
		public void GetRawSupportingDocuments_Ncts()
		{
			var now = DateTime.UtcNow;
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(now);

			var httpClientMock = new Mock<IHttpClient>();
			httpClientMock.Setup(x => x.Get(It.IsAny<string>())).Returns(@"
{
  ""@odata.context"": ""http://refdbrepoupdate.wisecloud.zone/Update/odata/$metadata#RefCusCodeListUpdate"",
  ""value"": [
	{
	  ""ZZD_PK"": ""c1c9b220-d503-4c39-a880-5c7346d6d484"",
	  ""ZZD_ZZK_NKCodeType"": ""DC44N"",
	  ""ZZD_Code"": ""C085"",
	  ""ZZD_Description"": ""Common Health Entry Document"",
	  ""ZZD_StartDate"": ""2024-10-04T00:00:00Z"",
	  ""ZZD_EndDate"": ""2079-06-06T23:59:00Z"",
	  ""ZZD_ZZZ_NKDataGrouping"": ""EUN"",
	  ""ZZD_SysStartTime"": ""2024-10-07T00:27:38.3151383Z"",
	  ""ZZD_SysEndTime"": ""9999-12-31T23:59:59.9999999Z""
	},
	{
	  ""ZZD_PK"": ""fd7d50da-c27f-4f81-8aef-575f7f651f26"",
	  ""ZZD_ZZK_NKCodeType"": ""DC44N"",
	  ""ZZD_Code"": ""C400"",
	  ""ZZD_Description"": ""Presentation of the required \u0022CITES\u0022 certificate"",
	  ""ZZD_StartDate"": ""2024-10-18T00:00:00Z"",
	  ""ZZD_EndDate"": ""2079-06-06T23:59:00Z"",
	  ""ZZD_ZZZ_NKDataGrouping"": ""EUN"",
	  ""ZZD_SysStartTime"": ""2024-10-21T00:27:38.4429474Z"",
	  ""ZZD_SysEndTime"": ""9999-12-31T23:59:59.9999999Z""
	}
]
}");

			var rawSupportingDocumentsLoader = (IRawSupportingDocumentsLoader)new RefDataRepoEuropeanSupportingDocumentCachedLoader(dateTimeProviderMock.Object, httpClientMock.Object, Types.SupportingDocumentNcts);
			var result = rawSupportingDocumentsLoader.GetRawSupportingDocuments();

			AssertHttpRequest(httpClientMock, now, Types.SupportingDocumentNcts);

			Assert.AreEqual(2, result.Count(), "Number of RawSupportingDocuments found");

			var c085Document = result.OrderBy(x => x.Code).First();
			Assert.Multiple(() =>
			{
				Assert.That(c085Document.Code, Is.EqualTo("C085"), "Document code");
				Assert.That(c085Document.Description, Is.EqualTo("Common Health Entry Document"), "Description");
				Assert.That(c085Document.StartDate, Is.EqualTo(new DateTime(2024, 10, 04, 0, 0, 0)), "Start Date");
				Assert.That(c085Document.EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 0)), "End Date");
				Assert.That(c085Document.Type, Is.EqualTo(SupportingDocumentType.European), "Type");
			});

			var c400Document = result.OrderBy(x => x.Code).Last();
			Assert.Multiple(() =>
			{
				Assert.That(c400Document.Code, Is.EqualTo("C400"), "Document code");
				Assert.That(c400Document.Description, Is.EqualTo("Presentation of the required \u0022CITES\u0022 certificate"), "Description");
				Assert.That(c400Document.StartDate, Is.EqualTo(new DateTime(2024, 10, 18, 0, 0, 0)), "Start Date");
				Assert.That(c400Document.EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 0)), "End Date");
				Assert.That(c400Document.Type, Is.EqualTo(SupportingDocumentType.European), "Type");
			});

			Assert.AreSame(result, rawSupportingDocumentsLoader.GetRawSupportingDocuments());
		}

		[Test]
		public void GetRawSupportingDocuments_AdditionalReference()
		{
			var now = DateTime.UtcNow;
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(now);

			var httpClientMock = new Mock<IHttpClient>();
			httpClientMock.Setup(x => x.Get(It.IsAny<string>())).Returns(@"
{
  ""@odata.context"": ""http://refdbrepoupdate.wisecloud.zone/Update/odata/$metadata#RefCusCodeListUpdate"",
  ""value"": [
    {
      ""ZZD_PK"": ""8f698750-5228-4e21-8cdd-06d8546e2c0f"",
      ""ZZD_ZZK_NKCodeType"": ""AR44E"",
      ""ZZD_Code"": ""Y006"",
      ""ZZD_Description"": ""Stamp (at beginning/end of each piece) and directly transported"",
      ""ZZD_StartDate"": ""2018-02-27T00:00:00Z"",
      ""ZZD_EndDate"": ""2079-06-06T23:59:00Z"",
      ""ZZD_ZZZ_NKDataGrouping"": ""EUN"",
      ""ZZD_SysStartTime"": ""2023-02-10T03:09:40.6960506Z"",
      ""ZZD_SysEndTime"": ""9999-12-31T23:59:59.9999999Z""
    },
    {
      ""ZZD_PK"": ""a9a79b55-d4d1-409a-a911-c6b3dd369472"",
      ""ZZD_ZZK_NKCodeType"": ""AR44E"",
      ""ZZD_Code"": ""Y007"",
      ""ZZD_Description"": ""Seal (fixed to each piece) and directly transported"",
      ""ZZD_StartDate"": ""2018-02-27T00:00:00Z"",
      ""ZZD_EndDate"": ""2079-06-06T23:59:00Z"",
      ""ZZD_ZZZ_NKDataGrouping"": ""EUN"",
      ""ZZD_SysStartTime"": ""2023-02-10T03:09:40.6960506Z"",
      ""ZZD_SysEndTime"": ""9999-12-31T23:59:59.9999999Z""
    }
]
}");

			var rawSupportingDocumentsLoader = (IRawSupportingDocumentsLoader)new RefDataRepoEuropeanSupportingDocumentCachedLoader(dateTimeProviderMock.Object, httpClientMock.Object, Types.SupportingDocumentAdditionalReference);
			var result = rawSupportingDocumentsLoader.GetRawSupportingDocuments();

			AssertHttpRequest(httpClientMock, now, Types.SupportingDocumentAdditionalReference);

			Assert.AreEqual(2, result.Count(), "Number of RawSupportingDocuments found");

			var y006Document = result.OrderBy(x => x.Code).First();
			Assert.Multiple(() =>
			{
				Assert.That(y006Document.Code, Is.EqualTo("Y006"), "Document code");
				Assert.That(y006Document.Description, Is.EqualTo("Stamp (at beginning/end of each piece) and directly transported"), "Description");
				Assert.That(y006Document.StartDate, Is.EqualTo(new DateTime(2018, 2, 27, 0, 0, 0)), "Start Date");
				Assert.That(y006Document.EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 0)), "End Date");
				Assert.That(y006Document.Type, Is.EqualTo(SupportingDocumentType.European), "Type");
			});

			var y007Document = result.OrderBy(x => x.Code).Last();
			Assert.Multiple(() =>
			{
				Assert.That(y007Document.Code, Is.EqualTo("Y007"), "Document code");
				Assert.That(y007Document.Description, Is.EqualTo("Seal (fixed to each piece) and directly transported"), "Description");
				Assert.That(y007Document.StartDate, Is.EqualTo(new DateTime(2018, 2, 27, 0, 0, 0)), "Start Date");
				Assert.That(y007Document.EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 0)), "End Date");
				Assert.That(y007Document.Type, Is.EqualTo(SupportingDocumentType.European), "Type");
			});

			Assert.AreSame(result, rawSupportingDocumentsLoader.GetRawSupportingDocuments());
		}

		void AssertHttpRequest(Mock<IHttpClient> httpClientMock, DateTime now, string codeType)
		{
			httpClientMock.Verify(x => x.Get(
				It.Is<string>(url =>
					url.Contains($"ZZD_ZZK_NKCodeType EQ '{codeType}'") &&
					url.Contains("ZZD_ZZZ_NKDataGrouping EQ 'EUN'") &&
					url.Contains($"ZZD_StartDate LE {now:yyyy-MM-ddTHH:mm:ssZ}") &&
					url.Contains($"ZZD_EndDate GE {now:yyyy-MM-ddTHH:mm:ssZ}"))),
				Times.Once);
		}
	}
}
