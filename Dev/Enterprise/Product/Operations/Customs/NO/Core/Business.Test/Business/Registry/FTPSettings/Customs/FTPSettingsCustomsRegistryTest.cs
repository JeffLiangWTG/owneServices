using Enterprise.Customs.Business.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(FTPSettingsCustomsRegistry))]
sealed class FTPSettingsCustomsRegistryTest : RegistryBusinessObjectTemplateTestCase<FTPSettingsCustomsRegistry>
{
	public void TestSendToCustomFolder_Attributes() => CombineAssertions(() =>
		AssertEntity<FTPSettingsCustomsRegistry>()
			.HasProperty(x => x.SendToCustomFolder)
			.WithCaption("Folder for send to Customs"));

	public void TestReceiveFromCustomFolder_Attributes() => CombineAssertions(() =>
		AssertEntity<FTPSettingsCustomsRegistry>()
			.HasProperty(x => x.ReceiveFromCustomFolder)
			.WithCaption("Folder for receive from Customs"));

	public void TestDefaultValues() => CombineAssertions(() =>
	{
		AssertEquals("URL", "ftp.ec.evry.com", FTPSettingsCustomsRegistry.DefaultValues.Url);
		AssertEquals("Port", "21", FTPSettingsCustomsRegistry.DefaultValues.Port);
		AssertEquals("SendToCustomFolder", "in", FTPSettingsCustomsRegistry.DefaultValues.SendToCustomFolder);
		AssertEquals("ReceiveFromCustomFolder", "out", FTPSettingsCustomsRegistry.DefaultValues.ReceiveFromCustomFolder);
	});

	public void TestValidation()
	{
		var ftpSettingsCustomsRegistry = new FTPSettingsCustomsRegistry();
		ValidationTestHelper.AssertErrorIfNotEntered(ftpSettingsCustomsRegistry.SendToCustomFolderInfo);
		ValidationTestHelper.AssertErrorIfNotEntered(ftpSettingsCustomsRegistry.ReceiveFromCustomFolderInfo);
	}

	protected override FTPSettingsCustomsRegistry GetBusinessObjectToClone() => new();

	protected override FTPSettingsCustomsRegistry GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;
}
