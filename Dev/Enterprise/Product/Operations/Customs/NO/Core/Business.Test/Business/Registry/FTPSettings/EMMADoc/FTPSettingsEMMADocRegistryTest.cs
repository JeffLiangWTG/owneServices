using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NO.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(FTPSettingsEMMADocRegistry))]
sealed class FTPSettingsEMMADocRegistryTest : TestCaseWithFactory
{
	public void TestDefaultValues() => CombineAssertions(() =>
	{
		var defaultValues = FTPSettingsEMMADocRegistry.DefaultValues;
		AssertNotSame("DefaultValues should either be NOT cached or immutable, otherwise changes to one company settings can leak to another.", defaultValues, FTPSettingsEMMADocRegistry.DefaultValues);
		AssertEquals("Port", "21", defaultValues.Port);
		AssertEquals("Url", "ftpedoc.emma.no", defaultValues.Url);
	});
}
