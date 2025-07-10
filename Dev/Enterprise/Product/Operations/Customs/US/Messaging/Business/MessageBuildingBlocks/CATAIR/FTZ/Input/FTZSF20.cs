namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	// This message block has been removed from the latest spec but we still keep it here for old messages
	[InputBlock("SF20")]
	public partial class FTZSF20 : MessageBlock
	{
		public FTZSF20()
			: base("SF20")
		{
		}

		/// <summary>
		/// Code that defines the Reference Identifier.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString ReferenceIdentifierQualifier;

		/// <summary>
		/// Do not include spaces, hyphens, slashes or other special characters.
		/// </summary>
		[MessageBlockString(50, 8, "M")]
		public ZString ReferenceIdentifier;
	}
}
