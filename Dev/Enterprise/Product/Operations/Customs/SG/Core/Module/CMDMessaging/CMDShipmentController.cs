using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.SG.V4.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.SG.V4.Module
{
	public class CMDShipmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.SG.V4.Module.Res.GetData("PlugInTabPage|CMDShipment", "CMD", "CMD", "CMD", ""); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new CMDShipmentPlugIn((ForwardingShipment)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.SG.CMDShipment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingShipment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("");
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}
	}
}
