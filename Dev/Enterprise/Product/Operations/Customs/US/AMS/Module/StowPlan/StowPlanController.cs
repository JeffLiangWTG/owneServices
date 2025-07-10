using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
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

namespace Enterprise.Customs.US.AMS.Module
{
	public class StowPlanController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This controller does not have GUI.");
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.StowPlan; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.NotAssigned; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobVoyage); }
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|StowPlanMessaging", "Stow Plan Messages", "The Stow Plan Messages tab."); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.SailingScheduleStowPlanMessaging; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SailingScheduleStowPlanMessaging; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SailingScheduleStowPlanMessaging; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SailingScheduleStowPlanMessaging; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new StowPlanPlugin((JobVoyage)businessEntity);
		}
	}
}
