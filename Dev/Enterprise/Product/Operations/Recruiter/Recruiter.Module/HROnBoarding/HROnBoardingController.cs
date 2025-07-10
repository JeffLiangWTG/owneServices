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
	public class HROnBoardingController : ZController
	{
		public HROnBoardingController()
		{
		}

		public override ControllerID ID => ControllerIDs.HROnBoarding;

		public override ModuleIdentifier ModuleID => ModuleIDs.HROnBoarding;

		public override Type TypeOfTopLevelBusinessObject => typeof(HROnBoarding);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WorkItemEditModifyStaffAssignment;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WorkItemEditModifyStaffAssignment;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WorkItemEditModifyStaffAssignment;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WorkItemEditModifyStaffAssignment;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new HROnBoardingForm((HROnBoarding)businessEntity);
		}
	}
}
