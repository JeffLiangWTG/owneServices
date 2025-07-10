namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("V4")]
	public sealed partial class HTSV4 : MessageBlock
	{
		public HTSV4()
			: base("V4")
		{
		}

		/// <summary>
		/// A code located in the Harmonized Tariff Schedule of the United States Annotated (HTS) representing the tariff number. If this number is less than 10 positions, it is left justified. This number is the same as the number reported in Record Identifier V1.
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString TariffNumber;

		/// <summary>
		/// A code representing the value edit.
		/// </summary>
		[MessageBlockString(3, 13, "C")]
		public ZString ValueEditCode;

		/// <summary>
		/// A value representing the minimum value edit. Five decimal places are implied. If this record contains date edits (positions 36-53), space fill.
		/// </summary>
		[MessageBlockDecimal(10, 16, "C", 5)]
		public ZDecimal ValueLowBounds;

		/// <summary>
		/// A value representing the maximum value edit. Five decimal places are implied. If this record contains date edits (positions 36-53), space fill.
		/// </summary>
		[MessageBlockDecimal(10, 26, "C", 5)]
		public ZDecimal ValueHighBounds;

		/// <summary>
		/// A code representing the first entry date restriction code.
		/// </summary>
		[MessageBlockString(1, 36, "C")]
		public ZString EntryDateRestrictionCode1;

		/// <summary>
		/// A numeric date in MMDD (month and day) format representing the first begin restriction date used in the edit. If this record contains a value edit (positions 13-35), space fill.
		/// </summary>
		[MessageBlockShort(4, 37, "C")]
		public ZShort BeginRestrictionDate1;

		/// <summary>
		/// A numeric date in MMDD (month and day) format representing the first end restriction date used in the edit. If this record contains a value edit (positions 13-35), space fill.
		/// </summary>
		[MessageBlockShort(4, 41, "C")]
		public ZShort EndRestrictionDate1;

		/// <summary>
		/// A code representing the second entry date restriction code.
		/// </summary>
		[MessageBlockString(1, 45, "C")]
		public ZString EntryDateRestrictionCode2;

		/// <summary>
		/// A numeric date in MMDD (month and day) format representing the second begin restriction date used in the edit. If this record contains a value edit (positions 13-35), space fill.
		/// </summary>
		[MessageBlockShort(4, 46, "C")]
		public ZShort BeginRestrictionDate2;

		/// <summary>
		/// A numeric date in MMDD (month and day) format representing the second end restriction date used in the edit. If this record contains a value edit (positions 13-35), space fill.
		/// </summary>
		[MessageBlockShort(4, 50, "C")]
		public ZShort EndRestrictionDate2;

		/// <summary>
		/// A code representing the ISO country of origin edit code. This code is either an ISO country of origin code; a code of 01 indicating the country of origin is a country eligible for the general column 1 duty rate; or a code of 02 indicating the country of origin is a country listed in General Note 3(a)(iv)(b) of the HTS and is eligible for the column 2 duty rate. Valid ISO country codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 54, "C")]
		public ZString ISOCountryOfOriginEditCode;

		/// <summary>
		/// A code representing the quantity edit code.
		/// </summary>
		[MessageBlockString(3, 58, "C")]
		public ZString QuantityEditCode;

		/// <summary>
		/// A value representing the minimum (lowest) quantity edit. Five decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(10, 61, "C", 5)]
		public ZDecimal QuantityEditLowerBound;

		/// <summary>
		/// A value representing the maximum (highest) quantity edit. Five decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(10, 71, "C", 5)]
		public ZDecimal QuantityEditUpperBound;
	}
}
