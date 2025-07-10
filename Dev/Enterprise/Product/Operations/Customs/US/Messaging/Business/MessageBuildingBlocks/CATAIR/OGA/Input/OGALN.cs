namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("FDLN")]
	public partial class OGALN : MessageBlock
	{
		public OGALN()
			: base("FDLN")
		{
		}

		/// <summary>
		/// The CBP line item number identifying the other government agency item.
		/// </summary>
		[MessageBlockInt(3, 5, "M")]
		public ZInt CBPLine;

		/// <summary>
		/// The tariff number associated with the line item number. This number must match the tariff number that was originally filed and must be sequenced in the exact order within the entry as originally transmitted when certified for cargo release.
		/// </summary>
		[MessageBlockString(10, 8, "M")]
		public ZString TariffNumber;
	}
}
