using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.NZ.Module.MAFeBACCa.Testing
{
	abstract class BaseMAFeBACCaPlugInControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.NewZealand;
			}
		}
	}
}
