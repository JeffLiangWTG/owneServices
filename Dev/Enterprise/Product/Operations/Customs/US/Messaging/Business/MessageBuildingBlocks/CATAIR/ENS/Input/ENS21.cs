namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("21")]
	public abstract partial class ENS21 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS21()
			: base("21")
		{
		}

		/// <summary>
		/// Whole dollar bond amount.
		/// </summary>
		[MessageBlockDecimal(10, 3, "M", 0, FillType.AlwaysZeroFill)]
		public ZDecimal BondAmount;

		/// <summary>
		/// An identification number assigned by a surety company to a specific entry filer. The surety producer account number is a unique identifier that identifies a particular office of a broker or importer (entry filer) to a surety or the surety’s agent representative. It is needed particularly to differentiate when a filer has multiple offices and uses more than one surety/agent or surety/ agent’s office. When the surety/agent sends a bill or invoice to a filer’s office, this is the account number on that document. This number is typically printed on each single transaction bond by the filer’s system printer, or is manually typed on the bond.
		/// </summary>
		[MessageBlockString(10, 13, "M")]
		public ZString BondProducerAccountNumber;
	}
}
