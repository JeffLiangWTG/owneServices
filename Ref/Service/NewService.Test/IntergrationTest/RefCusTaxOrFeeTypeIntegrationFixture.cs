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
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test;

[TestFixture]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
[CreateDatabase("FFF485F2C27E4FAC856EBE8E519C75B2", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
class RefCusTaxOrFeeTypeIntegrationFixture
{
	[Test]
	public async Task ShouldNotHaveDummyRecord_IfNoDataReturned()
	{
		var providerMock = ArrangeAuthentication();
		await PrepareDataWithoutTaxOrFee();

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
		var uri = new Uri("http://localhost/RefCusTaxOrFeeType/GetDataStream");
		const string jsonContent = """
					{
					    "LowerTimestamp": null,
					    "UpperTimestamp": "2099-07-01T05:34:20.1193414",
					    "Checkpoint": null,
					    "Chunksize": 1000,
					    "Version": "SRDb_569",
					    "DataSet": "RefCusTaxOrFeeType"
					}
					""";
		using var client = factory.CreateClient();
		using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
		using var response = await client.PostAsync(uri, content);

		response.EnsureSuccessStatusCode();

		var responseContent = await response.Content.ReadAsStringAsync();
		Assert.That(responseContent, Is.Not.Empty);

		var responseJObject = JObject.Parse(responseContent);
		var taxOrFees = responseJObject["RefCusTaxOrFees"]?.ToObject<Models.RefCusTaxOrFee[]>();

		Assert.That(taxOrFees, Has.Length.EqualTo(0));
	}

	[Test]
	public async Task ShouldHaveDummyRecord_IfDataReturned()
	{
		var providerMock = ArrangeAuthentication();
		await PrepareDataWithTaxOrFee();

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
		var uri = new Uri("http://localhost/RefCusTaxOrFeeType/GetDataStream");
		const string jsonContent = """
									{
									    "LowerTimestamp": null,
									    "UpperTimestamp": "2099-07-01T05:34:20.1193414",
									    "Checkpoint": null,
									    "Chunksize": 1000,
									    "Version": "SRDb_569",
									    "DataSet": "RefCusTaxOrFeeType"
									}
									""";
		using var client = factory.CreateClient();
		using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
		using var response = await client.PostAsync(uri, content);

		response.EnsureSuccessStatusCode();

		var responseContent = await response.Content.ReadAsStringAsync();
		Assert.That(responseContent, Is.Not.Empty);

		var taxOrFeeTypes = new List<Models.RefCusTaxOrFeeType>();
		using (var sr = new StringReader(responseContent))
		await using (var reader = new JsonTextReader(sr) { SupportMultipleContent = true })
		{
			var serializer = new JsonSerializer();
			while (await reader.ReadAsync())
			{
				var taxOrFeeType = serializer.Deserialize<Models.RefCusTaxOrFeeType>(reader);
				if (taxOrFeeType != null)
				{
					taxOrFeeTypes.Add(taxOrFeeType);
				}
			}
		}

		Assert.That(taxOrFeeTypes, Has.Count.EqualTo(2));
		var dummyTaxOrFeeType = taxOrFeeTypes.First();
		Assert.That(dummyTaxOrFeeType.RefCusTaxOrFees, Has.Length.EqualTo(1));
		Assert.That(dummyTaxOrFeeType.RefCusTaxOrFees[0].ZZF_ZZZ_NKDataGrouping, Is.EqualTo(RoundingIssueConstants.Flag));
		Assert.That(dummyTaxOrFeeType.RefCusTaxOrFees[0].ZZF_Value, Is.EqualTo(RoundingIssueConstants.Value));
	}

	[SetUp]
	public void SetUp()
	{
		ConnectionString = TestConnectionString.GetAdmin(CreateDatabaseAttribute.DbNamePrefix + "FFF485F2C27E4FAC856EBE8E519C75B2");
	}

	async Task PrepareDataWithTaxOrFee()
	{
		using var referenceDataRepo = new ReferenceDataRepository(ConnectionString);
		await PrepareTaxorFeeType(referenceDataRepo);

		referenceDataRepo.Add(new RefCusTaxOrFee
		{
			ZZF_Code = "Q1F",
			ZZF_Description = "test",
			ZZF_Value = 30.0m,
			ZZF_StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			ZZF_EndDate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
			ZZF_ZZZ_NKDataGrouping = "AU",
			ZZF_Minimum = 0.0m,
			ZZF_Maximum = 100.0m,
			ZZF_Threshold = 0.0m,
			ZZF_ZX0_NKTaxOrFeeType = "FLA"
		});
		await referenceDataRepo.SaveChangesAsync();

		var refDbVersionControl = referenceDataRepo.Get<RefDbVersionControl>().First(rvc => rvc.RVC_ParentPK == Guid.Parse("D1B02820-6324-4ED5-A18F-8CC01D5F6E44"));
		refDbVersionControl.RVC_IsPublished = true;
		refDbVersionControl.RVC_DataSetId = 7;
		refDbVersionControl.RVC_LastUpdatedUTC = DateTime.UtcNow;
		referenceDataRepo.Update(refDbVersionControl);
		await referenceDataRepo.SaveChangesAsync();
	}

	async Task PrepareDataWithoutTaxOrFee()
	{
		using var referenceDataRepo = new ReferenceDataRepository(ConnectionString);
		await PrepareTaxorFeeType(referenceDataRepo);

		var refDbVersionControl = referenceDataRepo.Get<RefDbVersionControl>().First(rvc => rvc.RVC_ParentPK == Guid.Parse("D1B02820-6324-4ED5-A18F-8CC01D5F6E44"));
		refDbVersionControl.RVC_IsPublished = true;
		refDbVersionControl.RVC_DataSetId = 7;
		refDbVersionControl.RVC_LastUpdatedUTC = DateTime.UtcNow;
		referenceDataRepo.Update(refDbVersionControl);
		await referenceDataRepo.SaveChangesAsync();
	}

	static async Task PrepareTaxorFeeType(ReferenceDataRepository referenceDataRepo)
	{
		referenceDataRepo.Add(new RefDataGrouping
		{
			ZZZ_DataGrouping = "AU",
			ZZZ_Description = "Australia"
		});
		referenceDataRepo.Add(new RefDataSetInformation
		{
			RDS_DataSetId = 7,
			RDS_DataSetName = nameof(RefCusTaxOrFeeType),
			RDS_TableName = nameof(RefCusTaxOrFeeType),
			RDS_DataSetTableCode = "ZX0",
			RDS_PriorityLevel = 0,
			RDS_IsPush = false
		});
		await referenceDataRepo.SaveChangesAsync();

		referenceDataRepo.Add(new RefCusTaxOrFeeType
		{
			ZX0_PK = Guid.Parse("D1B02820-6324-4ED5-A18F-8CC01D5F6E44"),
			ZX0_TaxOrFeeType = "FLA",
			ZX0_Description = "Flat",
		});
		await referenceDataRepo.SaveChangesAsync();
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
