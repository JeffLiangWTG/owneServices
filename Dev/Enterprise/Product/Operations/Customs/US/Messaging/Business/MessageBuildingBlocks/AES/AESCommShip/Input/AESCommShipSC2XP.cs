namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipSC2XP : AESCommShipSC2XPBase
	{
	}

	[InputBlock("SC2")]
	public abstract class AESCommShipSC2XPBase : MessageBlock
	{
		protected AESCommShipSC2XPBase()
			: base("SC2")
		{
		}

		/// <summary>
		/// An indication of whether the shipment is being 'transported under bond'.
		/// 36 = Warehouse withdrawal for IE
		/// 37 = Warehouse withdrawal for T&E 
		/// 67 = Foreign Trade Zone withdrawal for IE
		/// 68 = Foreign Trade Zone withdrawal for T&E
		/// 70 = Merchandise NOT shipped inbond
		/// </summary>
		[MessageBlockString(2, 4, "M")]
		public ZString InbondCode;

		/// <summary>
		/// The import entry number for a shipment 'transported under bond' or the import entry number when an FTZ NAFTA Deferred Duty claim is made. 
		/// 
		/// Left justify; NO trailing zeros; NO imbedded slashes or dashes. Space fill if NOT required.
		/// </summary>
		[MessageBlockString(15, 6, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString EntryNumber;

		/// <summary>
		/// Identity of the Foreign Trade Zone from which the merchandise was withdrawn. 
		/// The first 3 positions must be numeric and represent the general purpose zone. The next 2 positions are alphanumeric and represent the subzone. The last 2 positions are alphanumeric and represent the site. Left justify; NO trailing spaces. Insert zeros when there is no sub zone or site. Report leading zero(s) when the general purpose zone is less than 3 numerics, and when the subzone or site is 1 alphanumeric (See example in Note 4 below).
		/// </summary>
		[MessageBlockString(9, 21, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ForeignTradeZoneIdentifier;

		/// <summary>
		/// An indication that the foreign principal party in interest has authorized a U.S. forwarding or other agent to facilitate the export of the merchandise from the U.S.
		/// Y = Yes; shipment is a routed transaction.
		/// N = No; shipment is NOT a routed transaction
		/// </summary>
		[MessageBlockString(1, 41, "M")]
		public ZString RoutedExportTransactionIndicator;

		/// <summary>
		/// The Original ITN is the ITN associated with a previously filed shipment that is replaced or divided and for which additional shipment(s) must be filed.
		/// The original ITN field can be used in certain scenarios, such as, but not limited to, shipments sold en route or cargo
		/// </summary>
		[MessageBlockString(15, 42, "C")]
		public ZString OriginalITN;
	}
}
