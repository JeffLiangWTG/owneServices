using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CustomsGlobalReportModule : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CustomsGlobalReport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CustomsGlobalReport; }
		}
	}
}
