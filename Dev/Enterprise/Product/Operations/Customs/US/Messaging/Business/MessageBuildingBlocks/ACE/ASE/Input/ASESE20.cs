namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE20")]
	public abstract partial class ASESE20 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE20()
			: base("SE20")
		{
		}

		/// <summary>
		/// Code that defines the Reference Identifier.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString ReferenceIdentifierQualifier;

		/// <summary>
		/// Reference data.
		/// </summary>
		[MessageBlockString(50, 8, "M")]
		public ZString ReferenceIdentifier;
	}
}
