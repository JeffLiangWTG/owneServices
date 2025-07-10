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
	public class HRJobApplicantController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public HRJobApplicantController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.HRJobApplicant; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.HRJobApplicant; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(HRJobApplicant); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new HRJobApplicantForm((HRJobApplicant)businessEntity);
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.HRJobApplicantDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.HRJobApplicantEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.HRJobApplicantNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.HRJobApplicantView; }
		}

		#endregion
	}
}
