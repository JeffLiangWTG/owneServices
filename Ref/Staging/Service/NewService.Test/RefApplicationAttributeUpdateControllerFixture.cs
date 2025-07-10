using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class RefApplicationAttributeUpdateControllerFixture
	{
		[TestCase(true)]
		[TestCase(false)]
		public async Task CheckIsJobGroupValid(bool isValid)
		{
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("20345582-4F3F-42D3-B312-864F3390D67C", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken, It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult(new JwtSecurityToken(accessToken)));
			var refDbRepoCrypto = new Mock<IRefDbRepoCrypto>();
			using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepository.Object, refDbRepoCrypto.Object).WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<ITokenValidationHelper>();
					services.AddScoped(sp => authorizationHelper.Object)
					.AddScoped(sp => tokenValidationHelper.Object);

					services.Configure<TestServerOptions>(options =>
					{
						options.AllowSynchronousIO = true;
					});
				});
			}))

			using (var client = factory.CreateClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				var attribute = isValid ? validAttribute : invalidAttribute;
				foreach (var request in GetHttpRequestMessages(attribute))
				{
					var response = await client.SendAsync(request);
					var responseMessage = await response.Content.ReadAsStringAsync();
					if (isValid)
					{
						response.EnsureSuccessStatusCode();
					}
					else
					{
						Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
						Assert.True(responseMessage.Contains("Invalid job group: ABC"));
					}
				}
			}
		}

		[Test]
		[CreateDatabase("C6CB576ABEF24F368BC21F29064BB847", DbSchema.RefDbRepoStaging)]
		public async Task TestHandleCredentialAttribute()
		{
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("D85D4E02-77A4-47D4-8A04-6488745ED73D", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken, It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult(new JwtSecurityToken(accessToken)));
			var refDbRepoCrypto = new RefDbRepoCrypto(ConfigurationProvider.CipherPublicKeyName, ConfigurationProvider.CipherPrivateKeyName, ConfigurationProvider.AesKeyName);
			var dbName = CreateDatabaseAttribute.GetDbName("C6CB576ABEF24F368BC21F29064BB847");
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				stagingRepo.Add(credentialAttributeType);
				stagingRepo.Add(credentialJobDetail);
				await stagingRepo.SaveChangesAsync();

				using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo, refDbRepoCrypto).WithWebHostBuilder(builder =>
				{
					builder.ConfigureTestServices(services =>
					{
						services.RemoveAll<ITokenValidationHelper>();
						services.AddScoped(sp => authorizationHelper.Object)
							.AddScoped(sp => tokenValidationHelper.Object);

						services.Configure<TestServerOptions>(options =>
						{
							options.AllowSynchronousIO = true;
						});
					});
				}))

				using (var client = factory.CreateClient())
				using (var postRequest = CreateHttpRequestMessage(credentialAttribute, HttpMethod.Post, "http://localhost/odata/RefApplicationAttributeUpdate"))
				{
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					var postResponse = await client.SendAsync(postRequest);
					postResponse.EnsureSuccessStatusCode();
				}
			}

			connectionString = TestConnectionString.GetAdmin(dbName);
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var refApplicationAttributes = stagingRepo.Get<RefApplicationAttribute>().ToList();
				Assert.That(refApplicationAttributes, Has.Count.EqualTo(1));
				var refApplicationAttribute = refApplicationAttributes[0];
				Assert.That(refApplicationAttribute.RAA_RAT_NKType, Is.EqualTo("Credential"));
				Assert.That(refApplicationAttribute.RAA_Value, Is.Empty);
				Assert.That(refApplicationAttribute.RAA_AttributeName, Is.EqualTo(credentialAttribute.RAA_AttributeName));
				Assert.That(refApplicationAttribute.RAA_Content, Is.Not.Empty);
			}
		}

		[Test]
		[CreateDatabase("4DE2631F7A844557B5595489D7864C70", DbSchema.RefDbRepoStaging)]
		public async Task TestDeleteAttribute()
		{
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("D85D4E02-77A4-47D4-8A04-6488745ED73D", DateTime.Now);
			var tokenValidationHelper = new Mock<ITokenValidationHelper>();
			tokenValidationHelper.Setup(x => x.ShouldHandle(accessToken)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(accessToken, It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult(new JwtSecurityToken(accessToken)));
			var refDbRepoCrypto = new RefDbRepoCrypto(ConfigurationProvider.CipherPublicKeyName, ConfigurationProvider.CipherPrivateKeyName, ConfigurationProvider.AesKeyName);
			var dbName = CreateDatabaseAttribute.GetDbName("4DE2631F7A844557B5595489D7864C70");
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				stagingRepo.Add(credentialAttributeType);
				stagingRepo.Add(credentialJobDetail);
				stagingRepo.Add(credentialAttribute);
				await stagingRepo.SaveChangesAsync();

				using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo, refDbRepoCrypto).WithWebHostBuilder(builder =>
				{
					builder.ConfigureTestServices(services =>
					{
						services.RemoveAll<ITokenValidationHelper>();
						services.AddScoped(sp => authorizationHelper.Object)
							.AddScoped(sp => tokenValidationHelper.Object);

						services.Configure<TestServerOptions>(options =>
						{
							options.AllowSynchronousIO = true;
						});
					});
				}))

				using (var client = factory.CreateClient())
				using (var postRequest = CreateHttpRequestMessage(null, HttpMethod.Delete, "http://localhost/odata/RefApplicationAttributeUpdate(68756069-2FFF-4421-8756-F143B8793E6B)"))
				{
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					var postResponse = await client.SendAsync(postRequest);
					postResponse.EnsureSuccessStatusCode();
				}
			}

			connectionString = TestConnectionString.GetAdmin(dbName);
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var refApplicationAttributes = stagingRepo.Get<RefApplicationAttribute>().ToList();
				Assert.That(refApplicationAttributes, Has.Count.EqualTo(0));
			}
		}

		HttpRequestMessage[] GetHttpRequestMessages(RefApplicationAttribute attribute)
		{
			var attributePK = attribute.RAA_PK.ToString();
			var postRequest = CreateHttpRequestMessage(attribute, HttpMethod.Post, "http://localhost/odata/RefApplicationAttributeUpdate");
			var putRequest = CreateHttpRequestMessage(attribute, HttpMethod.Put, $"http://localhost/odata/RefApplicationAttributeUpdate({attributePK})");
			var patchRequest = CreateHttpRequestMessage(attribute, HttpMethod.Patch, $"http://localhost/odata/RefApplicationAttributeUpdate({attributePK})");

			return new[] { postRequest, putRequest, patchRequest };
		}

		HttpRequestMessage CreateHttpRequestMessage(RefApplicationAttribute attribute, HttpMethod method, string url)
		{
			var request = new HttpRequestMessage(method, new Uri(url));
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.9D));
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.1D));
			request.Content = new StringContent(JsonConvert.SerializeObject(attribute), Encoding.UTF8, "application/json");
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			return request;
		}

		RefApplicationAttribute validAttribute = new RefApplicationAttribute
		{
			RAA_PK = Guid.Parse("D29BB5E5-6E41-47AE-AA8A-BFAB144DD2F7"),
			RAA_ConfigFilePath = "CargoWise.RefDbRepo.AUReferenceData.CmdLine.config.json",
			RAA_AttributeName = "OutputFilePath",
			RAA_Value = "UxmlFiles1",
			RAA_RAT_NKType = "String",
			RAA_JobGroup = "AU Customs",
		};

		RefApplicationAttribute credentialAttribute = new RefApplicationAttribute
		{
			RAA_PK = Guid.Parse("68756069-2FFF-4421-8756-F143B8793E6B"),
			RAA_ConfigFilePath = "CargoWise.RefDbRepo.BEReferenceData.CmdLine.config.json",
			RAA_AttributeName = "TestPassword",
			RAA_Value = "23123123322",
			RAA_RAT_NKType = "Credential",
			RAA_JobGroup = "BE Customs",
		};

		RefApplicationAttribute invalidAttribute = new RefApplicationAttribute
		{
			RAA_PK = Guid.Parse("73E19BF9-3779-4F77-A213-1E7F72E69071"),
			RAA_ConfigFilePath = "ABC.CmdLine.exe.config",
			RAA_AttributeName = "OutputFilePath",
			RAA_Value = "UxmlFiles2",
			RAA_RAT_NKType = "String",
			RAA_JobGroup = "ABC",
		};

		QRTZ_JOB_DETAILS jobDetails = new QRTZ_JOB_DETAILS
		{
			JOB_PK = Guid.Parse("BB7584FC-3DCB-4B59-B4A9-3E619DD7D733"),
			SCHED_NAME = "RefDbRepoQuartzServer",
			JOB_NAME = "AU Tariff",
			JOB_GROUP = "AU Customs",
			CountryCode = "AU",
			ProgramExePath = @"..\..\UniversalXMLProducers\net8.0\CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe"
		};

		QRTZ_JOB_DETAILS credentialJobDetail = new QRTZ_JOB_DETAILS
		{
			JOB_PK = Guid.Parse("B08BBC6E-796D-4C90-B18E-E8265506A140"),
			SCHED_NAME = "RefDbRepoQuartzServer",
			JOB_NAME = "BE Tariff",
			JOB_GROUP = "BE Customs",
			JOB_CLASS_NAME = "CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common",
			CountryCode = "BE",
			ProgramExePath = @"..\..\UniversalXMLProducers\net8.0\CargoWise.RefDbRepo.BEReferenceData.CmdLine.exe",
			REQUESTS_RECOVERY = false,
			JOB_DATA = Serializer.Serialize(new Dictionary<string, string>
			{
				{"CountryCode", "BE"},
				{"ProgramArgs", "arg"},
				{"ProgramExePath", @"..\..\UniversalXMLProducers\net8.0\CargoWise.RefDbRepo.BEReferenceData.CmdLine.exe"}
			})
		};

		RefApplicationAttributeType credentialAttributeType = new RefApplicationAttributeType
		{
			RAT_PK = Guid.Parse("E16E1CEC-A3FD-4D50-991A-453FFCDE5770"),
			RAT_Type = "Credential",
			RAT_Description = "credential type"
		};

		Mock<IStagingRepository> stagingRepository;
		Mock<IAuthorizationHelper> authorizationHelper;

		[SetUp]
		public void Setup()
		{
			stagingRepository = new Mock<IStagingRepository>();
			stagingRepository.Setup(x => x.Get<QRTZ_JOB_DETAILS>()).Returns(new[] { jobDetails }.AsQueryable());
			stagingRepository.Setup(x => x.Get<RefApplicationAttribute>()).Returns(new[] { validAttribute, invalidAttribute }.AsQueryable());
			authorizationHelper = new Mock<IAuthorizationHelper>();
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefApplicationAttribute>(), It.IsAny<string>())).Returns(true);
		}
	}
}
