namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("30")]
	[OutputBlock("30")]
	public abstract partial class AENS30 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS30()
			: base("30")
		{
		}

		/// <summary>
		/// Filer's identification code of the associated Warehouse or Re-Warehouse Entry.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString AssociatedWarehouseEntryFilerCode;

		/// <summary>
		/// Unique identifying number assigned to the associated Warehouse or Re-Warehouse Entry.
		/// </summary>
		[MessageBlockString(8, 8, "M")]
		public ZString AssociatedWarehouseEntryNumber;

		/// <summary>
		/// The code for the U.S. port that where associated Warehouse or Re-Warehouse Entry entered.
		/// </summary>
		[MessageBlockString(4, 17, "M")]
		public ZString AssociatedWarehouseEntryDistrictPortCode;

		/// <summary>
		/// An indication that the Warehouse Withdrawal is the final withdrawal for the Associated Warehouse or Re-Warehouse Entry.
		/// 
		/// Y = The final withdrawal.
		/// 
		/// Space fill if not the final withdrawal.
		/// </summary>
		[MessageBlockString(1, 21, "C")]
		public ZString FinalWarehouseWithdrawalIndicator;
	}
}
