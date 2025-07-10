namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	public interface ITrip
	{
		/// <summary>
		/// US: (M/4), Carrier SCAC Code.
		/// CA: (M/4), A unique ID assigned by the CBSA to an approved carrier.
		/// </summary>
		ZString CarrierCode { get; }

		/// <summary>
		/// Mode by which the merchandise crosses the international border. 
		/// US,CA: (M/2).
		/// </summary>
		ZString MethodOfTransportation { get; }

		/// <summary>
		/// A unique number assigned by the carrier issuing the Manifest to each movement of a conveyance. 
		/// US,CA: (M/25).
		/// </summary>
		ZString TripReference { get; }

		/// <summary>
		/// Estimated Date/Time conveyance will arrive at the first port in USA/CA.
		/// US: (M/MMDDYYYY; O/HHMM).
		/// CA: (M/CCYYMMDDHHMM; C/ZHHMM), Condition: ZHHMM = time zone given as offset from Coordinated Universal Time (UTC).
		///		If time zone is not provided, Eastern Time will be assumed.
		/// </summary>
		ZDateTime EstimatedDateOfArrival { get; }

		/// <summary>
		/// US: First port where conveyance will enter the United States (Schedule D - US Domestic Ports). (M/4)
		/// CA: The Port of Report will be the expected first Canadian Port (CBSA office) of Arrival (FPOA). (M/4)
		/// </summary>
		ZString FirstExpectedPortOfArrival { get; }

		/// <summary>
		/// Used to specify amendment reason  for changes after complete submission of Manifest to Customs.
		/// US,CA: (C/2), Condition: Used to amend a completed Manifest.
		/// </summary>
		ZString AmendmentReasonCode { get; set; }
	}
}
