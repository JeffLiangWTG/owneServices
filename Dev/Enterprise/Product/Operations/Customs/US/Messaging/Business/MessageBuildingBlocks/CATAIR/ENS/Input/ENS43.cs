namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("43")]
	public abstract partial class ENS43 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS43()
			: base("43")
		{
		}

		/// <summary>
		/// The administrative number assigned by CBP to a binding ruling, or under the pre-importation review program. In accordance with the rulings program, a binding ruling number may only be reported when the ruling specifically cites both the importer and merchandise covered by this summary line. When filing EIP entries use “INVREQ” for “Invoice by Request” on merchandise that doesn’t have a ruling assigned.
		/// </summary>
		[MessageBlockString(6, 3, "C")]
		public ZString PreImportationReviewProgramPIRPRulingsNumber;

		/// <summary>
		/// A code representing the type of ruling. Valid codes are:
		/// 
		/// R = Binding Rulings
		/// C = Pre-classification
		/// P = Pre-approval
		/// D = Commercial Description
		/// 
		/// When using “INVREQ” the Binding Ruling’s Code “R” is required.
		/// </summary>
		[MessageBlockString(1, 9, "M")]
		public ZString TypeIndicator;

		/// <summary>
		/// The invoice line description of the commodity. Broad, generalized language is unacceptable, as are tariff descriptions. Complete commercial terminology, in the English language, is mandatory.
		/// </summary>
		[MessageBlockString(70, 11, "C")]
		public ZString CommercialDescription;
	}
}
