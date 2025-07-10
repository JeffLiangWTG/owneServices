using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IBillofLading
	{
		///<summary>
		/// Xml Tag: AcentaAdi
		///</summary>
		ZString ContainerAgentName { get; }

		///<summary>
		/// Xml Tag: AcentaVergiNo
		///</summary>
		ZString ContainerAgentRegNo { get; }

		///<summary>
		/// Xml Tag: AliciAdi
		///</summary>
		ZString ConsigneeName { get; }

		///<summary>
		/// Xml Tag: AliciAdi
		///</summary>
		ZString ConsigneeRegNo { get; }

		///<summary>
		/// Xml Tag: AmbarHariciMi
		///</summary>
		ZString IsWarehouseExternal { get; }

		///<summary>
		/// Xml Tag: BildirimTarafiAdi
		///</summary>
		ZString NotifyPartyName { get; }

		///<summary>
		/// Xml Tag: BildirimTarafiVergiNo
		///</summary>
		ZString NotifyPartyRegNo { get; }

		///<summary>
		/// Xml Tag: DuzenlendigiUlke
		///</summary>
		ZString Origin { get; }

		///<summary>
		/// Xml Tag: EmniyetGuvenlikT
		///</summary>
		ZString SafetySecurityT { get; }

		///<summary>
		/// Xml Tag: EsyaninBulunduguYer
		///</summary>
		ZString GoodsLocation { get; }

		///<summary>
		/// Xml Tag: FaturaDoviz
		///</summary>
		ZString TransportValueCurrency { get; }

		///<summary>
		/// Xml Tag: FaturaToplami
		///</summary>
		ZDecimal TransportTotalValue { get; }

		///<summary>
		/// Xml Tag: GondericiAdi
		///</summary>
		ZString ShipperName { get; }

		///<summary>
		/// Xml Tag: GondericiVergiNo
		///</summary>
		ZString ShipperRegNo { get; }

		///<summary>
		/// Xml Tag: GrupMu
		///</summary>
		ZString IsGroup { get; }

		///<summary>
		/// Xml Tag: IhracatBilgisi
		///</summary>
		IEnumerable<ILadingExports> LadingExports { get; }

		///<summary>
		/// Xml Tag: KonteynerMi
		///</summary>
		ZString Iscontainer { get; }

		///<summary>
		/// Xml Tag: NavlunDoviz
		///</summary>
		ZString NKFreightValueCurrency { get; }

		///<summary>
		/// Xml Tag: NavlunTutari
		///</summary>
		ZDecimal FreightValue { get; }

		///<summary>
		/// Xml Tag: OdemeSekli
		///</summary>
		ZString PaymentType { get; }

		///<summary>
		/// Xml Tag: OncekiSeferNumarasi
		///</summary>
		ZString PreviousVoyageNo { get; }

		///<summary>
		/// Xml Tag: OncekiSeferTarihi
		///</summary>
		ZDateTime PreviousVoyageArrivalDate { get; }

		///<summary>
		/// Xml Tag: OzetBeyanNo
		///</summary>
		ZString EntryNumber { get; }

		///<summary>
		/// Xml Tag: RoroMu
		///</summary>
		ZString IsRoro { get; }

		///<summary>
		/// Xml Tag: SenetSiraNo
		///</summary>
		ZString SequenceNo { get; }

		///<summary>
		/// Xml Tag: AktarmaYapilacakMi
		///</summary>
		ZString IsTransshipmentType { get; }

		///<summary>
		/// Xml Tag: AktarmaTipi
		///</summary>
		ZString TransshipmentType { get; }

		///<summary>
		/// Xml Tag: TasimaSatirlari
		///</summary>
		IEnumerable<ILadingLines> LadingLines { get; }

		///<summary>
		/// Xml Tag: TasimaSenediNo
		///</summary>
		ZString BillNumber { get; }

		///<summary>
		/// Xml Tag: UgranilanUlkeler
		///</summary>
		IEnumerable<IBillVisitedCountry> BillVisitedCountry { get; }
	}
}
