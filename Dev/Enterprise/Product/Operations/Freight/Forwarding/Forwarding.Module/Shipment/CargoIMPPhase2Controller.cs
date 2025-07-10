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
	class CargoIMPPhase2Controller : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Freight.Forwarding.Module.Res.GetData("PlugInTabPage|CargoIMPPhase2", "CargoIMP Phase 2"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new CargoIMPPhase2PlugIn((ForwardingShipment)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CargoIMPPhase2; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Non persistent top level business object (CargoIMPPhase2MessageManager)"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not implemented yet");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CargoIMPPhase2; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CargoIMPPhase2; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MaintainShipmentNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MaintainShipmentDelete; }
		}
	}
}
