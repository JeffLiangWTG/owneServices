using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffChangeRequestController : ZController
	{
		public GlbStaffChangeRequestController() { }

		public override ControllerID ID => ControllerIDs.GlbStaffChangeRequest;

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbStaffChangeRequest;

		public override Type TypeOfTopLevelBusinessObject => typeof(GlbStaffChangeRequest);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlbStaffChangeRequestView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbStaffChangeRequestForm((GlbStaffChangeRequest)businessEntity);
		}
	}
}
