namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("GO20")]
	public partial class AGO20 : MessageBlock
	{
		public AGO20()
			: base("GO20")
		{
		}

		/// <summary>
		/// Identifies the type of identifier.
		/// (LEI, GLN, DUNS)
		/// </summary>
		[MessageBlockString(4, 5, "M")]
		public ZString ReferenceIdentifierQualifier;

		/// <summary>
		/// A unique numeric or alphanumeric entity identifier.
		/// </summary>
		[MessageBlockString(20, 9, "M")]
		public ZString ReferenceIdentifier;
	}
}
