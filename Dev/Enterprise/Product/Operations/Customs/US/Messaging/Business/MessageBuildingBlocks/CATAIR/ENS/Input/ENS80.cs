namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("80")]
	public abstract partial class ENS80 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS80()
			: base("80")
		{
		}

		/// <summary>
		/// A code located in the Harmonized Tariff Schedule of the United States Annotated (HTS) representing the third tariff number.
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString TariffNumber3;

		/// <summary>
		/// A code representing the special programs indicator. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockString(1, 13, "C")]
		public ZString SpecialProgramsIndicatorSecondary;

		/// <summary>
		/// A value representing the duty associated with the third tariff number. If two duty rates are required (i.e., specific and ad valorem), combine the amounts. Two decimal places are implied. If the duty is a whole number, the two low-order (cents) positions contain zeros. If zero duty is valid, enter zeros or space fill.
		/// </summary>
		[MessageBlockDecimal(10, 14, "C", 2)]
		public ZDecimal Duty;

		/// <summary>
		/// A value representing the first or only quantity associated with the third tariff number. Two decimal places are implied. If the quantity is a whole number, the two low-order positions contain zeros. Report quantities in whole numbers for statistical purposes unless fractions of units are required for other CBP purposes (e.g., distilled spirits). When expressing fractions, use only decimals.
		/// </summary>
		[MessageBlockDecimal(12, 24, "C", 2)]
		public ZDecimal Quantity1;

		/// <summary>
		/// A code representing the first or only unit of measure associated with the third tariff number. The unit of measure must match exactly the unit of measure as listed in the HTS for the tariff number shown in positions 3-12 of this record.
		/// </summary>
		[MessageBlockString(3, 36, "C")]
		public ZString Unit1;

		/// <summary>
		/// A value representing the second quantity associated with the third tariff number. Two decimal places are implied. If the quantity is a whole number, the two low-order positions contain zeros. Report quantities in whole numbers for statistical purposes unless fractions of units are required for other CBP purposes (e.g., distilled spirits). When expressing fractions, use only decimals.
		/// </summary>
		[MessageBlockDecimal(12, 39, "C", 2)]
		public ZDecimal Quantity2;

		/// <summary>
		/// A code representing the second unit of measure associated with the third tariff number. The unit of measure must match exactly the unit of measure as listed in the HTS for the tariff number shown in positions 3-12 of this record.
		/// </summary>
		[MessageBlockString(3, 51, "C")]
		public ZString Unit2;

		/// <summary>
		/// A value representing the third quantity associated with the third tariff number. Two decimal places are implied. If the quantity is a whole number, the two low-order positions contain zeros. Report quantities in whole numbers for statistical purposes unless fractions of units are required for other CBP purposes (e.g., distilled spirits). When expressing fractions, use only decimals.
		/// </summary>
		[MessageBlockDecimal(12, 54, "C", 2)]
		public ZDecimal Quantity3;

		/// <summary>
		/// A code representing the third unit of measure associated with the third tariff number. The unit of measure must match exactly the unit of measure as listed in the HTS for the tariff number shown in positions 3-12 of this record.
		/// </summary>
		[MessageBlockString(3, 66, "C")]
		public ZString Unit3;

		/// <summary>
		/// The value of the goods classified under the third tariff number.
		/// </summary>
		[MessageBlockDecimal(10, 69, "C", 0)]
		public ZDecimal Value;

		/// <summary>
		/// A code representing either the primary or country special programs indicator. If there is both a primary and country special programs indicator code, enter only one. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockString(2, 79, "C")]
		public ZString SpecialProgramsIndicatorPrimaryOrCountry;
	}
}
