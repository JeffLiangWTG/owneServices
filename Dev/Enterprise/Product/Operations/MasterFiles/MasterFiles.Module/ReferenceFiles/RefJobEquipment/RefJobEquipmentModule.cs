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
	public class RefJobEquipmentModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.RefJobEquipment;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.RefJobEquipment;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			=> ZControllerFactory.Create(ControllerIDs.RefJobEquipment);

		protected override FilterBusinessObject GetNewFilterBusinessObject()
			=> new RefJobEquipmentFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
			=> new RefJobEquipmentFilterControl(GridCollection, (RefJobEquipmentFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobEquipmentCollection(Factory);

		public override bool SupportsWorkflow => false;
	}
}
