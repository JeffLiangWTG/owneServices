using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IOpeningLadingLines
	{
		///<summary>
		/// Xml Tag: AmbarKodu
		///</summary>
		ZString WarehouseCode { get; }

		///<summary>
		/// Xml Tag: AmbardakiMiktar
		///</summary>
		ZDecimal AmountInWarehouse { get; }

		///<summary>
		/// Xml Tag: AmbardakiMiktar
		///</summary>
		ZDecimal AmountTtoOpen { get; }

		///<summary>
		/// Xml Tag: MarkaNo
		///</summary>
		ZString BrandNo { get; }

		///<summary>
		/// Xml Tag: EsyaCinsi
		///</summary>
		ZString ItemType { get; }

		///<summary>
		/// Xml Tag: Birim
		///</summary>
		ZString Unit { get; }

		///<summary>
		/// Xml Tag: ToplamMiktar
		///</summary
		ZInt TotalQuantity { get; }

		///<summary>
		/// Xml Tag: ToplamMiktar
		///</summary
		ZInt AmountClosed { get; }

		///<summary>
		/// Xml Tag: OlcuBirimi
		///</summary
		ZString MeasurementUnit { get; }

		///<summary>
		/// Xml Tag: AcmaSatirNo
		///</summary
		ZInt OpeningLineNumber { get; }
	}
}
