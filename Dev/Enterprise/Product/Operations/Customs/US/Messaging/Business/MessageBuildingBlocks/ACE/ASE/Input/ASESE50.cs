namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE50")]
	public abstract partial class ASESE50 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE50()
			: base("SE50")
		{
		}

		/// <summary>
		/// Code identifying the type of commercial entity being reported.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString EntityCode;

		/// <summary>
		/// The name of the entity identified by the Entity Code.
		/// </summary>
		[MessageBlockString(35, 8, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString EntityName;

		/// <summary>
		/// This field must be used if the entity identifier is being supplied in lieu of name and address. This field is mandatory if the entity code is CN.
		/// </summary>
		[MessageBlockString(3, 43, "C")]
		public ZString EntityIdentifierQualifier;

		/// <summary>
		/// The code identified by the entity identifier qualifier. This field is mandatory if the entity code is CN.
		/// </summary>
		[MessageBlockString(20, 46, "C")]
		public ZString EntityIdentifier;
	}
}
