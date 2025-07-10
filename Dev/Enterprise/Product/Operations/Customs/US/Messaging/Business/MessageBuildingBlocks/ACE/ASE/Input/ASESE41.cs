namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE41")]
	public abstract partial class ASESE41 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE41()
			: base("SE41")
		{
		}

		/// <summary>
		/// Code indicating FTZ status. This field is required for entry type 06 (FTZ) entries.
		/// 
		/// Code P = Privileged Foreign
		/// Code N = Non-privileged Foreign
		/// </summary>
		[MessageBlockString(1, 5, "C")]
		public ZString ZoneStatus;

		/// <summary>
		/// *(See Note 3 for use guidance)
		/// 
		/// For Privileged Foreign status, (i.e., that merchandise that has not been manipulated or manufactured so as to effect a change in tariff), a numeric date in MMDDYY (month, day, year) format representing the date the merchandise was granted Privileged Foreign Status.
		/// 
		/// Space fill if NOT Privileged Foreign.
		/// 
		/// Not for use with HTS numbers which are currently active.
		/// </summary>
		[MessageBlockDate(6, "C", "MMddyy")]
		public ZDate PrivilegedFTZMerchandiseFilingDate;

		/// <summary>
		/// Enter the quantity in units of this HTS Line (SE40 record) to be removed from the FTZ and entered into the Commerce of the U.S.
		/// 
		/// Quantity entered must be a whole number.
		/// Quantity entered must be greater than Zero.
		/// </summary>
		[MessageBlockInt(8, 12, "M")]
		public ZInt FTZLineItemQuantity;
	}
}
