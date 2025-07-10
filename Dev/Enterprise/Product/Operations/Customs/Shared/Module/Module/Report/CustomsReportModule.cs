using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CustomsReportModule : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CustomsReport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CustomsReport; }
		}
	}
}
