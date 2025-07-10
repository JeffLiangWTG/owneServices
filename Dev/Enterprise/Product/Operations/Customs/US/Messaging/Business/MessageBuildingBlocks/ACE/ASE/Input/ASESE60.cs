namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE60")]
	public abstract partial class ASESE60 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE60()
			: base("SE60")
		{
		}

		/// <summary>
		/// The appropriate duty/statistical reporting number under which the article is classified in the Harmonized Tariff Schedule of the United States Annotated (HTS).
		/// </summary>
		[MessageBlockString(10, 5, "M")]
		public ZString HTSNumber;

		/// <summary>
		/// The line item value in whole US dollars.
		/// </summary>
		[MessageBlockDecimal(10, 15, "O", 0)]
		public ZDecimal LineItemValue;

		/// <summary>
		/// An indicator for whether the filer is claiming this HTS is not subject to sanctions.
		/// </summary>
		[MessageBlockString(1, 25, "C")]
		public ZString SanctionDisclaimIndicator;
	}
}
