using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IBillVisitedCountry
	{
		///<summary>
		/// Xml Tag: LimanYerAdi
		///</summary
		ZString PortLocationName { get; }

		///<summary>
		/// Xml Tag: UlkeKodu
		///</summary
		ZString CountryCode { get; }
	}
}
