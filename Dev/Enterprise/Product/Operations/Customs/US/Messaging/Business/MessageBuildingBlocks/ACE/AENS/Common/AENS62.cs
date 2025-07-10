namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("62")]
	[OutputBlock("62")]
	public abstract partial class AENS62 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS62()
			: base("62")
		{
		}

		/// <summary>
		/// CBP accounting classification code representing a specific fee type. 
		/// 
		/// See 'AE Table 6 - User Fee Accounting Class Codes' for a list those codes supported at the Entry Summary line level.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString AccountingClassCode;

		/// <summary>
		/// The applicable fee amount in U.S. dollars and cents. 
		/// 
		/// Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(8, 6, "M", 2)]
		public ZDecimal UserFeeAmount;
	}
}
