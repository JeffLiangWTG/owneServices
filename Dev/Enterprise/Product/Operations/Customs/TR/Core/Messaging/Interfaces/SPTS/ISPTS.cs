using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ISPTS : IMessageSender
	{
		///<summary>
		/// Xml Tag: HareketGumrukIdaresi
		///</summary
		ZString PortOfPresentationDCode { get; }

		///<summary>
		/// Xml Tag: VarisGumrukIdaresi
		///</summary
		ZString DestinationPortDCode { get; }

		///<summary>
		/// Xml Tag: BeyanSahibiVergiNo
		///</summary
		ZString BusinessRegNo { get; }

		///<summary>
		/// Xml Tag: TasiyiciFirmaVergiNo
		///</summary
		ZString CarrierBusinessRegNo { get; }

		///<summary>
		/// Xml Tag: TasimaSekli
		///</summary
		ZString TransportType { get; }

		///<summary>
		/// Xml Tag: GuncellenecekTescilNo
		///</summary
		ZString RegistrationNoToBeUpdated { get; }

		///<summary>
		/// Xml Tag: KullaniciKodu
		///</summary
		ZString UserID { get; }

		///<summary>
		/// Xml Tag: SeferNumarasi
		///</summary
		ZString VoyageNumber { get; }

		///<summary>
		/// Xml Tag: SeferTarihi
		///</summary
		ZDateTime VoyageDate { get; }

		///<summary>
		/// Xml Tag: XmlRefId
		///</summary
		ZString XmlRefId { get; }

		///<summary>
		/// Xml Tag: AktarmaSenetleri
		///</summary
		IEnumerable<ISPTSBills> SPTSBills { get; }

		///<summary>
		/// Xml Tag: AktarmaUldleri
		///</summary
		IEnumerable<ISPTSUlds> SPTSUlds { get; }
	}
}
