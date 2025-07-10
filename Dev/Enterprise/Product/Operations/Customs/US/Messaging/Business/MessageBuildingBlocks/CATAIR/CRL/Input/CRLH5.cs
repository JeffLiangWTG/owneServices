namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("H5")]
	public abstract partial class CRLH5 : MessageBlock // Need to add interface for BIRD System
	{
		public CRLH5()
			: base("H5")
		{
		}

		/// <summary>
		/// The record control number begins with 001 and is incremented by one each time Record Identifier H5 is repeated.
		/// </summary>
		[MessageBlockInt(3, 3, "M")]
		public ZInt RecordControlNumber;

		/// <summary>
		/// The International Organization for Standardization (ISO) country code representing the country of origin. Valid ISO codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 6, "M")]
		public ZString CountryOfOrigin;

		/// <summary>
		/// The appropriate duty/statistical reporting number under which the article is classified in the Harmonized Tariff Schedule of the United States Annotated (HTS).
		/// </summary>
		[MessageBlockString(10, 8, "M")]
		public ZString TariffNumber;

		/// <summary>
		/// A code representing the invoicing party. Refer to CBP Directive 3500-13, dated November 24, 1986, for the formula to derive the code.
		/// </summary>
		[MessageBlockString(15, 18, "M")]
		public ZString ManufacturerShipper;

		/// <summary>
		/// A code identifying the line item ultimate consignee. If the ultimate consignee is reported in Record Identifier H2, there is no line item ultimate consignee.
		/// </summary>
		[MessageBlockString(12, 33, "C")]
		public ZString LineItemUltimateConsignee;

		/// <summary>
		/// The line item value in whole dollars.
		/// </summary>
		[MessageBlockDecimal(10, 45, "O", 0, FillType.ZeroFillUnlessEmpty)] // need zero padding
		public ZDecimal LineItemValue;
	}
}
