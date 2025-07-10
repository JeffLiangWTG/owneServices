namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE50")]
	public partial class AGE50 : MessageBlock
	{
		public AGE50()
			: base("GE50")
		{
		}

		/// <summary>
		/// The type of identifier being reported.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString ReferenceIdentifierType;

		/// <summary>
		/// A unique numeric or alphanumeric entity identifier.
		/// </summary>
		[MessageBlockString(15, 8, "M")]
		public ZString ReferenceIdentifierValue;
	}
}
