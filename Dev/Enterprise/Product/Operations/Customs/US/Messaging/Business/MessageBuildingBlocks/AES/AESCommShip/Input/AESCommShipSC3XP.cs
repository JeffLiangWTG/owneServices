namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipSC3XP : AESCommShipSC3XPBase
	{
	}

	[InputBlock("SC3")]
	public abstract class AESCommShipSC3XPBase : MessageBlock
	{
		protected AESCommShipSC3XPBase()
			: base("SC3")
		{
		}

		/// <summary>
		/// The identity of the equipment or shipping container that contains the cargo. 
		/// 
		/// Left justify, no imbedded spaces, space fill. Space fill if NOT required.
		/// </summary>
		[MessageBlockString(14, 4, "C")]
		public ZString EquipmentNumber;

		/// <summary>
		/// The number from the Customs and Border Protection seal on a shipping container. 
		/// 
		/// Left justify, no imbedded spaces, space fill. Space fill if NOT required.
		/// </summary>
		[MessageBlockString(15, 18, "C")]
		public ZString SealNumber;

		/// <summary>
		/// A number referencing a transportation booking, waybill, ocean bill-of-lading, pro-bill, or master air waybill. 
		/// 
		/// Left justify, no imbedded spaces, space fill. Space fill if NOT required.
		/// </summary>
		[MessageBlockString(30, 33, "C")]
		public ZString TransportationReferenceNumber;
	}
}
