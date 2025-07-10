namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common
{
	using CargoWise.Types;

	[InputBlock("A01")]
	[OutputBlock("A01")]
	public partial class MADA01 : MessageBlock
	{
		public MADA01()
			: base("A01")
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
		/// A code representing the CBP type of manifest amendment. Valid codes are:
		/// 
		/// A = Add a bill of lading
		/// B = Add In-bond Movement
		/// C = Add New CBP Broker
		/// D = Delete a bill of lading
		/// E = Cancellation of subsequent in-bond
		/// R = Replace the existing manifest data with new manifest data
		/// N = Delete 2nd notify party (not available at this time)
		/// S = Add 2nd notify party (not available at this time)
		/// </summary>
		[MessageBlockString(1, 12, "M")]
		public ZString ActionCode;

		/// <summary>
		/// The bill of lading sequence number. Do not include the issuer code as it is contained in the associated J01 record.
		/// </summary>
		[MessageBlockString(12, 13, "M")]
		public ZString BillOfLadingSequenceNumber;

		/// <summary>
		/// If the action code is R on this record, enter the new bill of lading quantity.
		/// </summary>
		[MessageBlockDecimal(10, 25, "C", 0)] // It should be decimal
		public ZDecimal Quantity;

		/// <summary>
		/// A code representing the reason(s) for the amendment of the manifest record. See Appendix C of this publication for a complete listing of valid Amendment codes.
		/// </summary>
		[MessageBlockString(2, 35, "M")]
		public ZString AmendmentCode;

		/// <summary>
		/// A code used to report individual portions of a consolidated shipment. In concept, it performs the same function for vessel shipments as the house air waybill does for air shipments. Insert the carrier-assigned number that identifies the portion of the consolidation being reported. The number must be unique within the bill of lading. If a bill of lading is added with this record (action code A) this is the new bill number. (This is for future use.)
		/// </summary>
		[MessageBlockString(12, 37, "C")]
		public ZString HouseBillNumber;

		/// <summary>
		/// A code representing the second notify party. This data element is used with action codes "N" or "S".
		/// </summary>
		[MessageBlockString(4, 49, "C")]
		public ZString CarrierCode1;

		/// <summary>
		/// This data element is required if action code is "C". Code ABI = ABI Office Routing code. This code is used by rail AMIL participant in subsequent in-bond movements.
		/// </summary>
		[MessageBlockString(3, 53, "C")]
		public ZString CodeQualifier;

		/// <summary>
		/// A code related to the preceding code qualifier ABI office routing code. This code is used by rail AMS participants in subsequent in-bond movements.
		/// </summary>
		[MessageBlockString(17, 56, "C")]
		public ZString IDCode;
	}
}
