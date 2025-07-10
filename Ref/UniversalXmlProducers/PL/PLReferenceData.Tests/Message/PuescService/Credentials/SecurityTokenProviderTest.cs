using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Message;

[TestFixture]
sealed class SecurityTokenProviderTest
{
	[Test]
	public void TestAddSecurityToken()
	{
		const string testUserName = "TEST_USER_NAME";
		const string testTokenId = "TEST_TOKEN_ID";
		const string testPassword = "TEST_PASSWORD";
		const string expectedNonce = "FMkbl0dxVeoNttMqS8lMA1IR0yA=";
		const string expectedDigest = "s4o+TcSnQCM+eTQ21Jf+XVAWQtU=";
		const string expectedCreated = "2000-01-30T05:55:55.000Z";

		var digestBuilderMock = new Mock<IDigestBuilder>();
		digestBuilderMock
			.Setup(x => x.Build(testPassword, null, null))
			.Returns((expectedNonce, expectedDigest, expectedCreated));
		var securityTokenProvider = new SecurityTokenProvider(digestBuilderMock.Object);
		var tokenElements = securityTokenProvider.GetSecurityTokenElements(testTokenId, testUserName, testPassword);

		const string expectedTokenElements =
$@"<o:UsernameToken u:Id=""{testTokenId}"" xmlns:u=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" xmlns:o=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"">
  <o:Username>{testUserName}</o:Username>
  <o:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest"">{expectedDigest}</o:Password>
  <o:Nonce EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">{expectedNonce}</o:Nonce>
  <u:Created>{expectedCreated}</u:Created>
</o:UsernameToken>";
		Assert.That(tokenElements, Is.EqualTo(expectedTokenElements));
	}
}
