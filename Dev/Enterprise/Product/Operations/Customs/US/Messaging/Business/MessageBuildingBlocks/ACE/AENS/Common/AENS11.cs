namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("11")]
	[OutputBlock("11")]
	public abstract partial class AENS11 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS11()
			: base("11")
		{
		}

		/// <summary>
		/// Identification of the U.S. party or other entity (individual or firm) liable for payment of all duties and meeting all statutory and regulatory requirements incurred as a result of importation.
		/// </summary>
		[MessageBlockString(12, 3, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue, IsPersonalInformation = true)]
		public ZString ImporterOfRecordNumber;

		/// <summary>
		/// Identification of the U.S. party or other entity (individual or firm) on whose account the merchandise is shipped. 
		/// 
		/// Space fill if not required/not reported
		/// .
		/// </summary>
		[MessageBlockString(12, 15, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue, IsPersonalInformation = true)]
		public ZString ConsigneeNumber;

		/// <summary>
		/// Identification of the U.S. party or other entity (individual or firm) to whom refunds, bills, or notices of extension of suspension of liquidation are to be sent (if other than the Importer of Record). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(12, 27, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString DesignatedNotifyParty4811Number;

		/// <summary>
		/// Date that the Filer expects to enter the cargo for clearance. 
		/// 
		/// Provide an estimated date of entry if certifying for cargo release processing from the entry summary or if the date is to be used to determine the relevant classification, fee, AD/CVD case, or bond applicability date and is not pre-empted by another date. 
		/// 
		/// See Usage Notes (i) Bond and Surety Reporting, (j) Basic Article Classification and Tariff Considerations - Duty Rate Date Matrix Hierarchy, and (ff) Articles Subject to Anti-Dumping / Countervailing Duty.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockDate(42, "C", "MMddyy")]
		public ZDate EstimatedEntryDate;

		/// <summary>
		/// For merchandise arriving by Vessel, the date the vessel arrived within the limits of the U.S. Port of Arrival with the intent to unlade. For other transport modes, the date the merchandise arrived within the U.S. Port of Arrival. 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockDate(48, "C", "MMddyy")]
		public ZDate DateOfImportation;

		/// <summary>
		/// For merchandise being entered from a Foreign Trade Zone, the identifier of the zone. 
		/// 
		/// Note: Only data elements relevant to types '01', '03', '11', '51', and '52' are supported at this time. Until entry type 06 is supported by ACE, this data element will not be accepted.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(7, 54, "C")]
		public ZString ForeignTradeZoneIdentifier;

		/// <summary>
		/// The code for the U.S. state, U.S. territory or U.S. possession where the merchandise is destined. Report a valid USPS State code. 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(2, 61, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString USStateOfDestinationCode;

		/// <summary>
		/// For merchandise being entered from a Foreign Trade Zone, the identifier of the zone.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(9, 63, "C")]
		public ZString NewForeignTradeZoneIdentifier;
	}
}
