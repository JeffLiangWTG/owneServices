using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public delegate ZQuery GetReportingManagerRolesQuery(ZQuery staffRoleFilter);

	public class StaffReportingManagerRoleModuleFilter : ModuleTextFilter
	{
		public StaffReportingManagerRoleModuleFilter(ZString description)
			: base(description, EmptyQuery, StaffRoles)
		{
			this.reportingManagerRolesQueryDelegate = (q) => q;
		}

		public StaffReportingManagerRoleModuleFilter(ZString description, GetReportingManagerRolesQuery rolesQueryDelegate)
			: base(description, EmptyQuery, StaffRoles)
		{
			if (rolesQueryDelegate == null)
			{
				throw new ArgumentNullException(nameof(rolesQueryDelegate));
			}

			this.reportingManagerRolesQueryDelegate = rolesQueryDelegate;
		}

		#region ComparisonOperator

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new string[] {
					string.Empty,
					ComparisonConstants.Exact,
					ComparisonConstants.NotEqual,
					ComparisonConstants.IsBlank
				};
			}
		}

		#endregion

		#region Properties

		#region Manager

		[List("Managers")]
		public ZGuid Manager
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.IsBlank)
				{
					return ZGuid.Empty;
				}
				return manager;
			}
			set => SetNonPersistentPropertyValue(ManagerInfo, ref manager, value);
		}
		ZGuid manager;

		public ZPropertyInfo ManagerInfo
		{
			get { return GetZPropertyInfo(nameof(Manager)); }
		}

		protected bool Manager_ReadOnly
		{
			get { return ComparisonOperator == ComparisonConstants.IsBlank; }
		}

		public GlbStaffCollection Managers
		{
			get { return new GlbStaffCollection(new BusinessObjectFactory()); }
		}

		#endregion

		#region Management Role

		[BusinessObjectTestExclude]
		[List("ReportingRolesList")]
		public ZString ReportingRole
		{
			get
			{
				return reportingRole;
			}
			set
			{
				if (SetNonPersistentPropertyValue(ReportingRoleInfo, ref reportingRole, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateReportingRole();
					}

					ReportingRoleInfo.RefreshBinding();
				}
			}
		}
		ZString reportingRole;

		public ZPropertyInfo ReportingRoleInfo
		{
			get { return GetZPropertyInfo(nameof(ReportingRole)); }
		}

		public ICodeDescriptionBoolList ReportingRolesList
		{
			get { return reportingRolesList ?? (reportingRolesList = CreateReportingRolesList()); }
		}
		ICodeDescriptionBoolList reportingRolesList;

		public static ICodeDescriptionBoolList CreateReportingRolesList()
		{
			return SystemDataRegistry.Instance.StaffReportingRoles.Value;
		}

		static ICodeDescriptionBoolList StaffRoles
		{
			get { return CreateReportingRolesList(); }
		}

		#endregion

		#endregion

		#region Validation

		public new StaffReportingManagerRoleModuleFilterValidation Validation
		{
			get { return (StaffReportingManagerRoleModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new StaffReportingManagerRoleModuleFilterValidation(this);
		}

		#endregion

		#region Query

		static ZQuery EmptyQuery(ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : reportingManagerRolesQueryDelegate(GetStaffReportingManagerRoleFilter());
		}

		ZQuery GetStaffReportingManagerRoleFilter()
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));
			var managerSubQuery = new ZDBOnlySubQuery(typeof(GlbStaffManager), GlbStaffManagerSchema.GSM_GS_Staff, ComparisonOperator == ComparisonConstants.IsBlank);

			managerSubQuery.AddToFilter(GlbStaffManagerSchema.GSM_EffectiveDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Today);
			var endDateQuery = new ZQuery();
			endDateQuery.AddToFilter(GlbStaffManagerSchema.GSM_EndDate, DBNull.Value);
			endDateQuery.AddToFilter(JoinCondition.Or, GlbStaffManagerSchema.GSM_EndDate, SQLComparisonOperator.GreaterThan, ZDateTime.Today);
			managerSubQuery.AddToFilter(endDateQuery, JoinCondition.And);

			if (!Manager.IsEmpty && ComparisonOperator == ComparisonConstants.Exact)
			{
				managerSubQuery.AddToFilter(GlbStaffManagerSchema.GSM_GS_Manager, SQLComparisonOperator.Equal, Manager);
			}

			if (!ReportingRole.IsEmpty)
			{
				managerSubQuery.AddToFilter(GlbStaffManagerSchema.GSM_ManagerType, ReportingRole);
			}

			query.AddSubQuery(managerSubQuery, JoinCondition.And);

			if (!Manager.IsEmpty && ComparisonOperator == ComparisonConstants.NotEqual)
			{
				var queryParams = new ZSqlParameterCollection();
				queryParams.Add("@Manager", Manager, GlbStaffManagerSchema.GSM_GS_Manager);
				ZString notManagerQuery = @"
NOT EXISTS 
(
	SELECT TOP 1 GSM_PK 
	FROM dbo.GlbStaffManager 
	WHERE GSM_GS_Staff = GS_PK 
	AND GSM_GS_Manager = @Manager
)";

				query.AddFilterAndZSQLParameterCollection(notManagerQuery, queryParams);
			}

			return query;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("Manager", Manager.ToString());
			writer.WriteElementString("ReportingRole", ReportingRole);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "Manager")
			{
				Manager = new ZGuid(reader.ReadElementString("Manager"));
			}

			if (reader.Name == "ReportingRole")
			{
				ReportingRole = reader.ReadElementString("ReportingRole");
			}
		}

		#endregion

		#region Implementation

		protected override bool IsEmptyCore => base.IsEmptyCore && ReportingRole.IsEmpty && Manager.IsEmpty && ComparisonOperator != ComparisonConstants.IsBlank;

		protected override void ClearCore()
		{
			base.ClearCore();
			ReportingRole = ZString.Empty;
			Manager = ZGuid.Empty;
		}

		readonly GetReportingManagerRolesQuery reportingManagerRolesQueryDelegate;

		#endregion
	}
}
