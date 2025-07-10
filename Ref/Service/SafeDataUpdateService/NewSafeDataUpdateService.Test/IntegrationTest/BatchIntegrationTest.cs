using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	public class BatchIntegrationTest
	{
		#region Const

		readonly RefAccTaxRate Record1 = new RefAccTaxRate
		{
			ZAT_PK = Guid.NewGuid(),
			ZAT_RN_NKCountry = "DE",
			ZAT_ReferenceRateType = "EMPTY",
			ZAT_StartDate = new DateTime(2024, 1, 1),
			ZAT_EndDate = new DateTime(2024, 3, 31),
			ZAT_RateNumerator = 3,
			ZAT_RateDenominator = 3
		};
		readonly RefAccTaxRate Record2 = new RefAccTaxRate
		{
			ZAT_PK = Guid.NewGuid(),
			ZAT_RN_NKCountry = "DE",
			ZAT_ReferenceRateType = "EMPTY",
			ZAT_StartDate = new DateTime(2024, 4, 1),
			ZAT_EndDate = new DateTime(2024, 6, 30),
			ZAT_RateNumerator = 5,
			ZAT_RateDenominator = 7
		};
		readonly RefAccTaxRate Record3 = new RefAccTaxRate
		{
			ZAT_PK = Guid.NewGuid(),
			ZAT_RN_NKCountry = "DE",
			ZAT_ReferenceRateType = "EMPTY",
			ZAT_StartDate = new DateTime(2024, 7, 1),
			ZAT_EndDate = new DateTime(2024, 9, 30),
			ZAT_RateNumerator = 7,
			ZAT_RateDenominator = 7
		};

		readonly RefShippingLineUserView RefShippingLineDetails_one = new RefShippingLineUserView
		{
			RSL_PK = Guid.NewGuid(),
			RSL_IsActive = true,
			RSL_IsNVO = false,
			RSL_CarrierName = "ccvbcvbcv",
			RSL_StandardCarrierAlphaCode = "bbbb",
			RSL_CargoWiseOneCode = "gggd",
			RSL_OceanCarrierMessagingAvailable = false,
			RSL_GlobalSailingScheduleAvailable = false,
			RSL_ContainerAutomationAvailable = false,
			RSL_CargoSphereRatesAvailable = false,
			RSL_InvoiceAvailable = false,
			RSL_IsSystem = true,
			RSL_IsPublished = true,
			RSL_IsCW1User = true,
			RSL_EHubIds = "1234",
			RSL_BookingRequestAvailable = false,
			RSL_ShippingInstructionAvailable = false,
			RSL_VerifiedGrossContainerWeightAvailable = false,
			RSL_ShippingOrderAvailable = false,
			RSL_EManifestAvailable = false,
			RSL_IsShippingLine = true,
			RSL_IsEditable = true
		};

		#endregion

		[Test]
		[CreateDatabase("60413138C6D34F4E9814C3F3AFBFA920", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestBatchWithPostPutAndPatchAsync()
		{
			var dbName = CreateDatabaseAttribute.DbNamePrefix + "60413138C6D34F4E9814C3F3AFBFA920";
			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				entities.Add(Record1);
				entities.Add(Record2);
				await entities.SaveChangesAsync(null);
			}

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(true);
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("E2BAF5CB-6CDB-40FE-AE75-A4BE032A509B", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken,
				It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(),
				It.IsAny<CancellationToken>())).ReturnsAsync(new JwtSecurityToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"));
			using (var repo = new ReferenceDataRepository(true, TestConnectionString.GetAdmin(dbName)))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<ITokenValidationHelper>();
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
					services.RemoveAll(typeof(ODataBatchHandler));
					services.AddSingleton<ODataBatchHandler>(sp => new ODataBatchHandlerSingleTransaction(() => repo));
					services.AddScoped(sp => auth.Object);
					services.AddScoped(sp => tokenValidationHelper.Object);
				});
			}))
			{
				using (var client = factory.CreateClient())
				using (var batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/$batch"))
				{
					batchRequest.Content = CreateBatchRequestContent();
					batchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					var response = await client.SendAsync(batchRequest);
					Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
					var resultContent = await response.Content.ReadAsStringAsync();
					Assert.IsNotEmpty(resultContent);
				}
			}

			using (var repo1 = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				var records = repo1.Get<RefAccTaxRate>().ToArray();
				Assert.AreEqual(3, records.Length);

				var expectedRecords = new Dictionary<Guid, RefAccTaxRate>
				{
					{ Record1.ZAT_PK, Record1 },
					{ Record2.ZAT_PK, Record2 },
					{ Record3.ZAT_PK, Record3 }
				};

				foreach (var record in records)
				{
					var expected = expectedRecords[record.ZAT_PK];
					if (expected != null)
					{
						Assert.Multiple(() =>
						{
							Assert.AreEqual(expected.ZAT_RN_NKCountry, record.ZAT_RN_NKCountry);
							Assert.AreEqual(expected.ZAT_ReferenceRateType, record.ZAT_ReferenceRateType);
							Assert.AreEqual(expected.ZAT_StartDate, record.ZAT_StartDate);
							Assert.AreEqual(expected.ZAT_EndDate, record.ZAT_EndDate);
							Assert.AreEqual(expected.ZAT_RateNumerator, record.ZAT_RateNumerator);
							Assert.AreEqual(expected.ZAT_RateDenominator, record.ZAT_RateDenominator);
						});
					}
					else
					{
						Assert.Fail("Unexpected record");
					}
				}
			}
		}

		[Test]
		[CreateDatabase("60413138C6D34F4E9814C3F3AFBFA920", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestBatchBulkInsertWithPostPutAndPatchAsync()
		{
			const string testUserId = "testUserId";
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "60413138C6D34F4E9814C3F3AFBFA920";

			using var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName));
			entities.Add(Record1);
			entities.Add(Record2);
			await entities.SaveChangesAsync(null);

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized<RefAccTaxRateUserView>(It.IsAny<string>())).Returns(true);
			auth.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRateUserView>(), It.IsAny<string>())).Returns(true);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(It.IsAny<string>(), It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None))
				.Returns(Task.FromResult(new JwtSecurityToken("issuer", "audience", new[] { new Claim(AuthClaimType.UniqueName, testUserId) })));
			tokenValidationHelper.Setup(x => x.ShouldHandle(It.IsAny<string>())).Returns(true);

			using var repo = new ReferenceDataRepository(true, TestConnectionString.GetAdmin(dbName));
			await using var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<ITokenValidationHelper>();
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
					services.AddScoped(sp => auth.Object);
					services.AddScoped(sp => tokenValidationHelper.Object);
					services.PostConfigure<ODataOptions>(options =>
					{
						options.RouteComponents.Clear();
						options.AddRouteComponents("odata", ModelConfig.GetEdmModel(), service =>
						{
							service.AddSingleton<ODataBatchHandler>(sp => new ODataBatchHandlerSingleTransaction(() => repo));
						});
						options.TimeZone = TimeZoneInfo.Utc;
					});
				});
			});

			using var client = factory.CreateClient();
			using var batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/$batch");
			var batchContent = new MultipartContent("mixed", "batch_123");
			using var batchRequestContent = CreateBatchRequestContent(true);
			batchContent.Add(batchRequestContent);
			batchRequest.Content = batchContent;
			batchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", testUserId);
			var response = await client.SendAsync(batchRequest);
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			var resultContent = await response.Content.ReadAsStringAsync();
			Assert.That(resultContent, Is.Not.Empty);

			using var readRepo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName));
			var records = readRepo.Get<RefAccTaxRateUserView>().ToArray();
			var versionControls = readRepo.Get<RefDbVersionControl>().ToArray();
			Assert.That(records.Length, Is.EqualTo(3));
			Assert.That(versionControls.Length, Is.EqualTo(3));

			var expectedRecords = new Dictionary<Guid, RefAccTaxRate>
			{
				{ Record1.ZAT_PK, Record1 },
				{ Record2.ZAT_PK, Record2 },
				{ Record3.ZAT_PK, Record3 }
			};
			foreach (var record in records)
			{
				var expected = expectedRecords[record.ZAT_PK];
				if (expected != null)
				{
					Assert.Multiple(() =>
					{
						Assert.That(record.ZAT_RN_NKCountry, Is.EqualTo(expected.ZAT_RN_NKCountry));
						Assert.That(record.ZAT_ReferenceRateType, Is.EqualTo(expected.ZAT_ReferenceRateType));
						Assert.That(record.ZAT_StartDate, Is.EqualTo(expected.ZAT_StartDate));
						Assert.That(record.ZAT_EndDate, Is.EqualTo(expected.ZAT_EndDate));
						Assert.That(record.ZAT_RateNumerator, Is.EqualTo(expected.ZAT_RateNumerator));
						Assert.That(record.ZAT_RateDenominator, Is.EqualTo(expected.ZAT_RateDenominator));
					});
				}
				else
				{
					Assert.Fail("Unexpected record");
				}
			}
			foreach (var versionControl in versionControls)
			{
				Assert.That(versionControl.RVC_LastEditedUser, Is.EqualTo(testUserId));
			}

		}

		[Test]
		[CreateDatabase("60413138C6D34F4E9814C3F3AFBFA920", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestRefShippingLineBatchWithPostPutAndPatchAsync()
		{
			var dbName = CreateDatabaseAttribute.DbNamePrefix + "60413138C6D34F4E9814C3F3AFBFA920";
			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				entities.Add(RefShippingLineDetails_one);
				await entities.SaveChangesAsync(null);
			}

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(true);
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("E2BAF5CB-6CDB-40FE-AE75-A4BE032A509B", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken,
				It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(),
				It.IsAny<CancellationToken>())).ReturnsAsync(new JwtSecurityToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"));
			using (var repo = new ReferenceDataRepository(true, TestConnectionString.GetAdmin(dbName)))
			{
				using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
				{
					builder.ConfigureTestServices(services =>
					{
						services.RemoveAll<ITokenValidationHelper>();
						services.RemoveAll(typeof(IReferenceDataRepository));
						services.AddScoped<IReferenceDataRepository>(sp => repo);
						services.RemoveAll(typeof(ODataBatchHandler));
						services.AddSingleton<ODataBatchHandler>(sp => new ODataBatchHandlerSingleTransaction(() => repo));
						services.AddScoped(sp => auth.Object);
						services.AddScoped(sp => tokenValidationHelper.Object);
					});
				}))
				{
					using (var client = factory.CreateClient())
					using (var batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/$batch"))
					{
						batchRequest.Content = CreateRefShippingLineBatchRequestContent();
						batchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
						var response = await client.SendAsync(batchRequest);
						Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
						var resultContent = await response.Content.ReadAsStringAsync();
						Assert.IsNotEmpty(resultContent);
					}
				}
			}
			using (var repo1 = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				var records = repo1.Get<RefShippingLineUserView>().ToArray();
				Assert.AreEqual(1, records.Length);

				var expectedRecords = new Dictionary<Guid, RefShippingLineUserView>
				{
					{ RefShippingLineDetails_one.RSL_PK, RefShippingLineDetails_one }
				};

				foreach (var record in records)
				{
					var expected = expectedRecords[record.RSL_PK];
					if (expected != null)
					{
						Assert.Multiple(() =>
						{
							Assert.AreEqual(expected.RSL_IsActive, record.RSL_IsActive);
							Assert.AreEqual(expected.RSL_IsNVO, record.RSL_IsNVO);
							Assert.AreEqual(expected.RSL_CarrierName, record.RSL_CarrierName);
							Assert.AreEqual(expected.RSL_StandardCarrierAlphaCode, record.RSL_StandardCarrierAlphaCode);
							Assert.AreEqual(expected.RSL_CargoWiseOneCode, record.RSL_CargoWiseOneCode);
							Assert.AreEqual(expected.RSL_OceanCarrierMessagingAvailable, record.RSL_OceanCarrierMessagingAvailable);
							Assert.AreEqual(expected.RSL_GlobalSailingScheduleAvailable, record.RSL_GlobalSailingScheduleAvailable);
							Assert.AreEqual(expected.RSL_IsSystem, record.RSL_IsSystem);
							Assert.AreEqual(expected.RSL_IsCW1User, record.RSL_IsCW1User);
							Assert.AreEqual(expected.RSL_EHubIds, record.RSL_EHubIds);
							Assert.AreEqual(expected.RSL_IsShippingLine, record.RSL_IsShippingLine);
						});
					}
					else
					{
						Assert.Fail("Unexpected record");
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
		HttpContent CreateBatchRequestContent(bool isUpdateView = false)
		{
			var changesetContent = new MultipartContent("mixed", "changeset_825d-598d-f1dc");

			var content3 = CreateInsertContent(0, Record3, isUpdateView);
			changesetContent.Add(content3);

			Record1.ZAT_RateNumerator = 8;
			Record1.ZAT_RateDenominator = 8;
			var content1 = CreateUpdateContent(HttpMethod.Put, 1, Record1, isUpdateView);
			changesetContent.Add(content1);

			Record2.ZAT_RateNumerator = 42;
			Record2.ZAT_RateDenominator = 42;
			var content2 = CreateUpdateContent(HttpMethod.Patch, 2, Record2, isUpdateView);
			changesetContent.Add(content2);

			return changesetContent;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
		HttpContent CreateInsertContent(int id, RefAccTaxRate row, bool isUpdateView = false)
		{
			var req = new HttpRequestMessage(HttpMethod.Post, isUpdateView ? "http://localhost/odata/RefAccTaxRateUserViewUpdate": "http://localhost/odata/RefAccTaxRateUpdate");
			req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.9D));
			req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.1D));
			req.Headers.Add("OData-Version", "4.0");
			req.Headers.Add("OData-MaxVersion", "4.0");
			req.Headers.Add("Content-ID", $"{id}");
			req.Content = new StringContent(JsonSerializer.Serialize(row).Replace("T00:00:00", ""), Encoding.UTF8, "application/json");

			var result = new HttpMessageContent(req);
			result.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
			result.Headers.Add("Content-Transfer-Encoding", "binary");
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
		HttpContent CreateUpdateContent(HttpMethod httpMethod, int id, RefAccTaxRate row, bool isUpdateView = false)
		{
			var req = new HttpRequestMessage(httpMethod, isUpdateView ? $"http://localhost/odata/RefAccTaxRateUserViewUpdate({row.ZAT_PK})" : $"http://localhost/odata/RefAccTaxRateUpdate({row.ZAT_PK})");
			req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.9D));
			req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.1D));
			req.Headers.Add("OData-Version", "4.0");
			req.Headers.Add("OData-MaxVersion", "4.0");
			req.Headers.Add("Content-ID", $"{id}");
			req.Content = new StringContent(JsonSerializer.Serialize(row).Replace("T00:00:00", ""), Encoding.UTF8, "application/json");

			var result = new HttpMessageContent(req);
			result.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
			result.Headers.Add("Content-Transfer-Encoding", "binary");
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
		HttpContent CreateRefShippingLineBatchRequestContent()
		{
			var changesetContent = new MultipartContent("mixed", "changeset_825d-598d-f1dc");

			var content3 = CreateInsertContentRefShippingLine(0, RefShippingLineDetails_one);
			changesetContent.Add(content3);

			return changesetContent;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
		HttpContent CreateInsertContentRefShippingLine(int id, RefShippingLineUserView row)
		{
			var req = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefShippingLineUserViewUpdate");
			req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.9D));
			req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.1D));
			req.Headers.Add("OData-Version", "4.0");
			req.Headers.Add("OData-MaxVersion", "4.0");
			req.Headers.Add("Content-ID", $"{id}");
			req.Content = new StringContent(JsonSerializer.Serialize(row).Replace("T00:00:00", ""), Encoding.UTF8, "application/json");

			var result = new HttpMessageContent(req);
			result.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
			result.Headers.Add("Content-Transfer-Encoding", "binary");
			return result;
		}
	}
}
