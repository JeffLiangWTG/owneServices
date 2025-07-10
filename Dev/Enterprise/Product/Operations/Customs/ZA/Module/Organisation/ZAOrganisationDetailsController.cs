using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class ZAOrganisationDetailsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrgDetailsViewCountryDefaults; }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		public override ControllerID ID
		{
			get { return ZAControllerIDs.OrganisationDetailsPlugIn; }
		}

		public override System.Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("No form"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("No form"); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.OrganisationDetailsPlugIn((OrgHeader)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.ZA.Module.Res.GetData("PlugInTabPage|ZAOrganisationDetailsController", "South Africa"); }
		}
	}
}
