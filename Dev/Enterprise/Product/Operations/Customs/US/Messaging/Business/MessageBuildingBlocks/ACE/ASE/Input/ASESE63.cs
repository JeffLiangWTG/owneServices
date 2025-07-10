namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("SE63")]
	public partial class ASESE63 : MessageBlock
	{
		public ASESE63()
			: base("SE63")
		{
		}

		/// <summary>
		/// This field is used to capture the actual value of the proceeding Field Name entered in the SE62 record.
		/// </summary>
		[MessageBlockString(76, 5, "M")]
		public ZString FieldValue;
	}
}
