namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("PE15")]
	public partial class SEPAPE15 : MessageBlock
	{
		public SEPAPE15()
			: base("PE15")
		{
		}

		/// <summary>
		/// Code identifying the type of Bill of Lading Number. Valid codes are: 
		/// R = Regular / Simple Bill of Lading; 
		///  Shipment Control Number (Mode 
		///  Truck) 
		/// M = Master Bill of Lading; 
		/// H = House Bill of Lading; 
		///  Bill Control Number (Mode Truck) 
		/// = Sub-House Bill of Lading 
		///  (future use) 
		/// = Express Carrier Tracking Number 
		///  (air only) 
		/// I = In-bond number
		/// </summary>
		[MessageBlockString(1, 5, "C")]
		public ZString BillTypeIndicator;

		/// <summary>
		/// A code representing the issuer of the bill of lading. Space fill for Air shipments.
		/// </summary>
		[MessageBlockString(4, 6, "C")]
		public ZString IssuerCodeOfBillOfLadingNumber;

		/// <summary>
		/// The unique identifying number associated to the Bill Type Indicator in this record. For a bill of lading number it is the number as listed on the manifest or the In-Bond number. If the number is less than 50 positions, it is left justified. Do not include spaces, hyphens, slashes or other special characters. Include the AWB prefix for Air Shipments for Master/Simple of Lading and Tracking Numbers.
		/// </summary>
		[MessageBlockString(50, 10, "C")]
		public ZString BillOfLadingNumber;

		/// <summary>
		/// The confirmation number received from FDA when prior notice is received.
		/// </summary>
		[MessageBlockString(21, 60, "C")]
		public ZString PriorNoticeConfirmationNumber;
	}
}
