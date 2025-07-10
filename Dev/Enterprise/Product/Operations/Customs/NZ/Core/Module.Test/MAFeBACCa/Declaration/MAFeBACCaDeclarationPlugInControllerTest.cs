using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.MAFeBACCa.Testing
{
	[TestedType(typeof(MAFeBACCaDeclarationPlugInController))]
	sealed class MAFeBACCaDeclarationPlugInControllerTest : BaseMAFeBACCaPlugInControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.NZ.MAFeBACCaDeclarationPlugin;
		}
	}
}
