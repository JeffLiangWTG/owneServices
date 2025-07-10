namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("41")]
	[OutputBlock("41")]
	public abstract partial class AENS41 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS41()
			: base("41")
		{
		}

		/// <summary>
		/// An indication as to the 'status' of the FTZ merchandise:
		/// 
		/// P = Privileged Foreign
		/// N = Non-Privileged Foreign
		/// D = Domestic
		/// Z = Zone Restricted
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString FTZMerchandiseStatusCode;

		/// <summary>
		/// For Privileged Foreign (i.e., that merchandise that has not been manipulated or manufactured so as to effect a change in tariff), the date the merchandise entered the zone. 
		/// 
		/// Space fill if NOT Privileged Foreign. See Usage Note '(j) Basic Article Classification and Tariff Considerations - Duty Rate Date Matrix Hierarchy'.
		/// </summary>
		[MessageBlockDate(4, "C", "MMddyy")]
		public ZDate PrivilegedFTZMerchandiseFilingDate;

		/// <summary>
		/// Quantity of units removed from the FTZ and entered into the commerce of the U.S.  
		/// 
		/// Report a value greater than zero in whole units.
		/// </summary>
		[MessageBlockInt(10, 10, "M")]
		public ZInt FTZLineItemQuantity;
	}
}
