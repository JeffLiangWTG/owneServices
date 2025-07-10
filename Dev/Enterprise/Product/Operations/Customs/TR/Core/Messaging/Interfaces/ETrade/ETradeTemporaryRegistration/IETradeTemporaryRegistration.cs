using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IETradeTemporaryRegistration : IMessageSender
	{
		///<summary>
		/// Xml Tag: guncellenecekTCGBGeciciTescilNo
		///</summary>
		ZString ReferenceNoToUpdate
		{
			get;
		}
		///<summary>
		/// Xml Tag: beyan1
		///</summary>
		ZString Declaration1
		{
			get;
		}
		///<summary>
		/// Xml Tag: beyan2
		///</summary>
		ZString Declaration2
		{
			get;
		}
		///<summary>
		/// Xml Tag: beyan3
		///</summary>
		ZString Declaration3
		{
			get;
		}
		///<summary>
		/// Xml Tag: kalemSayisi
		///</summary>
		ZInt TotalLineCount
		{
			get;
		}
		///<summary>
		/// Xml Tag: toplamKapAdedi
		///</summary>
		ZInt TotalBoxQty
		{
			get;
		}
		///<summary>
		/// Xml Tag: referansNumarasi
		///</summary>
		ZString ReferenceNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: beyanSahibiTemsilci | adiUnvani
		///</summary>
		ZString DeclaringRepresentativeNameAndTitle
		{
			get;
		}
		///<summary>
		/// Xml Tag: beyanSahibiTemsilci | vergiTCNo
		///</summary>
		ZString DeclaringRepresentativeTCTaxNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: cikistakiAracBilgileri| tipi
		///</summary>
		ZString TypeOfVehicleOnExit
		{
			get;
		}
		///<summary>
		/// Xml Tag: cikistakiAracBilgileri | numarasi
		///</summary>
		ZString VehiclePlateOfVehicleOnExit
		{
			get;
		}
		///<summary>
		/// Xml Tag: cikistakiAracBilgileri | ulke
		/// 
		///</summary>
		ZString CountryCodeOfVehicleOnExit
		{
			get;
		}
		///<summary>
		/// Xml Tag: konteyner
		///</summary>
		ZBool IsContainer
		{
			get;
		}
		///<summary>
		/// Xml Tag: siniriGecenHareketliTasimaAraclari | tipi
		///</summary>
		ZString TypeOfVehicleOnBorder
		{
			get;
		}
		///<summary>
		/// Xml Tag: siniriGecenHareketliTasimaAraclari | numarasi
		///</summary>
		ZString VehiclePlateOfVehicleOnBorder
		{
			get;
		}
		///<summary>
		/// Xml Tag: siniriGecenHareketliTasimaAraclari | ulke
		/// 
		///</summary>
		ZString CountryCodeOfVehicleOnBorder
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeToplamFaturaBedeliBilgileri | dovizTuru
		///</summary>
		ZString TotalInvoiceCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeToplamFaturaBedeliBilgileri | bedeli
		///</summary>
		ZDecimal TotalInvoiceCurrencyValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeToplamFaturaBedeliBilgileri | dovizKuru
		///</summary>
		ZDecimal TotalInvoiceExchangeRate
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeToplamFaturaBedeliBilgileri | faturaBedeliBilgisiTurkLirasi
		///</summary>
		ZDecimal InvoiceAmountInformationTurkishLira
		{
			get;
		}
		///<summary>
		/// Xml Tag: istatistikiKiymet
		///</summary>
		ZDecimal StatisticalValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: toplamHarcamalar | navlunBilgileri | dovizTuru
		///</summary>
		ZString TotalExpensesFreightCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: toplamHarcamalar | navlunBilgileri | bedeli
		///</summary>
		ZDecimal TotalExpensesFreightCurrencyValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: toplamHarcamalar | sigortaBilgileri | dovizTuru
		///</summary>
		ZString TotalExpensesInsuranceCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: toplamHarcamalar | sigortaBilgileri | bedeli
		///</summary>
		ZDecimal TotalExpensesInsuranceCurrencyValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: digerYurtDisiHarcamaBilgileri |  dovizTuru
		///</summary>
		ZString OtherOverseasExpenditureCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: digerYurtDisiHarcamaBilgileri | bedeli
		///</summary>
		ZDecimal OtherOverseasExpenditureCurrencyValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: yurtIciHarcamalar
		///</summary>
		ZDecimal DomesticExpenditures
		{
			get;
		}
		///<summary>
		/// Xml Tag: dahiliTasimaSekliKodu
		///</summary>
		ZString TransportTypeCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: yuklemeBosaltmaYeri
		///</summary>
		ZString LoadingUnloadingPlace
		{
			get;
		}
		///<summary>
		/// Xml Tag: girisCikisGumrukIdaresiKodu
		///</summary>
		ZString CustomsOfficeCodeOfEntryExit
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyaninBulunduguYer | yerKodu
		///</summary>
		ZString GoodsLocationCode
		{
			get;
		}

		///<summary>
		/// Xml Tag: esyaninBulunduguYer | yerAdi
		///</summary>
		ZString GoodsLocationName
		{
			get;
		}
		///<summary>
		/// Xml Tag: ayarlama
		///</summary>
		ZString Adjustment
		{
			get;
		}
		///<summary>
		/// Xml Tag: antreponunTipiKodu
		///</summary>
		ZString WarehouseTypeCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: asilSorumlu | adiUnvani
		///</summary>
		ZString PrincipleResponsibleNameAndTitle
		{
			get;
		}
		///<summary>
		/// Xml Tag: asilSorumlu | vergiTCNo
		///</summary>
		ZString PrincipleResponsibleTCTaxNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: ongorulenGumrukIdareleriVeUlke | gumrukKodu
		///</summary>
		ZString CustomsOfficeCodeOfPredicted
		{
			get;
		}
		///<summary>
		/// Xml Tag: ongorulenGumrukIdareleriVeUlke | ulkesi
		///</summary>
		ZString CountryCodeOfPredicted
		{
			get;
		}
		///<summary>
		/// Xml Tag: toplamTeminatlar | tcgbTeminat | turu
		///</summary>
		ZString TotalGuaranteesType
		{
			get;
		}
		///<summary>
		/// Xml Tag: toplamTeminatlar | tcgbTeminat | tutari
		///</summary>
		ZDecimal TotalGuaranteesValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: varisGumrukIdaresi
		///</summary>
		ZString CustomsOfficeCodeOfDestination
		{
			get;
		}
		///<summary>
		/// Xml Tag: aktarmalar | aktarmaUlkesi
		///</summary>
		ZString TransfersCountryCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: aktarmalar | aktarmaYeri
		///</summary>
		ZString TransfersPlace
		{
			get;
		}
		///<summary>
		/// Xml Tag: aktarmalar | yeniTasitAraciBilgileri | aracReferansNumarasi
		///</summary>
		ZString TransfersNewVehicleReferanceNumber
		{
			get;
		}

		///<summary>
		/// Xml Tag: aktarmalar | yeniTasitAraciBilgileri | ulkesi 
		/// 
		///</summary>
		ZString TransfersNewCountryCode
		{
			get;
		}

		///<summary>vExpanses
		/// Xml Tag: aktarmalar | konteyner
		///</summary>
		ZBool IsTransfersContainer
		{
			get;
		}
		///<summary>
		/// Xml Tag: aktarmalar | oncekiKonteynerNo
		///</summary>
		ZString TransfersPreviousContainerNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: aktarmalar | yeniKonteynerNo
		///</summary>
		ZString TransfersNewContainerNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: aciklamalar
		///</summary>
		ZString Explanations
		{
			get;
		}
		///<summary>
		/// Xml Tag: tasimaSenetleri
		///</summary>
		IEnumerable<IBill> Bills
		{
			get;
		}
	}
}
