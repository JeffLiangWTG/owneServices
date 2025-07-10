using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class USOrganisationController : ZController
	{
		public USOrganisationController()
		{
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.US.OrganisationCustomsMessaging;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|USOrganisationCustomsMessaging", "Customs Messaging", "The Customs Messaging tab.");

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.QueryMessagesView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.QueryMessages;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.QueryMessages;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.QueryMessages;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new GUI.OrganisationPlugIn((OrgHeader)businessEntity);

		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);
	}
}
