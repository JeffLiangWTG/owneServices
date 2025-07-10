using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(WarehouseOrderAssemblyData),
	Constants.DocManagerCodes.WarehouseOrderDocket)]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseOrderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsOrder); } }
		protected override Type CollectionType
		{
			get { return typeof(WhsOrderCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsOrderCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.WhsOrder; } }
		public override string ReferenceType { get { return Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("6724154f-7d44-406d-bf5d-91c0958d6ed4", "Warehouse Order Job"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		#region GetQuery

		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(WhsOrder));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Order);

			SetOrganisationSubQuery(query, assemblyDataParams);

			if (assemblyDataParams.IsJobClosedDatesSpecified)
			{
				query.AddToFilter(WhsDocketSchema.WD_FinalisedDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
				WarehouseAssemblyDataHelper.AddDateTimeOffsetFilters(query, WhsDocketSchema.WD_FinalisedDate, assemblyDataParams.JobClosedFrom.Date, assemblyDataParams.JobClosedTo.Date);
			}

			return query;
		}

		#region SetOrganisationSubQuery

		void SetOrganisationSubQuery(ZDBOnlyQuery query, AssemblyDataParams assemblyDataParams)
		{
			if (assemblyDataParams.IncludeConsignor && assemblyDataParams.IncludeConsignee)
			{
				ZDBOnlySubQuery organizationSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketSchema.PK);
				organizationSubQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, assemblyDataParams.Organisation);
				organizationSubQuery.AddSubQuery(GetConsigneeSubQuery(assemblyDataParams.Organisation), JoinCondition.Or);

				query.AddSubQuery(organizationSubQuery, JoinCondition.And);
			}
			else if (assemblyDataParams.IncludeConsignor)
			{
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, assemblyDataParams.Organisation);
			}
			else if (assemblyDataParams.IncludeConsignee)
			{
				query.AddSubQuery(GetConsigneeSubQuery(assemblyDataParams.Organisation), JoinCondition.And);
			}
		}

		ZDBOnlySubQuery GetConsigneeSubQuery(ZGuid orgPK)
		{
			ZDBOnlySubQuery consigneeSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			consigneeSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, WhsDocketSchema.Constants.Prefix);

			ZDBOnlySubQuery consigneeOrgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			consigneeOrgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			consigneeSubQuery.AddSubQuery(consigneeOrgAddressSubQuery, JoinCondition.And);

			return consigneeSubQuery;
		}

		#endregion

		#endregion
	}
}
