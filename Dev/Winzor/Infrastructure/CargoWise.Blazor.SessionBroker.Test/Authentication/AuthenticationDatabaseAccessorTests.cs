using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.SessionBroker.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test;

public class AuthenticationDatabaseAccessorTests : TestWithDatabase
{
	const string _accessToken = "xxx";
	AuthenticationDatabaseAccessor authDbAccessor;

	[SetUp]
	public override void Setup()
	{
		base.Setup();
		authDbAccessor = new AuthenticationDatabaseAccessor(databaseAccessor, NullLogger.Instance);
	}

	[Test]
	public void TestPeekAccessToken()
	{
		var value = authDbAccessor.TryPeekAccessToken(_accessToken);
		Assert.That(value, Is.False);
	}

	[Test]
	public async Task ConcurrentQueryIsSupported()
	{
		var tasks = Enumerable.Repeat(0, 999)
			.Select(_ => Task.Run(() =>
			{
				var value = authDbAccessor.TryPeekAccessToken(_accessToken);
				Assert.That(value, Is.False);
			}));
		await Task.WhenAll(tasks);
	}
}
