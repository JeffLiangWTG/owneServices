using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ISummaryDeclarationInformation : IMessageSender
	{
		///<summary>
		/// Xml Tag: BeyanSahibiVergiNo
		///</summary>
		ZString BusinessRegNo { get; }

		///<summary>
		/// Xml Tag: BeyanTuru
		///</summary>
		ZString ManifestType { get; }

		///<summary>
		/// Xml Tag: Diger
		///</summary>
		ZString Other { get; }

		///<summary>
		/// Xml Tag: DorseNo1
		///</summary>
		ZString Trailer1RegNo { get; }

		///<summary>
		/// Xml Tag: DorseNo1Uyrugu
		///</summary>
		ZString Trailer1RegCountry { get; }

		///<summary>
		/// Xml Tag: DorseNo2
		///</summary>
		ZString Trailer2RegNo { get; }

		///<summary>
		/// Xml Tag: DorseNo2Uyrugu
		///</summary>
		ZString Trailer2RegCountry { get; }

		///<summary>
		/// Xml Tag: EkBelgeSayisi
		///</summary>
		ZInt NumberofBillsInDeclaration { get; }

		///<summary>
		/// Xml Tag: EmniyetGuvenlik
		///</summary>
		ZString SafetySecurity { get; }

		///<summary>
		/// Xml Tag: GrupTasimaSenediNo
		///</summary>
		ZString GroupBillofLadingNumber { get; }

		///<summary>
		/// Xml Tag: GumrukIdaresi
		///</summary>
		ZString PresentationCustomsOffice { get; }

		///<summary>
		/// Xml Tag: KullaniciKodu
		///</summary>
		ZString UserID { get; }

		///<summary>
		/// Xml Tag: Kurye
		///</summary>
		ZString AgentType { get; }

		///<summary>
		/// Xml Tag: LimanYerAdiBos
		///</summary>
		ZString CustomsDischargePort { get; }

		///<summary>
		/// Xml Tag: LimanYerAdiYuk
		///</summary>
		ZString CustomsLoadPort { get; }

		///<summary>
		/// Xml Tag: LimanYerAdiYuk
		///</summary>
		ZString PreviousBillNumber { get; }

		///<summary>
		/// Xml Tag: PlakaSeferNo
		///</summary>
		ZString Voyage { get; }

		///<summary>
		/// Xml Tag: ReferansNumarasi
		///</summary>
		ZString LloydsNumber { get; }

		///<summary>
		/// Xml Tag: RefNo
		///</summary>
		ZString RegistrationNumber { get; }

		///<summary>
		/// Xml Tag: Rejim
		///</summary>
		ZString Nature { get; }

		///<summary>
		/// Xml Tag: TasimaSekli
		///</summary>
		ZString TransportType { get; }

		///<summary>
		/// Xml Tag: TasimaSenediBilgisi
		///</summary>
		IEnumerable<IBillofLading> BillofLadings { get; }

		///<summary>
		/// Xml Tag: OzbyAcmaBilgisi
		///</summary>
		IEnumerable<IOpeningSummaryDeclaration> OpeningSummaryDeclaration { get; }

		///<summary>
		/// Xml Tag: TasitinAdi
		///</summary>
		ZString Vessel { get; }

		///<summary>
		/// Xml Tag: TasitinUgradigiUlkeBilgisi
		///</summary>
		IEnumerable<IVehicleVisitedCountry> VehicleVisitedCountry { get; }

		///<summary>
		/// Xml Tag: TasiyiciFirma
		///</summary>
		IEnumerable<ICarrierCompany> CarrierCompany { get; }

		///<summary>
		/// Xml Tag: TasiyiciVergiNo
		///</summary>
		ZString CarrierBusinessRegNo { get; }

		///<summary>
		/// Xml Tag: TirAtaKarneNo
		///</summary>
		ZString TruckATAScorecardNumber { get; }

		///<summary>
		/// Xml Tag: UlkeKodu
		///</summary>
		ZString ConveyanceNationality { get; }

		///<summary>
		/// Xml Tag: UlkeKodu
		///</summary>
		ZString CustomsDischargeCountryCode { get; }

		///<summary>
		/// Xml Tag: UlkeKoduYuk
		///</summary>
		ZString CustomsLoadCountryCode { get; }

		///<summary>
		/// Xml Tag: YuklemeBosaltmaYeri
		///</summary>
		ZString LoadingUnloadingPlace { get; }

		///<summary>
		/// Xml Tag: VarisCikisGumrukIdaresi
		///</summary>
		ZString CustomsOffice { get; }

		///<summary>
		/// Xml Tag: VarisTarihSaati
		///</summary>
		ZDateTime DateAtCustomsOffice { get; }

		///<summary>
		/// Xml Tag: XmlRefId
		///</summary>
		ZString XmlRefId { get; }
	}
}
