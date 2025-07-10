using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	public class OrgSupplierPartModuleTest : MasterFiles.Module.Testing.OrgSupplierPartModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.SupplierPart;
	}
}
