using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Message;

[TestFixture]
sealed class PuescClientCredentialsTest
{
	[Test]
	public void TestCreateSecurityTokenManager() =>
		Assert.That(new PuescClientCredentials().CreateSecurityTokenManager(), Is.InstanceOf<PuescSecurityTokenManager>());

	[Test]
	public void TestCloneCore() =>
		Assert.That(new PuescClientCredentials().Clone(), Is.InstanceOf<PuescClientCredentials>());
}
