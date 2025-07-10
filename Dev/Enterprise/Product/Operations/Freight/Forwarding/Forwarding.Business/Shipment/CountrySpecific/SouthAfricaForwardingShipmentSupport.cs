using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SouthAfricaForwardingShipmentSupport : CountrySpecificForwardingShipmentSupport
	{
		public const string CustomsOfficeCode = "COC";
		public static string ConsigneeChangedCause
		{
			get { return Res.GetString("b55a12be-74a1-40fa-be59-edbd46789605", "due to change of Consignee by user"); }
		}
		public static string OriginDestinationChangedCause
		{
			get { return Res.GetString("18cb8201-0b18-4f67-9be0-717eddb877a1", "due to change of Origin/Destination by user"); }
		}

		protected override ZString CountryCode
		{
			get { return Constants.CountryCodes.SouthAfrica; }
		}

		protected override ZBool AddCountryCodeToEntryDetailsCaptionCore => false;
	}
}
