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
	public class HRHiringRequestController : ZController
	{
		public HRHiringRequestController()
		{
		}

		public override ControllerID ID => ControllerIDs.HRHiringRequest;

		public override ModuleIdentifier ModuleID => ModuleIDs.HRHiringRequest;

		public override Type TypeOfTopLevelBusinessObject => typeof(HRHiringRequest);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForView => Env.Security.HRHiringRequestView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new HRHiringRequestForm((HRHiringRequest)businessEntity);
		}
	}
}
