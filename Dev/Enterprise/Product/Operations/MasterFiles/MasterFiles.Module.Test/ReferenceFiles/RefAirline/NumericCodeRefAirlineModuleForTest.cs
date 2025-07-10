using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class NumericCodeRefAirlineModuleForTest : NumericCodeRefAirlineModule
	{
		public NumericCodeRefAirlineModuleForTest()
		{
		}

		public ZController Controller => GetNewController(null);
	}
}
