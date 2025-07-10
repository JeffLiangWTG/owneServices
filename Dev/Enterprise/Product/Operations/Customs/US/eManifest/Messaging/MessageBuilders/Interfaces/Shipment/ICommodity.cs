using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface ICommodity : Customs.Business.MessageBuilders.eManifest.ICommodity
	{
		/// <summary>
		/// Vehicle identification number if commodity is a vehicle. (C/30)
		/// Condition: If commodity is vehicle
		/// </summary>
		IEnumerable<ZString> VehicleIdentificationNumbers { get; }

		/// <summary>
		/// 14 Character code assigned by Customs. (C/14)
		/// Condition: for BRASS (Border Release Advanced Screening and Selectivity) Shipments
		/// </summary>
		IEnumerable<ZString> C4Codes { get; }

		/// <summary>
		/// Customs shipment value. In whole dollars. (C/12)
		/// For Sec 321 releases this will be the acutal value. The estimated value will be used for IE and TE.
		/// Condition: specify for 321 & In-Bond
		/// </summary>
		ZInt CustomsValue { get; }

		/// <summary>
		/// The country of manufacture, production, or growth of any article of foreign origin entering the U.S. (C/3)
		/// Further work or material added to an article in another country must effect a substantial transformation in order to render such other country the  "country of origin". 
		/// Condition: specify for 321
		/// </summary>
		ZString CountryOfOrigin { get; }

		/// <summary>
		/// Equipment ACE id/Equipment number/Equipment license plate. (C/10/18/17)
		/// Condition: Mandatory if:
		///  - modifying shipment details and only the boarded quantity is being reported;
		///  - or the goods are loaded into or on to equipment;
		/// </summary>
		IEquipment Equipment { get; }
	}
}
