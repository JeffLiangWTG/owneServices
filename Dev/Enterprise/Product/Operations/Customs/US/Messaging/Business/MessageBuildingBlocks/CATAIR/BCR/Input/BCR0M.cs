namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("0M")]
	public abstract partial class BCR0M : MessageBlock // Need to add interface for BIRD System
	{
		public BCR0M()
			: base("0M")
		{
		}

		/// <summary>
		/// Master bill number as listed on the manifest. If the number is less than 12 positions it is left justified. Do not include spaces, hyphens, slashes, or other special characters. A bill number is not necessary for FTZ Type 06 entries. If the mode of transportation code is 30 or 31 (truck), the bill of lading number must be reported. If the port is operational for Rail AMS, these data elements are required.
		/// </summary>
		[MessageBlockString(12, 15, "M")]
		public ZString MasterBillNumber;

		/// <summary>
		/// House bill number as listed on the manifest. If the number is less than 12 positions it is left justified. Do not include spaces, hyphens, slashes, or other special characters. If the mode of transportation code is 30 or 31 (truck), the bill of lading number must be reported.
		/// </summary>
		[MessageBlockString(12, 27, "C")]
		public ZString HouseBillNumber;

		/// <summary>
		/// Sub-house bill number as it appears on the manifest. If the number is less than 12 positions it is left justified. Do no include spaces, hyphens, slashes or other special characters. If the mode of transportation code is 30 or 31 (truck), the bill of lading number must be reported.
		/// </summary>
		[MessageBlockString(12, 39, "C")]
		public ZString SubHouseBillNumber;

		/// <summary>
		/// The quantity that is associated with the lowest level of the bill number being reported. It is the smallest exterior packaging unit. If the transmission is for a straight bill of lading, the quantity is the bill of lading quantity. If the transmission is for a House Bill of Lading, both the Master Bill and House Bill numbers are reported but the only quantity reported is the House Bill quantity. Similarly, if a Sub-house Bill is being reported, numbers for all levels of the bill would be transmitted; but, only the sub-house quantity would be sent. If the port is operational for Rail AMS, these data elements are required.
		/// </summary>
		[MessageBlockInt(8, 51, "M")]
		public ZInt Quantity;

		/// <summary>
		/// The unit of measure on the bill of lading. The standard generic unit of pieces (PCS) is acceptable when there are multiple units of measure associated with the bill of lading; however, this does not necessarily relate to the unit of measure associated with the tariff schedule number in the Harmonized Tariff Schedule of the United States Annotated.
		/// </summary>
		[MessageBlockString(5, 59, "C")]
		public ZString Unit;

		/// <summary>
		/// A code representing the Standard Carrier Alpha Code (SCAC) of the party who actually issued the truck or rail bill of lading. Do not confuse the issuers of the bill with the operator of the truck or train. For entry type 06 (Foreign Trade Zone), do not input an issuer of Master Bill Number. If the port is operational for Rail AMS, these data elements are required.
		/// </summary>
		[MessageBlockString(4, 70, "C")]
		public ZString IssuerOfMasterBillNumber;

		/// <summary>
		/// A code representing the SCAC of the party who issued the automated ocean/rail house bill of lading. This party may be either an automated NVOCC or the automated issuer of the master bill. When the issuer code of house bill number is transmitted for MOT 10, 11, 20, or 21, the house bill of lading will also be required. For entry types “06”, do not input an issuer code of house bill number.
		/// </summary>
		[MessageBlockString(4, 74, "C")]
		public ZString IssuerCodeOfHouseBillNumber;
	}
}
