using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.Module
{
	class ContainersPackingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Freight.Forwarding.Module.Res.GetData("PlugInTabPage|JobConsolContainersPacking", "Packing"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ForwardingContainersPackingPlugIn((CommonConsol)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobConsolContainersPacking; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Form is not supported");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.MaintainConsolContainersPacking; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.MaintainConsolContainersPacking; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MaintainConsolNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MaintainConsolDelete; }
		}
	}
}
