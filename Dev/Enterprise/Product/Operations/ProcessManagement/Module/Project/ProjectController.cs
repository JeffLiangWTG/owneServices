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
	public class ProjectController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ProjectForm((Project)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Project; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Project; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Project); }
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ProjectDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ProjectEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ProjectNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ProjectView; }
		}

		#endregion

		#region CRM Security

		readonly ProjectCRMSecurityProvider SecurityProvider = new ProjectCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as Project, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as Project, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as Project, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
