using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbBranchNotCurrentCompanyRelatedModule : GlbBranchModule
	{
		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbBranchNotCurrentCompanyRelated; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbBranchNotCurrentCompanyRelatedFilterBusinessObject();
		}

		#endregion
	}
}
