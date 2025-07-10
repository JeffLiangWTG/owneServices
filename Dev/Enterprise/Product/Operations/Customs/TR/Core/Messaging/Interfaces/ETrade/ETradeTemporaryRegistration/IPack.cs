using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IPack
	{
		///<summary>
		/// Xml Tag: kalemNo
		///</summary>
		ZString PackNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyaninticariTanimi
		///</summary>

		ZString CommercialDescription
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyalar | seriNumarasi
		///</summary>
		ZString ItemsSerialNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyalar | adedi
		///</summary>
		ZInt ItemsQuantity
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyalar | markasi
		///</summary>
		ZString ItemsBrand
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyalar | modeli
		///</summary>
		ZString ItemsModel
		{
			get;
		}
		///<summary>
		/// Xml Tag: kullanilmisEsyaKodu
		///</summary>
		ZString UsedItemCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyaKodu | esyaKodu1
		///</summary>
		ZString ItemCode1
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyaKodu | esyaKodu2
		///</summary>
		ZString ItemCode2
		{
			get;
		}
		///<summary>
		/// Xml Tag: esyaKodu | esyaKodu3
		///</summary>
		ZString ItemCode3
		{
			get;
		}
		///<summary>
		/// Xml Tag: tercihliTarifeKodlari | tercihliTarifeKodu1
		///</summary>
		ZString PreferentialTariffCode1
		{
			get;
		}
		///<summary>
		/// Xml Tag: tercihliTarifeKodlari | tercihliTarifeKodu2
		///</summary>
		ZString PreferentialTariffCode2
		{
			get;
		}
		///<summary>
		/// Xml Tag: menseUlkeKodu
		/// 
		///</summary>
		ZString CountryCodeOfOrigin
		{
			get;
		}
		///<summary>
		/// Xml Tag: kiymetBildirimFormu
		///</summary>
		ZString ValueStatementForm
		{
			get;
		}
		///<summary>
		/// Xml Tag: tarimPolitikasi
		///</summary>
		ZString AgriculturePolicy
		{
			get;
		}
		///<summary>
		/// Xml Tag: kota
		///</summary>
		ZBool IsQuota
		{
			get;
		}
		///<summary>
		/// Xml Tag: tamamlayiciOlculer | turu1
		///</summary>
		ZString SupplementaryMeasuresType1
		{
			get;
		}
		///<summary>
		/// Xml Tag: tamamlayiciOlculer | miktari1
		///</summary>
		ZDecimal SupplementaryMeasuresQuantity1
		{
			get;
		}
		///<summary>
		/// Xml Tag: tamamlayiciOlculer | turu2
		///</summary>
		ZString SupplementaryMeasuresType2
		{
			get;
		}
		///<summary>
		/// Xml Tag: tamamlayiciOlculer | miktari2
		///</summary>
		ZDecimal SupplementaryMeasuresQuantity2
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeFaturaBedeli | dovizTuru
		///</summary>
		ZString BillAmountCurrencyCode
		{
			get;
		}
		///<summary>
		/// Xml Tag: dovizVeFaturaBedeli | bedeli
		///</summary>
		ZDecimal BillAmountValue
		{
			get;
		}
		///<summary>
		/// Xml Tag: hesaplamaYontemi
		///</summary>
		ZString CalculationMethod
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
	}
}
