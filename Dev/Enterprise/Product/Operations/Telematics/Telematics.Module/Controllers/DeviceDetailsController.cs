using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Telematics.Module.Controllers
{
	public class DeviceDetailsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ZController

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.LDaaSDevicesEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.LDaaSDevicesView;

		protected override IZForm GetForm(IBusiness businessEntity) => new GlbDeviceForm((GlbDevice)businessEntity);

		public override ControllerID ID => ControllerIDs.DeviceDetails;

		public override Type TypeOfTopLevelBusinessObject => typeof(GlbDevice);

		public override ModuleIdentifier ModuleID => ModuleIDs.LDaaSDevices;

		#endregion
	}
}
