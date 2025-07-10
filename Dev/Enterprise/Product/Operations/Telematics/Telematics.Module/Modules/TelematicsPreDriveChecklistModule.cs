using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Module.Filters;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Telematics.Module
{
	public class TelematicsPreDriveChecklistModule : ZFilterGridModule
	{
		#region ZFilterGridModule

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.TelPreDriveChecklist);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TelPreDriveChecklistHeaderFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TelPreDriveChecklistHeaderFilterControl(GridCollection, (TelPreDriveChecklistHeaderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new TelPreDriveChecklistHeaderCollection(Factory);

		public override ModuleIdentifier ID => ModuleIDs.TelematicsPreDriveChecklists;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.TelematicsPreDriveChecklistHeader;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		#endregion
	}
}
