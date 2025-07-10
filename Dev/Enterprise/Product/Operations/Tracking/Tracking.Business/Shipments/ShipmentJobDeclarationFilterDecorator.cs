using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Shipments;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public static class ShipmentJobDeclarationFilterDecorator
	{
		public static void Decorate(JobDeclarationFilterBusinessObject declarationFilterBusinessObject)
		{
			AddFilters(declarationFilterBusinessObject);
			AddHelpers(declarationFilterBusinessObject);
		}

		static void AddHelpers(JobDeclarationFilterBusinessObject declarationFilterBusinessObject)
		{
			declarationFilterBusinessObject.CustomFilterStripsHelpers.Add(new WorkflowFilterStripsHelperWithRoutingSupport(typeof(BaseJobDeclaration), "", declarationFilterBusinessObject.Factory, GetNoResultWorkflowQuery()) { ShouldAddRelatedMilestoneFilters = true });
		}

		static ZDBOnlySubQuery GetNoResultWorkflowQuery()
		{
			var result = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			result.AddToFilter(ZQuery.NoResultQuery);
			return result;
		}

		static void AddFilters(JobDeclarationFilterBusinessObject declarationFilterBusinessObject)
		{
			var declarationFilters = declarationFilterBusinessObject.ModuleFilters;

			var eTDFilter = declarationFilters.AddDateFilter(declarationFilters.GetUniqueDescription(TrackingDeclarationFilterConstants.DateAtOrigin), JobDeclarationSchema.JE_DateAtOrigin);
			eTDFilter.Category = FilterCategories.Dates;

			var domesticInternationalFilter = declarationFilters.AddTextFilter(declarationFilters.GetUniqueDescription(TrackingDeclarationFilterConstants.DomesticInternational), GetDomesticInternationalQuery);
			domesticInternationalFilter.Category = FilterCategories.Locations;

			var importerNameFilter = declarationFilters.AddTextFilter(declarationFilters.GetUniqueDescription(TrackingDeclarationFilterConstants.ImporterCompanyName), GetOrgHeaderQueryWithOperator);
			importerNameFilter.SubGroup = new ImporterSubGroup();
			importerNameFilter.Category = FilterCategories.Organisations;

			var supplierNameFilter = declarationFilters.AddTextFilter(declarationFilters.GetUniqueDescription(TrackingDeclarationFilterConstants.SupplierCompanyName), GetOrgHeaderQueryWithOperator);
			supplierNameFilter.SubGroup = new SupplierSubGroup();
			supplierNameFilter.Category = FilterCategories.Organisations;

			var forwarderLookups = declarationFilterBusinessObject.Lookups.Forwarders;
			var sendReceiveForwarderFilter = declarationFilters.AddGuidFilter(declarationFilters.GetUniqueDescription(TrackingDeclarationFilterConstants.SendReceiveForwarders), ModuleIDs.Organisation, GetSendReceiveForwardesQuery, forwarderLookups, forwarderLookups);
			sendReceiveForwarderFilter.Category = FilterCategories.Organisations;

			var statusFilter = declarationFilters.AddTextFilter(declarationFilters.GetUniqueDescription(TrackingDeclarationFilterConstants.Status), GetStatusQuery, new ShipmentStatus());
			statusFilter.SubGroup = new JobDocsAndCartageSubGroup();
			statusFilter.Category = FilterCategories.StatusAndFlags;

			var lasteEditFilter = declarationFilters.AddDateFilter(declarationFilters.GetUniqueDescription(TrackingDeclarationFilterConstants.LastEditTime), JobDeclarationSchema.JE_SystemLastEditTimeUtc);
			lasteEditFilter.Category = FilterCategories.AuditInformation;

			var factory = declarationFilterBusinessObject.Factory;
			AddServiceLevelFilter(declarationFilters, factory);
			AddNoResultFilters(declarationFilters, factory);
			AddWebAuditFilters(declarationFilters);
		}

		static void AddWebAuditFilters(ModuleFilterCollection filters)
		{
			var createdTimeFilter = filters.AddDateFilter(Descriptions.AuditInformation.CreatedTime, JobDeclarationSchema.JE_SystemCreateTimeUtc);
			createdTimeFilter.Category = FilterCategories.AuditInformation;
			createdTimeFilter.MultilingualDescription = ResString.GetMultilingualString("Declaration|DeclarationFilterControl|CreatedTime", "Created Time");
		}

		static void AddNoResultFilters(ModuleFilterCollection filters, BusinessObjectFactory factory)
		{
			var orgs = new OrgHeaderCollection(factory);

			filters.AddFlagsFilter(TrackingDeclarationFilterConstants.NoResultFilters.FlagsFilter, new string[] { (NoResString)"foo" }, new GetFlagsQuery[] { GetNoResultFlagsQuery }); // i really don't think we want to translate "foo" ! WTF
			filters.AddNumberFilter(TrackingDeclarationFilterConstants.NoResultFilters.NumberFilter, GetNoResultTextQueryWithOperator);
			filters.AddGuidFilter(TrackingDeclarationFilterConstants.NoResultFilters.GuidsFilter, ModuleIDs.Organisation, GetNoResultGuidsQuery, orgs, orgs);
			filters.AddGuidFilter(TrackingDeclarationFilterConstants.NoResultFilters.GuidFilter, ModuleIDs.Organisation, GetNoResultGuidQuery, orgs);
			filters.AddTextFilter(TrackingDeclarationFilterConstants.NoResultFilters.TextFilter, GetNoResultTextQueryWithOperator);
			filters.AddDateFilter(TrackingDeclarationFilterConstants.NoResultFilters.DateFilter, GetNoResultDateQuery);
		}

		static ZQuery GetNoResultDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return ZQuery.NoResultQuery;
		}

		static ZQuery GetNoResultGuidQuery(ZGuid value)
		{
			return ZQuery.NoResultQuery;
		}

		static ZQuery GetNoResultGuidsQuery(ZGuid value1, ZGuid value2)
		{
			return ZQuery.NoResultQuery;
		}

		static ZQuery GetNoResultTextQueryWithOperator(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ZQuery.NoResultQuery;
		}

		static ZQuery GetNoResultFlagsQuery(ZBool value)
		{
			return ZQuery.NoResultQuery;
		}

		class JobDocsAndCartageSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

				var jobDocsAndCartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				jobDocsAndCartageSubQuery.AddToFilter(filter);
				result.AddSubQuery(jobDocsAndCartageSubQuery, JoinCondition.And);

				return result;
			}
		}

		static ZQuery GetStatusQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDocsAndCartage));
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

		static ZQuery GetSendReceiveForwardesQuery(ZGuid sendingForwarderPK, ZGuid receivingForwarderPK)
		{
			if (!sendingForwarderPK.IsEmpty && !receivingForwarderPK.IsEmpty)
			{
				return ZQuery.NoResultQuery;
			}

			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			if (!sendingForwarderPK.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export);
				result.AddToFilter(JobDeclarationSchema.JE_OH_Forwarder, sendingForwarderPK);
			}
			else if (!receivingForwarderPK.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export);
				result.AddToFilter(JobDeclarationSchema.JE_OH_Forwarder, receivingForwarderPK);
			}
			return result;
		}

		class SupplierSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobDeclarationSchema.JE_OH_Supplier);
				orgHeaderSubQuery.AddToFilter(filter);
				result.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

				return result;
			}
		}

		class ImporterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobDeclarationSchema.JE_OH_Importer);
				orgHeaderSubQuery.AddToFilter(filter);
				result.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

				return result;
			}
		}

		static ZQuery GetOrgHeaderQueryWithOperator(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(OrgHeader));
			result.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);

			return result;
		}

		static ZQuery GetDomesticInternationalQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			string sql = null;
			switch (value)
			{
				case JobShipmentFilterBusinessObject.DomesticInternationalFilterItems.Domestic:
					sql = string.Format(CultureInfo.InvariantCulture, "SUBSTRING({0}, 1, 2) = SUBSTRING({1}, 1, 2)", JobDeclarationSchema.JE_RL_NKOrigin.Name, JobDeclarationSchema.JE_RL_NKFinalDestination.Name); // SQL Query
					break;

				case JobShipmentFilterBusinessObject.DomesticInternationalFilterItems.International:
					sql = string.Format(CultureInfo.InvariantCulture, "SUBSTRING({0}, 1, 2) <> SUBSTRING({1}, 1, 2)", JobDeclarationSchema.JE_RL_NKOrigin.Name, JobDeclarationSchema.JE_RL_NKFinalDestination.Name); // SQL Query
					break;
			}

			if (sql != null)
			{
				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				result.AddFilterAndZSQLParameterCollection(sql, null);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		static void AddServiceLevelFilter(ModuleFilterCollection filters, BusinessObjectFactory factory)
		{
			var serviceLevelFilter = filters[DeclarationFilterConstants.ServiceLevel];
			if (serviceLevelFilter != null)
			{
				filters.RemoveFilter(serviceLevelFilter);
			}
			serviceLevelFilter = filters.AddNkFilter(DeclarationFilterConstants.ServiceLevel, JobDeclarationSchema.JE_RS_NKServiceLevel, ModuleIDs.ServiceLevel, new ActiveServiceLevelCollection(factory));
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;
		}

		public static class Descriptions
		{
			public static class AuditInformation
			{
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
				public const string CreatedTime = "Created Time";
			}
		}
	}
}
