using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IBill
	{
		///<summary>
		/// Xml Tag: tasimaSenediSiraNo
		///</summary>
		ZString BillOfLadingLineNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: TasimaSenediNumarasi
		///</summary>
		ZString BillOfLadingNumber
		{
			get;
		}
		///<summary>
		/// Xml Tag: OzetBeyanNo
		///</summary>
		ZString SummaryDeclarationNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: kaplar | turu
		///</summary>
		ZString PackType
		{
			get;
		}
		///<summary>
		/// Xml Tag: kaplar | adedi
		///</summary>
		ZInt PackQuantity
		{
			get;
		}
		///<summary>
		/// Xml Tag: konteynerler | konteynerMarkasi
		///</summary>
		ZString ContainerBrand
		{
			get;
		}
		///<summary>
		/// Xml Tag: konteynerler | konteynerNo
		///</summary>
		ZString ContainerNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: konteynerler | kapBilgileri | kapBilgisi | turu
		///</summary>
		ZString ContainerPackType
		{
			get;
		}
		///<summary>
		/// Xml Tag: konteynerler | kapBilgileri | kapBilgisi | adedi
		///</summary>
		ZInt ContainerPackQuantity
		{
			get;
		}
		///<summary>
		/// Xml Tag: brutAgirlik
		///</summary>
		ZDecimal GrossWeight
		{
			get;
		}
		///<summary>
		/// Xml Tag: netAgirlik
		///</summary>
		ZDecimal NetWeight
		{
			get;
		}
		///<summary>
		/// Xml Tag: gondericiIhracatci | adiUnvani
		///</summary>
		ZString ForwarderNameAndTitle
		{
			get;
		}
		///<summary>
		/// Xml Tag: gondericiIhracatci | vergiTCNo
		///</summary>
		ZString ForwarderTCTaxNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: alici | adi
		///</summary>
		ZString ConsigneeName
		{
			get;
		}
		///<summary>
		/// Xml Tag: alici | Unvani
		///</summary>
		ZString ConsigneeTitle
		{
			get;
		}
		///<summary>
		/// Xml Tag: alici | vergiTCNo
		///</summary>
		ZString ConsigneeTCTaxNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: alici | caddeSokakNo
		///</summary>
		ZString ConsigneeStreetNumber
		{
			get;
		}
		///<summary>
		/// Xml Tag: alici | ilKodu
		///</summary>
		ZString ConsigneeCityCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: alici | ilce
		///</summary>
		ZString ConsigneeTown
		{
			get;
		}
		///<summary>
		/// Xml Tag: alici | postaKodu
		///</summary>
		ZString ConsigneePostalCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: pazaryeri | adiUnvani
		///</summary>
		ZString MarketPlaceNameAndTitle
		{
			get;
		}
		///<summary>
		/// Xml Tag: pazaryeri | vergiTCNo
		///</summary>
		ZString MarketPlaceTCTaxNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: sevkGidecegiUlkeKodu
		///</summary>
		ZString ReferralDestinationCountryCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: ticaretYapilanUlkeKodu
		///</summary>
		ZString TradeCountryCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: cikisIhracatUlkesiKodu
		///</summary>
		ZString ExportCountryCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: gidecegiUlkeKodu
		///</summary>
		ZString DestinationCountryCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: teslimSekliBilgileri | teslimSekli
		///</summary>
		ZString DeliveryMethod
		{
			get;
		}
		///<summary>
		/// Xml Tag: teslimSekliBilgileri | teslimYeri
		///</summary>
		ZString DeliverLocation
		{
			get;
		}
		///<summary>
		/// Xml Tag: isleminNiteligi
		///</summary>
		ZString TransactionNature
		{
			get;
		}
		///<summary>
		/// Xml Tag: muafiyetler | muafiyetKodu1
		///</summary>
		ZString ExceptionCode1
		{
			get;
		}
		///<summary>
		/// Xml Tag: muafiyetler | muafiyetKodu2
		///</summary>
		ZString ExceptionCode2
		{
			get;
		}
		///<summary>
		/// Xml Tag: rejimKodu
		///</summary>
		ZString RegimeCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: belgeler
		///</summary>
		IEnumerable<IDocument> Documents
		{
			get;
		}
		///<summary>
		/// Xml Tag: finansalVeBankacilikVerileri | bankaKodu
		///</summary>
		ZString FinancialBankingCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: finansalVeBankacilikVerileri | odemeSekli
		///</summary>
		ZString FinancialBankingPaymentType
		{
			get;
		}
		///<summary>
		/// Xml Tag: finansalVeBankacilikVerileri | tutar
		///</summary>
		ZDecimal FinancialBankingAmount
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeToplamFaturaBedeliBilgileriTs | FaturaBedeliBilgisi | dovizTuru
		///</summary>
		ZString InvoiceCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeToplamFaturaBedeliBilgileriTs | FaturaBedeliBilgisi | bedeli
		///</summary>
		ZDecimal InvoiceAmount
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeToplamFaturaBedeliBilgileriTs | FaturaBedeliBilgisi | dovizKuru
		///</summary>
		ZDecimal InvoiceExchangeRate
		{
			get;
		}
		///<summary>
		/// Xml Tag: vergiler
		///</summary>
		IEnumerable<ITax> Taxes
		{
			get;
		}
		///<summary>
		/// Xml Tag: vergiOdemesi | odenecekVergiTutariToplami
		///</summary>
		ZDecimal TaxPaymentTotalTaxPaymentAmount
		{
			get;
		}
		///<summary>
		/// Xml Tag: vergiOdemesi | teminataBaglanacakVergiTutari
		///</summary>
		ZDecimal TaxPaymentTaxAmountToBeBonded
		{
			get;
		}
		///<summary>
		/// Xml Tag: vergiOdemesi | sonraOdenecekVergiTutariToplami
		///</summary>
		ZDecimal TaxPaymentTotalTaxAmountPayableLater
		{
			get;
		}
		///<summary>
		/// Xml Tag: vergiOdemesi | toplam
		///</summary>
		ZDecimal TaxPaymentTotal
		{
			get;
		}
		///<summary>
		/// Xml Tag: teminatlar | turu
		///</summary>
		ZString GuaranteeType
		{
			get;
		}
		///<summary>
		/// Xml Tag: teminatlar | tutari
		///</summary>
		ZDecimal GuaranteeAmount
		{
			get;
		}
		///<summary>
		/// Xml Tag: teminatlar | referansNo
		///</summary>
		ZString GuaranteeReferenceNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: harcamalar | navlunBilgileri | turu
		///</summary>
		ZString FreightInformationCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: harcamalar | navlunBilgileri | bedeli
		///</summary>
		ZDecimal FreightInformationValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: harcamalar | sigortaBilgileri | turu
		///</summary>
		ZString InsuranceInformationCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: harcamalar | sigortaBilgileri | bedeli
		///</summary>
		ZDecimal InsuranceInformationValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: harcamalar | digerYurtDisiHarcamaBilgileri | turu
		///</summary>
		ZString OtherOverseasExpansesCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: harcamalar | digerYurtDisiHarcamaBilgileri | bedeli
		///</summary>
		ZDecimal OtherOverseasExpansesValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: harcamalar | yurtIciHarcamalar
		///</summary>
		ZDecimal DomesticExpanses
		{
			get;
		}
		///<summary>
		/// Xml Tag: kalemler
		///</summary>
		IEnumerable<IPack> Packs
		{
			get;
		}
		///<summary>
		/// Xml Tag: hacim
		///</summary>
		ZString Volume
		{
			get;
		}
		///<summary>
		/// Xml Tag: ticaretSekli
		///</summary>
		ZString TradeType
		{
			get;
		}
	}
}
