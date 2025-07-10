using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class EventsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ZController

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Events; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WorkflowEventForm((StmEvent)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Events; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmEvent); }
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EventsEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Events; }
		}

		#endregion
	}
}
