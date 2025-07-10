using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.Business;

public sealed class InboundInterchangeProcessorLegacy() : BranchInboundInterchangeProcessor(SupportedApplicationCodes)
{
	[ThreadSafe]
	public static readonly string[] SupportedApplicationCodes = [ApplicationCodeList.Codes.PLCustoms, ApplicationCodeList.Codes.PLCustomsNCTS, ApplicationCodeList.Codes.PLCustomsExitControl];

	protected override bool AddNoteOnException => true;

	protected override ZString GetStatusForProcessFailure(EDIInterchange interchange)
		=> interchange.EI_Status == EDIInterchange.Status.Error ? EDIInterchange.Status.Error : EDIInterchange.Status.Failed;

	protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		=> messagesCreator ??= new InboundMessageCreatorLegacy(Logger);
	InboundMessageCreatorLegacy messagesCreator;
}
