using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsBondedWarehouseAttribute
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }

		ZString WB_AddInfo { get; set; }
		ZDecimal WB_BondedWhsQty { get; set; }
		ZString WB_BondedWhsUnitOfQty { get; set; }
		ZDecimal WB_CustomsQty { get; set; }
		ZString WB_CustomsUnitOfQty { get; set; }
		ZString WB_DeclarationReference { get; set; }
		ZDate WB_CustomsDeadline { get; set; }
		ZString WB_InwardStyle { get; set; }
		ZString WB_InwardProcedure { get; set; }
		ZDateTime WB_EntryDate { get; set; }
		ZString WB_EntryKey { get; set; }
		ZShort WB_EntryLineNo { get; set; }
		ZGuid WB_ParentID { get; set; }
		ZString WB_ParentTableCode { get; set; }
		ZString WB_RN_NKCountryOfOrigin { get; set; }
		ZString WB_RX_NKTILVCurrency { get; set; }
		ZDecimal WB_TILV { get; set; }
		ZDecimal WB_ValueForDuty { get; set; }
		ZGuid WB_WB_InwardsEntry { get; set; }
		ZDecimal WB_CustomsSecondQuantity { get; set; }
		ZString WB_CustomsSecondUnitQty { get; set; }
		ZDecimal WB_CustomsThirdQuantity { get; set; }
		ZString WB_CustomsThirdUnitQty { get; set; }
		ZGuid WB_OA_ManufacturerAddress { get; set; }
		ZString WB_Tariff { get; set; }
		ZString WB_PrimaryPreference { get; set; }
		ZString WB_MatchingKey { get; set; }
		ZString WB_OutwardType { get; set; }
		ZDecimal WB_AllDutiesAmount { get; set; }
		ZDecimal WB_VATAmount { get; set; }
	}
}
