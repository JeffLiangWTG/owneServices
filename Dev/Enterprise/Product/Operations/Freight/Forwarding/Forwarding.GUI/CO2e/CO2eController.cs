using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class CO2eController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("This controller does not have a GUI.");

		public override ControllerID ID => ControllerIDs.CO2ePlugin;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("Not supported");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new CO2ePlugin(businessEntity);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
	}
}
