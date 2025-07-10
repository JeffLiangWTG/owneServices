using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("EN")]
	[OutputBlock("EN")]
	public sealed partial class BRDEN : MessageBlock
	{
		public BRDEN()
			: base("EN")
		{
		}

		/// <summary>
		/// Reference Number assigned by corresponding broker
		/// </summary>
		[MessageBlockString(20, 3, "C")]
		public ZString CorrespondingRefNumber;

		[MessageBlockString(3, 23, "C")]
		public ZString FilerCode;

		[MessageBlockString(9, 26, "C", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// FIRMS code
		/// </summary>
		[MessageBlockString(4, 35, "C")]
		public ZString LocationCode;
	}
}
