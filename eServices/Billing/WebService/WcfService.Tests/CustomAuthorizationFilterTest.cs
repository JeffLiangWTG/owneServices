using System.Collections.Generic;
using Castle.Windsor;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class CustomAuthorizationFilterTest
	{
		[Test]
		public void Authorize_ShouldReturnTrue_WhenIpIsInAllowedList()
		{
			var environment = new Dictionary<string, object>
			{
				{ "owin.RequestHeaders", new Dictionary<string, string[]> { { "X-FORWARDED-FOR", new[] { "10.2.69.19" } } } },
				{ "server.IsLocal", false }
			};
			var context = new OwinDashboardContext(new SqlServerStorage("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"), new DashboardOptions(),
				environment);
			var result = new CustomAuthorizationFilter().Authorize(context);
			Assert.True(result);
		}

		[Test]
		public void Authorize_ShouldReturnFalse_WhenIpIsNotInAllowedList()
		{
			var environment = new Dictionary<string, object>
			{
				{ "owin.RequestHeaders", new Dictionary<string, string[]> { { "X-FORWARDED-FOR", new[] { "10.20.69.19" } } } },
				{ "server.IsLocal", false }
			};
			var context = new OwinDashboardContext(new SqlServerStorage("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"), new DashboardOptions(),
				environment);
			var result = new CustomAuthorizationFilter().Authorize(context);
			Assert.False(result);
		}

		[Test]
		public void Authorize_ShouldReturnTrue_WhenRunOnProdAndLocal()
		{
			var environment = new Dictionary<string, object>
			{
				{ "owin.RequestHeaders", new Dictionary<string, string[]> { { "X-FORWARDED-FOR", new[] { "10.20.69.19" } } } },
				{ "server.IsLocal", true }
			};
			var context = new OwinDashboardContext(new SqlServerStorage("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"), new DashboardOptions(),
				environment);
			var result = new CustomAuthorizationFilter().Authorize(context);
			Assert.True(result);
		}

		[SetUp]
		public void SetUp()
		{
			mockConfiguration = new Mock<IConfigurationProvider>();
			var containerMock = new Mock<IWindsorContainer>();
			mockDnsResolver = new Mock<IDnsResolver>();
			mockConfiguration.Setup(c => c.HostedServerNames).Returns(new[] { "au2wt-siis-401a.test.wisecloud.zone","au2wt-siis-401b.test.wisecloud.zone" ,"au2sp-siis-402a.sand.wtg.zone","au2sp-siis-402b.sand.wtg.zone"});
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(mockConfiguration.Object);
			Global.WindsorContainer = containerMock.Object;
			mockDnsResolver.Setup(x => x.GetHostAddresses(It.IsAny<string>()))
				.Returns(new[] { System.Net.IPAddress.Parse("10.2.69.77"),System.Net.IPAddress.Parse("10.2.69.78"), System.Net.IPAddress.Parse("10.2.69.19"),System.Net.IPAddress.Parse("10.2.69.20") });
			containerMock.Setup(x => x.Resolve<IDnsResolver>()).Returns(mockDnsResolver.Object);
		}

		private Mock<IConfigurationProvider> mockConfiguration;
		private Mock<IDnsResolver> mockDnsResolver;
	}
}

