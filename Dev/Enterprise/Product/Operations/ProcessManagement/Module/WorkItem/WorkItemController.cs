using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Module
{
	public class WorkItemController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WorkItemForm((WorkItem)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.WorkItem; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WorkItem; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WorkItem); }
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WorkItemDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WorkItemEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WorkItemNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WorkItemView; }
		}

		#endregion
	}
}
