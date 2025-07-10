namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("40")]
	public abstract partial class ENS40 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS40()
			: base("40")
		{
		}

		/// <summary>
		/// A line item refers to a commodity from one country, covered by a line which includes a net quantity, entered value, tariff number, charges, and rate of duty and tax. However, a line item can require more than one tariff number and value. For additional information on a line item requiring more than one tariff number, refer to Appendix F of this publication. For each new CBPF-7501, the Line Item Number must begin with 001 and be incremented by one for each additional line item printed on the CBPF-7501. This facilitates locating errors on the CBPF-7501 when error messages relating to line item data are reported to the broker. Valid Line Item Numbers are 001 to 999.
		/// </summary>
		[MessageBlockInt(3, 3, "M")]
		public ZInt LineItemNumber;

		/// <summary>
		/// A code representing the International Organization for Standardization (ISO) Country Code. Valid ISO codes are listed in Appendix B of this publication. In cases where the country of origin is unknown, enter two asterisks (**) in lieu of the ISO Code. This will indicate that the country of origin is unknown and that the country of export will be used for statistical data collected by the Bureau of the Census.
		/// </summary>
		[MessageBlockString(2, 6, "M")]
		public ZString CountryOfOrigin;

		/// <summary>
		/// Enter the value of the line item in whole U.S. dollars.
		/// </summary>
		[MessageBlockDecimal(10, 8, "M", 0)]
		public ZDecimal Value;

		/// <summary>
		/// A gross weight of at least 0 (zero) kilograms is required for all modes of transportation.
		/// </summary>
		[MessageBlockDecimal(10, 18, "M", 0)]
		public ZDecimal GrossWeight;

		/// <summary>
		/// A value in whole U.S. dollars to be used exclusively and in lieu of any other line-item value to calculate ADD duty. If there is no value present, the ADD duty will be computed from the line-item value.
		/// </summary>
		[MessageBlockDecimal(10, 28, "C", 0)]
		public ZDecimal ADDSpecificDepositValue;

		/// <summary>
		/// A value in whole U.S. dollars to be used exclusively and in lieu of any other line-item value to calculate CVD duty. If there is no value present, the CVD duty will be computed from the line-item value.
		/// </summary>
		[MessageBlockDecimal(10, 38, "C", 0)]
		public ZDecimal CVDSpecificDepositValue;

		/// <summary>
		/// The charges in whole U.S. dollars for shipments arriving in the U.S. for all modes of transportation (MOT) codes except MOT code 60 (passenger, hand- carried).
		/// </summary>
		[MessageBlockDecimal(10, 48, "C", 0)]
		public ZDecimal Charges;

		/// <summary>
		/// A code representing the foreign port of lading. These codes may be queried through the Extract Reference File. For additional information, refer to the Extract Reference File chapter of this publication. This code is only reported when the MOT is 10, 11 or 12.
		/// </summary>
		[MessageBlockString(5, 58, "C")]
		public ZString PortOfLading;

		/// <summary>
		/// A code representing the status of the Foreign Trade Zone. Valid zone status codes are:
		/// 
		/// P = Privileged Foreign Merchandise
		/// D = Domestic Merchandise
		/// N = Non-privileged Foreign Status
		/// Z = Zone Restricted Merchandise
		/// 
		/// This code is mandatory if the entry type code is 06 (Foreign Trade Zone).
		/// </summary>
		[MessageBlockString(1, 63, "C")]
		public ZString ZoneStatus;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the privileged status filing date. This date is a present or past date but it cannot be in the future. This field is mandatory for all privileged foreign status entries; otherwise, space fill.
		/// </summary>
		[MessageBlockDate(64, "C", "MMddyy")]
		public ZDate PrivilegedStatusFilingDate;

		/// <summary>
		/// A code representing a Canadian softwood lumber permit. For merchandise entered under one of the following tariff number subheadings of 44071000, 44091010, 44091020, or 44091090 and the reporting country of origin is “XA”, “XC”, “XO”, or “XQ”, position 70 must contain either “A”, “B”, “C”, “D”, "R" or "S". If position 70 contains an “A”, “B”, “C”, "R" or "S" then positions 71 and 72 must contain zeros or spaces. If position 70 contains a “D”, then positions 71 and 72 are required to have a 2-digit numeric code that falls within the range of 01 through 20 depending on the calendar quarter during which the permit is issued. The softwood lumber code requirement for Canadian softwood lumber products expired on 04/01/2001. Refer to Administrative message 01-0336.
		/// </summary>
		[MessageBlockString(3, 70, "C")]
		public ZString SoftwoodLumber;

		/// <summary>
		/// If there is more than one invoice, this data field is required. If the line item on the entry summary is the last line of an invoice, enter the abbreviation INV followed by a three-position number to identify the pertinent invoice (for example, 001, 002, 003).
		/// </summary>
		[MessageBlockString(6, 73, "C")]
		public ZString InvoiceDelimiter;

		/// <summary>
		/// A code representing a North American Free Trade Agreement (NAFTA) Net Cost condition. Valid NAFTA Net Code Indicator codes are:
		/// 
		/// Y = NAFTA Net Code Condition
		/// Space Fill = Non-NAFTA Net Cost Condition
		/// </summary>
		[MessageBlockString(1, 79, "C")]
		public ZString NAFTANetCostIndicator;
	}
}
