using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(WarehouseWorkOrderAssemblyData),
	Constants.DocManagerCodes.WarehouseWorkOrder)]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseWorkOrderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(WhsWorkOrder); }
		}

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		protected override Type CollectionType
		{
			get { return typeof(WhsWorkOrderCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsWorkOrderCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsWorkOrder; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("038c7c99-ffe9-4dab-8da2-bbc67bbd6bf3", "Warehouse Work Order Job"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}

		#region GetQuery

		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			var query = new ZDBOnlyQuery(typeof(WhsWorkOrder));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder);

			SetOrganisationSubQuery(typeof(WhsWorkOrder), query, assemblyDataParams);

			if (assemblyDataParams.IsJobClosedDatesSpecified)
			{
				WarehouseAssemblyDataHelper.AddDateTimeOffsetFilters(query, WhsDocketSchema.WD_FinalisedDate, assemblyDataParams.JobClosedFrom.Date, assemblyDataParams.JobClosedTo.Date);
			}

			return query;
		}

		static void SetOrganisationSubQuery(Type docketType, ZDBOnlyQuery query, AssemblyDataParams assemblyDataParams)
		{
			if (assemblyDataParams.IncludeConsignor && assemblyDataParams.IncludeConsignee)
			{
				var organizationSubQuery = new ZDBOnlySubQuery(docketType, WhsDocketSchema.PK);
				organizationSubQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, assemblyDataParams.Organisation);

				organizationSubQuery.AddSubQuery(WhsWorkOrderDocAddressQueryHelper.GetWorkOrderAddressQuery(DocAddressTypes.Codes.ConsigneeAddress, assemblyDataParams.Organisation), JoinCondition.Or);

				query.AddSubQuery(organizationSubQuery, JoinCondition.And);
			}
			else if (assemblyDataParams.IncludeConsignor)
			{
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, assemblyDataParams.Organisation);
			}
			else if (assemblyDataParams.IncludeConsignee)
			{
				query.AddSubQuery(WhsWorkOrderDocAddressQueryHelper.GetWorkOrderAddressQuery(DocAddressTypes.Codes.ConsigneeAddress, assemblyDataParams.Organisation), JoinCondition.And);
			}
		}

		#endregion
	}
}
