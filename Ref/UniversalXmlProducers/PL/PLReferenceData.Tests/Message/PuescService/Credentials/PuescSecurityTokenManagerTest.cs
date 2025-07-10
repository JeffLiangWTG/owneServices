using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Message;

[TestFixture]
sealed class PuescSecurityTokenManagerTest
{
	[Test]
	public void TestCreateSecurityTokenSerializer() =>
		Assert.That(new PuescSecurityTokenManager(new PuescClientCredentials()).CreateSecurityTokenSerializer(null), Is.InstanceOf<PuescSecurityTokenSerializer>());
}
