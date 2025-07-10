using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IGoodsInformation
	{
		///<summary>
		/// Xml Tag: BmEsyaKodu
		///</summary>
		ZString UNGoodCode { get; }

		///<summary>
		/// Xml Tag: BrutAgirlik
		///</summary>
		ZDecimal GrossWeight { get; }

		///<summary>
		/// Xml Tag: EsyaKodu
		///</summary>
		ZString TariffCode { get; }

		///<summary>
		/// Xml Tag: EsyaninTanimi
		///</summary>
		ZString GoodsDescription { get; }

		///<summary>
		/// Xml Tag: KalemFiyati
		///</summary>
		ZDecimal GoodsValue { get; }

		///<summary>
		/// Xml Tag: KalemFiyatiDoviz
		///</summary>
		ZString GoodsValueCurrency { get; }

		///<summary>
		/// Xml Tag: KalemSiraNo
		///</summary>
		ZInt OrderNo { get; }

		///<summary>
		/// Xml Tag: NetAgirlik
		///</summary>
		ZDecimal NetWeight { get; }

		///<summary>
		/// Xml Tag: OlcuBirimi
		///</summary>
		ZString CustomsUQ { get; }
	}
}
