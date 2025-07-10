using Enterprise.Customs.Business.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(FTPSettingsRegistry))]
sealed class FTPSettingsRegistryTest : RegistryBusinessObjectTemplateTestCase<FTPSettingsRegistry>
{
	public void TestUserName_Attributes() => CombineAssertions(() =>
		AssertEntity<FTPSettingsRegistry>()
			.HasProperty(x => x.Username)
			.WithCaption("Username"));

	public void TestPassword_Attributes() => CombineAssertions(() =>
		AssertEntity<FTPSettingsRegistry>()
			.HasProperty(x => x.Password)
			.WithCaption("Password"));

	public void TestURLAddress_Attributes() => CombineAssertions(() =>
		AssertEntity<FTPSettingsRegistry>()
			.HasProperty(x => x.Url)
			.WithCaption("URL"));

	public void TestPort_Attributes() => CombineAssertions(() =>
		AssertEntity<FTPSettingsRegistry>()
			.HasProperty(x => x.Port)
			.WithCaption("Port"));

	public void TestValidation()
	{
		var ftpSettingsRegistry = new FTPSettingsRegistry();
		ValidationTestHelper.AssertErrorIfNotEntered(ftpSettingsRegistry.UsernameInfo);
		ValidationTestHelper.AssertErrorIfNotEntered(ftpSettingsRegistry.PasswordInfo);
		ValidationTestHelper.AssertErrorIfNotEntered(ftpSettingsRegistry.UrlInfo);
		ValidationTestHelper.AssertErrorIfNotEntered(ftpSettingsRegistry.PortInfo);
	}

	protected override FTPSettingsRegistry GetBusinessObjectToClone() => new();

	protected override FTPSettingsRegistry GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;
}
