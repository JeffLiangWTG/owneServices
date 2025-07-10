namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("W")]
	public partial class HTSW56789ABCEFGHIJK : MessageBlock
	{
		public HTSW56789ABCEFGHIJK() : base("W")
		{
		}

		[MessageBlockString(1, 2, "M")]
		/// <summary>
		/// The original element that the duty rate was received upon through the reference file update (5-C) inclusive.
		/// </summary>
		public ZString DutyElement;

		/// <summary>
		/// A code located in the Harmonized Tariff Schedule of the United States Annotated (HTS) representing the tariff number. If this number is less than 10 positions, it is left justified. This number is the same as that reported in Record Identifier W1.
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString TariffNumber;

		/// <summary>
		/// A code representing the country. Valid ISO country codes are listed in Appendix B. E followed by a space (Caribbean Basin Initiative), and J followed by a space (Andian Trade Preference Act), and R followed by a space (Caribbean Trade Partnership Act), are also valid codes for special rates. Countries eligible for E and J are indicated in the ACS country code file and the Harmonized Tariff Schedule of the United States - Annotated (HTS).
		/// </summary>
		[MessageBlockString(2, 13, "C")]
		public ZString InternationalOrganizationForStandardizationISOCountryCode1;

		/// <summary>
		/// The specific rate of duty listed in the Special column in the HTS. Eight decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(12, 15, "C", 8)]
		public ZDecimal SpecificSpecialRate1;

		/// <summary>
		/// The ad valorem rate of duty listed in the Special column in the HTS. Eight decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(12, 27, "C", 8)]
		public ZDecimal AdValoremSpecialRate1;

		/// <summary>
		/// The rate of duty listed in the Special column in the HTS that is not a specific or ad valorem rate. Eight decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(12, 39, "C", 8)]
		public ZDecimal OtherSpecialRate1;

		/// <summary>
		/// A code representing the tax/fee class. Valid Tax/Fee Class Codes are listed in Appendix B.
		/// </summary>
		[MessageBlockString(3, 51, "C")]
		public ZString TaxFeeClassCode1;

		/// <summary>
		/// A code representing the tax/fee computation formula. Valid Tax/Fee Computation Codes are listed in Appendix F.
		/// </summary>
		[MessageBlockString(1, 54, "C")]
		public ZString TaxFeeComputationCode1;

		/// <summary>
		/// A code that indicates if a tax/fee is required. Valid Tax/Fee Flag Codes are:
		/// 
		/// 1 = Tax/fee required
		/// 2 = Tax/fee may be required
		/// </summary>
		[MessageBlockString(1, 55, "C")]
		public ZString TaxFeeFlag1;

		/// <summary>
		/// The specific rate of duty required to compute taxes and/or fees. Eight decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(12, 56, "C", 8)]
		public ZDecimal TaxFeeSpecificRate1;

		/// <summary>
		/// The ad valorem rate of duty required to compute taxes and/or fees. Eight decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(12, 68, "C", 8)]
		public ZDecimal TaxFeeAdValorem1;
	}
}
