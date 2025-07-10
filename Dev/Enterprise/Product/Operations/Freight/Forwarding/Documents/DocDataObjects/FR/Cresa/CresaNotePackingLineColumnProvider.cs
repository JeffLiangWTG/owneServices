using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	class CresaNotePackingLineColumnProvider : TransitLogTableHelper<BookingPackingLine, TransitLogColumnIDs.CRESABookingPackingLine>
	{
		protected override ZString GetHeader(TransitLogColumnIDs.CRESABookingPackingLine columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.CRESABookingPackingLine.Packs:
					return Res.GetString("cc4171ba-de4d-40d7-82d4-b184efdb76eb", "Packs");
				case TransitLogColumnIDs.CRESABookingPackingLine.Weight:
					return Res.GetString("f3502c71-1b1b-4645-b2df-f92b52bba943", "Weight");
				case TransitLogColumnIDs.CRESABookingPackingLine.Volume:
					return Res.GetString("5b404730-9720-4366-bc5b-ad01760ff00d", "Volume");
				case TransitLogColumnIDs.CRESABookingPackingLine.GoodsDescription:
					return Res.GetString("f6d2eaea-e8aa-4d52-a773-db1e24c9b716", "Goods Description");
				default:
					return ZString.Empty;
			}
		}

		protected override ZString GetValue(BookingPackingLine packingLine, TransitLogColumnIDs.CRESABookingPackingLine columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.CRESABookingPackingLine.Packs:
					return packingLine.Quantity.ToString();
				case TransitLogColumnIDs.CRESABookingPackingLine.Weight:
					return RoundTo3Digits(packingLine.Weight.Value).ToString();
				case TransitLogColumnIDs.CRESABookingPackingLine.Volume:
					return RoundTo3Digits(packingLine.Volume.Value).ToString();
				case TransitLogColumnIDs.CRESABookingPackingLine.GoodsDescription:
					return packingLine.GoodsDescription;
				default:
					return ZString.Empty;
			}
		}

		static ZDecimal RoundTo3Digits(ZDecimal value)
		{
			return ZArchitecture.Core.Utilities.Round(value, 3);
		}
	}
}
