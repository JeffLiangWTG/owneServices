using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Blazor.AppServer.Test
{
	using static PlaywrightTestContext;

	public class AuthTokenTests
	{
		IConfiguration config;
		CargoWiseOptions cwOptions;
		DebugClientTokenGenerator tokenGenerator;
		string header;

		[SetUp]
		public void Setup()
		{
			config = new TestConfiguration();
			cwOptions = ConfigurationBinder.Get<CargoWiseOptions>(config.GetSection("CargoWiseOptions")) ?? new CargoWiseOptions();
			header = $"{cwOptions.DbServerName} {cwOptions.DatabaseName}";
			tokenGenerator = new DebugClientTokenGenerator(Options.Create(cwOptions));
		}

		[Test]
		public void TestValidTokenLaunchesAndStaysRunning()
		{
			Process process = default;
			try
			{
				var token = tokenGenerator.GenerateClientToken();
				var appServer = TestAppServerProcess.FromTestContext(config).Create(header, token);
				process = appServer.Process;
				Assert.That(appServer.Address, Is.Not.Null);
				Assert.That(Uri.TryCreate(appServer.Address, UriKind.Absolute, out _));
				Assert.That(appServer.ProcessId, Is.GreaterThan(0));
				var runningProcess = Process.GetProcessById(appServer.ProcessId);
				Assert.That(runningProcess.HasExited, Is.False);
			}
			finally
			{
				process?.Kill(true);
			}
		}

		[Test]
		public void TestTokenNotInDbDoesntWork()
		{
			Assert.That(() => TestAppServerProcess.FromTestContext(config).Create(header, "ABCDEFG"), Throws.TypeOf<ProcessStartException>());
		}

		[Test]
		public void TestTokenWithInvalidTypeDoesntWork()
		{
			var token = tokenGenerator.GenerateClientToken(type: "BLD"); // should be BLC
			Assert.That(() => TestAppServerProcess.FromTestContext(config).Create(header, token), Throws.TypeOf<ProcessStartException>());
		}

		[Test]
		public void TestTokenWithInvalidParentFkDoesntWork()
		{
			var token = tokenGenerator.GenerateClientToken(parentId: Guid.NewGuid());
			Assert.That(() => TestAppServerProcess.FromTestContext(config).Create(header, token), Throws.TypeOf<ProcessStartException>());
		}

		[Test]
		public void TestTokenReferringToUnknownTableDoesntWork()
		{
			var token = tokenGenerator.GenerateClientToken(parentTableCode: "XX");
			Assert.That(() => TestAppServerProcess.FromTestContext(config).Create(header, token), Throws.TypeOf<ProcessStartException>());
		}

		[Test]
		public void TestExpiredTokenDoesntWork()
		{
			var token = tokenGenerator.GenerateClientToken(expiresAtUtc: DateTime.UtcNow.AddHours(-1));
			Assert.That(() => TestAppServerProcess.FromTestContext(config).Create(header, token), Throws.TypeOf<ProcessStartException>());
		}
	}
}
