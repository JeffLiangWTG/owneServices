namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipEV1XP : AESCommShipEV1XPBase
	{
	}

	[InputBlock("EV1")]
	public abstract class AESCommShipEV1XPBase : MessageBlock
	{
		protected AESCommShipEV1XPBase()
			: base("EV1")
		{
		}

		/// <summary>
		/// Identification of the reported used vehicle. Report a 'Product ID' number for used selfpropelled vehicles that do not have a VIN.
		/// </summary>
		[MessageBlockString(25, 4, "M")]
		public ZString VehicleIdentificationNumberVINProductID;

		/// <summary>
		/// Type of used vehicle number reported:
		/// V = VIN
		/// P = Product ID
		/// </summary>
		[MessageBlockString(1, 29, "M")]
		public ZString VehicleIDQualifier;

		/// <summary>
		/// Title number as issued by a Motor Vehicle Administration (MVA).
		/// 
		/// Left justify; space fill. Space fill if NOT required.
		/// </summary>
		[MessageBlockString(15, 30, "C")]
		public ZString VehicleTitleNumber;

		/// <summary>
		/// U.S. State code of the Motor Vehicle Administration (MVA) that issued the title.
		/// 
		/// Report a valid USPS State code.
		/// Report 'US' for diplomatic vehicle.
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(2, 45, "C")]
		public ZString VehicleTitleStateCode;
	}
}
