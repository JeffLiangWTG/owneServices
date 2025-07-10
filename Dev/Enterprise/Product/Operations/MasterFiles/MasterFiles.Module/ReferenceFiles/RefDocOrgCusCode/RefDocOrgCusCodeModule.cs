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
	public class RefDocOrgCusCodeModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.RefDocOrgCusCode;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefDocOrgCusCode);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefDocOrgCusCodeFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefDocOrgCusCodeFilterControl(GridCollection, (RefDocOrgCusCodeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefDocOrgCusCodeCollection(Factory);
		}

		#endregion

		#region New / Edit / Delete

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		#endregion
	}
}
