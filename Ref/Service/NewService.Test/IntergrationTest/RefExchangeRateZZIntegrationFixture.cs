using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test;

[TestFixture]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
[CreateDatabase("3F300C92DEAA45E5B42482308204A589", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
class RefExchangeRateZZIntegrationFixture
{
	[Test]
	public async Task ShouldNotHaveDummyRecord_IfNoDataReturned()
	{
		var providerMock = ArrangeAuthentication();
		await using var factory = WebApplicationFactoryHelper.WebAppFactory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureTestServices(services =>
			{
				services.RemoveAll<IReferenceDataRepository>();
				services.RemoveAll<IReadOnlyReferenceDataRepository>();
				services.RemoveAll<IDataBlockCacheHelper>();
				services.AddScoped<IReferenceDataRepository>(_ => new ReferenceDataRepository(ConnectionString));
				services.AddScoped<IReadOnlyReferenceDataRepository>(_ => new ReadOnlyReferenceDataRepository(ConnectionString));
				services.AddScoped(_ => new Mock<IDataBlockCacheHelper>().Object);
				services.AddSingleton(providerMock.Object);

				services.Configure<TestServerOptions>(options =>
				{
					options.AllowSynchronousIO = true;
				});
			});
		});
		var uri = new Uri("http://localhost/RefExchangeRateZZ/GetDataStream");
		const string jsonContent = """
					{
					    "LowerTimestamp": null,
					    "UpperTimestamp": "2099-07-01T05:34:20.1193414",
					    "Checkpoint": null,
					    "Chunksize": 1000,
					    "Version": "SRDb_569",
					    "DataSet": "RefExchangeRateZZ"
					}
					""";
		using var client = factory.CreateClient();
		using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
		using var response = await client.PostAsync(uri, content);

		response.EnsureSuccessStatusCode();

		var responseContent = await response.Content.ReadAsStringAsync();
		Assert.That(responseContent, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task ShouldHaveDummyRecord_IfDataReturned()
	{
		var providerMock = ArrangeAuthentication();
		await PrepareData();

		await using var factory = WebApplicationFactoryHelper.WebAppFactory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureTestServices(services =>
			{
				services.RemoveAll<IReferenceDataRepository>();
				services.RemoveAll<IReadOnlyReferenceDataRepository>();
				services.RemoveAll<IDataBlockCacheHelper>();
				services.AddScoped<IReferenceDataRepository>(p => new ReferenceDataRepository(ConnectionString));
				services.AddScoped<IReadOnlyReferenceDataRepository>(p => new ReadOnlyReferenceDataRepository(ConnectionString));
				services.AddScoped(_ => new Mock<IDataBlockCacheHelper>().Object);
				services.AddSingleton(providerMock.Object);

				services.Configure<TestServerOptions>(options =>
				{
					options.AllowSynchronousIO = true;
				});
			});
		});
		var uri = new Uri("http://localhost/RefExchangeRateZZ/GetDataStream");
		const string jsonContent = """
									{
									    "LowerTimestamp": null,
									    "UpperTimestamp": "2099-07-01T05:34:20.1193414",
									    "Checkpoint": null,
									    "Chunksize": 1000,
									    "Version": "SRDb_569",
									    "DataSet": "RefExchangeRateZZ"
									}
									""";
		using var client = factory.CreateClient();
		using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
		using var response = await client.PostAsync(uri, content);

		response.EnsureSuccessStatusCode();

		var responseContent = await response.Content.ReadAsStringAsync();
		Assert.That(responseContent, Is.Not.Empty);
		var rates = new List<Models.RefExchangeRateZZ>();
		using (var sr = new StringReader(responseContent))
		await using (var reader = new JsonTextReader(sr) { SupportMultipleContent = true })
		{
			var serializer = new JsonSerializer();
			while (await reader.ReadAsync())
			{
				var rate = serializer.Deserialize<Models.RefExchangeRateZZ>(reader);
				if (rate != null)
				{
					rates.Add(rate);
				}
			}
		}

		Assert.That(rates, Has.Count.EqualTo(2));
		var dummyRate = rates.First();
		Assert.That(dummyRate.ZZN_RN_NKCountry, Is.EqualTo(RoundingIssueConstants.Flag));
		Assert.That(dummyRate.ZZN_Rate, Is.EqualTo(RoundingIssueConstants.Value));
	}

	async Task PrepareData()
	{
		using var referenceDataRepo = new ReferenceDataRepository(ConnectionString);
		referenceDataRepo.Add(new RefDataSetInformation
		{
			RDS_DataSetId = 5,
			RDS_TableName = nameof(RefExchangeRateZZ),
			RDS_DataSetName = nameof(RefExchangeRateZZ),
			RDS_DataSetTableCode = "ZZN",
			RDS_PriorityLevel = 0,
			RDS_IsPush = false
		});
		referenceDataRepo.Add(new RefExchangeRateZZ
		{
			ZZN_PK = Guid.Parse("615A5F1C-A3EF-4B31-8D0E-5F40C8CC96FE"),
			ZZN_ExRateType = "CUS",
			ZZN_StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			ZZN_EndDate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
			ZZN_Rate = 1.8m,
			ZZN_RX_NKExCurrency = "USD",
			ZZN_RN_NKCountry = "CA",
			ZZN_AsPublished = string.Empty
		});
		await referenceDataRepo.SaveChangesAsync();

		var refDbVersionControl = referenceDataRepo.Get<RefDbVersionControl>().First(rvc => rvc.RVC_ParentPK == Guid.Parse("615A5F1C-A3EF-4B31-8D0E-5F40C8CC96FE"));
		refDbVersionControl.RVC_IsPublished = true;
		refDbVersionControl.RVC_DataSetId = 5;
		refDbVersionControl.RVC_LastUpdatedUTC = DateTime.UtcNow;
		referenceDataRepo.Update(refDbVersionControl);
		await referenceDataRepo.SaveChangesAsync();
	}

	[SetUp]
	public void SetUp()
	{
		ConnectionString = TestConnectionString.GetAdmin(CreateDatabaseAttribute.DbNamePrefix + "3F300C92DEAA45E5B42482308204A589");
	}

	static Mock<IAuthenticationHandlerProvider> ArrangeAuthentication()
	{
		var providerMock = new Mock<IAuthenticationHandlerProvider>();
		var handlerMock = new Mock<IAuthenticationHandler>();
		var principal = new ClaimsPrincipal(new GenericIdentity("Dummy"));
		var authenticateTicket = new AuthenticationTicket(principal, AuthType.BasicAuth);
		var result = AuthenticateResult.Success(authenticateTicket);
		handlerMock.Setup(x => x.AuthenticateAsync()).ReturnsAsync(result);
		handlerMock.Setup(x => x.InitializeAsync(It.IsAny<AuthenticationScheme>(), It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
		providerMock.Setup(x => x.GetHandlerAsync(It.IsAny<HttpContext>(), It.IsAny<string>())).ReturnsAsync(handlerMock.Object);
		return providerMock;
	}

	string ConnectionString { set; get; }
}
