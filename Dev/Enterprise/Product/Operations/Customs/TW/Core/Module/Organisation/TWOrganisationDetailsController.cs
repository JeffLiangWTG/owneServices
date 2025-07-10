using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.Module
{
	public class TWOrganisationDetailsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.OrgConfigModifyCountryDefaults;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.OrgConfigModifyCountryDefaults;

		protected override Security.SecurityCheckpoint CheckPointForNew => Env.Security.OrgConfigModifyCountryDefaults;

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.OrgDetailsViewCountryDefaults;

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		public override ControllerID ID => ControllerIDs.Customs.TW.OrganisationDetailsPlugIn;

		public override System.Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("No form");

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.OrganisationDetailsPlugIn((OrgHeader)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption => Enterprise.Customs.TW.Module.Res.GetData("PlugInTabPage|TWOrganisationDetailsController", "Taiwan");
	}
}
