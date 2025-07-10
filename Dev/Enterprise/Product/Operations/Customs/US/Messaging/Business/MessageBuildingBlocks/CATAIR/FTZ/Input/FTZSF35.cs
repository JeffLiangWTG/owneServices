namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	// This message block has been removed from the latest spec but we still keep it here for old messages
	[InputBlock("SF35")]
	public partial class FTZSF35 : MessageBlock
	{
		public FTZSF35()
			: base("SF35")
		{
		}

		/// <summary>
		/// Address Component Qualifier describing the Address Information data element.
		/// </summary>
		[MessageBlockString(2, 5, "M")]
		public ZString AddressComponentQualifier;

		/// <summary>
		/// Address Information corresponding to the Address Component Qualifier data element.
		/// </summary>
		[MessageBlockString(35, 7, "M")]
		public ZString AddressInformation;

		/// <summary>
		/// Address Component Qualifier describing the Address Information data element.
		/// </summary>
		[MessageBlockString(2, 42, "O")]
		public ZString AddressComponentQualifier1;

		/// <summary>
		/// Address Information corresponding to the Address Component Qualifier in data element.
		/// </summary>
		[MessageBlockString(35, 44, "O")]
		public ZString AddressInformation1;
	}
}
