using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GlbGroupFilterBusinessObject : FilterStripBusinessObject
	{
		public GlbGroupFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddADLinkedFilterIfRequired(result);
			AddTextFilters(result);
			AddDateRangeFilter(result);
			AddOSMGFilter(result);
			AddCustomFilters(result);
			AddDomainNameFilter(result);
			AddNonSecurityGroupFilter(result);
			result.AddGuidFilter(GlbGroupSchema.Constants.GG_GG_ParentGroup, ModuleIDs.GlbGroup, GlbGroupSchema.GG_GG_ParentGroup, () => new GlbGroupCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("b28e11fb-f057-41b3-8191-9c038f49228b", "Parent Group");

			return result;
		}

		#region System

		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;
				query.AddToFilter(GlbGroupSchema.GG_IsSales, false);
				query.AddToFilter(JoinCondition.And, GlbGroupSchema.PK, SQLComparisonOperator.NotEqual, GlbGroup.DbDeveloperGroupPK);
				query.AddToFilter(JoinCondition.And, GlbGroupSchema.PK, SQLComparisonOperator.NotEqual, GlbGroup.DbReaderGroupPK);
				query.AddToFilter(JoinCondition.And, GlbGroupSchema.PK, SQLComparisonOperator.NotEqual, GlbGroup.BackupOperatorGroupPK);
				return query;
			}
		}

		#endregion

		#region ADLinked

		void AddADLinkedFilterIfRequired(ModuleFilterCollection filters)
		{
			if (ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled)
			{
				filters.AddFlagsFilter("ADLinked", new[] { Res.GetString("7e98b994-6b12-4508-840b-838fc24ad0dd", "Linked") }, new GetFlagsQuery[] { GetADLinkedQuery }).MultilingualDescription = ResString.GetMultilingualString("25940eb9-d561-4676-96de-043bd023466a", "Linked with Active Directory");
			}
		}

		ZQuery GetADLinkedQuery(ZBool value)
		{
			var query = new ZQuery();
			if (!value)
			{
				query.AddToFilter(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.Equal, null)
					.AddToFilter(JoinCondition.Or, GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.Equal, ZGuid.Invalid);
			}
			else
			{
				query.AddToFilter(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null)
					.AddToFilter(JoinCondition.And, GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Invalid);
			}
			return query;
		}

		#endregion

		#region DomainName

		void AddDomainNameFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(GlbGroupSchema.Constants.GG_DomainName, GlbGroupSchema.GG_DomainName, ObjectFactory.Get<IADRegistry>().DomainCredentialsCollectionAsCodeDescriptionPairList);
			filter.MultilingualDescription = ResString.GetMultilingualString("4E6566AF-D056-4299-A3CB-0AB89B486D1F", "Domain Name");
			filter.ShowDescription = false;
		}

		#endregion

		#region OSMG

		void AddOSMGFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagsFilter(OrgMiscServSchema.Constants.OM_GG_OrgSecurityGroup, new[] { Res.GetString("0516c37a-df4d-4b18-b2e2-ac5e48598778", "Show Groups with Organizations Only") }, new GetFlagsQuery[] { GetOSMGQuery });
			filter.MultilingualDescription = ResString.GetMultilingualString("069f7900-5edc-4fe7-a21d-1dfc071f401a", "Show Groups with Organizations");
			filter.Property0 = true;
		}

		ZQuery GetOSMGQuery(ZBool value)
		{
			if (value)
			{
				var query = new ZDBOnlyQuery(typeof(GlbGroup));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_GG_OrgSecurityGroup);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region Non-Security Group

		void AddNonSecurityGroupFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(NSGStatus, GetNSGQuery, GetNSGList());
			filter.DefaultProperty = NSGAllCode;
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("1cc86eae-50e4-4294-b6e5-7514ba8555ca", "Is Non-Security Group");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
		const string NSGStatus = "Is Non-Security Group";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		protected const string NSGAllCode = "All";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		protected const string NSGNSGCode = "Non-Security Group";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		protected const string NSGNotNSGCode = "Security Group";
		protected virtual string NSGAllDescription => Res.GetString("f54dda50-48cb-4202-b869-6de23b64da1f", "All");
		protected virtual string NSGNSGDescription => Res.GetString("f2e933e2-69e9-4a1b-8c90-3c6b6188e6f0", "Non-Security Group");
		protected virtual string NSGNotNSGDescription => Res.GetString("cfdc246c-4255-45b6-bd45-fd7e385a2a0f", "Security Group");

		CodeDescriptionPairList GetNSGList()
		{
			var systemDefinedStatusList = new CodeDescriptionPairList();
			systemDefinedStatusList.AddPair(NSGAllCode, NSGAllDescription);
			systemDefinedStatusList.AddPair(NSGNSGCode, NSGNSGDescription);
			systemDefinedStatusList.AddPair(NSGNotNSGCode, NSGNotNSGDescription);
			return systemDefinedStatusList;
		}

		ZQuery GetNSGQuery(ZString value)
		{
			var query = new ZQuery();

			if (value == NSGNSGCode)
			{
				query.AddToFilter(GlbGroupSchema.GG_IsSecurityEnabled, ZBool.False);
			}
			else if (value == NSGNotNSGCode)
			{
				query.AddToFilter(GlbGroupSchema.GG_IsSecurityEnabled, ZBool.True);
			}

			return query;
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", GlbGroupSchema.GG_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbGroupFilter|Code", "Code");
			filters.AddTextFilter("Description", GlbGroupSchema.GG_Desc).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbGroupFilter|Description", "Description");
			filters.AddTextFilter("Category", GlbGroupSchema.GG_Category, () => SystemDataRegistry.Instance.GroupCategoryList.Value).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbGroupFilter|Category", "Category");
			filters.AddTextFilter("External Id", GlbGroupSchema.GG_ExternalId).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbGroupFilter|ExternalId", "External Id");
		}

		#endregion

		#region Security Modified

		void AddDateRangeFilter(ModuleFilterCollection filters)
		{
			ModuleDateFilter filter = filters.AddDateFilter("Security Modified", GetSecurityModifiedFilter);
			filter.HideFutureDates = true;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbGroupFilter|SecurityModified", "Security Modified");
		}

		ZQuery GetSecurityModifiedFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbGroup));

			ZDBOnlySubQuery stmAlogSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			stmAlogSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.SecurityModified.Code);

			ZQuery securityModifiedDateFilter = new ZQuery();
			AddDateRange(securityModifiedDateFilter, comparisonOperator, JoinCondition.And, StmALogSchema.SL_EventTime, fromDate.Date, toDate.Date);

			stmAlogSubQuery.AddToFilter(securityModifiedDateFilter);
			query.AddSubQuery(stmAlogSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Custom

		#region Branches

		public GlbBranchCollection Branches
		{
			get { return branches ?? (branches = new GlbBranchCollection(Factory)); }
		}

		GlbBranchCollection branches;

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}

				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

		#endregion

		void AddCustomFilters(ModuleFilterCollection filters)
		{
			GroupSecurityModuleFilter securityModuleFilter = new GroupSecurityModuleFilter("Security Rights", Branches, Departments);
			securityModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|SecurityRights", "Security Rights");
			filters.AddCustomFilter(securityModuleFilter);
		}

		internal GroupSecurityModuleFilter SecurityRightsFilter
		{
			get { return (GroupSecurityModuleFilter)this["Security Rights"]; }
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var groupWorkflowFilterHelper = new WorkflowFilterStripsHelper(typeof(GlbGroup), WorkflowDescriptors.GlbGroupWorkflowDescriptorCode, Factory)
			{
				ShouldAddMilestoneFilters = false,
				ShouldAddMiscFilters = false,
				ShouldAddExceptionsInMiscFilters = false
			};
			groupWorkflowFilterHelper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(groupWorkflowFilterHelper);
			return helpers;
		}

		#endregion

		#endregion
	}
}
