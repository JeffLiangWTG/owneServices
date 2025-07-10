using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CommunicationFilterBusinessObject : FilterStripBusinessObject
	{
		public CommunicationFilterBusinessObject()
			: this(null)
		{
		}

		public CommunicationFilterBusinessObject(OrgHeader header)
		{
			mandatoryOrganization = header;

			LayoutLoaded += CommunicationFilterBusinessObject_LayoutLoaded;
			attendeeSubGroup = new AttendeeSubGroup();
		}

		readonly AttendeeSubGroup attendeeSubGroup;

		void CommunicationFilterBusinessObject_LayoutLoaded(object sender, EventArgs e)
		{
			var filters = GetFilterStrips();
			if (filters != null)
			{
				foreach (var filter in filters)
				{
					if (filter.FilterDescription == FilterDescription.StaffCoordinator
						&& !Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.IsAllowed
						&& filter.CurrentModuleFilter is ModuleNkFilter moduleNkFilter)
					{
						filter.OrCategory = FilterOrCategory.Grey;
						moduleNkFilter.Property = GlbStaff.CurrentUser.GS_Code;
					}
					else if (filter.FilterDescription == FilterDescription.Client
							 && MandatoryOrganization != null
							 && filter.CurrentModuleFilter is ModuleGuidFilter moduleGuidFilter)
					{
						filter.OrCategory = FilterOrCategory.None;
						moduleGuidFilter.Property = MandatoryOrganization.PK;
					}
				}
			}
		}

		protected virtual List<FilterStrip> GetFilterStrips()
		{
			return FilterStrips?.OfType<FilterStrip>().ToList();
		}

		#region Filters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleFountainFilter(FilterDescription.CommunicationID, OrgSalesCallSchema.OQ_CommunicationID, "CM");
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|CommunicationID", "Communication ID");
			return filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddOrganizationFilter(filters);

			var overallDispositionFilter = filters.AddTextFilter("Overall Disposition", GetOverallDispositionQuery, OverallDisposition_List);
			overallDispositionFilter.Category = FilterCategories.StatusAndFlags;
			overallDispositionFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|OverallDisposition", "Overall Disposition");

			filters.AddDateFilter(FilterDescription.Date, GetDateQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|Date", "Date");
			filters.AddTextFilter(FilterDescription.Subject, OrgSalesCallSchema.OQ_CallSummary).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|Subject", "Subject");

			filters.AddTextFilter(FilterDescription.CommunicationMethod, OrgSalesCallSchema.OQ_TypeOfCall, OQ_TypeOfCall_List).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|Method", "Method");

			filters.AddTextFilter(FilterDescription.Category, OrgSalesCallSchema.OQ_Category, OQ_Category_List).MultilingualDescription = OrganisationsDataRegistry.Instance.CategoryListLabel.Value;

			AddStaffCoordinatorAndAttendeeFilters(filters);

			var statusFilter = filters.AddTextFilter(FilterDescription.Status, OrgSalesCallSchema.OQ_Status, OQ_Status_List);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|Status", "Status");

			var primaryContactFilter = filters.AddGuidFilter(FilterDescription.PrimaryContact, ModuleIDs.OrgContacts, OrgSalesCallSchema.OQ_OC, Contacts);
			primaryContactFilter.Category = FilterCategories.Organisations;
			primaryContactFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|PrimaryContact", "Primary Contact");

			var contactAttendeeFilter = filters.AddGuidFilter(FilterDescription.ContactAttendee, ModuleIDs.OrgContacts, GetContactAttendeeQuery, Contacts);
			contactAttendeeFilter.Category = FilterCategories.Organisations;
			contactAttendeeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|ContactAttendee", "Contact Attendee");
			contactAttendeeFilter.SubGroup = attendeeSubGroup;

			AddWorkflowTasksFilters(filters);

			filters.AddWorkflowCustomFieldsFilters(Factory, OrgSalesCallWorkflowDescriptor.WorkflowTypeCode, typeof(OrgSalesCall));

			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);

			return filters;
		}

		#region Organization Filter

		void AddOrganizationFilter(ModuleFilterCollection filters)
		{
			var orgFilter = filters.AddGuidFilter(FilterDescription.Client, ModuleIDs.Organisation, OrgSalesCallSchema.OQ_OH, OrgHeaders);
			orgFilter.Category = FilterCategories.Organisations;
			orgFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|Client", "Client");

			if (MandatoryOrganization != null)
			{
				UpdateMandatoryOrganizationFilter(orgFilter, MandatoryOrganization);
			}
		}

		OrgHeader mandatoryOrganization;
		public OrgHeader MandatoryOrganization
		{
			get { return mandatoryOrganization; }
			set
			{
				mandatoryOrganization = value;

				var orgFilter = (ModuleGuidFilter)ModuleFilters[FilterDescription.Client];
				UpdateMandatoryOrganizationFilter(orgFilter, mandatoryOrganization);
			}
		}

		void UpdateMandatoryOrganizationFilter(ModuleGuidFilter orgFilter, OrgHeader org)
		{
			if (org != null)
			{
				orgFilter.Visibility = FilterVisibility.AlwaysApplied;
				orgFilter.OrCategory = FilterOrCategory.None;
				orgFilter.ReadOnly = true;
				orgFilter.Property = org.PK;
			}
			else
			{
				orgFilter.Visibility = FilterVisibility.Visible;
				orgFilter.OrCategory = FilterOrCategory.None;
				orgFilter.ReadOnly = false;
				orgFilter.Property = ZGuid.Empty;
			}
		}

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();

			var orgFilter = (ModuleGuidFilter)this[FilterDescription.Client];
			UpdateMandatoryOrganizationFilter(orgFilter, MandatoryOrganization);
		}

		#endregion

		#region Staff Coordinator and Attendee Filters

		void AddStaffCoordinatorAndAttendeeFilters(ModuleFilterCollection filters)
		{
			var staffFilter = filters.AddNkFilter(FilterDescription.StaffCoordinator, OrgSalesCallSchema.OQ_GS_NKSalesRep, ModuleIDs.GlbStaff, Staff);
			staffFilter.Category = FilterCategories.Organisations;
			staffFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|StaffCoordinator", "Staff Coordinator");

			var staffAttendeeFilter = filters.AddGuidFilter(FilterDescription.StaffAttendee, ModuleIDs.GlbStaff, GetStaffAttendeeQuery, Staff);
			staffAttendeeFilter.Category = FilterCategories.Organisations;
			staffAttendeeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|CommunicationFilter|StaffAttendee", "Staff Attendee");
			staffAttendeeFilter.SubGroup = attendeeSubGroup;

			if (!Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.IsAllowed)
			{
				staffFilter.ComparisonOperator_List.Clear();
				staffFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
				staffFilter.Visibility = FilterVisibility.AlwaysVisible;
				staffFilter.OrCategory = FilterOrCategory.Grey;
				staffFilter.PropertyValidation = MandatoryStaffCoordinatorFilterValidation;
				staffFilter.DefaultProperty = GlbStaff.CurrentUser.GS_Code;

				staffAttendeeFilter.OrCategory = FilterOrCategory.Grey;
				staffAttendeeFilter.DefaultProperty = GlbStaff.CurrentUser.PK;
			}
		}

		class AttendeeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(OrgSalesCall));
				var subquery = new ZDBOnlySubQuery(typeof(OrgSalesCallAdditionalAttendee), OrgSalesCallAdditionalAttendeeSchema.O6_OQ);
				subquery.AddToFilter(filter);
				query.AddSubQuery(subquery, JoinCondition.And);

				return query;
			}
		}

		ZQuery GetStaffAttendeeQuery(ZGuid value)
		{
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.And, OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeID, value);
			query.AddToFilter(JoinCondition.And, OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeTableCode, GlbStaffSchema.Constants.Prefix);
			return query;
		}

		void MandatoryStaffCoordinatorFilterValidation(ZPropertyInfo info)
		{
			var value = (ZString)info.Value;
			if (!value.EqualsIgnoringCase(GlbStaff.CurrentUser.GS_Code))
			{
				info.AddError(ViewWithoutBeingRelatedStaffErrorMessage);
			}
		}

		string ViewWithoutBeingRelatedStaffErrorMessage
		{
			get
			{
				return ResString.GetMultilingualString("e3c3186a-1041-4cc6-b023-9fd850f62e08", @"Your current security rights only allow you to view communications where you are the staff coordinator or an staff attendee.
If you think this is incorrect, please contact your system administrator.");
			}
		}

		#endregion

		#region WorkflowTasks Filters

		void AddWorkflowTasksFilters(ModuleFilterCollection filters)
		{
			var helper = new ProcessTaskFiltersHelper<OrgSalesCall>(Factory);
			helper.AddCurrentTaskAssignedToFilter(filters);
			helper.AddCurrentTaskAssignedGroupFilter(filters);
			helper.AddCurrentTaskStatusFilter(filters);
			helper.AddCurrentTaskScheduledStartFilter(filters);
			helper.AddCurrentTaskActualStartFilter(filters);
		}

		#endregion

		#endregion

		#region Lookups

		#region OrgHeaders

		OrgHeaderCollection orgHeaders;
		public OrgHeaderCollection OrgHeaders
		{
			get { return orgHeaders ?? (orgHeaders = new OrgHeaderCollection(Factory)); }
		}

		#endregion

		#region OverallDisposition

		ICodeDescriptionPairList OverallDisposition_List
		{
			get { return new OrgSalesCallOverallDispositionList(); }
		}

		ZQuery GetOverallDispositionQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSalesCall));

			var closedStatusCodes =
				from CommunicationStatus status
					in OrganisationsDataRegistry.Instance.CommunicationStatusList.Value
				where status.Closed
				select status.Code;

			var comparisonOperator = (value == OrgSalesCallOverallDispositionList.Codes.Closed ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual);
			query.AddToFilter(OrgSalesCallSchema.OQ_Status, comparisonOperator, closedStatusCodes);

			return query;
		}

		#endregion

		ICodeDescriptionPairList OQ_Status_List
		{
			get { return OrganisationsDataRegistry.Instance.CommunicationStatusList.Value; }
		}

		ICodeDescriptionPairList OQ_TypeOfCall_List
		{
			get { return OrganisationsDataRegistry.Instance.CommunicationType.Value; }
		}

		ICodeDescriptionPairList OQ_Category_List
		{
			get { return OrganisationsDataRegistry.Instance.CategoryList.Value; }
		}

		#region Date

		protected ZQuery GetDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZQuery query = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				query.AddToFilter(JoinCondition.And, OrgSalesCallSchema.OQ_CallDate, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, OrgSalesCallSchema.OQ_NextCall, SQLComparisonOperator.Equal, null);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				query.AddToFilter(JoinCondition.Or, OrgSalesCallSchema.OQ_CallDate, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(JoinCondition.Or, OrgSalesCallSchema.OQ_NextCall, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				var scheduledDateQuery = new ZQuery(OrgSalesCallSchema.OQ_CallDate, SQLComparisonOperator.Equal, null);
				AddDateTimeRange(scheduledDateQuery, comparisonOperator, JoinCondition.And, OrgSalesCallSchema.OQ_NextCall, value1, value2, true);

				var actualDateQuery = new ZQuery();
				AddDateTimeRange(actualDateQuery, comparisonOperator, JoinCondition.And, OrgSalesCallSchema.OQ_CallDate, value1, value2, true);

				query.AddToFilter(scheduledDateQuery, JoinCondition.Or);
				query.AddToFilter(actualDateQuery, JoinCondition.Or);
			}

			return query;
		}

		#endregion

		#region Staff

		GlbStaffCollection staff;
		GlbStaffCollection Staff
		{
			get { return staff ?? (staff = new GlbStaffCollection(Factory)); }
		}

		#endregion

		#region Contacts

		OrgContactCollection contacts;
		OrgContactCollection Contacts
		{
			get
			{
				if (contacts == null)
				{
					contacts = new OrgContactCollection(Factory);
					contacts.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation", "Property", ZGuid.Empty));
				}
				return contacts;
			}
		}

		ZQuery GetContactAttendeeQuery(ZGuid value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeID, value);
			return query;
		}

		#endregion

		#endregion

		internal static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string CommunicationID = "Communication ID";
			public const string Client = "Client";
			public const string OverallDisposition = "Overall Disposition";
			public const string Date = "Date";
			public const string Subject = "Subject";
			public const string CommunicationMethod = "Method";
			public const string Category = "Purpose";
			public const string StaffCoordinator = "Staff Coordinator";
			public const string StaffAttendee = "Staff Attendee";
			public const string Status = "Status";
			public const string PrimaryContact = "Primary Contact";
			public const string ContactAttendee = "Contact Attendee";
			public const string CurrentTask = "Current Task";
			public const string CurrentTaskAssignedTo = "Current Task Assigned To";

			#endregion
		}

		readonly CommunicationCRMSecurityProvider SecurityProvider = new CommunicationCRMSecurityProvider();
	}
}
