using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bunit.Extensions;
using CargoWise.Data;
using CargoWise.Winzor.AppServer.Controllers;
using CargoWise.Winzor.AppServer.Helpers;
using Enterprise.Registry.Business;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test.Controllers
{
	sealed class TestTrustedDomains
	{
		[Test]
		public async Task TestGetTrustedDomain()
		{
			var originalUrl = string.Empty;

			await using var ctx = new InMemoryAppServerTestContext(needCargoWiseRuntime: true);

			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				originalUrl = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals");
			});
			using var client = new HttpClient();
			var response = await client.GetAsync($"{ctx.ServerBaseUrl}/_clientapi/trusted-domains");

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			var trustedDomains = await response.Content.ReadAsStringAsync();
			Assert.That(trustedDomains, Does.Contain("https://glowdev/Portals"));

			if (!originalUrl.IsNullOrEmpty())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalUrl);
			}
		}

		[Test]
		public void TestGetTrustedDomainShouldNotHandleDatabaseUpgradeException()
		{
			// Arrange
			var loggerMock = new Mock<ILogger<ClientApiController>>();
			var trustedDomainGeneratorMock = new Mock<ITrustedDomainGenerator>();
			trustedDomainGeneratorMock
				.Setup(g => g.GetTrustedDomains())
				.Throws(new DatabaseUpgradeInProgressException());
			var trustedDomainGenerators = new List<ITrustedDomainGenerator>()
			{
				trustedDomainGeneratorMock.Object,
			};
			using var controller = new ClientApiController(loggerMock.Object, trustedDomainGenerators);
			var dbEnvMock = new DbEnvMock();

			using (DbEnv.SetTemporaryDbEnvironment(dbEnvMock))
			{
				// Act
				var result = controller.GetTrustedAuxiliaryDomainsForUri();

				// Assert
				Assert.That(result, Is.Not.Null);
				dbEnvMock.Mock.Verify(m =>
					m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()),
					Times.Never,
					"DbEnv.Instance.ConnectionGuiPlugin should not handle DatabaseUpgradeException"
				);
			}
		}

		[Test]
		public void TestGetTrustedDomainShouldLogExceptionsAndReturnCode500()
		{
			// Arrange
			var loggerMock = new Mock<ILogger<ClientApiController>>();
			var trustedDomainGeneratorMock = new Mock<ITrustedDomainGenerator>();
			trustedDomainGeneratorMock
				.Setup(g => g.GetTrustedDomains())
				.Throws(new Exception());
			var trustedDomainGenerators = new List<ITrustedDomainGenerator>()
			{
				trustedDomainGeneratorMock.Object,
			};
			using var controller = new ClientApiController(loggerMock.Object, trustedDomainGenerators);

			// Act
			var result = controller.GetTrustedAuxiliaryDomainsForUri();

			// Assert
			Assert.That(result, Is.AssignableTo<StatusCodeResult>());
			Assert.That(((StatusCodeResult)result).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
			loggerMock.Verify(
				x => x.Log(
					LogLevel.Warning,
					It.IsAny<EventId>(),
					It.IsAny<It.IsAnyType>(),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		class DbEnvMock : BaseDbEnvironment
		{
			public override IDbConnectionGuiPlugin ConnectionGuiPlugin => Mock.Object;
			public Mock<IDbConnectionGuiPlugin> Mock { get; } = new Mock<IDbConnectionGuiPlugin>();
		}
	}
}
