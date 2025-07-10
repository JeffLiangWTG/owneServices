namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE35")]
	public abstract partial class ASESE35 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE35()
			: base("SE35")
		{
		}

		/// <summary>
		/// Address Component Qualifier describing the information in the Address Information data element
		/// </summary>
		[MessageBlockString(2, 5, "M")]
		public ZString AddressComponentQualifier;

		/// <summary>
		/// Address Information corresponding to the Address Component Qualifier data element
		/// </summary>
		[MessageBlockString(35, 7, "M", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString AddressInformation;

		/// <summary>
		/// Address Component Qualifier describing the information in the Address Information data element
		/// </summary>
		[MessageBlockString(2, 42, "O")]
		public ZString AddressComponentQualifier1;

		/// <summary>
		/// Address Information corresponding to the Address Component Qualifier
		/// </summary>
		[MessageBlockString(35, 44, "O", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString AddressInformation1;
	}
}
