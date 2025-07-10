using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class HRJobRoleController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public HRJobRoleController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.HRJobRole; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(HRJobRole); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new HRJobRoleForm((HRJobRole)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.HRJobRole; }
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.HRJobRoleDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.HRJobRoleEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.HRJobRoleNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.HRJobRoleView; }
		}

		#endregion
	}
}
