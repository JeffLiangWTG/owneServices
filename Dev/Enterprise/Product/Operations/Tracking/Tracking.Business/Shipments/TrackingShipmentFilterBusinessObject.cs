using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Tracking.Business.Shipments;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingShipmentFilterBusinessObject : JobShipmentFilterBusinessObject
	{
		public new static class Descriptions
		{
			public static readonly string Status = (NoResString)"Status"; // Filter name
			public static readonly string CustomerCommonReferences = (NoResString)"Customer Common References"; // Filter name

			public static class AuditInformation
			{
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
				public const string CreatedTime = "Created Time";
			}
		}

		#region Overrides

		protected override ModuleFilterCollection GetJobShipmentModuleFiltersCore()
		{
			ModuleFilterCollection result = base.GetJobShipmentModuleFiltersCore();

			AddStatusAndFlagsFiltersInThisClass(result);
			AddNumberFiltersInThisClass(result);
			AddWebAuditFilters(result);

			result["Load / Discharge"].Visibility = FilterVisibility.Visible;
			result["Origin / Destination"].Visibility = FilterVisibility.Visible;

			if (ZArchitecture.Environment.Globals.IsWeb)
			{
				result.AddAttributeFilters(new AttributeManager().GetAllAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, LoggedInWebUsersOrg), GetAttributeFilter, FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("E3A00887-298C-453A-B581-24727EE362A2", "Commercial Invoice Attribute Search")));
				result.AddNkFilter(TrackingDeclarationFilterConstants.DeclarationCountry, GetCountryQuery, ZArchitecture.Modules.ModuleIDs.RefCountry, new RefCountryCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("92C379BD-A987-4641-A649-21BE6E9AC628", TrackingDeclarationFilterConstants.DeclarationCountry);
			}

			if (RemoveExclusivity)
			{
				result.AddFilter(base.GetModuleFilterThatOverridesAllOtherFiltersCore());
			}

			return result;
		}

		ZQuery GetCountryQuery(ZString userEnteredCountryCode)
		{
			//The Declaration Country filter doesn't apply to shipment, but we have to provide a delegate which returns a non-null ZQuery for the filter
			//The Declaration Country filter will be mapped to Country filter in JobDeclarationBusinessObject and be using the GetCountryQuery logic there			
			return new ZQuery();
		}

		protected override ZQuery GetContainerNoQuery(SQLComparisonOperator comparisonOperator, ZString containerNo)
		{
			ZDBOnlyQuery baseQuery = (ZDBOnlyQuery)base.GetContainerNoQuery(comparisonOperator, containerNo);
			ZDBOnlySubQuery cusContainerSubQuery = new ZDBOnlySubQuery(typeof(Integration.Customs.Shared.IBaseCusContainer), CusContainerSchema.CO_JE);
			cusContainerSubQuery.AddToFilter_PossiblyCommaSeparated(CusContainerSchema.CO_ContainerNumber, containerNo);
			baseQuery.AddSubQuery(cusContainerSubQuery, JoinCondition.Or);
			return baseQuery;
		}

		public bool RemoveExclusivity { get; set; }

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			if (RemoveExclusivity)
			{
				return null;
			}

			return base.GetModuleFilterThatOverridesAllOtherFiltersCore();
		}

		protected override void CustomizeClientAssignedStaffFilter(OrgClientAssignedStaffModuleFilter filter)
		{
			filter.SupportsFiltersMatchComparisonOperator = false;
		}

		#endregion

		#region Audit Filters

		void AddWebAuditFilters(ModuleFilterCollection filters)
		{
			ModuleDateFilter createdTimeFilter = filters.AddDateFilter(Descriptions.AuditInformation.CreatedTime, JobShipmentSchema.JS_SystemCreateTimeUtc);
			createdTimeFilter.Category = FilterCategories.AuditInformation;
			createdTimeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipment|ShipmentFilterControl|CreatedTime", "Created Time");
		}

		#endregion

		#region Number Filters

		void AddNumberFiltersInThisClass(ModuleFilterCollection filters)
		{
			string description = filters.GetUniqueDescription(Descriptions.CustomerCommonReferences);

			var numberFilter = filters.AddNumberFilter(description, GetCustomerCommonReferencesQuery);
			numberFilter.MultilingualDescription = ResString.GetMultilingualString("7b9bc6d6-0cc9-466c-bbcf-67ff3947f2a0", "Customer Common References");
			numberFilter.Category = FilterCategories.NumbersAndReferences;
		}

		ZQuery GetCustomerCommonReferencesQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(TrackingShipment));

			result.AddSubQuery(GetPackLineSubQuery(comparisonOperator, value), JoinCondition.And);
			result.AddSubQuery(GetOrderRefSubQuery(comparisonOperator, value), JoinCondition.Or);
			result.AddSubQuery(GetOrderRefCartageSubQuery(comparisonOperator, value), JoinCondition.Or);
			result.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobShipmentSchema.JS_BookingReference, comparisonOperator, value);

			return result;
		}

		ZDBOnlySubQuery GetPackLineSubQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var packLinesSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);

			packLinesSubQuery.AddToFilter(JobPackLinesSchema.JL_RefNumber, comparisonOperator, value);
			packLinesSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobPackLinesSchema.JL_CustomAttrib1, comparisonOperator, value);
			packLinesSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobPackLinesSchema.JL_CustomAttrib2, comparisonOperator, value);
			packLinesSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobPackLinesSchema.JL_CustomAttrib3, comparisonOperator, value);
			packLinesSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobPackLinesSchema.JL_CustomAttrib4, comparisonOperator, value);

			return packLinesSubQuery;
		}

		public ZDBOnlySubQuery GetOrderRefSubQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);

			orderSubQuery.AddToFilter_PossiblyCommaSeparated(
				JoinCondition.And,
				JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, value);

			return orderSubQuery;
		}

		public ZDBOnlySubQuery GetOrderRefCartageSubQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var cartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
			var orderItemSubQuery = new ZDBOnlySubQuery(typeof(OrderItem), JobOrderItemSchema.JT_JP);

			orderItemSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderItemSchema.JT_OrderReference, comparisonOperator, value);
			cartageSubQuery.AddSubQuery(orderItemSubQuery, JoinCondition.And);

			return cartageSubQuery;
		}

		#endregion

		#region StatusAndFlags Filters

		void AddStatusAndFlagsFiltersInThisClass(ModuleFilterCollection filters)
		{
			string description = filters.GetUniqueDescription(Descriptions.Status);

			ModuleFilter statusFilter = filters.AddTextFilter(description, GetStatusQuery, StatusFilterList);
			statusFilter.SubGroup = new JobDocsAndCartageSubGroup();
			if (filters[Descriptions.Status] != null && ZArchitecture.Environment.Globals.IsWeb)
			{
				statusFilter.MultilingualDescription = ResString.GetMultilingualString("6F166400-3D5D-41DD-BE06-4A16E3C4C395", "Status (Web)");
			}
			else
			{
				statusFilter.MultilingualDescription = ResString.GetMultilingualString("1d572ceb-8747-45b5-affb-86633fe523ce", "Status");
			}
			statusFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetStatusQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobDocsAndCartage));
			if (value == ShipmentStatus.Codes.Delivered)
			{
				result.AddToFilter(JoinCondition.And, JobDocsAndCartageSchema.JP_DeliveryCartageCompleted, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);
			}
			else if (value == ShipmentStatus.Codes.Undelivered)
			{
				result.AddToFilter(JoinCondition.And, JobDocsAndCartageSchema.JP_DeliveryCartageCompleted, SQLComparisonOperator.Equal, null);
			}
			return result;
		}

		protected class JobDocsAndCartageSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(TrackingShipment));

				var jobDocsAndCartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				jobDocsAndCartageSubQuery.AddToFilter(filter);
				result.AddSubQuery(jobDocsAndCartageSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region ModuleFilter Lists

		#region StatusFilterList

		public ShipmentStatus StatusFilterList
		{
			get
			{
				if (statusFilterList == null)
				{
					statusFilterList = new ShipmentStatus();
				}
				return statusFilterList;
			}
		}

		protected ShipmentStatus statusFilterList;

		#endregion

		#endregion

		ZQuery GetAttributeFilter(ZQuery filter, SchemaColumn column)
		{
			if (column.TableSchema == JobComInvoiceLineSchema.Instance)
			{
				return GetCommercialInvoiceQuery(filter);
			}

			throw new NotImplementedException(string.Format("{0} schema is not supported", column.TableSchema));
		}

		public ZQuery GetDeclarationQuery()
		{
			var declarationFilterBusinessObjectFactory = new TrackingJobDeclarationFilterBusinessObjectFactory();
			var webUser = (OrgContactWebUser)WebEnv.AppInstance?.SiteUser;
			var declarationFilterBusinessObject = declarationFilterBusinessObjectFactory.GetJobDeclarationFilterBusinessObject(webUser?.GetCountryCode() ?? ZString.Empty, webUser?.LoggedInOrganisation);

			ShipmentJobDeclarationFilterDecorator.Decorate(declarationFilterBusinessObject);
			declarationFilterBusinessObject.AddActiveStatusFilters(typeof(BaseJobDeclaration));

			ShipmentToDeclarationFilterHelper helper = new ShipmentToDeclarationFilterHelper(this, declarationFilterBusinessObject);
			helper.MapFilters();

			ZQuery filter = declarationFilterBusinessObject.Filter;
			filter.AddToFilter(JobDeclarationSchema.JE_JS, null);
			filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>());

			return filter;
		}

		public ZDBOnlySubQuery GetTransactionNumberQuery(string countryCode, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var cusEntryNumber = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.CATransactionNumber);

			return cusEntryNumber;
		}
	}
}
