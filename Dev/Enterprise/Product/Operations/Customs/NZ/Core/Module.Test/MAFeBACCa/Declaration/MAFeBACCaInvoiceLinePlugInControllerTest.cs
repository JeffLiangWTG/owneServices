using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.MAFeBACCa.Testing
{
	[TestedType(typeof(MAFeBACCaInvoiceLinePlugInController))]
	sealed class MAFeBACCaInvoiceLinePlugInControllerTest : BaseMAFeBACCaPlugInControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.NZ.MAFeBACCaInvoiceLinePlugin;
		}
	}
}
