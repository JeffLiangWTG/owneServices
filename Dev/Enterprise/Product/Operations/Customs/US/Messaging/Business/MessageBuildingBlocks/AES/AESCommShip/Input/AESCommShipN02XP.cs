namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipN02XP : AESCommShipN02XPBase
	{
	}

	[InputBlock("N02")]
	public abstract class AESCommShipN02XPBase : MessageBlock
	{
		protected AESCommShipN02XPBase()
			: base("N02")
		{
		}

		/// <summary>
		/// Mailing address (first line).
		/// </summary>
		[MessageBlockString(32, 4, "M")]
		public ZString AddressLine1;

		/// <summary>
		/// Mailing address (next line).
		/// </summary>
		[MessageBlockString(32, 36, "O")]
		public ZString AddressLine2;

		/// <summary>
		/// Company contact phone number.
		/// 
		/// If reported, left justify; NO leading spaces or imbedded dashes.
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(13, 68, "C")]
		public ZString ContactPhoneNumber;
	}
}
