using Enterprise.Customs.NO.Registry;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(FTPSettingsCustomsLayoutBuilder<>))]
sealed class FTPSettingsCustomsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<FTPSettingsCustomsLayoutBuilder<FTPSettingsCustomsRegistry>, FTPSettingsCustomsRegistry, FTPSettingsControlBag>
{
	protected override FTPSettingsCustomsLayoutBuilder<FTPSettingsCustomsRegistry> GetColumnLayoutBuilderForTesting()
	{
		return new ();
	}

	protected override int ExpectedMaxColumns => 1;
}
