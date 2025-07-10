using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.CFS.Module
{
	class LoadListContainersPackingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Freight.CFS.Module.Res.GetData("PlugInTabPage|LoadListContainersPacking", "Containers"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new CFSContainersPackingPlugIn((CommonConsol)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.LoadListContainersPacking; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CFSLoadListConsol); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Form is not supported");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CFSLoadList; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CFSLoadListModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CFSLoadListModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CFSLoadListModify; }
		}
	}
}
