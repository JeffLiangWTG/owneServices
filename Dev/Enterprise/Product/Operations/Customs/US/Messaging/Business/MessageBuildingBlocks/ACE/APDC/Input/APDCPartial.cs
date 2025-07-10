
namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("CA10")]
	public partial class APDCCA10 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("CA40")]
	public partial class APDCCA40 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("CA60")]
	public partial class APDCCA60 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("CA61")]
	public partial class APDCCA61 : MessageBlock
	{
	}
}
