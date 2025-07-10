namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("FC01")]
	public abstract partial class OGAFC01 : MessageBlock // Need to add interface for BIRD System
	{
		public OGAFC01()
			: base("FC01")
		{
		}

		/// <summary>
		/// A code (01-08) identifying the FCC import condition.
		/// </summary>
		[MessageBlockString(2, 5, "M")]
		public ZString ImportConditionNumber;

		/// <summary>
		/// A code of Y (yes) if the Import Condition Number is 03 and prior approval is received from the FCC for more than 200 items; otherwise, space fill.
		/// </summary>
		[MessageBlockString(1, 7, "C")]
		public ZString ImportConditionNumberQuantityApproval; // spec typo

		/// <summary>
		/// A code identifying the line number beginning with 001 within a CBP line and incremented by one for each subsequent FCC line number.
		/// </summary>
		[MessageBlockInt(3, 8, "M")]
		public ZInt FCCLineNumber;

		/// <summary>
		/// A code assigned by the FCC. This code is mandatory if the Import Condition Number is 01. Include hyphens and dashes.
		/// </summary>
		[MessageBlockString(17, 11, "C")]
		public ZString FCCIdentifier;

		/// <summary>
		/// The trading name of the product imported.
		/// </summary>
		[MessageBlockString(30, 28, "M")]
		public ZString TradeName;

		/// <summary>
		/// A code identifying the product imported.
		/// </summary>
		[MessageBlockString(17, 58, "M")]
		public ZString ModelTypeNumber;
	}
}
