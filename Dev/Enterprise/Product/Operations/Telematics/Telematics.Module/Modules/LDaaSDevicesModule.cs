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
	public class LDaaSDevicesModule : ZFilterGridModule
	{
		#region ZFilterGridModule

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.DeviceDetails);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GlbDeviceFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GlbDeviceFilterControl(GridCollection, (GlbDeviceFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new GlbDeviceCollection(Factory);

		public override ModuleIdentifier ID => ModuleIDs.LDaaSDevices;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.LDaaSDevices;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => true;

		#endregion
	}
}
