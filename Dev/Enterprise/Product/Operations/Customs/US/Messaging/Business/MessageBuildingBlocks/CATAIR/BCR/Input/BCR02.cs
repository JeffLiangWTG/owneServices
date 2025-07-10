namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("02")]
	public abstract partial class BCR02 : MessageBlock // Need to add interface for BIRD System
	{
		public BCR02()
			: base("02")
		{
		}

		/// <summary>
		/// An International Organization for Standardization (ISO) country code representing the country of origin. Valid ISO codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 3, "M")]
		public ZString CountryOfOrigin;

		/// <summary>
		/// A code listed in the Harmonized Tariff Schedule of the United States Annotated (HTS) representing the tariff number.
		/// </summary>
		[MessageBlockString(10, 5, "M")]
		public ZString TariffNumber;

		/// <summary>
		/// A code identifying the manufacturer. For information on determining the manufacturer code, refer to CBP Directive 3500-13, dated November 24, 1986.
		/// </summary>
		[MessageBlockString(16, 15, "M")]
		public ZString ManufacturerIDCode;

		/// <summary>
		/// A code identifying the ultimate consignee.
		/// </summary>
		[MessageBlockString(12, 31, "C")]
		public ZString UltimateConsignee;

		/// <summary>
		/// The line item value in whole dollars.
		/// </summary>
		[MessageBlockDecimal(10, 43, "O", 0)]
		public ZDecimal LineItemValue;
	}
}
