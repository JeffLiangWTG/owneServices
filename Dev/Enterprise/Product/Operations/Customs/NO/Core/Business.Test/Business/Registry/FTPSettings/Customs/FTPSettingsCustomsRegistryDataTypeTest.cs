using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(FTPSettingsCustomsRegistryDataType))]
sealed class FTPSettingsCustomsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FTPSettingsCustomsRegistryDataType>
{
	protected override FTPSettingsCustomsRegistryDataType GetNewDataType() => new();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var registryWithValues1 = new FTPSettingsCustomsRegistry()
		{
			Url = "ftp.ec.evry.com/",
			Port = "21",
			SendToCustomFolder = "in/",
			ReceiveFromCustomFolder = "out/",
			Username = "admin1",
			Password = "password1",
		};

		var registryWithValues2 = new FTPSettingsCustomsRegistry
		{
			Url = "ftp.ec.evry.com/",
			Port = "21",
			SendToCustomFolder = "in/",
			ReceiveFromCustomFolder = "out/",
			Username = "admin2",
			Password = "password2",
		};

		return
		[
			new ValidSampleAndBinaryValueInDB(registryWithValues1, DataType.Serialise(registryWithValues1)),
			new ValidSampleAndBinaryValueInDB(registryWithValues2, DataType.Serialise(registryWithValues2))
		];
	}

	protected override object[] GetInvalidSamples() => new object[] { new FTPSettingsCustomsRegistry() };

	protected override string ExpectedEditorName => "FTPSettingsCustomsRegistryItemEditor";
}
