using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.Module
{
	public class StaffCredentialsPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new StaffCredentialsPlugIn((GlbStaff)businessEntity);
		}

		public override ControllerID ID => ControllerIDs.StaffCredentialsPlugIn;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.Staff;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.Staff;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.Staff;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.Staff;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("52B19B12-6436-4C5F-9238-DE0E9D4C52C7", "Credentials");
	}
}
