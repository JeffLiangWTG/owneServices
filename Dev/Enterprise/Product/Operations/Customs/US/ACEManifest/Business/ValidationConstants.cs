
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public static class ValidationConstants
	{
		public static string EstDateAtFirstArrivalRequired => Res.GetString("{96a83b51-4ea0-4cfe-97b9-401cc6aa0d46}", "Est Date at First Port is required when Port of First Arrival is entered.");
		internal static string FlightNumberIsNonCompliant => Res.GetString("A4231A96-3DBB-4E2E-A984-4177F6AF753B", "Flight Number should begin with the Carrier Code and must be a 2 or 3 character IATA code followed by the flight number (in format NNN, NNNA, NNNN or NNNNA)");
		internal static string FlightNumberIsWrongLength => Res.GetString("60951425-088C-4C4B-9689-0481C6840C5D", "Flight Number needs to be between 3 and 8 characters");
		internal static string BillNumberIsWrongLength => Res.GetString("CD1C61F7-F639-4C4B-93CF-36DA269DF98D", "This Manifest Type requires the Bill Number to be 12 characters maximum.");
		internal static string FlightNumberIsDuplicated => Res.GetString("4F206DB2-F269-4202-841D-0E871D0DE57F", "Flight Details are duplicated - (Flight No. / ETA)");
		internal static string AirAMSOriginatorCodeRequired => Res.GetString("13B0B7B1-4252-45B2-8885-7ED6A023E88F", "The currently selected address does not have a valid AMO Code. This can be added under CFS Address > Details > Config > Registration Numbers / Codes, (using type 'AMO').");
	}
}
