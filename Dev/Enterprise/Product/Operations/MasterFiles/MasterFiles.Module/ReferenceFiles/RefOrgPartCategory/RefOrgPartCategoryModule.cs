using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	class RefOrgPartCategoryModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefOrgPartCategory; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefOrgPartCategory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefOrgPartCategoryFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgPartCategoryCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RefOrgPartCategory; }
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new RefOrgPartCategoryFilterControl(GridCollection, (RefOrgPartCategoryFilterBusinessObject)FilterBusinessObject);
		}
	}
}
