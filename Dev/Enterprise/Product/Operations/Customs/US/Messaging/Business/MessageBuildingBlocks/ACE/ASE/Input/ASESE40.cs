namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE40")]
	public abstract partial class ASESE40 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE40()
			: base("SE40")
		{
		}

		/// <summary>
		/// The line item identifier begins with 001 and is incremented by one each time record SE40 is repeated.
		/// </summary>
		[MessageBlockInt(3, 5, "M")]
		public ZInt LineItemIdentifier;

		/// <summary>
		/// The International Organization for Standardization (ISO) country code representing the country of origin. Valid ISO codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 8, "M")]
		public ZString CountryOfOrigin;

		/// <summary>
		/// A clear description of the commercial invoice line item in English. Broad, generalized language is unacceptable, as are tariff descriptions. Commercial description is not the same as the Harmonized Tariff description. Provide the description according to other agency instructions.
		/// </summary>
		[MessageBlockString(70, 11, "O", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString CommercialInvoiceDescription;
	}
}
