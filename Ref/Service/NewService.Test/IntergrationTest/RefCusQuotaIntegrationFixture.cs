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
[CreateDatabase("5443C9F0C8D0487195CEF76F07903B87", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
class RefCusQuotaIntegrationFixture
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
		var uri = new Uri("http://localhost/RefCusQuota/GetDataStream");
		const string jsonContent = """
					{
					    "LowerTimestamp": null,
					    "UpperTimestamp": "2099-07-01T05:34:20.1193414",
					    "Checkpoint": null,
					    "Chunksize": 1000,
					    "Version": "SRDb_569",
					    "DataSet": "RefCusQuota"
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
		var uri = new Uri("http://localhost/RefCusQuota/GetDataStream");
		const string jsonContent = """
									{
									    "LowerTimestamp": null,
									    "UpperTimestamp": "2099-07-01T05:34:20.1193414",
									    "Checkpoint": null,
									    "Chunksize": 1000,
									    "Version": "SRDb_569",
									    "DataSet": "RefCusQuota"
									}
									""";
		using var client = factory.CreateClient();
		using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
		using var response = await client.PostAsync(uri, content);

		response.EnsureSuccessStatusCode();

		var responseContent = await response.Content.ReadAsStringAsync();
		Assert.That(responseContent, Is.Not.Empty);
		var quotas = new List<Models.RefCusQuota>();
		using (var sr = new StringReader(responseContent))
		await using (var reader = new JsonTextReader(sr) { SupportMultipleContent = true })
		{
			var serializer = new JsonSerializer();
			while (await reader.ReadAsync())
			{
				var quota = serializer.Deserialize<Models.RefCusQuota>(reader);
				if (quota != null)
				{
					quotas.Add(quota);
				}
			}
		}

		Assert.That(quotas, Has.Count.EqualTo(2));
		var dummyQuota = quotas.First();
		Assert.That(dummyQuota.ZXQ_OrderNumber, Is.EqualTo("XXXXXX"));
		Assert.That(dummyQuota.ZXQ_Balance, Is.EqualTo(RoundingIssueConstants.Value));
		Assert.That(dummyQuota.ZXQ_ZZZ_NKDataGrouping, Is.EqualTo(RoundingIssueConstants.Flag));
	}

	async Task PrepareData()
	{
		using var referenceDataRepo = new ReferenceDataRepository(ConnectionString);
		referenceDataRepo.Add(new RefDataGrouping
		{
			ZZZ_DataGrouping = "EUN",
			ZZZ_Description = "European Union"
		});
		await referenceDataRepo.SaveChangesAsync();

		referenceDataRepo.Add(new RefDataSetInformation
		{
			RDS_DataSetId = 53,
			RDS_TableName = nameof(RefCusQuota),
			RDS_DataSetName = nameof(RefCusQuota),
			RDS_DataSetTableCode = "ZXQ",
			RDS_PriorityLevel = 0,
			RDS_IsPush = false
		});
		referenceDataRepo.Add(new RefCusQuota
		{
			ZXQ_PK = Guid.Parse("3802BDD7-A9F2-477B-A99B-498AC057C09C"),
			ZXQ_OrderNumber = "223232",
			ZXQ_InitialAmount = 0.0m,
			ZXQ_UnitOfMeasure = "KGM",
			ZXQ_Balance = 0.0m,
			ZXQ_StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			ZXQ_EndDate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
			ZXQ_ZZZ_NKDataGrouping = "EUN"
		});
		await referenceDataRepo.SaveChangesAsync();

		var refDbVersionControl = referenceDataRepo.Get<RefDbVersionControl>().First(rvc => rvc.RVC_ParentPK == Guid.Parse("3802BDD7-A9F2-477B-A99B-498AC057C09C"));
		refDbVersionControl.RVC_IsPublished = true;
		refDbVersionControl.RVC_DataSetId = 53;
		refDbVersionControl.RVC_LastUpdatedUTC = DateTime.UtcNow;
		referenceDataRepo.Update(refDbVersionControl);
		await referenceDataRepo.SaveChangesAsync();
	}

	[SetUp]
	public void SetUp()
	{
		ConnectionString = TestConnectionString.GetAdmin(CreateDatabaseAttribute.DbNamePrefix + "5443C9F0C8D0487195CEF76F07903B87");
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
