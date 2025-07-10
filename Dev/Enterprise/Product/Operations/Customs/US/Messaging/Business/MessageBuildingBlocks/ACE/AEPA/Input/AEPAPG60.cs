namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("PG60")]
	public partial class AEPAPG60 : MessageBlock
	{
		public AEPAPG60()
			: base("PG60")
		{
		}

		/// <summary>
		/// Code indicating the type of additional information being provided
		/// </summary>
		[MessageBlockString(3, 5, "C")]
		public ZString AdditionalInformationQualifierCode;

		/// <summary>
		/// Text of the additional information related to the additional reference qualifier code
		/// </summary>
		[MessageBlockString(72, 8, "C")]
		public ZString AdditionalInformation;
	}
}
