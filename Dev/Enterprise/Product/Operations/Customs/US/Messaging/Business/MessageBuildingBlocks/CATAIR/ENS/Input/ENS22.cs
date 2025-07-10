namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("22")]
	public abstract partial class ENS22 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS22()
			: base("22")
		{
		}

		/// <summary>
		/// A code representing the in-bond number (also called the IT number). Left justify.
		/// </summary>
		[MessageBlockString(12, 3, "C")]
		public ZString InBondNumber;

		/// <summary>
		/// A code representing the master bill number. Left justify. This code is required if the mode of transportation code is 10 (vessel, non-container), 11 (vessel, container), 20 (rail, non-container), 21 (rail, container), 40 (air, non-container), or 41 (air, container); otherwise, space fill. For a description of the mode of transportation codes, refer to Appendix B of this publication.
		/// </summary>
		[MessageBlockString(12, 15, "C")]
		public ZString MasterBillNumber;

		/// <summary>
		/// A code representing the house bill number. Left justify.
		/// </summary>
		[MessageBlockString(12, 27, "C")]
		public ZString HouseBillNumber;

		/// <summary>
		/// A code representing the sub-house bill number. Left justify.
		/// </summary>
		[MessageBlockString(12, 39, "C")]
		public ZString SubHouseBillNumber;

		/// <summary>
		/// A value representing the quantity associated with the Immediate Transportation (IT), bill of lading (B/L), Automated Manifest System (AMS) master in-bond number, or the air waybill (AWB) number. The most detailed level of the shipment is reported (i.e., the smallest exterior packaging unit).
		/// </summary>
		[MessageBlockInt(8, 51, "M")]
		public ZInt Quantity;

		/// <summary>
		/// An abbreviation representing the unit of measure as indicated on the B/L or AWB. A standard generic unit of PCS (pieces) is acceptable when there are multiple units of measure associated with the bill of lading or air waybill; however, this does not relate to the unit of measure required for a specific tariff number.
		/// </summary>
		[MessageBlockString(5, 59, "M")]
		public ZString Unit;

		/// <summary>
		/// A numeric date related to the in-bond number in MMDDYY (month, day, year) format. Multiple ITs may be combined on an entry summary only if all the IT dates are the same.
		/// </summary>
		[MessageBlockDate(64, "C", "MMddyy")]
		public ZDate ITDate;

		/// <summary>
		/// A code representing the Standard Carrier Alpha Code (SCAC) of the party who actually issued the ocean bill of lading. Do not confuse the issuer of the bill with the operator of the vessel.
		/// </summary>
		[MessageBlockString(4, 70, "C")]
		public ZString IssuerCodeOfMasterBillNumber;

		/// <summary>
		/// A code representing the SCAC of the party who issued the automated ocean/rail house bill of lading. This party may be either an automated NVOCC or the automated issuer of the master bill. When the issuer code of house bill number is transmitted for MOT 10, 11, 20, or 21, the house bill of lading will also be required. For entry types “06”, do not input an issuer code of house bill number.
		/// </summary>
		[MessageBlockString(4, 74, "C")]
		public ZString IssuerCodeOfHouseBillNumber;
	}
}
