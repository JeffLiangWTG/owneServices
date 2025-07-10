namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipBXP : AESCommShipBXPBase
	{
	}

	[InputBlock("B")]
	public abstract class AESCommShipBXPBase : MessageBlock
	{
		protected AESCommShipBXPBase()
			: base("B")
		{
		}

		/// <summary>
		/// Identifier of the US Principal Party in Interest (USPPI) for the enclosed shipments within this block.
		/// </summary>
		[MessageBlockString(11, 4, "M")]
		public ZString USPPIID;

		/// <summary>
		/// Type of USPPI ID reported: 
		/// D = DUNS 
		/// S = SSN 
		/// E = EIN
		/// T = Foreign Entity
		/// </summary>
		[MessageBlockString(1, 15, "M")]
		public ZString USPPIIDType;

		/// <summary>
		/// Name of the USPPI for the enclosed commodity shipment transactions within this block
		/// </summary>
		[MessageBlockString(30, 26, "M")]
		public ZString USPPIName;
	}
}
