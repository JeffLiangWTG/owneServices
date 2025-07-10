using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class OrgStaffAssignmentsValidation : AutoOrgStaffAssignmentsValidation
	{
		public OrgStaffAssignmentsValidation(AutoOrgStaffAssignments parent)
			: base(parent)
		{
			this.Parent = (OrgStaffAssignments)parent;
		}

		public new OrgStaffAssignments Parent;

		#region O8_Department

		protected override void CheckO8_Department()
		{
			base.CheckO8_Department();
			MandatoryValidation.CheckEntered(Parent.O8_DepartmentInfo);
			ListValidation.ErrorIfInvalidCode(Parent.O8_DepartmentInfo);
		}

		#endregion

		#region O8_GS_NKPersonResponsible

		protected override void CheckO8_GS_NKPersonResponsible()
		{
			base.CheckO8_GS_NKPersonResponsible();
			MandatoryValidation.CheckEntered(Parent.O8_GS_NKPersonResponsibleInfo);
			if (IsResponsiblePersonMandatory && Parent.O8_GS_NKPersonResponsibleInfo.HasErrors())
			{
				Parent.O8_GS_NKPersonResponsibleInfo.AddError(Res.GetString("dca6342d-abff-4c06-9e76-b07065c489ba", "Sales Representative must be entered because System -> Registry -> Organizations -> Make Sales Rep Mandatory is ON and current organization is marked as Receivables or Sales. Please enter a Sales representative or turn off this setting in the Registry."));
			}
			ListValidation.ErrorIfInvalidCode(Parent.O8_GS_NKPersonResponsibleInfo);
		}

		public bool IsResponsiblePersonMandatory { get; set; }

		#endregion

		#region O8_Role

		protected override void CheckO8_Role()
		{
			base.CheckO8_Role();
			MandatoryValidation.CheckEntered(Parent.O8_RoleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.O8_RoleInfo);
			ValidateUniqueAssignment();
			if (!Parent.O8_RoleInfo.HasErrors()
				&& (!Parent.IsInDatabase || Parent.O8_RoleInfo.HasChanges)
				&& (Parent.Header != null && !Parent.Header.SecurityProvider.CanModifyDetailsStaffAssignment(Parent.O8_Role)))
			{
				Parent.O8_RoleInfo.AddError(Res.GetString("42213b83-378d-4ba9-9522-2597d6e54448", "You do not have the required security for the selected role."));
			}
		}

		protected override void CheckO8_GC()
		{
			base.CheckO8_GC();
			if (!Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed && Parent.HasChanges && (Parent.O8_GC.IsEmpty || Parent.O8_GC != GlbCompany.CurrentCompany.PK))
			{
				Parent.O8_GCInfo.AddError(Res.GetString("ef3a9397-8e9e-4274-8711-fe8a2d883f99", "You do not have required security rights to maintain staff assignments for other companies.\r\nThis can be changed by your system administrator at:\r\n{0}", Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.DisplayTextPathToSecurityRight));
			}
		}

		void ValidateUniqueAssignment()
		{
			if (Parent.Header != null)
			{
				foreach (OrgStaffAssignments assignment in StaffAssignmentsForValidation)
				{
					if (assignment.PK != Parent.PK &&
						assignment.O8_Role == Parent.O8_Role &&
						assignment.O8_Department == Parent.O8_Department &&
						assignment.O8_Product == Parent.O8_Product &&
						(assignment.O8_GC == Parent.O8_GC || Parent.O8_GC.IsEmpty && assignment.O8_GC.IsEmpty))
					{
						Parent.O8_RoleInfo.AddError(Res.GetString("4fa6cc11-1ebb-4a90-bcb2-ae01b505bd18", "This staff assignment already has a Staff member assigned to it."));
						break;
					}
				}
			}
		}

		protected OrgStaffAssignmentsCollection StaffAssignmentsForValidation =>
			Parent.Header.StaffAssignments.CompanySpecific ? Parent.Header.StaffAssignments : Parent.Header.StaffAssignmentsNotCompanySpecific;

		#endregion
	}
}
