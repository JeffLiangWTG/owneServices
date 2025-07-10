using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccAllowedBranchDepartmentCombo : AutoAccAllowedBranchDepartmentCombo
	{
		public AccAllowedBranchDepartmentCombo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Department Code

		public ZString DepartmentCode
		{
			get { return Department != null && !Department.IsDeleted ? Department.GE_Code : ZString.Empty; }
		}

		public ZPropertyInfo DepartmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentCode)); }
		}

		#endregion

		#region Department Description

		public ZString DepartmentDescription
		{
			get { return Department != null && !Department.IsDeleted ? Department.GE_DescMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo DepartmentDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentDescription)); }
		}

		#endregion

		#region Logging

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsDeleted && !IsInDatabase)
			{
				AddRelationshipLog(LoggingAction.Attach);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				AddRelationshipLog(LoggingAction.Detach);
			}
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				AddRelationshipLog(LoggingAction.Detach);
			}
			base.Delete();
		}

		void AddRelationshipLog(LoggingAction action)
		{
			var branch = Branch;
			var department = Department;

			if (branch != null && !branch.IsDeleted && department != null && !department.IsDeleted)
			{
				branch.AddRelationshipLog(action, department.GE_Code, department.GE_Desc);
			}
		}

		#endregion
	}
}
