using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.DataProtection;
using Enterprise.Winzor.Architecture;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Environment;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test
{
	[TestFixture]
	public class UserMonitorEndpointTest
	{
		[Test]
		public async Task TestRefreshMonitorConfigurationHandleSqlException()
		{
			await using var ctx = new InMemoryAppServerTestContext(needCargoWiseRuntime: true);

			var mockLogger = new Mock<ILogger<UserMonitorRegistry>>();
			var dataServiceFactory = ctx.HostServices.GetRequiredService<IProtectedDataServiceFactory>();
			var sqlConnectionProvider = ctx.HostServices.GetRequiredService<ISqlConnectionProvider>();

			var userMonitorRegistry = new UserMonitorRegistry(mockLogger.Object, dataServiceFactory, sqlConnectionProvider);
			string invalidServerName = "InvalidServer";
			string invalidDatabaseName = "InvalidDatabase";
			Assert.That(() => userMonitorRegistry.RefreshMonitorConfiguration(invalidServerName, invalidDatabaseName), Throws.Nothing);
			mockLogger.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v,t) => v.ToString().Contains("Monitor Url could not created due to SQLException")),
				It.IsAny<SqlException>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()
			),
			Times.Once
		);
		}

		[Test]
		public async Task TestReportMonitoringEventWithoutMonitoringEnabled()
		{
			await using var ctx = new InMemoryAppServerTestContext(needCargoWiseRuntime: true);
			DataRegistry.Instance.WebVersionUserMonitoringEnabled = false;
			var userMonitorRegistry = ctx.HostServices.GetRequiredService<UserMonitorRegistry>();
			userMonitorRegistry.RefreshMonitorConfiguration(Db.ServerName, Db.DatabaseName);
			Assert.That(userMonitorRegistry.MonitoringEnabled, Is.EqualTo(false));

			using var client = new HttpClient();
			using var data = new StringContent("{\"Test\":\"Test\"}", Encoding.UTF8, "application/json");
			var response = await client.PostAsync($"{ctx.ServerBaseUrl}/usermonitoring/events", data);
			var content = await response.Content.ReadAsStringAsync();
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
			Assert.That(content, Is.EqualTo("user monitoring is not enabled"));
		}

		[Test]
		public async Task TestReportMonitoringEventWithEmptyMonitoringURL()
		{
			await using var ctx = new InMemoryAppServerTestContext(needCargoWiseRuntime: true);
			DataRegistry.Instance.WebVersionUserMonitoringEnabled = true;
			DataRegistry.Instance.WebVersionUserMonitoringUrl = string.Empty;
			var userMonitorRegistry = ctx.HostServices.GetRequiredService<UserMonitorRegistry>();
			userMonitorRegistry.RefreshMonitorConfiguration(Db.ServerName, Db.DatabaseName);
			Assert.That(userMonitorRegistry.MonitoringEnabled, Is.EqualTo(true));
			Assert.That(userMonitorRegistry.MonitoringURI, Is.Null);

			using var client = new HttpClient();
			using var data = new StringContent("{\"Test\":\"Test\"}", Encoding.UTF8, "application/json");
			var response = await client.PostAsync($"{ctx.ServerBaseUrl}/usermonitoring/events", data);
			var content = await response.Content.ReadAsStringAsync();
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
			Assert.That(content, Is.EqualTo("monitoring server url is empty"));
		}

		[Test]
		public async Task TestReportMonitoringEvent()
		{
			var mockMonitoringServerURL = "http://localhost:8000";
			var rumEndpointPath = "/intake/v2/rum/events";
			var builder = WebApplication.CreateBuilder();
			builder.WebHost.UseUrls(mockMonitoringServerURL);
			using var app = builder.Build();
			var checkCookiesCompletionSource = new TaskCompletionSource<IRequestCookieCollection>();
			app.MapPost(rumEndpointPath, (context) =>
			{
				context.Response.StatusCode = 202;
				checkCookiesCompletionSource.SetResult(context.Request.Cookies);
				return Task.CompletedTask;
			});
			await app.StartAsync();

			await using var ctx = new InMemoryAppServerTestContext(needCargoWiseRuntime: true);

			DataRegistry.Instance.WebVersionUserMonitoringEnabled = true;
			DataRegistry.Instance.WebVersionUserMonitoringUrl = mockMonitoringServerURL + rumEndpointPath;
			var userMonitorRegistry = ctx.HostServices.GetRequiredService<UserMonitorRegistry>();
			userMonitorRegistry.RefreshMonitorConfiguration(Db.ServerName, Db.DatabaseName);
			Assert.That(userMonitorRegistry.MonitoringEnabled, Is.EqualTo(true));
			Assert.That(userMonitorRegistry.MonitoringURI.OriginalString, Is.EqualTo(mockMonitoringServerURL + rumEndpointPath));

			using var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Cookie", "session_id=12345;");
			using var data = new StringContent("{\"Test\":\"Test\"}", Encoding.UTF8, "application/json");
			var response = await client.PostAsync($"{ctx.ServerBaseUrl}/usermonitoring/events", data);
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
			var cookies = await checkCookiesCompletionSource.Task;
			Assert.That(cookies.Count, Is.EqualTo(0));
			await app.StopAsync();
		}
	}
}
