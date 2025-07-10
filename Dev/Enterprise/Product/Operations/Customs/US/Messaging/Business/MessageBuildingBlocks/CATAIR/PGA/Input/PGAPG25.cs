namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	// duplicate record in document
	[InputBlock("PG25")]
	public abstract partial class PGAPG25 : MessageBlock // Need to add interface for BIRD System
	{
		public PGAPG25()
			: base("PG25")
		{
		}

		/// <summary>
		/// Product Temperature Category
		/// 
		/// A= Ambient, F=Frozen
		/// R=Refrigerated, D=Dried Ice
		/// H=Fresh
		/// </summary>
		[MessageBlockString(1, 5, "C")]
		public ZString StorageTemperatureQualifier;

		/// <summary>
		/// F = Fahrenheit, C = Celsius , K = Kelvin
		/// </summary>
		[MessageBlockString(1, 6, "C")]
		public ZString DegreeType;

		/// <summary>
		/// If the actual temperature is in the negative numbers use an “X”
		/// </summary>
		[MessageBlockString(1, 7, "C")]
		public ZString NegativeNumber;

		/// <summary>
		/// Reported temperature of product being imported. Two decimals places are implied.
		/// </summary>
		[MessageBlockDecimal(6, 8, "C", 2)]
		public ZDecimal ActualTemperature;

		/// <summary>
		/// Identifies recorded temperature is for 
		/// 
		/// A = product 
		/// B = container
		/// C = conveyance
		/// </summary>
		[MessageBlockString(1, 14, "C")]
		public ZString StorageTypeLocationOfTemperatureRecording;

		/// <summary>
		/// The lot number that the manufacturer/producer/grower assigned to the product.
		/// </summary>
		[MessageBlockString(25, 15, "C")]
		public ZString LotNumber;

		/// <summary>
		/// The start date and end date of the LOT. The date can either be “MMDDYY” (month, day year), or “MMDDYYMMDDYY” (month, day, year, month, day year) format.
		/// </summary>
		[MessageBlockString(12, 40, "C")]
		public ZString ProductionDateRangeOfTheProduct; // needs to be a string

		/// <summary>
		/// The value of the PGA line in whole dollars
		/// </summary>
		[MessageBlockDecimal(12, 52, "C", 0)]
		public ZDecimal PGALineValue;

		/// <summary>
		/// The value of the lowest unit of measure reported.
		/// </summary>
		[MessageBlockDecimal(12, 64, "C", 0)]
		public ZDecimal PGAUnitValue;
	}
}
