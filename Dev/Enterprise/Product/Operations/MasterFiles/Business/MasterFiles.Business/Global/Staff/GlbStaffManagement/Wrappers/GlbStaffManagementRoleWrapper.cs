using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffManagementRoleWrapper : GlbStaffManagementTreeBizObjWrapperBase
	{
		public GlbStaffManagementRoleWrapper(GlbStaffManagementTreeModel treeModel, GlbStaff staff, StaffReportingRole registryRole, bool isManager = false)
			: base(treeModel)
		{
			Staff = staff;
			RegistryRole = registryRole;
			IsManager = isManager;
		}

		#region Properties

		public GlbStaff Staff { get; }
		public StaffReportingRole RegistryRole { get; }
		public readonly bool IsManager;

		#endregion

		#region Overrides

		#region Role

		public override ZString Role => RegistryRole.Description;

		#endregion

		public override ZString JobTitle => ZString.Empty;
		public override ZString EffectiveDate => ZString.Empty;
		public override ZString Branch => ZString.Empty;

		public override GlbStaffManagementTreeBizObjWrapperBase[] Children
		{
			get
			{
				var result = new List<GlbStaffManagementTreeBizObjWrapperBase>();

				var staffPKQuerySchemaColumn = IsManager ? GlbStaffManagerSchema.GSM_GS_Manager : GlbStaffManagerSchema.GSM_GS_Staff;

				var currentManagerQuery = new ZQuery(staffPKQuerySchemaColumn, Staff.PK);
				currentManagerQuery.AddToFilter(GlbStaffManagerSchema.GSM_ManagerType, RegistryRole.Code);

				AddValidManagerRecords(result, currentManagerQuery);

				return result.ToArray();
			}
		}

		void AddValidManagerRecords(ICollection<GlbStaffManagementTreeBizObjWrapperBase> resultList, ZQuery currentManagerQuery)
		{
			var isDirectManagerRole = RegistryRole.Code == DefaultStaffReportingRoles.Codes.DirectManager;

			if (!isDirectManagerRole)
			{
				var endDateQuery = new ZQuery(GlbStaffManagerSchema.GSM_EndDate, DBNull.Value);
				endDateQuery.AddToFilter(JoinCondition.Or, GlbStaffManagerSchema.GSM_EndDate, SQLComparisonOperator.GreaterThan, ZDateTime.Today);
				currentManagerQuery.AddToFilter(endDateQuery, JoinCondition.And);
			}

			var staffManagerRecords = Factory.Load<GlbStaffManager>(currentManagerQuery);

			if (isDirectManagerRole)
			{
				staffManagerRecords = staffManagerRecords.Where(x => !x.GSM_EffectiveDate.IsEmpty && (x.GSM_EndDate.IsEmpty || x.GSM_EndDate > ZDate.Today)).ToArray();
			}

			foreach (var record in staffManagerRecords)
			{
				resultList.Add(new GlbStaffManagementManagerWrapper(TreeModel, record, IsManager));
			}
		}

		#endregion
	}
}
