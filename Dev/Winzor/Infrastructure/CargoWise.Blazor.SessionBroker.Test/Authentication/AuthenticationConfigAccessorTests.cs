using System.Linq;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Helpers;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class AuthenticationConfigAccessorTests
	{
		[Test]
		public void GetOIDCConfigShouldBeWinzorOIDCConfigWhenItIsEnabled()
		{
			var registryAccessor = new Mock<IRegistryAccessor>();
			registryAccessor.Setup(r => r.GetBinaryValue("WinzorOIDCConfig")).Returns(OIDCConfigBinaryValue(true));

			var configAccessor = new AuthenticationConfigAccessor(registryAccessor.Object);
			var config = configAccessor.GetOIDCConfig();

			registryAccessor.Verify(r => r.GetBinaryValue("WinzorOIDCConfig"), Times.Once);
			registryAccessor.Verify(r => r.GetBinaryValue("OIDCConfig"), Times.Never);

			Assert.That(config, Is.Not.Null);
			Assert.That((bool)config.IsOIDCEnabled, Is.True);
			Assert.That(config.AuthorityURL.ToString(), Is.EqualTo("https://loginsimulator.wisetechglobal.com/"));
		}

		[Test]
		public void GetOIDCConfigShouldFallBackToOIDCConfigWhenWinzorOIDCConfigIsDisabled()
		{
			var registryAccessor = new Mock<IRegistryAccessor>();
			registryAccessor.Setup(r => r.GetBinaryValue("WinzorOIDCConfig")).Returns(OIDCConfigBinaryValue(false));
			registryAccessor.Setup(r => r.GetBinaryValue("OIDCConfig")).Returns(OIDCConfigBinaryValue(true));

			var configAccessor = new AuthenticationConfigAccessor(registryAccessor.Object);
			var config = configAccessor.GetOIDCConfig();

			registryAccessor.Verify(r => r.GetBinaryValue("WinzorOIDCConfig"), Times.Once);
			registryAccessor.Verify(r => r.GetBinaryValue("OIDCConfig"), Times.Once);

			Assert.That(config, Is.Not.Null);
			Assert.That((bool)config.IsOIDCEnabled, Is.True);
			Assert.That(config.AuthorityURL.ToString(), Is.EqualTo("https://loginsimulator.wisetechglobal.com/"));
		}

		[Test]
		public void GetOIDCConfigShouldFallBackToOIDCConfigWhenWinzorOIDCConfigIsNull()
		{
			var registryAccessor = new Mock<IRegistryAccessor>();
			registryAccessor.Setup(r => r.GetBinaryValue("WinzorOIDCConfig")).Returns((byte[])null);
			registryAccessor.Setup(r => r.GetBinaryValue("OIDCConfig")).Returns(OIDCConfigBinaryValue(true));

			var configAccessor = new AuthenticationConfigAccessor(registryAccessor.Object);
			var config = configAccessor.GetOIDCConfig();

			registryAccessor.Verify(r => r.GetBinaryValue("WinzorOIDCConfig"), Times.Once);
			registryAccessor.Verify(r => r.GetBinaryValue("OIDCConfig"), Times.Once);

			Assert.That(config, Is.Not.Null);
			Assert.That((bool)config.IsOIDCEnabled, Is.True);
			Assert.That(config.AuthorityURL.ToString(), Is.EqualTo("https://loginsimulator.wisetechglobal.com/"));
		}

		byte[] OIDCConfigBinaryValue(bool isEnabled)
		{
			var xmlString = @$"<?xml version=""1.0"" encoding=""utf-16""?>
<OIDCConfig>
    <Version>1</Version>
    <IsOIDCEnabled>{(isEnabled ? "Y" : "N")}</IsOIDCEnabled>
    <OIDCServerTypeCode>AZU</OIDCServerTypeCode>
    <AuthorityURL>https://loginsimulator.wisetechglobal.com/</AuthorityURL>
    <ClientIdentifier>dummyIdP</ClientIdentifier>
    <ArrayOfOIDCClaimsMapping>
        <OIDCClaimsMapping>
            <Version>1</Version>
            <ClaimName>user_name</ClaimName>
            <Identifier>GlbStaff.GS_LoginName</Identifier>
        </OIDCClaimsMapping>
    </ArrayOfOIDCClaimsMapping>
    <ArrayOfOIDCScope>
        <OIDCScope>
            <Version>1</Version>
            <ScopeName>dummyIdP</ScopeName>
        </OIDCScope>
    </ArrayOfOIDCScope>
</OIDCConfig>";
			return System.Text.Encoding.Unicode.GetBytes(xmlString);
		}
	}
}
