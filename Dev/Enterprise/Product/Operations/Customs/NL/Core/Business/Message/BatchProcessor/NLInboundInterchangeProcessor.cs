using System;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class NLInboundInterchangeProcessor : InboundInterchangeProcessor
{
	public NLInboundInterchangeProcessor(LoggingInformation logger)
		: base(logger)
	{
	}

	protected override string[] ApplicationCodes
	{
		get { return new string[] { EDIMessage.ApplicationCodes.NLCustoms }; }
	}

	protected override Type TypeOfInterchangeToCreate()
	{
		return typeof(EDIInterchange);
	}

	protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
	{
		return messageCreator ?? (messageCreator = new InboundMessageCreator());
	}
	IInboundMessageCreator messageCreator;

	protected override bool IsNoBranchFilter => true;
	protected override bool SupportEnvironmentSwitch => true;
}
