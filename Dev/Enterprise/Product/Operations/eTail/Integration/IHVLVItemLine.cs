using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVItemLine : IBusiness
	{
		ZGuid PK { get; }

		ZString CustomsCountryCode { get; }
		ZString ShipmentOriginCountryCode { get; }
		ZString ShipmentDestinationCountryCode { get; }

		ZGuid HVS_HVI_HVLVItem { get; set; }
		ZGuid HVS_CC_Lookup { get; set; }
		ZInt HVS_ClusterKey { get; set; }
		ZDecimal HVS_CustomsValue { get; set; }
		ZString HVS_DestinationTariff { get; set; }
		ZString HVS_FormattedDestinationTariff { get; set; }
		ZString HVS_FormattedOriginTariff { get; set; }
		ZString HVS_GoodsDescription { get; set; }
		ZDecimal HVS_GrossWeight { get; set; }
		ZDecimal HVS_IntrinsicValue { get; set; }
		ZString HVS_ItemURL { get; set; }
		ZDecimal HVS_NetWeight { get; set; }
		ZString HVS_OriginTariff { get; set; }
		ZString HVS_ProductCode { get; set; }
		ZShort HVS_Quantity { get; set; }
		ZString HVS_RN_NKOriginCountryCode { get; set; }
		ZDateTime HVS_SystemCreateTimeUtc { get; set; }
		ZString HVS_SystemCreateUser { get; set; }
		ZDateTime HVS_SystemLastEditTimeUtc { get; set; }
		ZString HVS_SystemLastEditUser { get; set; }
		ZString HVS_WeightUnit { get; set; }

		IHVLVConsignment Consignment { get; }

		IHVLVItem ParentItem { get; }
		IRefCountry OriginCountryCode { get; }
		Enterprise.Integration.Customs.IBaseCusClassification ClassificationLookup { get; }

		Logs Logs { get; }
		Notes Notes { get; }
	}
}
