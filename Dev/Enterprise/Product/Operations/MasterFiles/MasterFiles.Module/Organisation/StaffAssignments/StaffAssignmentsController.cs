using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class StaffAssignmentsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.StaffAssignments; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.StaffAssignments; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgStaffAssignments); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrgDetailsViewCompanysStaffAssignments; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgDetailsModifyStaffAssignments; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgDetailsModifyStaffAssignments; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrgDetailsModifyStaffAssignments; }
		}
	}
}
