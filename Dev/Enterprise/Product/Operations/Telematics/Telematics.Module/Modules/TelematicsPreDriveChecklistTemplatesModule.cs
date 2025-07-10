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
	public class TelematicsPreDriveChecklistTemplatesModule : ZFilterGridModule
	{
		#region ZFilterGridModule

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.TelPreDriveChecklistTemplate);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TelPreDriveChecklistTemplateHeaderFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TelPreDriveChecklistTemplateHeaderFilterControl(GridCollection, (TelPreDriveChecklistTemplateHeaderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new TelPreDriveChecklistTemplateHeaderCollection(Factory);

		public override ModuleIdentifier ID => ModuleIDs.TelematicsPreDriveChecklistTemplates;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.TelematicsPreDriveChecklistHeaderTemplate;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => true;

		#endregion
	}
}
