using CargoWise.EntityFramework;
using Enterprise.Core.DialogDefault;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	class DialogDefaultModule : ZFilterGridModule
	{
		#region Implementation
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DialogDefault; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.DialogDefault);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DialogDefaultFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ActiveBusinessObjectCollection<StmDialogDefault>(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DialogDefaultFilterBusinessObject();
		}

		#endregion

		public override bool AllowNew
		{
			get { return false; }
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DialogDefault; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
