using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(typeof(WarehouseAdjustmentAssemblyData), Constants.DocManagerCodes.WarehouseAdjustmentDocket)]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseAdjustmentAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsAdjustment); } }
		protected override Type CollectionType
		{
			get { return typeof(WhsAdjustmentCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsAdjustmentCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.WhsAdjustment; } }
		public override string ReferenceType { get { return Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("2678c0ed-1fd9-4af4-b510-7d94817f70ef", "Warehouse Adjustment Job"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		#region GetQuery

		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			if (assemblyDataParams.IncludeConsignor)
			{
				query.AddToFilter(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Adjustment);
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, assemblyDataParams.Organisation);

				if (assemblyDataParams.IsJobClosedDatesSpecified)
				{
					query.AddToFilter(WhsDocketSchema.WD_DocketStatus, DocketStatus.Codes.Finalised);
					WarehouseAssemblyDataHelper.AddDateTimeOffsetFilters(query, WhsDocketSchema.WD_FinalisedDate, assemblyDataParams.JobClosedFrom.Date, assemblyDataParams.JobClosedTo.Date);
				}
			}
			else
			{
				query.IsNoResultQuery = true;
			}
			return query;
		}

		#endregion
	}
}
