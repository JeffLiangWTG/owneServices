using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class TariffsAndRatesReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.TariffRateReports; }
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TariffsAndRatesReports; }
		}

		#endregion
	}
}

