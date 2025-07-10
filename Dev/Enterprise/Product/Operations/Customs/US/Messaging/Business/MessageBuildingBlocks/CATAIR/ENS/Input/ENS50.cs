namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("50")]
	public abstract partial class ENS50 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS50()
			: base("50")
		{
		}

		/// <summary>
		/// A code representing the primary special programs indicator. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockString(1, 3, "C")]
		public ZString SpecialProgramsIndicatorPrimary;

		/// <summary>
		/// A code located in the Harmonized Tariff Schedule of the United States Annotated (HTS) representing the tariff number.
		/// </summary>
		[MessageBlockString(10, 4, "M")]
		public ZString TariffNumber1;

		/// <summary>
		/// A value representing the duty associated with the first tariff number. If two duty rates are required (i.e., specific and ad valorem), combine the two amounts. Two decimal places are implied. If the value is a whole number, the two low-order (cents) positions contain zeros. If a zero duty is valid, enter zeros or space fill.
		/// </summary>
		[MessageBlockDecimal(10, 14, "C", 2)]
		public ZDecimal Duty;

		/// <summary>
		/// A value representing the first or only quantity associated with the first tariff number. Two decimal places are implied. If the quantity is a whole number, the two low-order positions contain zeros. A whole number with the two low-order positions containing zeros is mandatory when a category number is associated with the tariff number. Report quantities in whole numbers for statistical purposes unless fractions of units are required for other CBP purposes (e.g., distilled spirits). When expressing fractions, use only decimals.
		/// </summary>
		[MessageBlockDecimal(12, 24, "C", 2)]
		public ZDecimal Quantity1;

		/// <summary>
		/// A code representing the first or only unit of measure associated with the first tariff number. If there is more than one unit of measure associated with a tariff number, report the first unit. The unit of measure must match exactly the unit of measure as listed in the Harmonized Tariff Schedule (HTS) for the tariff number shown in positions 4-13 of this record.
		/// </summary>
		[MessageBlockString(3, 36, "C")]
		public ZString UnitOfMeasure1;

		/// <summary>
		/// A value representing the second quantity associated with the first tariff number. Two decimal places are implied. If the quantity is a whole number, the two low-order positions contain zeros. For information on goods requiring more than one quantity, refer to Appendix F of this publication. Report quantities in whole numbers for statistical purposes unless fractions of units are required for other CBP purposes (e.g., distilled spirits). When expressing fractions, use only decimals.
		/// </summary>
		[MessageBlockDecimal(12, 39, "C", 2)]
		public ZDecimal Quantity2;

		/// <summary>
		/// A code representing the second unit of measure associated with the first tariff number. The second unit of measure must match exactly the second unit of measure as listed in the HTS for the tariff number in positions 4-13 of this record.
		/// </summary>
		[MessageBlockString(3, 51, "C")]
		public ZString UnitOfMeasure2;

		/// <summary>
		/// A value representing the third quantity associated with the first tariff number. Two decimal places are implied. If the quantity is a whole number, the two low-order positions contain zeros. For information on goods requiring more than one quantity, refer to Appendix F of this publication.
		/// </summary>
		[MessageBlockDecimal(12, 54, "C", 2)]
		public ZDecimal Quantity3;

		/// <summary>
		/// A code representing the third unit of measure associated with the first tariff number. The third unit of measure must match exactly the third unit of measure as listed in the HTS for the tariff number in positions 4-13 of this record.
		/// </summary>
		[MessageBlockString(3, 66, "C")]
		public ZString UnitOfMeasure3;

		/// <summary>
		/// An International Organization for Standardization (ISO) country code representing the country of exportation. Valid ISO codes are listed in Appendix B of this publication. This data element is mandatory for all claims of GSP, AGOA and CBTPA regardless of entry type.
		/// </summary>
		[MessageBlockString(2, 69, "C")]
		public ZString CountryOfExport;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of exportation. If the entry summary is from a Foreign Trade Zone (FTZ), and it is non-quota, space fill. If the entry summary is from an FTZ and it is quota, then the date of exportation must be shown.
		/// </summary>
		[MessageBlockDate(71, "C", "MMddyy")]
		public ZDate DateOfExportation;

		/// <summary>
		/// A code indicating if the transaction is between related parties. If the transaction is between related parties as defined in 19 U.S.C. § 1401a(g)(1)(F), as amended, enter Y (yes); otherwise, enter N (no). If the entry type code is 11 (informal) or 12 (informal entry, quota, other than textiles), space fill.
		/// </summary>
		[MessageBlockString(1, 77, "C")]
		public ZString RelatedPartyIndicator;

		/// <summary>
		/// A code representing the program which allows the special tariff treatment. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockString(2, 78, "C")]
		public ZString SpecialProgramsIndicatorCountry;

		/// <summary>
		/// A code representing the secondary special programs indicator. Refer to Appendix B of this publication for valid codes.
		/// </summary>
		[MessageBlockString(1, 80, "C")]
		public ZString SpecialProgramsIndicatorSecondary;
	}
}
