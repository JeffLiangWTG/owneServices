using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.Module
{
	class ElectronicBOLController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|ElectronicBOL", "Electronic Bill Of Lading");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new ElectronicBOLPlugIn((ForwardingShipment)businessEntity);

		public override ControllerID ID => ControllerIDs.ElectronicBOL;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("Non persistent top level business object (ElectronicBOL)");

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("Not implemented yet");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.MaintainShipmentAllowPublisheHBL;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.MaintainShipmentAllowPublisheHBL;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.MaintainShipmentNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.MaintainShipmentDelete;
	}
}
