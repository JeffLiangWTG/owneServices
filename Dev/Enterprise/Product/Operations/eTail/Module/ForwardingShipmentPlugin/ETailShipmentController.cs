using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.eTail.Module
{
	public class ETailShipmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|eTailShipment", "HVLV Consignments"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ETailShipmentPlugin((ForwardingShipment)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ETailShipment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Non persistent top level business object eTail"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Will not be implemented");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#region Security Access

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MaintainShipmentDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.MaintainShipmentEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MaintainShipmentNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.MaintainShipment; }
		}

		#endregion
	}
}
