//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffManagerValidation
//
//    This class should be used for overriding validation in AutoGlbStaffManagerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffManagerValidation : AutoGlbStaffManagerValidation
	{
		public GlbStaffManagerValidation(AutoGlbStaffManager parent) : base(parent)
		{
		}

		public new GlbStaffManager Parent
		{
			get { return (GlbStaffManager)base.Parent; }
		}

		protected override void CheckGSM_ManagerType()
		{
			MandatoryValidation.CheckEntered(Parent.GSM_ManagerTypeInfo);
			if (!Parent.IsInDatabase)
			{
				ListValidation.ErrorIfInvalidCode(Parent.GSM_ManagerTypeInfo);
			}

			if (!Parent.GSM_ManagerType.IsEmpty)
			{
				if (!Parent.CanAddRoleForStaff())
				{
					Parent.GSM_ManagerTypeInfo.AddWarning(CannotShareRoleMessage);
				}
			}
		}

		protected override void CheckGSM_GS_Manager()
		{
			if (Parent.GSM_GS_Manager == Parent.GSM_GS_Staff)
			{
				Parent.GSM_GS_ManagerInfo.AddError(Res.GetString("a1bba819-a2fd-431c-8ea0-f60ff1ede248", "A staff member cannot be their own manager."));
			}
		}

		public string CannotShareRoleMessage => Res.GetString("2f0d532a-afb8-4ba3-b067-0d2738959059", "This management role cannot be shared by staff members. Saving may cause existing records to be superseded.");

		protected override void CheckGSM_EffectiveDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckGSM_EffectiveDate()
		{
			if (Parent.GSM_ManagerType != DefaultStaffReportingRoles.Codes.DirectManager && Parent.GSM_EffectiveDate.IsEmpty)
			{
				Parent.GSM_EffectiveDateInfo.AddError(Res.GetString("6644ac7e-9f9f-4eea-a198-9bd597608d6e", "An Effective Date must be entered."));
			}
			else if (Parent.MustBeCurrent && !Parent.IsCurrentManager)
			{
				Parent.GSM_EffectiveDateInfo.AddError(Res.GetString("a5ab9c92-82ef-4e58-9d7f-62c11f94bc27", "This manager must be current. Please change the Effective Dates to make them current."));
			}
		}

		protected override void CheckGSM_EndDate()
		{
			if (!Parent.GSM_EffectiveDate.IsEmpty && !Parent.GSM_EndDate.IsEmpty && Parent.GSM_EndDate < Parent.GSM_EffectiveDate)
			{
				Parent.GSM_EndDateInfo.AddError(Res.GetString("3f616ced-0a1d-4d97-a1d6-5d9fbec114cc", "End Date cannot be earlier than Effective Date"));
			}
		}
	}
}
