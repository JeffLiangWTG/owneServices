namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("70")]
	public partial class INBQP70 : MessageBlock
	{
		public INBQP70()
			: base("70")
		{
		}

		/// <summary>
		/// The 10 character code located in the Harmonized Tariff Schedule of the United States Annotated which represents the tariff number or Harmonized Tariff Schedule B that represents the commodity export. The HTS number will be reported at a minimum of 6-positions. Left justify the number and fill any remaining positions with spaces.
		/// 
		/// On import manifests, the Harmonized Code is mandatory for in-bond entry types 62 (T&E) and 63 (IE).
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString HarmonizedNumber;

		/// <summary>
		/// A value greater than zero, in whole dollars, of the commodity. Twenty dollars per kilo may be used if the value is unknown. No decimals.
		/// </summary>
		[MessageBlockInt(8, 14, "M")] //whole value is required
		public ZInt Value;

		/// <summary>
		/// A value greater than zero representing the net weight in pounds or kilos of the commodity. No decimals.
		/// </summary>
		[MessageBlockDecimal(10, 22, "M", 0)]
		public ZDecimal Weight;

		/// <summary>
		/// A code representing the unit of measure. Valid codes are:
		/// 
		/// LB	=	Pounds
		/// KG	=	Kilograms 
		/// LT	=	Long Ton
		/// ST	=	Short Ton
		/// ET	=	Metric Ton
		/// MT	=	Measurement Ton
		/// </summary>
		[MessageBlockString(2, 32, "M")]
		public ZString WeightUnit;
	}
}
