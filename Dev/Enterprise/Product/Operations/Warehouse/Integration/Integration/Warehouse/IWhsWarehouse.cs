using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsWarehouse
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }

		ZBool WW_AutoPrintPackingSlip { get; set; }
		ZGuid WW_GB_RelatedCompanyBranch { get; set; }
		ZBool WW_IsBondedWarehouse { get; set; }
		ZBool WW_IsCustomsControlled { get; set; }
		ZBool WW_IsVirtualWarehouse { get; set; }
		ZBool WW_IsPortAuthorityControlled { get; set; }
		ZGuid WW_OA_WarehouseAddress { get; set; }
		ZGuid WW_WLT_DefaultLocationType { get; set; }
		ZString WW_WarehouseCode { get; set; }
		ZString WW_WarehouseName { get; set; }
		ZString WW_WarehouseType { get; set; }
		ZGuid WW_GG_ReleaseGroup { get; set; }
		ZString CountryCode { get; }
		IWhsAreaCollection Areas { get; }
		IOrgAddress WarehouseAddress { get; }
		IWhsRowCollection Rows { get; }
		bool IsApprovedKnown { get; }
		ZBool IsWarehouseBondEnabled { get; }
		ZBool IsVATFiscalEnabled { get; }
	}
}
