namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("40")]
	public partial class FTZFT40 : MessageBlock
	{
		public FTZFT40()
			: base("40")
		{
		}

		/// <summary>
		/// Minimum of 6, maximum of 35 positions. If importing carrier is automated with CBP, the bill of lading or Master/Simple air waybill will be validated against CBP database and input will be rejected if bill of lading is not on file or a match.
		/// </summary>
		[MessageBlockString(35, 3, "M")]
		public ZString BillOfLadingOrAirWaybill;

		/// <summary>
		/// House Bill data must be submitted as a minimum of 12AN. As needed, pad the submitted data value with leading zeros to achieve an input value with a minimum length of 12AN.
		/// Left justified.
		/// This field is Mandatory for air shipments involving a manifested Master/House combination.
		/// Space Fill if the Mode of Transportation in the immediately preceding FT20 Record is other than Air (MOT = 40 or 41).
		/// </summary>
		[MessageBlockString(20, 38, "C")]
		public ZString HouseBill;

		/// <summary>
		/// Enter the quantity associated with the lowest level bill being reported in this FT40 Record. It is the smallest exterior packaging unit.
		/// This field is not required if Admission Type is 'O', 'C', 'D', 'T', unless an In-Bond is involved [Admission Types 'D' and 'T'] in the movement of the shipment to the Port of Entry/Admission.
		/// If an In-Bond is being reported in the FT41 Record immediately subsequent to this record, the Quantity is required regardless of the admission type.
		/// </summary>
		[MessageBlockDecimal(10, 58, "C", 0)]
		public ZDecimal Quantity;

		/// <summary>
		/// ISO Code of country from where the goods were exported to the U.S. These fields are not required if Admission Type is ‘O', ‘C', ‘D', ‘Z', ‘T'.
		/// </summary>
		[MessageBlockString(2, 68, "C")]
		public ZString CountryOfExport;

		/// <summary>
		/// Census Schedule K code of foreign port where the goods were loaded. Only transmitted for MOT 10 or 11. These fields are not required if Admission Type is ‘O', ‘C', ‘D', ‘Z', ‘T'.
		/// </summary>
		[MessageBlockString(5, 70, "C")]
		public ZString ForeignLoadPort;

		/// <summary>
		/// FIRMS Code representing the Admission FTZ Site location. FIRMS Code is Mandatory when including an ePTT request with the filing of the FT transaction.
		/// </summary>
		[MessageBlockString(4, 75, "C")]
		public ZString FIRMSIdentifier;
	}

	[InputBlock("40", "01")]
	public partial class FTZFT40_01 : MessageBlock
	{
		public FTZFT40_01()
			: base("40")
		{
		}

		/// <summary>
		/// Minimum of 6, maximum of 35 positions. If importing carrier is automated with CBP, the bill of lading or Master/Simple air waybill will be validated against CBP database and input will be rejected if bill of lading is not on file or a match.
		/// </summary>
		[MessageBlockString(35, 3, "M")]
		public ZString BillOfLadingOrAirWaybill;

		/// <summary>
		/// House Bill data must be submitted as a minimum of 1AN.
		/// Left justified.
		/// This field is Mandatory for air shipments involving a manifested Master/House combination.
		/// Space Fill if the Mode of Transportation in the immediately preceding FT20 Record is other than Air (MOT = 40 or 41).
		/// </summary>
		[MessageBlockString(20, 38, "C")]
		public ZString HouseBill;

		/// <summary>
		/// Enter the quantity associated with the lowest level bill being reported in this FT40 Record. It is the smallest exterior packaging unit.
		/// Space fill if Admission Type = O, C, D, or T, unless an In-Bond is involved [Admission Type = D or T] in the movement of the shipment to the Port of Entry/Admission.
		/// If an In-Bond is being reported in the FT41 Record immediately subsequent to this record, the Quantity is required regardless of the admission type.
		/// </summary>
		[MessageBlockDecimal(10, 58, "C", 0)]
		public ZDecimal Quantity;

		/// <summary>
		/// ISO Code of country from where the goods were exported to the U.S.
		/// Space fill if Admission Type in FT10 = O, C, D, T or Z.
		/// </summary>
		[MessageBlockString(2, 68, "C")]
		public ZString CountryOfExport;

		/// <summary>
		/// Census Schedule K code of foreign port where the goods were loaded. Only transmitted for MOT 10 or 11.
		/// Space fill if Admission Type in FT10 = O, C, D, T or Z.
		/// </summary>
		[MessageBlockString(5, 70, "C")]
		public ZString ForeignLoadPort;
	}
}
