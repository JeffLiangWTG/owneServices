namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("WR4")]
	public partial class ACEQWR4 : MessageBlock
	{
		public ACEQWR4()
			: base("WR4")
		{
		}

		/// <summary>
		/// The in-bond number as listed on the manifest. If the in-bond number is less than 12 positions, it is left justified. Spaces, hyphens, slashes and other special characters are not included.
		/// </summary>
		[MessageBlockString(12, 4, "C")]
		public ZString InBondNumber;

		/// <summary>
		/// The master bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Spaces, hyphens, slashes and other special characters are not included.
		/// </summary>
		[MessageBlockString(12, 16, "C")]
		public ZString MasterBillNumber;

		/// <summary>
		/// The house bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Spaces, hyphens, slashes and other special characters are not included.
		/// </summary>
		[MessageBlockString(12, 28, "C")]
		public ZString HouseBillNumber;

		/// <summary>
		/// The sub-house bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Spaces, hyphens, slashes and other special characters are not included.
		/// </summary>
		[MessageBlockString(12, 40, "C")]
		public ZString SubHouseBillNumber;

		/// <summary>
		/// The quantity associated with the lowest level of the bill number being reported. It is the smallest exterior packaging unit.
		/// </summary>
		[MessageBlockInt(8, 52, "C")]
		public ZInt ManifestQuantity;

		/// <summary>
		/// A code representing the unit of measure. Valid unit of measure codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(5, 60, "C")]
		public ZString Unit;

		/// <summary>
		/// A code representing the Standard Carrier Alpha Code (SCAC) of the party who actually issued the ocean bill of lading. Do not confuse the issuer of the bill with the operator of the vessel.
		/// </summary>
		[MessageBlockString(4, 65, "C")]
		public ZString IssuerCodeOfMasterBillNumber;

		/// <summary>
		/// A code representing the SCAC of the party who issued the house bill of lading.
		/// </summary>
		[MessageBlockString(4, 69, "C")]
		public ZString IssuerCodeOfHouseBillNumber;

		/// <summary>
		/// A code representing the type of the bill of lading. Valid codes are:
		/// 
		/// 0 = Regular Bill of Lading
		/// M = Master Bill of Lading
		/// H = House Bill of Lading
		/// F = FROB (Freight Remaining on Board)
		/// </summary>
		[MessageBlockString(1, 73, "C")]
		public ZString BillOfLadingType;

		/// <summary>
		/// A code representing the receipt of an Importer Security Filing against a Bill of Lading. This is only used in Ocean AMS. Valid codes are:
		/// 
		/// Y = ISF on file
		/// N = ISF not on file
		/// </summary>
		[MessageBlockString(1, 74, "C")]
		public ZString ImporterSecurityFilingIndicator;

		/// <summary>
		/// A code indicating the method of transportation. Valid codes are:
		/// 
		/// 1 = Ocean
		/// 2 = Rail
		/// 3 = Truck
		/// </summary>
		[MessageBlockString(1, 75, "C")]
		public ZString ModeOfTransportationCode;
	}

	[OutputBlock("WR4", "1")]
	public partial class ACEQWR4_1 : MessageBlock
	{
		public ACEQWR4_1()
			: base("WR4")
		{
		}

		/// <summary>
		/// The in-bond number as listed on the manifest. If the in-bond number is less than 12 positions, it is left justified. Spaces, hyphens, slashes and other special characters are not included.
		/// </summary>
		[MessageBlockString(12, 4, "C")]
		public ZString InBondNumber;

		/// <summary>
		/// The master bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Spaces, hyphens, slashes and other special characters are not included.
		/// </summary>
		[MessageBlockString(12, 16, "C")]
		public ZString MasterBillNumber;

		/// <summary>
		/// The house bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Spaces, hyphens, slashes and other special characters are not included.
		/// </summary>
		[MessageBlockString(12, 28, "C")]
		public ZString HouseBillNumber;

		/// <summary>
		/// The sub-house bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Spaces, hyphens, slashes and other special characters are not included.
		/// </summary>
		[MessageBlockString(12, 40, "C")]
		public ZString SubHouseBillNumber;

		/// <summary>
		/// The quantity associated with the lowest level of the bill number being reported. It is the smallest exterior packaging unit.
		/// </summary>
		[MessageBlockDecimal(10, 52, "C", 0)]
		public ZDecimal ManifestQuantity;

		/// <summary>
		/// A code representing the unit of measure. Valid unit of measure codes are listed in ACE Ocean CAMIR Appendix N: Manifest Units of Measure.
		/// </summary>
		[MessageBlockString(5, 62, "C")]
		public ZString Unit;

		/// <summary>
		/// A code representing the Standard Carrier Alpha Code (SCAC) of the party who actually issued the ocean bill of lading. Do not confuse the issuer of the bill with the operator of the vessel.
		/// </summary>
		[MessageBlockString(4, 67, "C")]
		public ZString IssuerCodeOfMasterBillNumber;

		/// <summary>
		/// A code representing the SCAC of the party who issued the house bill of lading.
		/// </summary>
		[MessageBlockString(4, 71, "C")]
		public ZString IssuerCodeOfHouseBillNumber;

		/// <summary>
		/// A code representing the type of the bill of lading. Valid codes are:
		/// 
		/// 0 = Regular Bill of Lading
		/// M = Master Bill of Lading
		/// H = House Bill of Lading
		/// F = FROB (Freight Remaining on Board)
		/// </summary>
		[MessageBlockString(1, 75, "C")]
		public ZString BillOfLadingType;

		/// <summary>
		/// A code representing the receipt of an Importer Security Filing against a Bill of Lading. This is only used in Ocean AMS. Valid codes are:
		/// 
		/// Y = ISF on file
		/// N = ISF not on file
		/// </summary>
		[MessageBlockString(1, 76, "C")]
		public ZString ImporterSecurityFilingIndicator;

		/// <summary>
		/// A code indicating the method of transportation. Valid codes are:
		/// 
		/// 1 = Ocean
		/// 2 = Rail
		/// 3 = Truck
		/// </summary>
		[MessageBlockString(1, 77, "C")]
		public ZString ModeOfTransportationCode;

		/// <summary>
		/// A code indicating which version of the WR4 record is being sent to the trade.
		/// </summary>
		[MessageBlockString(1, 78, "C")]
		public ZString WR4RecordVersion;
	}
}
