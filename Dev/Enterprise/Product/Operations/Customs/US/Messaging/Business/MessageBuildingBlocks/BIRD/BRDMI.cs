using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("MI")]
	[OutputBlock("MI")]
	public sealed partial class BRDMI : MessageBlock
	{
		public BRDMI()
			: base("MI")
		{
		}

		/// <summary>
		/// LOC = Location of Goods (FIRMS) Code
		/// </summary>
		[MessageBlockString(3, 3, "C")]
		public ZString CodeQualifier;

		[MessageBlockString(30, 6, "C")]
		public ZString Code;
	}
}
