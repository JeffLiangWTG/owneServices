using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class StmMenuItemModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.StmMenuItem; }
		}

		public override ZBool HasActions
		{
			get { return ZBool.False; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new StmMenuItemFilterControl(GridCollection, (StmMenuItemFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmMenuItemCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new StmMenuItemFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Organisation; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}
	}
}
