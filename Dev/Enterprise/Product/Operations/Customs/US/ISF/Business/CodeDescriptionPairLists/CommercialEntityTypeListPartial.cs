using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.ISF.Business
{
	partial class CommercialEntityTypeList
	{
		public static ZString GetCodeFromDocAddressType(ZString addressType)
		{
			switch (addressType)
			{
				case AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress:
					return Codes.BookingParty;
				case AutoDocAddressTypes.Codes.BuyingParty:
					return Codes.BuyingParty;
				case AutoDocAddressTypes.Codes.ConsigneeAddress:
					return Codes.Consignee;
				case AutoDocAddressTypes.Codes.Consolidator:
					return Codes.Consolidator;
				case AutoDocAddressTypes.Codes.Manufacturer:
					return Codes.Manufacturer;
				case AutoDocAddressTypes.Codes.SellingParty:
					return Codes.SellingParty;
				case AutoDocAddressTypes.Codes.ShipToParty:
					return Codes.ShipToParty;
				case AutoDocAddressTypes.Codes.ScheduledContainerStuffingLocation:
					return Codes.StuffingLocation;
			}
			return ZString.Empty;
		}
	}
}
