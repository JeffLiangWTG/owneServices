namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("W3")]
	public partial class HTSW3 : MessageBlock
	{
		public HTSW3()
			: base("W3")
		{
		}

		/// <summary>
		/// A code located in the Harmonized Tariff Schedule of the United States Annotated (HTS) representing the tariff number. If this number is less than 10 positions, it is left justified. This number is the same as that reported in Record Identifier W1.
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString TariffNumber;

		/// <summary>
		/// The International Organization for Standardization (ISO) country code that indicates countries not eligible for preferential treatment under the Generalized System of Preferences (GSP). Up to 10 2-position country codes can be reported.
		/// </summary>
		[MessageBlockString(20, 13, "C")]
		public ZString GeneralizedSystemOfPreferencesGSPExcludedCountries;

		/// <summary>
		/// A code of 1 indicates the tariff number is subject to an antidumping duty; otherwise, space fill.
		/// </summary>
		[MessageBlockString(1, 48, "C")]
		public ZString AntidumpingDutyFlag;

		/// <summary>
		/// A code of 1 indicates the tariff number is subject to quota; otherwise, it is space filled.
		/// </summary>
		[MessageBlockString(1, 49, "C")]
		public ZString QuotaIndicator;

		/// <summary>
		/// A code indicating the textile category number assigned to the tariff number. If there is no textile category number, space fill.
		/// </summary>
		[MessageBlockString(3, 50, "C")]
		public ZString CategoryNumber;

		/// <summary>
		/// A code indicating if a tariff number is subject to a special program. Up to fourteen 2-position codes can be reported. The SPI codes are not reported in any particular order. If more than fourteen 2-position codes are required, they are reported on WD record. Refer to Record Identifier V3, Note 2 of this chapter for valid codes.
		/// </summary>
		[MessageBlockString(28, 53, "C")]
		public ZString SpecialProgramIndicatorSPICode;
	}
}
