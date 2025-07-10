using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class FTZWhsOrderLinesDataProvider : IFTZWhsOrderLinesDataProvider
	{
		public IEnumerable<IFTZWhsOrderLineData> GetFTZWhsOrderLinesData(IOrgHeader importer, IOrgAddress warehouseAddress, ZString outwardEntryNumber)
		{
			Argument.NotNull(importer, nameof(importer));
			Argument.NotNull(warehouseAddress, nameof(warehouseAddress));
			Argument.NotNullOrEmpty(outwardEntryNumber, nameof(outwardEntryNumber));

			if (!importer.OH_IsActive || !importer.OH_IsWarehouseClient)
			{
				throw new ArgumentException("Importer must be active and must be a Warehouse Client.");
			}

			var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsDocketSchema.WD_WW_Whs);
			warehouseSubQuery.AddToFilter(FTZWarehouseDataProvider.GetFTZWarehouseQuery(warehouseAddress));

			var pickSubQuery = new ZDBOnlySubQuery(typeof(WhsPick), WhsDocketSchema.WD_WP);
			pickSubQuery.AddToFilter(WhsPickSchema.WP_PickStatus, PickStatus.Codes.Finalised);

			var orderSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketLineSchema.WE_WD);
			orderSubQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, importer.PK);
			orderSubQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			orderSubQuery.AddToFilter(WhsDocketSchema.WD_DocketSubType, OrderType.Codes.CustomsReleaseWithPermit);
			orderSubQuery.AddToFilter(WhsDocketSchema.WD_FinalisedDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			orderSubQuery.AddSubQuery(warehouseSubQuery, JoinCondition.And);
			orderSubQuery.AddSubQuery(pickSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsOrderLine));
			query.AddSubQuery(orderSubQuery, JoinCondition.And);

			var bondedWarehouseAttrSubQuery = new ZDBOnlySubQuery(typeof(WhsBondedWarehouseAttribute), WhsDocketLineSchema.PK, WhsBondedWarehouseAttributeSchema.WB_ParentID);
			bondedWarehouseAttrSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_EntryKey, outwardEntryNumber);

			query.AddSubQuery(bondedWarehouseAttrSubQuery, JoinCondition.And);

			var orderLines = warehouseAddress.Factory.Load<WhsOrderLine>(query);

			var lines = from orderLine in orderLines
									select new FTZWhsOrderLineData(
									orderLine.ProductCode,
									orderLine.ProductDesc,
									orderLine.WE_TransactionQuantity,
									orderLine.ProductUQ,
									orderLine.CustomsData.WB_Tariff,
									orderLine.CustomsData.WB_RN_NKCountryOfOrigin,
									orderLine.CustomsData.WB_PrimaryPreference,
									orderLine.CustomsData.WB_BondedWhsQty,
									orderLine.CustomsData.WB_ValueForDuty,
									orderLine.CustomsData.WB_CustomsQty,
									orderLine.CustomsData.WB_CustomsUnitOfQty,
									orderLine.CustomsData.WB_CustomsSecondQuantity,
									orderLine.CustomsData.WB_CustomsSecondUnitQty,
									orderLine.CustomsData.WB_CustomsThirdQuantity,
									orderLine.CustomsData.WB_CustomsThirdUnitQty,
									orderLine.CustomsData.WB_AddInfo);

			return lines;
		}
	}
}
