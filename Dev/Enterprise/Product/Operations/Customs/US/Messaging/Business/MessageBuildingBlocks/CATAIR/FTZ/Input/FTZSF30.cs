namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	// This message block has been removed from the latest spec but we still keep it here for old messages
	[InputBlock("SF30")]
	public partial class FTZSF30 : MessageBlock
	{
		public FTZSF30()
			: base("SF30")
		{
		}

		/// <summary>
		/// Code identifying the type of commercial entity being reported.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString EntityCode;

		/// <summary>
		/// The name of the entity identified by the Entity Code. This field must be blank if an Entity Identifier is used.
		/// </summary>
		[MessageBlockString(35, 8, "C")]
		public ZString EntityName;

		/// <summary>
		/// This field must be used if the entity identifier is being supplied in lieu of name and address. This field is mandatory if the entity identifier is CN or IM.
		/// </summary>
		[MessageBlockString(3, 43, "C")]
		public ZString EntityIdentifierQualifier;

		/// <summary>
		/// The code identified by the Entity Identifier Qualifier. This field is mandatory if the entity identifier is CN or IM.
		/// </summary>
		[MessageBlockString(20, 46, "C")]
		public ZString EntityIdentifier;

		/// <summary>
		/// A code representing the manufacturer/ supplier. Refer to CBP Directive 3500-13, dated November 24, 1986, for the formula to derive the code.
		/// </summary>
		[MessageBlockString(15, 66, "C")]
		public ZString ManufacturerSupplierCode;
	}
}
