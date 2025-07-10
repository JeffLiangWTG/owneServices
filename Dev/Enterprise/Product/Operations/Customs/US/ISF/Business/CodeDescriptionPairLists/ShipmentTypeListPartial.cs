
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	partial class ShipmentTypeList : Integration.Customs.US.ISF.IShipmentTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static bool IsBondDataRequired(string shipmentType)
		{
			return shipmentType == ShipmentTypeList.Codes.StandardOrRegularFilings ||
				shipmentType == ShipmentTypeList.Codes.ToOrderShipments ||
				shipmentType == ShipmentTypeList.Codes.USReturnGoods ||
				shipmentType == ShipmentTypeList.Codes.FTZShipments ||
				shipmentType == ShipmentTypeList.Codes.OuterContinentalShelfShipments;
		}

		public static bool IsPassportAllowed(string shipmentType)
		{
			return shipmentType == ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects ||
				shipmentType == ShipmentTypeList.Codes.DiplomaticShipment ||
				shipmentType == ShipmentTypeList.Codes.Carnet;
		}

		public static bool IsDOBRequired(string shipmentType)
		{
			return IsPassportAllowed(shipmentType) || shipmentType == ShipmentTypeList.Codes.Informal;
		}

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList() => this;
	}
}
