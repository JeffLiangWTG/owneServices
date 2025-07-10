namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("PG25")]
	public partial class AEPAPG25 : MessageBlock
	{
		public AEPAPG25()
			: base("PG25")
		{
		}

		/// <summary>
		/// Temperature Category being reported.
		/// 
		/// A= Ambient, F=Frozen
		/// R=Refrigerated/Chilled, D=Dry Ice
		/// H=Fresh, U=Uncontrolled 
		/// P=Flashpoint
		/// </summary>
		[MessageBlockString(1, 5, "C")]
		public ZString TemperatureQualifier;

		/// <summary>
		/// F = Fahrenheit, C = Celsius , K = Kelvin
		/// </summary>
		[MessageBlockString(1, 6, "C")]
		public ZString DegreeType;

		/// <summary>
		/// If the actual temperature is in the negative numbers use an "X".
		/// </summary>
		[MessageBlockString(1, 7, "C")]
		public ZString NegativeNumber;

		/// <summary>
		/// Reported temperature. Two decimals places are implied.
		/// </summary>
		[MessageBlockString(6, 8, "C")]
		public ZString ActualTemperature;

		/// <summary>
		/// Identifies recorded temperature is for 
		/// 
		/// A = product 
		/// B = container
		/// C = conveyance
		/// </summary>
		[MessageBlockString(1, 14, "C")]
		public ZString LocationOfTemperatureRecording;

		/// <summary>
		/// Code of the entity that assigned the Lot number.
		/// 1 = Manufacturer
		/// 2 = Seller
		/// 3 = Grower
		/// 4 = Producer
		/// </summary>
		[MessageBlockString(1, 15, "C")]
		public ZString LotNumberQualifier;

		/// <summary>
		/// The lot number that the manufacturer/ producer/grower assigned to the product.
		/// </summary>
		[MessageBlockString(25, 16, "C")]
		public ZString LotNumber;

		/// <summary>
		/// The date when the production for the Lot started. A numeric date in MMDDCCYY (month, day, century, year) format.
		/// </summary>
		[MessageBlockDate(41, "C", "MMddyyyy")]
		public ZDate ProductionStartDateOfTheLot;

		/// <summary>
		/// The date when the production for the Lot ended. A numeric date in MMDDCCYY (month, day, century, year) format.
		/// </summary>
		[MessageBlockDate(49, "C", "MMddyyyy")]
		public ZDate ProductionEndDateOfTheLot;

		/// <summary>
		/// The value associated with the PGA line number in whole dollars.
		/// </summary>
		[MessageBlockDecimal(12, 57, "C", 0)]
		public ZDecimal PGALineValue;

		/// <summary>
		/// The value of the lowest unit of measure reported in PG26. Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(12, 69, "C", 2)]
		public ZDecimal PGAUnitValue;
	}
}
