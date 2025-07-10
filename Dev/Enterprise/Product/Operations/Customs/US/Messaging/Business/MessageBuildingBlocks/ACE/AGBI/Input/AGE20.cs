namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE20")]
	public partial class AGE20 : MessageBlock
	{
		public AGE20()
			: base("GE20")
		{
		}

		/// <summary>
		/// Identifies the type of identifier submitted.
		/// (LEI, GLN, DUNS) 
		/// </summary>
		[MessageBlockString(4, 5, "M")]
		public ZString GBIIdentifierQualifier;

		/// <summary>
		/// A unique numeric or alphanumeric entity identifier.
		/// </summary>
		[MessageBlockString(20, 9, "M")]
		public ZString GBIIdentifier;
	}
}
