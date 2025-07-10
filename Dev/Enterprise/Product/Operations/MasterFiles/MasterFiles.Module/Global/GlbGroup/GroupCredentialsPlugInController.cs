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
	public class GroupCredentialsPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GroupCredentialsPlugIn((GlbGroup)businessEntity);
		}

		public override ControllerID ID => ControllerIDs.GroupCredentialsPlugIn;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.Groups;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.Groups;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.Groups;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.Groups;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("d61d79ac-5a59-4189-8077-549279295f7b", "Credentials");
	}
}
