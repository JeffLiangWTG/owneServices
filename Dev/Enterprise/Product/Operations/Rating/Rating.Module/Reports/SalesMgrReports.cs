using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class SalesMgrReports : ZReportModule
	{
		public SalesMgrReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.SalesMgrReports; }
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RelationshipManagerReports; }
		}

		#endregion
	}
}
