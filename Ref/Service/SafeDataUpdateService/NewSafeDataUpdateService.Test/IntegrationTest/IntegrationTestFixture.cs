using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class IntegrationTestFixture
	{
		[Test]
		public void IntegrationTest()
		{
			var uri = new Uri("http://localhost/odata/RefAccTaxRateUpdate");
			var result = string.Empty;
			Assert.DoesNotThrowAsync(async () =>
			{
				using (var factory = IntegrationTestHelper.WebAppFactory)
				using (var client = factory.CreateClient())
				using (var response = await client.GetAsync(uri))
				{
					response.EnsureSuccessStatusCode();
					result = await response.Content.ReadAsStringAsync();
				}
			});
			Assert.AreEqual(RefAccTaxRateResponse, result);
		}

		[Test]
		public void RefCusTaxOrFeeUpdate()
		{
			var uri = new Uri("http://localhost:37016/odata/RefCusTaxOrFeeUpdate/Default.GetWithOptimizedExpand()?$filter=(ZZF_Code eq 'LFT' eq true or ZZF_Code eq 'LNT' eq true)");
			var result = string.Empty;
			Assert.DoesNotThrowAsync(async () =>
			{
				using (var factory = IntegrationTestHelper.WebAppFactory)
				using (var client = factory.CreateClient())
				using (var response = await client.GetAsync(uri))
				{
					response.EnsureSuccessStatusCode();
					result = await response.Content.ReadAsStringAsync();
				}
			});
			Assert.AreEqual(RefCusTaxOrFeeResponse, result);
		}

		[Test]
		[CreateDatabase("4350E309494C4FD5BF01EA311FF7A3C0", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task IntegrationTestForQueryingHistoricalData()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "4350E309494C4FD5BF01EA311FF7A3C0";
			var utcTime = await ArrangeRefShippingLine(dbName);
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp =>
					{
						var interceptor = sp.GetRequiredService<SystemVersionInterceptor>();
						return new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName), interceptor);
					});
				});
			}))
			{
				var uri = new Uri($"http://localhost/odata/RefShippingLineUserViewUpdate?$filter=RSL_PK eq 53D56270-8013-11EC-92E6-011864910191&$top=1001&SystemVersionUTC={utcTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}");
				var result = string.Empty;
				Assert.DoesNotThrowAsync(async () =>
				{
					using (var client = factory.CreateClient())
					using (var response = await client.GetAsync(uri))
					{
						response.EnsureSuccessStatusCode();
						result = await response.Content.ReadAsStringAsync();
					}
				});
				var refShippinglineUserViewsResponse = JsonConvert.DeserializeObject<RefODataMultipleResultsResponse<RefShippingLineUserView>>(result);
				Assert.That(refShippinglineUserViewsResponse.Value.Count == 1);
				var refShippinglineUserView = refShippinglineUserViewsResponse.Value[0];
				Assert.That(refShippinglineUserView.RSL_CarrierName == "TST");
			}
		}

		[Test]
		[CreateDatabase("2219719c064f46a5bef00f0e50ce4e37", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task IntegrationTest_QueryingHistoricalTariffData()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "2219719c064f46a5bef00f0e50ce4e37";
			var utcTime = await ArrangeRefCusTariff(dbName);
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp =>
					{
						var interceptor = sp.GetRequiredService<SystemVersionInterceptor>();
						return new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName), interceptor);
					});
				});
			}))
			{
				var uri = new Uri($"http://localhost/odata/RefCusTariffUpdate?$filter=ZZ1_PK eq 0d912576-7a56-4e3f-afb1-0b30b43274e1&$top=1001&SystemVersionUTC={utcTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}");
				var result = string.Empty;

				using (var client = factory.CreateClient())
				using (var response = await client.GetAsync(uri))
				{
					response.EnsureSuccessStatusCode();
					result = await response.Content.ReadAsStringAsync();
				}

				var tariffResponse = JsonConvert.DeserializeObject<RefODataMultipleResultsResponse<RefCusTariff>>(result);
				Assert.That(tariffResponse.Value.Count == 1);
				var tariff = tariffResponse.Value[0];
				Assert.That(tariff.ZZ1_Description == "Tariff 1");
			}
		}

		[Test]
		public void GetWithSelect()
		{
			var uri = new Uri("http://localhost/odata/RefAccTaxRateUpdate?$select=ZAT_PK,ZAT_ReferenceRateType");
			var result = string.Empty;
			Assert.DoesNotThrowAsync(async () =>
			{
				using (var factory = IntegrationTestHelper.WebAppFactory)
				using (var client = factory.CreateClient())
				using (var response = await client.GetAsync(uri))
				{
					response.EnsureSuccessStatusCode();
					result = await response.Content.ReadAsStringAsync();
				}
			});
			var expectedResult = "{\"@odata.context\":\"http://localhost/odata/$metadata#RefAccTaxRateUpdate(ZAT_PK,ZAT_ReferenceRateType)\",\"value\":[{\"ZAT_PK\":\"7edae0a0-06e1-43c9-a8d9-33635b66f038\",\"ZAT_ReferenceRateType\":\"STD\"}]}";
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		[CreateDatabase("4350E309494C4FD5BF01EA311FF7A3C0", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetWithSelect_RefPortPolygon()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "4350E309494C4FD5BF01EA311FF7A3C0";
			await ArrangeRefPortPolygonData(dbName);
			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefPortPolygonUpdate?$select=RPP_PortId,RPP_SerializedPolygon_WKT");
				using (var client = factory.CreateClient())
				using (var response = await client.GetAsync(uri))
				{
					response.EnsureSuccessStatusCode();
					var result = await response.Content.ReadAsStringAsync();
					var expectedResponse = "{\"@odata.context\":\"http://localhost/odata/$metadata#RefPortPolygonUpdate(RPP_PortId,RPP_SerializedPolygon_WKT)\",\"value\":[{\"RPP_PortId\":5,\"RPP_SerializedPolygon_WKT\":{\"Geography\":{\"CoordinateSystemId\":4326,\"WellKnownText\":\"POINT (10 20)\"}}}]}";
					Assert.AreEqual(expectedResponse, result);
				}
			}
		}

		[Test]
		public async Task ProcessBatchAsync_MaxOperationsPerChangeset()
		{
			using (var factory = IntegrationTestHelper.WebAppFactory)
			using (var client = factory.CreateClient())
			using (var batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/$batch"))
			using (var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost/odata/RefAccTaxRateUpdate"))
			{
				var batchContent = new MultipartContent("mixed", "batch_36522ad7-fc75-4b56-8c71-56071383e77b");
				message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
				using (var content = new HttpMessageContent(message))
				{
					content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
					content.Headers.Add("Content-Transfer-Encoding", "binary");
					for (var i = 0; i <= 100; i++)
					{
						batchContent.Add(content);
					}

					var contentString = await batchContent.ReadAsStringAsync();
					batchRequest.Content = batchContent;
					var response = await client.SendAsync(batchRequest);
					Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				}
			}
		}

		[Test]
		public async Task SetPropertyNamingPolicyToNull()
		{
			var uri = new Uri("http://localhost/api/ClientDataSetVersionSummary/Get");
			var expectedResponse = "[{\"DataSet\":\"DS1\",\"LastDataChangedTime\":\"0001-01-01T00:00:00\",\"NoOfCustomersUpdated\":0,\"NoOfCustomersFailed\":1,\"NoOfProductionCustomersFailed\":0}]";
			var timestamp = new DateTime(2023, 11, 30);
			var clientVersionControl = new ClientRefDbVersionControl { CVC_PK = Guid.Empty, CVC_ClientId = "AA", CVC_DataSet = "DS1", CVC_IsInUse = true, CVC_DataSetTimestamp = timestamp, CVC_LastUpdatedTimeUTC = timestamp, CVC_SystemType = "TST" };
			var safeRepo = new Mock<IReferenceDataRepository>();
			safeRepo.Setup(x => x.Get<ClientRefDbVersionControl>()).Returns(new[] { clientVersionControl }.AsQueryable());
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped(sp => safeRepo.Object);
					services.AddScoped(sp => new Mock<IClientLicenseInformationProvider>().Object);
				});
			}))
			using (var client = factory.CreateClient())
			using (var response = await client.GetAsync(uri))
			{
				response.EnsureSuccessStatusCode();
				var result = await response.Content.ReadAsStringAsync();
				Assert.AreEqual(expectedResponse, result);
			}
		}

		[Test]
		[CreateDatabase("4350E309494C4FD5BF01EA311FF7A3C0", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestTimeZoneIsUTC()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "4350E309494C4FD5BF01EA311FF7A3C0";
			await ArrangeRefExchangeRateZZData(dbName);
			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);

				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefExchangeRateZZUpdate/Default.GetWithOptimizedExpand()?$filter=ZZN_StartDate eq 2023-11-08T00%3A00%3A00Z");
				var result = string.Empty;
				Assert.DoesNotThrowAsync(async () =>
				{
					using (var client = factory.CreateClient())
					using (var response = await client.GetAsync(uri))
					{
						response.EnsureSuccessStatusCode();
						result = await response.Content.ReadAsStringAsync();
					}
				});

				var refODataResponse = JsonConvert.DeserializeObject<RefODataMultipleResultsResponse<RefExchangeRateZZ>>(result);
				var refExchangeRateZz = refODataResponse.Value[0];
				Assert.AreEqual(refExchangeRateZz.ZZN_StartDate, new DateTime(2023, 11, 08, 0, 0, 0, DateTimeKind.Utc));
				Assert.AreEqual(refExchangeRateZz.ZZN_EndDate, new DateTime(2023, 11, 08, 0, 0, 0, DateTimeKind.Utc));
			}
		}

		[Test]
		[CreateDatabase("4350E309494C4FD5BF01EA311FF7A3C0", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestGetGeometryData()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "4350E309494C4FD5BF01EA311FF7A3C0";
			await ArrangeRefPortPolygonData(dbName);
			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
				});
			}))
			{
				var uri = new Uri($"http://localhost/odata/RefPortPolygonUpdate/Default.GetWithOptimizedExpand()?$filter=RPP_PK eq 7cfc4ee1-06b3-4881-a6a2-07f70e3d9e02");
				using (var client = factory.CreateClient())
				using (var response = await client.GetAsync(uri))
				{
					response.EnsureSuccessStatusCode();
					var result = await response.Content.ReadAsStringAsync();
					var expectedResponse = "{\"@odata.context\":\"http://localhost/odata/$metadata#RefPortPolygonUpdate\",\"value\":[{\"RPP_PK\":\"7cfc4ee1-06b3-4881-a6a2-07f70e3d9e02\",\"RPP_PortId\":5,"
						+ "\"RPP_SerializedPolygon_WKT\":{\"Geography\":{\"CoordinateSystemId\":4326,\"WellKnownText\":\"POINT (10 20)\"}},"
						+ "\"RPP_SerializedPolygon\":{\"@odata.type\":\"#CargoWise.RefDbRepo.Service.Schema_0_9_New.SerializedGeometry\",\"Geography\":{\"CoordinateSystemId\":4326,\"WellKnownText\":\"POINT (10 20)\"}}}]}";
					Assert.AreEqual(expectedResponse, result);
				}
			}
		}

		[Test]
		[CreateDatabase("4350E309494C4FD5BF01EA311FF7A3C0", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestPostGeometryData()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "4350E309494C4FD5BF01EA311FF7A3C0";
			var connectionString = TestConnectionString.GetAdmin(dbName);

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized(It.IsAny<RefPortPolygon>(), It.IsAny<string>())).Returns(true);
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("2F7D254D-C420-4DFA-A076-FA5AF0462A34", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken,
				It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(),
				It.IsAny<CancellationToken>())).ReturnsAsync(new JwtSecurityToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"));
			using (var repo = new ReferenceDataRepository(false, connectionString))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<ITokenValidationHelper>();
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
					services.AddScoped(sp => auth.Object);
					services.AddScoped(sp => tokenValidationHelper.Object);
				});
			}))
			{
				var uri = new Uri($"http://localhost/odata/RefPortPolygonUpdate");
				string jsonContent = "{\"@odata.type\":\"#CargoWise.RefDbRepo.Service.Schema_0_9_New.RefPortPolygon\",\"RPP_PK\":\"d184ea9a-0555-4ea3-ac71-792176f0a166\",\"RPP_PortId\":1,\"RPP_SerializedPolygon_WKT\":{\"@odata.type\":\"#CargoWise.RefDbRepo.Service.Schema_0_9_New.SerializedGeometry\",\"Geography\":{\"CoordinateSystemId\":4326,\"WellKnownText\":\"SRID=4326;POLYGON ((1 0, 1 1, 0 1, 0 0, 1 0))\"}}}";

				using (var client = factory.CreateClient())
				{
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
					using (var response = await client.PostAsync(uri, content))
					{
						response.EnsureSuccessStatusCode();
					}
				}
			}
			using (var entities = new SafeDbContext(connectionString))
			{
				var refPortPolygon = entities.RefPortPolygons.Where(x => x.RPP_PortId == 1).FirstOrDefault();
				Assert.NotNull(refPortPolygon);
				Assert.AreEqual(4326, refPortPolygon.RPP_SerializedPolygon.SRID);
				Assert.AreEqual("POLYGON ((1 0, 1 1, 0 1, 0 0, 1 0))", refPortPolygon.RPP_SerializedPolygon.ToText());
			}
		}

		[Test]
		[CreateDatabase("4350E309494C4FD5BF01EA311FF7A3C0", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestPostRefCusTariffData()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "4350E309494C4FD5BF01EA311FF7A3C0";
			var connectionString = TestConnectionString.GetAdmin(dbName);

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized(It.IsAny<RefCusTariff>(), It.IsAny<string>())).Returns(true);
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("2F7D254D-C420-4DFA-A076-FA5AF0462A34", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken,
				It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(),
				It.IsAny<CancellationToken>())).ReturnsAsync(new JwtSecurityToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"));
			using (var repo = new ReferenceDataRepository(false, connectionString))
			{
				repo.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "Description",
					ZZZ_ZZZ_NKGrouping = null
				});
				await repo.SaveChangesAsync(null);
				repo.Add(new RefCusTariffType
				{
					ZZI_PK = Guid.Parse("8c8a3b81-c093-45c6-a077-f7d70c69af14"),
					ZZI_TariffType = "ADT",
					ZZI_Description = "Desc",
					ZZI_ZZZ_NKDataGrouping = "AU",
					ZZI_HasFormulaSpecificQuestions = false
				});
				await repo.SaveChangesAsync(null);

				using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
				{
					builder.ConfigureTestServices(services =>
					{
						services.RemoveAll<ITokenValidationHelper>();
						services.RemoveAll(typeof(IReferenceDataRepository));
						services.AddScoped<IReferenceDataRepository>(sp => repo);
						services.AddScoped(sp => auth.Object);
						services.AddScoped(sp => tokenValidationHelper.Object);
					});
				}))
				{
					var uri = new Uri($"http://localhost/odata/RefCusTariffUpdate");
					string jsonContent = "{\"@odata.type\":\"#CargoWise.RefDbRepo.Service.Schema_0_9_New.RefCusTariff\",\"ZZ1_CompositeKeyOnZZ5\":\"\",\"ZZ1_Description\":\"Grenztier\",\"ZZ1_EndDate\":\"2079-06-06T23:59:00Z\",\"ZZ1_IAMUnique\":0,\"ZZ1_PK\":\"472a83dd-51f8-43d8-bf2a-aef349e89257\",\"ZZ1_PublishedDate\":\"2022-10-01\",\"ZZ1_StartDate\":\"2008-09-01T00:00:00Z\",\"ZZ1_TariffCode\":\"290-002\",\"ZZ1_ZZF_NKTaxOrFeeCode\":\"\",\"ZZ1_ZZI_TariffType\":\"8c8a3b81-c093-45c6-a077-f7d70c69af14\",\"ZZ1_ZZZ_NKDataGrouping\":\"AU\"}";

					using (var client = factory.CreateClient())
					{
						client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
						using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
						using (var response = await client.PostAsync(uri, content))
						{
							response.EnsureSuccessStatusCode();
						}
					}
				}
			}
			using (var entities = new SafeDbContext(connectionString))
			{
				var tariff = entities.RefCusTariffs.Where(x => x.ZZ1_TariffCode == "290-002").FirstOrDefault();
				Assert.NotNull(tariff);
				Assert.AreEqual("AU", tariff.ZZ1_ZZZ_NKDataGrouping);
				Assert.AreEqual("Grenztier", tariff.ZZ1_Description);
			}
		}

		[Test]
		[CreateDatabase("4350E309494C4FD5BF01EA311FF7A3C0", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestPostRefCusCodeListData()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "4350E309494C4FD5BF01EA311FF7A3C0";
			var connectionString = TestConnectionString.GetAdmin(dbName);

			var auth = new Mock<IAuthorizationHelper>();
			auth.Setup(x => x.IsAuthorized(It.IsAny<RefCusCodeList>(), It.IsAny<string>())).Returns(true);
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("2F7D254D-C420-4DFA-A076-FA5AF0462A34", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken,
				It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(),
				It.IsAny<CancellationToken>())).ReturnsAsync(new JwtSecurityToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"));
			using (var repo = new ReferenceDataRepository(false, connectionString))
			{
				repo.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "Description",
					ZZZ_ZZZ_NKGrouping = null
				});
				await repo.SaveChangesAsync(null);
				repo.Add(new RefCusCodeType
				{
					ZZK_PK = Guid.NewGuid(),
					ZZK_CodeType = "ADTT",
					ZZK_Description = "Desc",
					ZZK_ZZZ_NKDataGrouping = "AU",
					ZZK_IsReadonly = false,
					ZZK_MaxLength = 10,
				});
				await repo.SaveChangesAsync(null);

				using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
				{
					builder.ConfigureTestServices(services =>
					{
						services.RemoveAll<ITokenValidationHelper>();
						services.RemoveAll(typeof(IReferenceDataRepository));
						services.AddScoped<IReferenceDataRepository>(sp => repo);
						services.AddScoped(sp => auth.Object);
						services.AddScoped(sp => tokenValidationHelper.Object);
					});
				}))
				{
					var uri = new Uri($"http://localhost/odata/RefCusCodeListUpdate");
					string jsonContent = "{\"@odata.type\":\"#CargoWise.RefDbRepo.Service.Schema_0_9_New.RefCusCodeList\",\"ZZD_PK\":\"472a83dd-51f8-43d8-bf2a-aef349e89257\",\"ZZD_ZZK_NKCodeType\":\"ADTT\",\"ZZD_Code\":\"ES\",\"ZZD_EndDate\":\"2079-06-06T23:59:00Z\",\"ZZD_Description\":\"Desc\",\"ZZD_StartDate\":\"2008-09-01T00:00:00Z\",\"ZZD_ZZZ_NKDataGrouping\":\"AU\"}";

					using (var client = factory.CreateClient())
					{
						client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
						using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
						using (var response = await client.PostAsync(uri, content))
						{
							response.EnsureSuccessStatusCode();
						}
					}
				}
			}
			using (var entities = new SafeDbContext(connectionString))
			{
				var codeList = entities.RefCusCodeLists.Where(x => x.ZZD_Code == "ES").FirstOrDefault();
				Assert.NotNull(codeList);
				Assert.AreEqual("AU", codeList.ZZD_ZZZ_NKDataGrouping);
				Assert.AreEqual("Desc", codeList.ZZD_Description);
			}
		}

		#region Records

		readonly RefExchangeRateZZ RefExchangeRateZZRecord = new RefExchangeRateZZ
		{
			ZZN_PK = Guid.NewGuid(),
			ZZN_ExRateType = "CUS",
			ZZN_StartDate = new DateTime(2023, 11, 08, 0, 0, 0, DateTimeKind.Utc),
			ZZN_EndDate = new DateTime(2023, 11, 08, 0, 0, 0, DateTimeKind.Utc),
			ZZN_Rate = 3,
			ZZN_RX_NKExCurrency = "BRL",
			ZZN_RN_NKCountry = "AU",
			ZZN_AsPublished = "",
		};

		readonly RefDataSetInformation RefDataSetInformationRecord = new RefDataSetInformation
		{
			RDS_PK = Guid.NewGuid(),
			RDS_DataSetId = 5,
			RDS_TableName = "RefExchangeRateZZRecord",
			RDS_DataSetName = "RefExchangeRateZZRecord",
			RDS_DataSetTableCode = "ZZN",
			RDS_PriorityLevel = 0,
			RDS_LastUpdatedUTC = new DateTime(2023, 05, 23, 08, 03, 49),
			RDS_IsPush = false
		};

		readonly RefPortPolygon RefPortPolygonRecord = new RefPortPolygon
		{
			RPP_PK = Guid.Parse("7cfc4ee1-06b3-4881-a6a2-07f70e3d9e02"),
			RPP_PortId = 5,
			RPP_SerializedPolygon = new Point(10, 20) { SRID = 4326 }
		};
		#endregion

		async Task ArrangeRefExchangeRateZZData(string dbName)
		{
			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				entities.Add(RefDataSetInformationRecord);
				entities.Add(RefExchangeRateZZRecord);
				await entities.SaveChangesAsync(null);
			}
		}

		async Task ArrangeRefPortPolygonData(string dbName)
		{
			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				entities.Add(RefPortPolygonRecord);
				await entities.SaveChangesAsync(null);
			}
		}

		async Task<DateTime> ArrangeRefShippingLine(string dbName)
		{
			var refShippingLineRecord = new RefShippingLine
			{
				RSL_PK = Guid.Parse("53D56270-8013-11EC-92E6-011864910191"),
				RSL_IsActive = true,
				RSL_CargoWiseOneCode = "CW00",
				RSL_CarrierName = "TST",
				RSL_IsShippingLine = true,
				RSL_IsNVO = true,
				RSL_StandardCarrierAlphaCode = "Test",
				RSL_OceanCarrierMessagingAvailable = false,
				RSL_GlobalSailingScheduleAvailable = false,
				RSL_ContainerAutomationAvailable = false,
				RSL_CargoSphereRatesAvailable = false,
				RSL_InvoiceAvailable = false,
				RSL_IsCW1User = false,
				RSL_EHubIds = string.Empty,
				RSL_BookingRequestAvailable = false,
				RSL_ShippingInstructionAvailable = false,
				RSL_VerifiedGrossContainerWeightAvailable = false,
				RSL_ShippingOrderAvailable = false,
				RSL_EManifestAvailable = false
			};
			var refDataSetInformationRecord = new RefDataSetInformation
			{
				RDS_PK = Guid.NewGuid(),
				RDS_DataSetId = 37,
				RDS_TableName = "RefShippingLine",
				RDS_DataSetName = "RefShippingLine",
				RDS_DataSetTableCode = "RSL",
				RDS_PriorityLevel = 0,
				RDS_IsPush = false
			};

			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				entities.Add(refDataSetInformationRecord);
				entities.Add(refShippingLineRecord);
				await entities.SaveChangesAsync(null);
			}
			var utcTime = DateTime.UtcNow;
			Thread.Sleep(1000);
			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				refShippingLineRecord.RSL_CarrierName = "TST Changed";
				entities.Update(refShippingLineRecord);
				await entities.SaveChangesAsync(null);
			}
			return utcTime;
		}

		async Task<DateTime> ArrangeRefCusTariff(string dbName)
		{
			var tariffPK = Guid.Parse("0d912576-7a56-4e3f-afb1-0b30b43274e1");
			var dataGrouping = new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "BR",
				ZZZ_Description = "group"
			};
			var tariffType = new RefCusTariffType
			{
				ZZI_PK = Guid.NewGuid(),
				ZZI_Description = "TariffType",
				ZZI_TariffType = "Type1",
				ZZI_ZZ9_NKNomenclatureGroupType = "ZA",
				ZZI_ZZZ_NKDataGrouping = "BR"
			};
			var tariff = new RefCusTariff
			{
				ZZ1_PK = tariffPK,
				ZZ1_ZZI_TariffType = tariffType.ZZI_PK,
				ZZ1_TariffCode = "CODE2",
				ZZ1_IAMUnique = 1,
				ZZ1_Description = "Tariff 1",
				ZZ1_StartDate = new DateTime(1900, 1, 1),
				ZZ1_EndDate = new DateTime(2000, 1, 1),
				ZZ1_ZZF_NKTaxOrFeeCode = "ABC",
				ZZ1_ZZZ_NKDataGrouping = "BR",
				ZZ1_CompositeKeyOnZZ5 = "AAX"
			};
			var refDataSetInformationRecord = new RefDataSetInformation
			{
				RDS_PK = Guid.NewGuid(),
				RDS_DataSetId = 23,
				RDS_TableName = "RefCusTariff",
				RDS_DataSetName = "RefCusTariff",
				RDS_DataSetTableCode = "ZZ1",
				RDS_PriorityLevel = 0,
				RDS_IsPush = false
			};

			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				entities.Add(refDataSetInformationRecord);
				entities.Add(dataGrouping);
				entities.Add(tariffType);
				entities.Add(tariff);
				await entities.SaveChangesAsync(null, true);
			}
			var utcTime = DateTime.UtcNow;
			Thread.Sleep(1000);
			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				tariff.ZZ1_Description = "Tariff 1 Update";
				entities.Update(tariff);
				await entities.SaveChangesAsync(null);
			}
			return utcTime;
		}

		readonly string RefAccTaxRateResponse = "{\"@odata.context\":\"http://localhost/odata/$metadata#RefAccTaxRateUpdate\",\"value\":[{\"ZAT_PK\":\"7edae0a0-06e1-43c9-a8d9-33635b66f038\",\"ZAT_RN_NKCountry\":\"AA\",\"ZAT_ReferenceRateType\":\"STD\",\"ZAT_StartDate\":\"0001-01-01\",\"ZAT_EndDate\":\"0001-01-01\",\"ZAT_RateNumerator\":0,\"ZAT_RateDenominator\":0}]}";
		readonly string RefCusTaxOrFeeResponse = "{\"@odata.context\":\"http://localhost:37016/odata/$metadata#RefCusTaxOrFeeUpdate\",\"value\":[{\"ZZF_PK\":\"ab07a112-8d73-40fb-8e68-0dce92a4ad84\",\"ZZF_Code\":\"LNT\",\"ZZF_Description\":\"LCT Normal Vehicle Threshold\",\"ZZF_Value\":0,\"ZZF_StartDate\":\"0001-01-01T00:00:00Z\",\"ZZF_EndDate\":\"0001-01-01T00:00:00Z\",\"ZZF_ZZZ_NKDataGrouping\":\"AU\",\"ZZF_Minimum\":0,\"ZZF_Maximum\":0,\"ZZF_Threshold\":0,\"ZZF_ZX0_NKTaxOrFeeType\":\"VAT\"}]}";
	}
}
