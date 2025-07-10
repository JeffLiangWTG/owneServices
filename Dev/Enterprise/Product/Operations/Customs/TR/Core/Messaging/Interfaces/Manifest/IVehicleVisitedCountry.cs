using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IVehicleVisitedCountry : IBillVisitedCountry
	{
		///<summary>
		/// Xml Tag: HareketTarihSaati
		///</summary
		ZDateTime MovementDateTime { get; }
	}
}
