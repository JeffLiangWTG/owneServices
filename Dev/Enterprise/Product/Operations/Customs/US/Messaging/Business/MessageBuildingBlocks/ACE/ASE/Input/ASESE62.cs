namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("SE62")]
	public partial class ASESE62 : MessageBlock
	{
		public ASESE62()
			: base("SE62")
		{
		}

		/// <summary>
		/// Two-digit number that groups related fields together. Record ID is unique to the grouping.
		/// </summary>
		[MessageBlockString(2, 5, "M")]
		public ZString RecordID;

		/// <summary>
		/// Value that correlates with the type of information the record relates to (e.g., Fishing Info, Mining Info etc.).
		/// </summary>
		[MessageBlockString(10, 7, "M")]
		public ZString RecordType;

		/// <summary>
		/// The name of the field for which a record is being provided (e.g., Method of Harvest, Vessel Name, Country of Harvest etc.).
		/// </summary>
		[MessageBlockString(64, 17, "M")]
		public ZString FieldName;
	}
}
