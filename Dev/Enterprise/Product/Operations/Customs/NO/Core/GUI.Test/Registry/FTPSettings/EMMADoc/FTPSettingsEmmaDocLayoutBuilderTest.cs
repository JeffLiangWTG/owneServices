using Enterprise.Customs.NO.Registry;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(FTPSettingsEmmaDocLayoutBuilder<>))]
sealed class FTPSettingsEmmaDocLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<FTPSettingsEmmaDocLayoutBuilder<FTPSettingsRegistry>, FTPSettingsRegistry, FTPSettingsControlBag>
{
	protected override FTPSettingsEmmaDocLayoutBuilder<FTPSettingsRegistry> GetColumnLayoutBuilderForTesting()
	{
		return new();
	}

	protected override int ExpectedMaxColumns => 1;
}
