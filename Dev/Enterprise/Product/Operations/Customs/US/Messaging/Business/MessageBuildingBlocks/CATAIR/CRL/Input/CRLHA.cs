namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("HA")]
	public abstract partial class CRLHA : MessageBlock // Need to add interface for BIRD System
	{
		public CRLHA()
			: base("HA")
		{
		}

		/// <summary>
		/// The in-bond number as listed on the manifest. If the in-bond number is less than 12 positions, it is left justified. Do not include spaces, hyphens, slashes or other special characters.
		/// </summary>
		[MessageBlockString(12, 3, "C")]
		public ZString InbondNumber;

		/// <summary>
		/// The master bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Do not include spaces, hyphens, slashes or other special characters. For entry type 06 (Foreign Trade Zone), do not input a Master Bill Number.
		/// </summary>
		[MessageBlockString(12, 15, "M")]
		public ZString MasterBillNumber;

		/// <summary>
		/// The house bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Do not include spaces, hyphens, slashes or other special characters.
		/// </summary>
		[MessageBlockString(12, 27, "C")]
		public ZString HouseBillNumber;

		/// <summary>
		/// The sub-house bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Do not include spaces, hyphens, slashes or other special characters.
		/// </summary>
		[MessageBlockString(12, 39, "C")]
		public ZString SubHouseBillNumber;

		/// <summary>
		/// Enter the quantity associated with the lowest level of the bill number being reported. It is the smallest exterior packaging unit.
		/// </summary>
		[MessageBlockInt(8, 51, "M")]
		public ZInt Quantity;

		/// <summary>
		/// The unit of measure as indicated on the bill of lading/air waybill. The standard generic unit of pieces (PCS) is acceptable when there are multiple units of measure associated with the bill of lading or air waybill; however, this does not necessarily relate to the unit of measure associated with the tariff schedule number in the Harmonized Tariff Schedule of the United States Annotated.
		/// </summary>
		[MessageBlockString(5, 59, "C")]
		public ZString Unit;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the IT date related to the in-bond number. To combine ITs, they must have the same IT date.
		/// </summary>
		[MessageBlockDate(64, "C", "MMddyy")]
		public ZDate ImmediateTransportationITDate;

		/// <summary>
		/// A code representing the Standard Carrier Alpha Code (SCAC) of the party who actually issued the ocean bill of lading. Do not confuse the issuer of the bill with the operator of the vessel. For entry type 06 (Foreign Trade Zone); do not input an Issuer of Master Bill Number.
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
