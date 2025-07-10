namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipSC1XP : AESCommShipSC1XPBase
	{
	}

	[InputBlock("SC1")]
	public abstract class AESCommShipSC1XPBase : MessageBlock
	{
		protected AESCommShipSC1XPBase()
			: base("SC1")
		{
		}

		/// <summary>
		/// An indication that, during the fiscal year, either the USPPI or foreign ultimate consignee reported owned 10% or more of the other's voting securities. 
		/// Y = Yes; Companies related. 
		/// N = No; Companies NOT related. 
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(1, 4, "C")]
		public ZString RelatedCompanyIndicator;

		/// <summary>
		/// The mode of transportation by which the merchandise is exported. 
		/// 
		/// See 'Appendix T - Mode of Transportation Codes'. 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(2, 5, "C")]
		public ZString ModeOfTransportationCodeMOT;

		/// <summary>
		/// The country in which the merchandise is to be consumed, manufactured or further processed at the time of shipment. 
		/// 
		/// See 'Appendix C - ISO Country Codes'. Space fill if NOT required.
		/// </summary>
		[MessageBlockString(2, 7, "C")]
		public ZString CountryOfUltimateDestinationCode;

		/// <summary>
		/// The U.S. state, U.S. territory or U.S. possession where the merchandise began its journey to the port of exportation. 
		/// 
		/// Report a valid USPS State code.
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(2, 9, "C")]
		public ZString USStateOfOriginCode;

		/// <summary>
		/// The entity ultimately responsible for the transportation of the exported merchandise from the domestic port of exportation. For vessel, rail or truck shipments: report the SCAC code. For air shipments: report the IATA code. 
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(4, 11, "C")]
		public ZString CarrierIDSCACIATA;

		/// <summary>
		/// Unique identifier of the shipment. Left justify NO leading or imbedded spaces.
		/// </summary>
		[MessageBlockString(17, 15, "M")]
		public ZString ShipmentReferenceNumber;

		/// <summary>
		/// The AES action requested for this commodity shipment transaction:
		/// A = Add. Report the shipment to AES for the first time. 
		/// R = Replace. ENTIRELY replace a previously accepted shipment (i.e., the shipment data and all commodity lines)
		/// C = Change. Correct/change a previously accepted shipment (i.e., the shipment and any/all commodity lines) 
		/// X = Cancel. Report that a previously accepted shipment will not be exported.
		/// </summary>
		[MessageBlockString(1, 32, "M")]
		public ZString ShipmentFilingActionRequestIndicator;

		/// <summary>
		/// For vessel shipments: the name of the vessel. For non-vessel, non-mail shipments: the name of the carrier. Left justify; trailing spaces. 
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(23, 33, "C")]
		public ZString ConveyanceNameCarrierName;

		/// <summary>
		/// An indication as to the type of filing 'option' chosen by the filer for the commodity shipment transaction:
		/// 2 = Predeparture. Full predeparture commodity shipment filing.
		/// 3 = Advance Export Information (AEI) partial or complete commodity shipment filing.
		/// 4 = Postdeparture. Authorized USPPI, full post-departure commodity shipment filing.
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(1, 56, "C")]
		public ZString FilingOptionIndicator;

		/// <summary>
		/// This is an indication as to the type of data being filed in conjunction with Filing Option Indicator '3':
		/// P = Partial predeparture
		/// F = Full postdeparture
		/// </summary>
		[MessageBlockString(1, 57, "C")]
		public ZString AEIFilingType;

		/// <summary>
		/// The foreign port of unlading of the shipment. 
		/// 
		/// See 'Appendix Z - Additional Information Sources' regarding the 'Schedule K - Classification of Foreign Ports by Geographic Trade Area and Country."
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(5, 58, "C")]
		public ZString PortOfUnladingCode;

		/// <summary>
		/// For vessel and air shipments: the U.S. port that merchandise is last laden for departure. For other MOT's: the U.S. port at the actual border crossing location or the nearest U.S. port to the actual border crossing location. 
		/// 
		/// See 'Appendix D - Export Port Codes'.
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(4, 63, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString PortOfExportationCode;

		/// <summary>
		/// Date of departure from the port of exportation.
		/// Estimate the date if the actual date is unknown.
		/// (YYYYMMDD format)
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockDate(67, "C", "yyyyMMdd")]
		public ZDate EstimatedDateOfExport;

		/// <summary>
		/// An indication that the shipment contains hazardous material as defined by the U.S. Department of Transportation.
		/// Y = Yes; shipment contains hazardous material.
		/// N = No; shipment does NOT contain hazardous material.
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(1, 76, "C")]
		public ZString HazardousMaterialIndicatorHAZMAT;
	}
}
