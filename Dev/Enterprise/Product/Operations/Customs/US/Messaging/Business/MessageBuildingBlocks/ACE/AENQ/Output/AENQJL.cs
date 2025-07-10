namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("JL")]
	public partial class AENQJL : MessageBlock
	{
		public AENQJL()
			: base("JL")
		{
		}

		/// <summary>
		/// A numeric date in MMDDYY format representing the collection date.
		/// </summary>
		[MessageBlockDate(3, "M", "MMddyy")]
		public ZDate CollectionDate;

		/// <summary>
		/// A value representing the total amount.Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(11, 9, "M", 2, true)]
		public ZDecimal TotalAmount;
	}

	[OutputBlock("JL", "01")]
	public partial class AENQJL_01 : MessageBlock
	{
		public AENQJL_01()
			: base("JL")
		{
		}

		[MessageBlockString(27, 4, "M")]
		public ZString NarrativeText;
	}
}
