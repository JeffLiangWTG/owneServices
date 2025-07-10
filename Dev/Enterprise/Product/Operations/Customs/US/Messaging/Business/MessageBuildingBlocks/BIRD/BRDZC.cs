using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("ZC")]
	[OutputBlock("ZC")]
	public abstract partial class BRDZC : MessageBlock // Need to add interface for BIRD System
	{
		public BRDZC()
			: base("ZC")
		{
		}

		[MessageBlockString(14, 3, "C")]
		public ZString ContainerNumber;

		/// <summary>
		/// If entered, it should be a size such as 20, 35, 40, 45 or 48
		/// ISO equipment codes are allowed as well.
		/// </summary>
		[MessageBlockString(4, 17, "O")]
		public ZString ContainerType;

		[MessageBlockString(14, 22, "C")]
		public ZString ContainerNumber2;

		/// <summary>
		/// If entered, it should be a size such as 20, 35, 40, 45 or 48
		/// ISO equipment codes are allowed as well.
		/// </summary>
		[MessageBlockString(4, 36, "O")]
		public ZString ContainerType2;

		[MessageBlockString(14, 40, "C")]
		public ZString ContainerNumber3;

		/// <summary>
		/// If entered, it should be a size such as 20, 35, 40, 45 or 48
		/// ISO equipment codes are allowed as well.
		/// </summary>
		[MessageBlockString(4, 54, "O")]
		public ZString ContainerType3;

		[MessageBlockString(14, 58, "C")]
		public ZString ContainerNumber4;

		/// <summary>
		/// If entered, it should be a size such as 20, 35, 40, 45 or 48
		/// ISO equipment codes are allowed as well.
		/// </summary>
		[MessageBlockString(4, 72, "O")]
		public ZString ContainerType4;
	}
}