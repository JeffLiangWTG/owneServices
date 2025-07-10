using System.Linq;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SalesTeamDetailsControlForTest : SalesTeamDetailsControl
	{
		public ZModuleButtonGrid CoveredUnlocosModuleButtonGrid_Exposed
		{
			get { return (ZModuleButtonGrid)Controls.Find("coveredUnlocosModuleButtonGrid", true).Single(); }
		}

		public ZModuleButtonGrid CoveredCountriesModuleButtonGrid_Exposed
		{
			get { return (ZModuleButtonGrid)Controls.Find("coveredCountriesModuleButtonGrid", true).Single(); }
		}
	}
}
