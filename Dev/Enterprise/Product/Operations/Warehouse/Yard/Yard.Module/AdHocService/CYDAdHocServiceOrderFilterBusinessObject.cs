
using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDAdHocServiceOrderFilterBusinessObject : CYDFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => CYDAdHocServiceOrderSchema.PK;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => CYDAdHocServiceOrderSchema.YAO_WW_Facility;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddJobNumberFilter(filters);
			AddServiceOrderStatusFilter(filters);
			AddClientFilter(filters);
			AddClientReferenceFilter(filters);
			AddBillingFilter(filters);

			return filters;
		}

		#region StatusList

		CodeDescriptionPairList statusList;

		public CodeDescriptionPairList StatusList
		{
			get
			{
				if (statusList == null)
				{
					statusList = new CodeDescriptionPairList();
					statusList.AddPair("PEN", "PENDING");
					statusList.AddPair("PRO", "INPROGRESS");
					statusList.AddPair("COM", "COMPLETE");
				}
				return statusList;
			}
		}

		#endregion

		#region Filters

		void AddJobNumberFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.JobNumber, CYDAdHocServiceOrderSchema.YAO_JobNumber).MultilingualDescription = ResString.GetMultilingualString("CYDAdHocServiceOrder|CYDAdHocServiceOrderFilterBusinessObject|JobNumber", "Job Number");
		}

		void AddServiceOrderStatusFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Service Order Status", GetStatusQuery, StatusList);
			filter.Category = FilterCategories.TextSearch;
			filter.MultilingualDescription = ResString.GetMultilingualString("CYDAdHocServiceOrder|CYDAdHocServiceOrderFilterBusinessObject|ServiceOrderStatus", "Service Order Status");
		}

		#region AddClientFilter

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientName = filters.AddTextFilter("Client", new GetTextQueryWithOperator((comparisonOperator, value) => GetClientNameQuery(comparisonOperator, value)));
			clientName.Category = FilterCategories.TextSearch;
			clientName.MultilingualDescription = ResString.GetMultilingualString("CYDAdHocServiceOrder|CYDAdHocServiceOrderFilterBusinessObject|Client", "Client");
		}

		ZQuery GetClientNameQuery(SQLComparisonOperator comparisonOperator, ZString clientName)
		{
			var innerMostQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			innerMostQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, clientName);

			var innerQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			innerQuery.AddSubQuery(OrgAddressSchema.OA_OH, innerMostQuery, JoinCondition.And);

			var subQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			subQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, innerQuery, JoinCondition.And);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CYDAdHocServiceOrder));
			query.AddSubQuery(CYDAdHocServiceOrderSchema.PK, subQuery, JoinCondition.And);

			return query;
		}
		#endregion

		void AddClientReferenceFilter(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Client Reference", CYDAdHocServiceOrderSchema.YAO_ClientReference).MultilingualDescription = ResString.GetMultilingualString("CYDAdHocServiceOrder|CYDAdHocServiceOrderFilterBusinessObject|ClientReference", "Client Reference");
		}

		void AddBillingFilter(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Billing Date", CYDAdHocServiceOrderSchema.YAO_BillingDate).MultilingualDescription = ResString.GetMultilingualString("CYDAdHocServiceOrder|CYDAdHocServiceOrderFilterBusinessObject|BillingDate", "Billing Date");
		}

		#region Status Queries
		ZQuery GetStatusQuery(ZString status)
		{
			ZQuery query = new ZQuery();
			switch (status)
			{
				case "PEN":
					query = PendingStatus();
					break;

				case "PRO":
					query = InProgressStatus();
					break;

				case "COM":
					query = CompletedStatus();
					break;
			}
			return query;
		}

		ZQuery PendingStatus()
		{
			ZDBOnlySubQuery joinQuery = new ZDBOnlySubQuery(typeof(JobService), JobServiceSchema.PK);
			joinQuery.AddToFilter(JobServiceSchema.ES_BookedDateTimeOffset, SQLComparisonOperator.NotEqual, DBNull.Value);

			ZDBOnlySubQuery innerQuery = new ZDBOnlySubQuery(typeof(CYDAdHocService), CYDAdHocServiceSchema.YAS_YAO_ServiceOrder);
			innerQuery.AddSubQuery(CYDAdHocServiceSchema.YAS_ES_JobService, joinQuery, JoinCondition.And);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CYDAdHocServiceOrder), CYDAdHocServiceOrderSchema.PK, notIn: true);
			subQuery.AddSubQuery(CYDAdHocServiceOrderSchema.PK, innerQuery, JoinCondition.And);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CYDAdHocServiceOrder));
			query.AddSubQuery(CYDAdHocServiceOrderSchema.PK, subQuery, JoinCondition.And);
			return query;
		}

		ZQuery InProgressStatus()
		{
			ZDBOnlySubQuery joinQuery = new ZDBOnlySubQuery(typeof(JobService), JobServiceSchema.PK);
			joinQuery.AddToFilter(JobServiceSchema.ES_BookedDateTimeOffset, SQLComparisonOperator.NotEqual, DBNull.Value);
			joinQuery.AddToFilter(JobServiceSchema.ES_CompletedDateTimeOffset, SQLComparisonOperator.Equal, DBNull.Value);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CYDAdHocService), CYDAdHocServiceSchema.YAS_YAO_ServiceOrder);
			subQuery.AddSubQuery(CYDAdHocServiceSchema.YAS_ES_JobService, joinQuery, JoinCondition.And);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CYDAdHocServiceOrder));

			query.AddSubQuery(CYDAdHocServiceOrderSchema.PK, subQuery, JoinCondition.And);
			return query;
		}

		ZQuery CompletedStatus()
		{
			ZDBOnlySubQuery joinQuery = new ZDBOnlySubQuery(typeof(JobService), JobServiceSchema.PK);
			joinQuery.AddToFilter(JobServiceSchema.ES_CompletedDateTimeOffset, SQLComparisonOperator.Equal, DBNull.Value);

			ZDBOnlySubQuery innerQuery = new ZDBOnlySubQuery(typeof(CYDAdHocService), CYDAdHocServiceSchema.YAS_YAO_ServiceOrder);
			innerQuery.AddSubQuery(CYDAdHocServiceSchema.YAS_ES_JobService, joinQuery, JoinCondition.And);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CYDAdHocServiceOrder), CYDAdHocServiceOrderSchema.PK, notIn: true);
			subQuery.AddSubQuery(CYDAdHocServiceOrderSchema.PK, innerQuery, JoinCondition.And);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CYDAdHocServiceOrder));
			query.AddSubQuery(CYDAdHocServiceOrderSchema.PK, subQuery, JoinCondition.And);
			return query;
		}
		#endregion

		#endregion
	}
}
