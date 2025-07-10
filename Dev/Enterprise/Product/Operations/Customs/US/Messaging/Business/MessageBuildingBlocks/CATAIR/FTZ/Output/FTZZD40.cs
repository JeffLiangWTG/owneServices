namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("40")]
	public partial class FTZZD40 : MessageBlock
	{
		public FTZZD40()
			: base("40")
		{
		}

		/// <summary>
		/// Minimum of 6, maximum of 35 positions. If importing carrier is automated with CBP, the bill of lading or air waybill will be validated against CBP database and input will be rejected if BOL not on file.
		/// </summary>
		[MessageBlockString(35, 3, "M")]
		public ZString BillOfLadingOrAirWaybillOfLading;

		/// <summary>
		/// House Bill Number issued by a consolidator or NVOCC.
		/// </summary>
		[MessageBlockString(20, 38, "O")]
		public ZString HouseBill;

		/// <summary>
		/// Whole number representing smallest exterior packing unit. These fields are not required for Admission Types ‘O', ‘C', ‘D', ‘Z', ‘T'.
		/// </summary>
		[MessageBlockDecimal(10, 58, "C", 0)]
		public ZDecimal Quantity;

		/// <summary>
		/// ISO Code of country from where the goods were exported to the U.S. These fields are not required for Admission Types ‘O', ‘C', ‘D', ‘Z', ‘T'.
		/// </summary>
		[MessageBlockString(2, 68, "C")]
		public ZString CountryOfExport;

		/// <summary>
		/// Census Schedule K code of port where goods loaded for transport by water to the United States. These fields are not required for Admission Types ‘O', ‘C', ‘D', ‘Z', ‘T'.
		/// </summary>
		[MessageBlockString(5, 70, "C")]
		public ZString ForeignLoadPort;

		/// <summary>
		/// FIRMS identifier of location goods moving on PTT are to be delivered. FIRMS required when requesting a PTT on Direct Delivery.
		/// </summary>
		[MessageBlockString(4, 75, "C")]
		public ZString FIRMSIdentifier;
	}

	[OutputBlock("40", "01")]
	public partial class FTZZD40_01 : MessageBlock
	{
		public FTZZD40_01()
			: base("40")
		{
		}

		/// <summary>
		/// Bill of lading or Air Waybill number.
		/// </summary>
		[MessageBlockString(35, 3, "M")]
		public ZString BillOfLadingOrAirWaybillOfLading;

		/// <summary>
		/// House Bill Number issued by a consolidator or NVOCC.
		/// </summary>
		[MessageBlockString(20, 38, "O")]
		public ZString HouseBill;

		/// <summary>
		/// Whole number representing smallest exterior packing unit. This field is not populated for Admission Types 'O', 'C', 'D', 'Z', 'T'.
		/// </summary>
		[MessageBlockDecimal(10, 58, "C", 0)]
		public ZDecimal Quantity;

		/// <summary>
		/// ISO Code of country from where the goods were exported to the U.S. This field is not populated for Admission Types 'O', 'C', 'D', 'Z', 'T'.
		/// </summary>
		[MessageBlockString(2, 68, "C")]
		public ZString CountryOfExport;

		/// <summary>
		/// Census Schedule K code of port where goods loaded for transport by water to the United States. This field is not populated for Admission Types 'O', 'C', 'D', 'Z', 'T'.
		/// </summary>
		[MessageBlockString(5, 70, "C")]
		public ZString ForeignLoadPort;
	}
}
