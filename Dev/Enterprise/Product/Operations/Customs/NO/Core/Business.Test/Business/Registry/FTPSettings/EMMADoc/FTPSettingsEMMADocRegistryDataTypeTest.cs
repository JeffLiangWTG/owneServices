using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(FTPSettingsEMMADocRegistryDataType))]
sealed class FTPSettingsEMMADocRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FTPSettingsEMMADocRegistryDataType>
{
	protected override FTPSettingsEMMADocRegistryDataType GetNewDataType() => new();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var registryWithValues1 = new FTPSettingsRegistry
		{
			Url = "ftpedoc.emma.no",
			Port = "21",
			Username = "admin1",
			Password = "password1",
		};

		var registryWithValues2 = new FTPSettingsRegistry
		{
			Url = "ftpedoc.emma.no",
			Port = "21",
			Username = "admin2",
			Password = "password2",
		};

		return
		[
			new ValidSampleAndBinaryValueInDB(registryWithValues1, DataType.Serialise(registryWithValues1)),
			new ValidSampleAndBinaryValueInDB(registryWithValues2, DataType.Serialise(registryWithValues2))
		];
	}

	protected override object[] GetInvalidSamples() => new object[] { new FTPSettingsRegistry() };

	protected override string ExpectedEditorName => "FTPSettingsEMMADocRegistryItemEditor";
}
