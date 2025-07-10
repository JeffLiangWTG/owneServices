using System;
using System.Linq;
using System.Text;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Helpers;
using Enterprise.Integration;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test;

public class AuthenticationRegistryAccessorTests
{
	[Test]
	public void DeserializeOIDCConfigFromBinary()
	{
		var obj = ConfigWithOIDCSettingsHelper.OIDCBinaryValue();
		var config = SerializationHelper.Deserialise<DeserializedOIDCConfig>(obj as byte[]);
		AssertConfig(config);
	}

	[Test]
	public void GetOIDCConfig()
	{
		var dbAccessorMock = ConfigWithOIDCSettingsHelper.DatabaseAccessorWithOIDCMock();
		var registryAccessor = new AuthenticationConfigAccessor(new RegistryAccessor(dbAccessorMock.Object));
		var config = registryAccessor.GetOIDCConfig();
		AssertConfig(config);
	}

	[Test]
	public void OIDCServerTypeReturnRightValue()
	{
		var config = new DeserializedOIDCConfig();

#pragma warning disable CS0618 // Type or member is obsolete
		config.OIDCServerTypeCode = null;
		Assert.That(config.OIDCServerType, Is.EqualTo(OIDCServerTypes.Generic));

		config.OIDCServerTypeCode = "";
		Assert.That(config.OIDCServerType, Is.EqualTo(OIDCServerTypes.Generic));

		config.OIDCServerTypeCode = "XYZ";
		Assert.That(config.OIDCServerType, Is.EqualTo(OIDCServerTypes.Generic));

		config.OIDCServerTypeCode = "AZU";
		Assert.That(config.OIDCServerType, Is.EqualTo(OIDCServerTypes.Azure));

		config.OIDCServerTypeCode = "OKT";
		Assert.That(config.OIDCServerType, Is.EqualTo(OIDCServerTypes.Okta));
#pragma warning restore CS0618 // Type or member is obsolete
	}

	static void AssertConfig(IOIDCConfig config)
	{
		Assert.That(config, Is.Not.Null);
		Assert.That((bool)config.IsOIDCEnabled, Is.True);
		Assert.That(config.AuthorityURL.ToString(),
			Is.EqualTo("https://CargoWiseB2C01.b2clogin.com/CargoWiseB2C01.onmicrosoft.com/B2C_1A_SIGNUP_SIGNIN/v2.0/"));
		Assert.That(config.ClaimsMappings.Count(), Is.EqualTo(1));
		Assert.That(config.ClaimsMappings.First().ClaimName.ToString(),
			Is.EqualTo("unique_name"));
		Assert.That(config.ClaimsMappings.First().Identifier.ToString(),
			Is.EqualTo("GlbStaff.GS_LoginName"));
		Assert.That(config.Scopes.Count(), Is.EqualTo(1));
		Assert.That(config.Scopes.First().ScopeName.ToString(),
			Is.EqualTo("3492b154-a8ce-4ce3-a956-511276cbd9e6"));

		Assert.That(config.OIDCServerTypeCode.ToString(), Is.EqualTo("AZU"));
		Assert.That(config.OIDCServerType, Is.EqualTo(OIDCServerTypes.Azure));
		Assert.That(config.ClientIdentifier.ToString(), Is.EqualTo("3492b154-a8ce-4ce3-a956-511276cbd9e6"));
	}

	[Test]
	public void DeserializeDefaultDomainHintFromBinary()
	{
		var obj = ConfigWithOIDCSettingsHelper.DomainHintBinaryValue(true);
		var domainHint = obj is not DBNull ? Encoding.Unicode.GetString(obj as byte[]) : "Azure";
		Assert.That(domainHint, Is.Not.Null);
		Assert.That(domainHint, Is.EqualTo("Azure"));
	}

	[Test]
	public void DeserializeDomainHintFromBinary()
	{
		var obj = ConfigWithOIDCSettingsHelper.DomainHintBinaryValue(false);
		var domainHint = obj is not DBNull ? Encoding.Unicode.GetString(obj as byte[]) : "Azure";
		Assert.That(domainHint, Is.Not.Null);
		Assert.That(domainHint, Is.EqualTo("WC_EDI"));
	}

	[Test]
	public void GetDefaultDomainHint()
	{
		var dbAccessorMock = ConfigWithOIDCSettingsHelper.DatabaseAccessorWithDomainHintMock();
		var registryAccessor = new AuthenticationConfigAccessor(new RegistryAccessor(dbAccessorMock.Object));
		var domainHint = registryAccessor.GetDomainHint();
		Assert.That(domainHint, Is.Not.Null);
		Assert.That(domainHint, Is.EqualTo("Azure"));
	}

	[Test]
	public void GetNoneDefaultDomainHint()
	{
		var dbAccessorMock = ConfigWithOIDCSettingsHelper.DatabaseAccessorWithDomainHintMock(false);
		var registryAccessor = new AuthenticationConfigAccessor(new RegistryAccessor(dbAccessorMock.Object));
		var domainHint = registryAccessor.GetDomainHint();
		Assert.That(domainHint, Is.Not.Null);
		Assert.That(domainHint, Is.EqualTo("WC_EDI"));
	}
}
