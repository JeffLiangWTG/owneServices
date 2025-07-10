namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("D20")]
	public partial class NDDD20 : MessageBlock
	{
		public NDDD20()
			: base("D20")
		{
		}

		/// <summary>
		/// The number assigned to the NAFTA country's export entry.
		/// </summary>
		[MessageBlockString(14, 4, "M")]
		public ZString NAFTACountryEntryNumber;

		/// <summary>
		/// A date in MMDDYY (month, date, and year) format representing the entry date of the NAFTA countries exports entry. Must be greater than or equal to 010196 and less than or equal to the system (transmission) date.
		/// </summary>
		[MessageBlockDate(18, "M", "MMddyy")]
		public ZDate NAFTACountryEntryDate;

		/// <summary>
		/// A code representing the NAFTA country tariff number. For Mexico only, when the HS number reported is at the 6 or 8-digit level, left justify the field and space fill the remaining positions with zeros (0).
		/// </summary>
		[MessageBlockString(10, 24, "M")]
		public ZString NAFTACountryHarmonizedTariffNumber;

		/// <summary>
		/// A value representing the Canadian or Mexican duty rate associated with the Canadian or Mexican tariff number.
		/// </summary>
		[MessageBlockDecimal(12, 34, "M", 2)]
		public ZDecimal NAFTACountryDutyRate;

		/// <summary>
		/// A value representing the paid duty.
		/// </summary>
		[MessageBlockDecimal(12, 46, "M", 2)]
		public ZDecimal NAFTACountryPaidDutyInNAFTACountryCurrency;

		/// <summary>
		/// A value representing the NAFTA country paid duty in U.S. dollars for the associated tariff.
		/// </summary>
		[MessageBlockDecimal(12, 58, "M", 2)]
		public ZDecimal NAFTACountryPaidDutyInUSDollars;
	}
}
