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
	public class RefAccessorialModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.RefAccessorial;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.RefAccessorial;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			=> ZControllerFactory.Create(ControllerIDs.RefAccessorial);

		protected override FilterBusinessObject GetNewFilterBusinessObject()
			=> new RefAccessorialFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
			=> new RefAccessorialFilterControl(GridCollection, (RefAccessorialFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new RefAccessorialCollection(Factory);

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool SupportsWorkflow => false;
	}
}
