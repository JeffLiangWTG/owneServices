using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer.Test
{
	internal class RefAccessorialXmlProducerTest
	{
		[TestCaseSource(nameof(TestCases))]
		public async Task ProduceXmlAsync(ResponseResult<AccessorialInfo[]> responseResult, string token, string publicationDate, string folderPath, string expectedFileContent)
		{
			//Setup
			var pageAccessCount = 0;
			var dateTime = DateTime.Parse(publicationDate, null);
			using var directoryTestHelper = new DirectoryTestHelper(folderPath);
			var httpWebHelper = MockHelper.GetHttpWebHelper((uri, token) =>
			{
				Assert.That(uri, Is.EqualTo($"https://cmb.wisegrid.net/api/Accessorial/accessorialInfo?pageNumber={pageAccessCount}"));
				Assert.That(token, Is.EqualTo(token));
			},
			() =>
			{
				if (++pageAccessCount > 1)
				{
					return Task.FromResult(new ResponseResult<AccessorialInfo[]>
					{
						IsSuccess = true,
						Message = "Success"
					});
				}
				else
				{
					return Task.FromResult(responseResult);
				}
			});
			var tokenProvider = MockHelper.GetTokenProvider(token);
			var refAccessorialXmlProducer = new RefAccessorialXmlProducer(httpWebHelper, tokenProvider);

			//Action
			await refAccessorialXmlProducer.ProduceXmlAsync(directoryTestHelper.DirectoryPath, dateTime).ConfigureAwait(false);

			//Assert
			directoryTestHelper.AssertFileContent("RefAccessorialList.xml", expectedFileContent);
		}

		static IEnumerable<TestCaseData> TestCases
		{
			get
			{
				var testCaseData1 = new TestCaseData(
					new ResponseResult<AccessorialInfo[]>
					{
						IsSuccess = true,
						Value =
						[
							new AccessorialInfo
							{
								Code = "Code1",
								Description = "Description1"
							},
							new AccessorialInfo
							{
								Code = "Code2",
								Description = "Description2"
							}
						]
					},
					"token123",
					"2023-10-01",
					"Prod",
					@"<UniversalReferenceData>
  <DataSource>Accessorial List</DataSource>
  <PublicationTime>2023-10-01T00:00:00</PublicationTime>
  <UpdateType>Full</UpdateType>
  <Schema>
    <EntityType Name=""RefAccessorial"" Data=""true"">
      <Key>
        <PropertyRef Name=""ASI_Code"" />
      </Key>
      <Property Name=""ASI_Code"" Type=""char"" MaxLength=""3"" />
      <Property Name=""ASI_Description"" Type=""varchar"" MaxLength=""50"" />
    </EntityType>
  </Schema>
  <RefAccessorial>
    <ASI_Code>Code1</ASI_Code>
    <ASI_Description>Description1</ASI_Description>
  </RefAccessorial>
  <RefAccessorial>
    <ASI_Code>Code2</ASI_Code>
    <ASI_Description>Description2</ASI_Description>
  </RefAccessorial>
</UniversalReferenceData>"
				);
				yield return testCaseData1.SetName("ProduceXmlAsyncSample1");

				var testCaseData2 = new TestCaseData(
					new ResponseResult<AccessorialInfo[]>
					{
						IsSuccess = true,
						Value =
						[
							new AccessorialInfo
							{
								Code = "Code3",
								Description = "Description3"
							},
							new AccessorialInfo
							{
								Code = "Code4",
								Description = "Description4"
							}
						]
					},
					"token234",
					"2024-12-03",
					"Staging",
					@"<UniversalReferenceData>
  <DataSource>Accessorial List</DataSource>
  <PublicationTime>2024-12-03T00:00:00</PublicationTime>
  <UpdateType>Full</UpdateType>
  <Schema>
    <EntityType Name=""RefAccessorial"" Data=""true"">
      <Key>
        <PropertyRef Name=""ASI_Code"" />
      </Key>
      <Property Name=""ASI_Code"" Type=""char"" MaxLength=""3"" />
      <Property Name=""ASI_Description"" Type=""varchar"" MaxLength=""50"" />
    </EntityType>
  </Schema>
  <RefAccessorial>
    <ASI_Code>Code3</ASI_Code>
    <ASI_Description>Description3</ASI_Description>
  </RefAccessorial>
  <RefAccessorial>
    <ASI_Code>Code4</ASI_Code>
    <ASI_Description>Description4</ASI_Description>
  </RefAccessorial>
</UniversalReferenceData>"
				);
				yield return testCaseData2.SetName("ProduceXmlAsyncSample2");

				var testCaseData3 = new TestCaseData(
					new ResponseResult<AccessorialInfo[]>
					{
						IsSuccess = true,
						Value = []
					},
					"token234",
					"2024-12-03",
					"Staging",
					@"<UniversalReferenceData>
  <DataSource>Accessorial List</DataSource>
  <PublicationTime>2024-12-03T00:00:00</PublicationTime>
  <UpdateType>Full</UpdateType>
  <Schema>
    <EntityType Name=""RefAccessorial"" Data=""true"">
      <Key>
        <PropertyRef Name=""ASI_Code"" />
      </Key>
      <Property Name=""ASI_Code"" Type=""char"" MaxLength=""3"" />
      <Property Name=""ASI_Description"" Type=""varchar"" MaxLength=""50"" />
    </EntityType>
  </Schema>
</UniversalReferenceData>"
				);
				yield return testCaseData3.SetName("ProduceXmlAsyncEmptyResponse");

				var testCaseData4 = new TestCaseData(
					new ResponseResult<AccessorialInfo[]>
					{
						IsSuccess = true,
						Value = null
					},
					"token234",
					"2024-12-03",
					"Staging",
					@"<UniversalReferenceData>
  <DataSource>Accessorial List</DataSource>
  <PublicationTime>2024-12-03T00:00:00</PublicationTime>
  <UpdateType>Full</UpdateType>
  <Schema>
    <EntityType Name=""RefAccessorial"" Data=""true"">
      <Key>
        <PropertyRef Name=""ASI_Code"" />
      </Key>
      <Property Name=""ASI_Code"" Type=""char"" MaxLength=""3"" />
      <Property Name=""ASI_Description"" Type=""varchar"" MaxLength=""50"" />
    </EntityType>
  </Schema>
</UniversalReferenceData>"
				);
				yield return testCaseData4.SetName("ProduceXmlAsyncValueIsNull");
			}
		}

		[Test]
		public void ProduceXmlAsync_ApiFailure_ThrowsException()
		{
			// Arrange
			var responseResult = new ResponseResult<AccessorialInfo[]>
			{
				IsSuccess = false,
				Message = "API error"
			};
			var token = "valid_token";
			var folderPath = "ApiFailureTest";

			using var directoryTestHelper = new DirectoryTestHelper(folderPath);
			var httpWebHelper = MockHelper.GetHttpWebHelper((uri, token) => { }, () => Task.FromResult(responseResult));
			var tokenProvider = MockHelper.GetTokenProvider(token);
			var refAccessorialXmlProducer = new RefAccessorialXmlProducer(httpWebHelper, tokenProvider);

			// Act & Assert
			Assert.ThrowsAsync<InvalidOperationException>(async () =>
			{
				await refAccessorialXmlProducer.ProduceXmlAsync(directoryTestHelper.DirectoryPath, DateTime.UtcNow).ConfigureAwait(false);
			});
		}
	}
}
