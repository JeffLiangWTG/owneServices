using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	sealed class OrgSupplierPartModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.SupplierPart;

		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
	}
}
