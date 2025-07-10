namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output
{
	using CargoWise.Types;

	[OutputBlock("Z")]
	[ApplicationIdentifier("XT", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipZXT : MessageBlock
	{
		public AESCommShipZXT()
			: base("Z")
		{
		}

		/// <summary>
		/// Identifier of the party filing the commodity shipment information.
		/// </summary>
		[MessageBlockString(9, 6, "M")]
		public ZString FilerID;

		/// <summary>
		/// Type of Filer ID reported.
		/// </summary>
		[MessageBlockString(1, 21, "M")]
		public ZString FilerIDType;

		/// <summary>
		/// Identifies the AES output batch type. 
		/// Always XT (Commodity Shipment filing response)
		/// </summary>
		[MessageBlockString(2, 22, "M")]
		public ZString ApplicationIdentifier;

		/// <summary>
		/// Filer/Transmitter's date of transmission (YYYYMMDD format).
		/// </summary>
		[MessageBlockDate(24, "M", "yyyyMMdd")]
		public ZDate TransmitterDate;

		/// <summary>
		/// Filer/Transmitter's internal batch identifier. 
		/// 
		/// Space fill if NOT used.
		/// </summary>
		[MessageBlockString(6, 32, "C")]
		public ZString BatchControlNumber;

		/// <summary>
		/// Identifier of the party transmitting the data.
		/// </summary>
		[MessageBlockString(9, 39, "M")]
		public ZString TransmitterID;

		// In the event no Z record was submitted in the input batch, an AES generated Z record will be returned.
		// With the exception of the 'Record Identifier' and the 'AES Generated Record Indicator', all AES generated Z record data elements will contain spaces.
		// The 'AES Generated Record Indicator' will be 1
		//[MessageBlockString(1, 1, "M")]
		//public ZString RecordIdentifier1;

		/// <summary>
		/// Always 1
		/// </summary>
		[MessageBlockString(1, 80, "M")]
		public ZString AESGeneratedRecordIndicator;
	}
}
