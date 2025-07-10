using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(WarehouseReceiveAssemblyData),
	Constants.DocManagerCodes.WarehouseReceiveDocket)]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseReceiveAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsReceive); } }
		protected override Type CollectionType
		{
			get { return typeof(WhsReceiveCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsReceiveCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.WhsReceive; } }
		public override string ReferenceType { get { return Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("31a106d4-720b-4101-b963-cfadbbe59471", "Warehouse Receive Job"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		#region GetQuery

		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			if (assemblyDataParams.IncludeConsignor)
			{
				query.AddToFilter(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, assemblyDataParams.Organisation);

				WarehouseAssemblyDataHelper.AddDateTimeOffsetFilters(query, WhsDocketSchema.WD_ETD, assemblyDataParams.ETDFrom.Date, assemblyDataParams.ETDTo.Date);
				WarehouseAssemblyDataHelper.AddDateTimeOffsetFilters(query, WhsDocketSchema.WD_ETA, assemblyDataParams.ETAFrom.Date, assemblyDataParams.ETATo.Date);

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
