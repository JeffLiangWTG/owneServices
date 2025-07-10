using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(SumARegisterReadOnlyModule))]
sealed class SumARegisterReadOnlyModuleTest : ZModuleBasherTest
{
	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.NO.TemporaryStorageRegisterReadOnly;

	protected override string CountryCode => Constants.CountryCodes.Norway;
}
