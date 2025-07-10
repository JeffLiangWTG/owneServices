using CargoWise.Types;
namespace Enterprise.Warehouse.Integration
{
	public interface IWhsDocketLine
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }

		ZDateTimeOffset WE_AdjustmentArrivalDate { get; set; }
		ZString WE_BondedEntryKey { get; set; }
		ZString WE_DocketLineStatus { get; set; }
		ZDate WE_ExpiryDate { get; set; }
		ZDecimal WE_ExtendedLinePrice { get; set; }
		ZString WE_F3_NKPackType { get; set; }
		ZDateTimeOffset WE_FinalisedDate { get; set; }
		ZString WE_OriginalInventoryStatus { get; set; }
		ZString WE_CurrentInventoryStatus { get; set; }
		ZString WE_WHC_NKOriginalInventoryHeldCode { get; set; }
		ZString WE_WHC_NKCurrentInventoryHeldCode { get; set; }
		ZGuid WE_OP { get; set; }
		ZString WE_PackageGroupId { get; set; }
		ZDate WE_PackingDate { get; set; }
		ZString WE_PartAttrib1 { get; set; }
		ZString WE_PartAttrib2 { get; set; }
		ZString WE_PartAttrib3 { get; set; }
		ZString WE_SerialNumber { get; set; }
		ZString WE_PalletID { get; set; }
		ZDecimal WE_PerPackageQty { get; set; }
		ZDecimal WE_StockOnHand { get; set; }
		ZDecimal WE_TransactionQuantity { get; set; }
		ZDecimal WE_ClientOrderedUnits { get; set; }
		ZDecimal WE_PackQuantity { get; set; }
		ZString PackUOM { get; }
		ZGuid WE_WD { get; set; }
		ZGuid WE_WE_ParentDocketLine { get; set; }
		ZGuid WE_WE_OriginalDocketLineForRating { get; set; }
		ZGuid WE_WL { get; set; }
		ZShort WE_LineNo { get; set; }
		ZShort WE_SubLineNo { get; set; }
		ZString WE_CustomAttrib1 { get; set; }
		ZGuid WE_WB_CustomsData { get; set; }
		IWhsBondedWarehouseAttribute CustomsData { get; }
	}
}
