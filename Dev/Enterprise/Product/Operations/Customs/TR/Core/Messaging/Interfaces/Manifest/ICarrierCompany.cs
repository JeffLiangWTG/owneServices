using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ICarrierCompany
	{
		///<summary>
		/// Xml Tag: AdiUnvani
		///</summary>
		ZString CarrierName { get; }

		///<summary>
		/// Xml Tag: CadSNo
		///</summary>
		ZString StreetNo { get; }

		///<summary>
		/// Xml Tag: Fax
		///</summary>
		ZString Fax { get; }

		///<summary>
		/// Xml Tag: IlIlce
		///</summary>
		ZString ProvinceDistrict { get; }

		///<summary>
		/// Xml Tag: KimlikNo
		///</summary>
		ZString IdentificationNumber { get; }

		///<summary>
		/// Xml Tag: KimlikTuru
		///</summary>
		ZString IdentityType { get; }

		///<summary>
		/// Xml Tag: PostaKodu
		///</summary>
		ZString PostCode { get; }

		///<summary>
		/// Xml Tag: Tel
		///</summary>
		ZString Phone { get; }

		///<summary>
		/// Xml Tag: UlkeKodu
		///</summary>
		ZString CountryCode { get; }

		///<summary>
		/// Xml Tag: VergiDairesikodu
		///</summary>
		ZString TaxOfficeCode { get; }
	}
}
