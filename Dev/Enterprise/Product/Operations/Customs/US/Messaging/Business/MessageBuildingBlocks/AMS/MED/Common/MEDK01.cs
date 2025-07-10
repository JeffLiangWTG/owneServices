namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common
{
	using CargoWise.Types;

	[InputBlock("K01")]
	[OutputBlock("K01")]
	public partial class MEDK01 : MessageBlock
	{
		public MEDK01()
			: base("K01")
		{
		}

		/// <summary>
		/// A code representing the importing carrier. This is the Standard Carrier Alpha Code (SCAC) issued by the National Motor Freight Traffic Association, Inc. located at 2200 Mill Road, Alexandria, VA 22310. In the case of water carriers who own their containers, the SCAC code is issued by the Intermodal Association of North America located at 6410 Kenilworth Ave., Suite 108, Riverdale, MD 20737.
		/// </summary>
		[MessageBlockString(4, 4, "M")]
		public ZString CarrierCode;

		/// <summary>
		/// A code representing the CBP district/port of lading/unlading. Use Census Schedule D, included as Appendix E of this publication, for valid district/port codes.
		/// </summary>
		[MessageBlockString(4, 8, "M")]
		public ZString CBPDistrictPort;

		/// <summary>
		/// A code representing the CBP type of manifest edit. Valid codes are:
		/// 
		/// A = Add a bill of lading
		/// D = Delete a bill of lading
		/// R = Change FROB Bill
		/// </summary>
		[MessageBlockString(1, 12, "M")]
		public ZString ActionCode;

		/// <summary>
		/// The bill of lading sequence number. Do not include the issuer code as it is contained in the associated J01 record.
		/// </summary>
		[MessageBlockString(12, 13, "M")]
		public ZString BillOfLadingSequenceNumber;

		/// <summary>
		/// The house bill number. (This is for future use.)
		/// </summary>
		[MessageBlockString(12, 25, "O")]
		public ZString HouseBillNumber;

		/// <summary>
		/// Required if action code is "R". The name of the conveyance the bill is being transferred to. A valid conveyance name is entered using no slashes. This data element is mandatory if Conveyance Code is left blank.
		/// </summary>
		[MessageBlockString(23, 37, "C")]
		public ZString ConveyanceName;

		/// <summary>
		/// Required if action code is "R". The voyage associated with the conveyance the bill is being transferred to. The voyage number entered using no slashes.
		/// </summary>
		[MessageBlockString(5, 60, "C")]
		public ZString VoyageNumber;

		/// <summary>
		/// The Lloyds Code associated with the conveyance the bill is being transferred to. The Lloyds of London Registry Codes represented the importing conveyance. This code is mandatory if Conveyance Name is not entered in positions 37-59 of this record.
		/// </summary>
		[MessageBlockString(7, 65, "O")] // Should be string
		public ZString ConveyanceCode;

		/// <summary>
		/// The estimated date of arrival of the conveyance/voyage the bill is being transferred to in the port indicated. A date in MMDDYY (month, day, year) format representing the original schedule date of arrival.
		/// </summary>
		[MessageBlockDate(72, "C", "MMddyy")]
		public ZDate EstimatedDate;
	}
}
