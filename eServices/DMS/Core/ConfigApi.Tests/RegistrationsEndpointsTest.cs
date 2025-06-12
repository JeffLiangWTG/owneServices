using eServices.Dms.Core.ConfigApi.Services;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using static eServices.Dms.Core.ConfigApi.Services.RegistrationsEndpoints;

namespace eServices.Dms.Core.ConfigApi.Tests
{
	public class RegistrationsEndpointsTest
	{
		private Mock<eHubTransactionsContext> _dbContext;
		private Mock<HttpContext> _httpContext;
		private static readonly eHubClient _client = new() { CC_PK = Guid.Parse("8A7EC22B-583E-4C80-A876-96A738F4BB0C"), CC_ID = "USC" };

		[SetUp]
		public void Setup()
		{
			_httpContext = new Mock<HttpContext>();
			_httpContext.Setup(x => x.User.IsInRole("read:config:USC")).Returns(true);

			var usRegistries = new List<eHubUSCustomsRegistry>
			{
				new()
				{
					ER_PK = Guid.Parse("AD8E760D-22D3-4DDF-A7DE-A1CF21239F18"),
					ER_ApplicationCode = "AMS",
					ER_CC_ClientNavigation = _client,
					ER_IsProduction = true,
					ER_Name = "Entry Filer Code",
					ER_Value = "SVAO"
				},
				new()
				{
					ER_PK = Guid.Parse("4E3CF498-A7DA-426C-B6BF-8CD890412D58"),
					ER_ApplicationCode = "AMA",
					ER_CC_ClientNavigation = _client,
					ER_IsProduction = true,
					ER_Name = "Participant Originator Code",
					ER_Value = "BCBTA18"
				},
				new()
				{
					ER_PK = Guid.Parse("877C621D-6590-4B11-93BF-E6C4FAFF1D97"),
					ER_ApplicationCode = "AMS",
					ER_CC_ClientNavigation = _client,
					ER_IsProduction = true,
					ER_Name = "Entry Filer Code",
					ER_Value = "DMAL"
				},
				new()
				{
					ER_PK = Guid.Parse("4FF06B43-AF91-4A35-BE56-5F3298319281"),
					ER_ApplicationCode = "AMS",
					ER_CC_ClientNavigation = new eHubClient { CC_PK = Guid.Parse("D923BB9B-7BE3-4028-8FE6-D2C4C66950A2"), CC_ID = "CAC" },
					ER_IsProduction = true,
					ER_Name = "Entry Filer Code",
					ER_Value = "UNSL"
				},
				new()
				{
					ER_PK = Guid.Parse("E19974F2-4951-4688-AF2A-C338E249AC69"),
					ER_ApplicationCode = "AMS",
					ER_CC_ClientNavigation = _client,
					ER_IsProduction = false,
					ER_Name = "Entry Filer Code",
					ER_Value = "MGFG"
				},
			};
			_dbContext = new Mock<eHubTransactionsContext>(new DbContextOptions<eHubTransactionsContext>());
			_dbContext.Setup(x => x.eHubUSCustomsRegistry).ReturnsDbSet(usRegistries);
		}

		[Test]
		public async Task TestGetByTypeAndId()
		{
			var result = await RegistrationsEndpoints.GetByTypeAndId(_dbContext.Object, _httpContext.Object, "USC",
				"AMS", _client.CC_ID, "true");

			var response = result.Result as Ok<RegistrationResult>;
			Assert.That(response, Is.Not.Null);
			Assert.Multiple(() =>
			{
				Assert.That(response.Value?.owner, Is.EqualTo("USC"));
				Assert.That(response.Value?.type, Is.EqualTo("AMS"));
				Assert.That(response.Value?.registrations, Is.EquivalentTo(new[]
				{
					new RegistrationItem("USC", "Entry Filer Code", "SVAO", true),
					new RegistrationItem("USC", "Entry Filer Code", "DMAL", true)
				}));
			});
		}

		[Test]
		public async Task TestGetByType()
		{
			var result = await RegistrationsEndpoints.GetByType(_dbContext.Object, _httpContext.Object, "USC", "AMS", "UNSL", "true");

			var response = result.Result as Ok<RegistrationResult>;
			Assert.That(response, Is.Not.Null);
			Assert.Multiple(() =>
			{
				Assert.That(response.Value?.owner, Is.EqualTo("USC"));
				Assert.That(response.Value?.type, Is.EqualTo("AMS"));
				Assert.That(response.Value?.registrations, Is.EquivalentTo(new[] { new RegistrationItem("CAC", "Entry Filer Code", "UNSL", true) }));
			});
		}

		[Test]
		public async Task TestEndpoints_Unauthorised()
		{
			var results = new[]
			{
				await RegistrationsEndpoints.GetByTypeAndId(_dbContext.Object, _httpContext.Object, "CAC", "AMS", _client.CC_ID, "true"),
				await RegistrationsEndpoints.GetByType(_dbContext.Object, _httpContext.Object, "CAC", "AMS", "UNSL", "true")
			};

			foreach (var result in results)
			{
				Assert.That(result.Result, Is.TypeOf<UnauthorizedHttpResult>());
			}
		}

		[Test]
		public async Task TestEndpoints_IncorrectOwner()
		{
			_httpContext.Setup(x => x.User.IsInRole("read:config:CAC")).Returns(true);

			var results = new[]
			{
				await RegistrationsEndpoints.GetByTypeAndId(_dbContext.Object, _httpContext.Object, "CAC", "AMS", _client.CC_ID, "true"),
				await RegistrationsEndpoints.GetByType(_dbContext.Object, _httpContext.Object, "CAC", "AMS", "UNSL", "true")
			};

			foreach (var result in results)
			{
				Assert.That(result.Result, Is.TypeOf<NotFound>());
			}
		}

		[Test]
		public async Task TestEndpoints_NotMatched()
		{
			_dbContext.Setup(x => x.eHubUSCustomsRegistry).ReturnsDbSet(new List<eHubUSCustomsRegistry>());

			var results = new[]
			{
				await RegistrationsEndpoints.GetByTypeAndId(_dbContext.Object, _httpContext.Object, "USC", "AMS", _client.CC_ID, "true"),
				await RegistrationsEndpoints.GetByType(_dbContext.Object, _httpContext.Object, "USC", "AMS", "UNSL", "true")
			};

			foreach (var result in results)
			{
				Assert.That(result.Result, Is.TypeOf<NotFound>());
			}
		}

		private static void AssertStatus(object? result, int i)
		{
			var statusResult = result as IStatusCodeHttpResult;
			Assert.That(statusResult, Is.Not.Null);
			Assert.That(statusResult!.StatusCode, Is.EqualTo(i));
		}
	}
}