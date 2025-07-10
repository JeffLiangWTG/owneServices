using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.MAFeBACCa.Testing
{
	[TestedType(typeof(MAFeBACCaConsolPlugInController))]
	sealed class MAFeBACCaConsolPlugInControllerTest : BaseMAFeBACCaPlugInControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.NZ.MAFeBACCaConsolPlugIn;
		}
	}
}
