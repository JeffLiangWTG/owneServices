namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output
{
	using CargoWise.Types;

	[OutputBlock("Z")]
	[ApplicationIdentifier("XN", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommWarnZXN : MessageBlock
	{
		public AESCommWarnZXN()
			: base("Z")
		{
		}

		/// <summary>
		/// Identifier of the entity that established the commodity shipments contained in the batch.
		/// </summary>
		[MessageBlockString(9, 6, "M")] // String
		public ZString FilerID;

		/// <summary>
		/// Type of Filer ID.
		/// </summary>
		[MessageBlockString(1, 21, "M")]
		public ZString FilerIDType;

		/// <summary>
		/// Identifies the AES output batch type. 
		/// Always XN (Commodity Shipment Warning Reminder)
		/// </summary>
		[MessageBlockString(2, 22, "M")]
		public ZString ApplicationIdentifier;

		/// <summary>
		/// Date the reminder batch was generated (YYYYMMDD format).
		/// </summary>
		[MessageBlockDate(24, "M", "yyyyMMdd")]
		public ZDate TransmitterDate;

		/// <summary>
		/// AES’s internal batch identifier.
		/// </summary>
		[MessageBlockString(6, 32, "M")]
		public ZString BatchControlNumber;

		/// <summary>
		/// Identifier of party originally transmitting the commodity shipments contained in the batch.
		/// </summary>
		[MessageBlockString(9, 39, "M")] // String
		public ZString TransmitterID;
	}
}
