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
	public class HRJobApplicationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public HRJobApplicationController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.HRJobApplication; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.HRJobApplication; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(HRJobApplication); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new HRJobApplicationForm((HRJobApplication)businessEntity);
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.HRJobApplicationDelete;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.HRJobApplicationEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.HRJobApplicationNew;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.HRJobApplicationView;

		#endregion
	}
}
