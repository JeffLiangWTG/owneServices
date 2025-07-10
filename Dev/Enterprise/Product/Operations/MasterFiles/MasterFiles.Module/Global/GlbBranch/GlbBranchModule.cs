using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbBranchModule : ZFilterGridModule
	{
		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbBranch; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbBranch);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbBranchFilterControl(GridCollection, (GlbBranchFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbBranchCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbBranchFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Branch; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; } //HY: this should not have any license checks, otherwise we will find it difficult to load a new license key to them if their current one expired.
		}

		#endregion
	}
}
