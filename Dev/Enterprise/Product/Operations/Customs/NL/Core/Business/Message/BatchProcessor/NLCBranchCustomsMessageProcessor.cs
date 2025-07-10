using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Integration.Customs.NL;

namespace Enterprise.Customs.NL.Business;

public class NLCBranchCustomsMessageProcessor : BranchCustomsMessageProcessor
{
	public NLCBranchCustomsMessageProcessor() : base(new ZString[] { EDIInterchange.ApplicationCodes.NLCustoms }, null)
	{
	}

	public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
	{
		cachedProcessors = cachedProcessors ?? new Dictionary<string, BranchCustomsApplicationTypeMessageProcessor>();
		var messageType = message.EM_MessageType;
		var messageSubType = message.EM_MessageSubType;
		var key = string.Concat(messageType, messageSubType);

		if (!cachedProcessors.TryGetValue(key, out var processor))
		{
			Type processorType = null;
			switch (messageType)
			{
				case NLEDIMessageTypes.Codes.DMS:
					dmsMessageProcessors.Value.TryGetValue(messageSubType, out processorType);
					break;
				case NLEDIMessageTypes.Codes.NCT:
					nctsMessageProcessors.Value.TryGetValue(messageSubType, out processorType);
					break;
				default:
					break;
			}

			if (processorType != null)
			{
				processor = (BranchCustomsApplicationTypeMessageProcessor)Activator.CreateInstance(processorType, Logger);
				cachedProcessors.Add(key, processor);
			}
		}

		return processor;
	}

	protected override bool ExcludeBranchFilter => true;

	protected override bool MessageShouldBeProcessedInASeparateFactory => true;

	readonly Lazy<Dictionary<string, Type>> dmsMessageProcessors = new Lazy<Dictionary<string, Type>>(() => new Dictionary<string, Type>
	{
		{ NLIncomingMessageSubTypeList.Codes.CC404A, typeof(IE404And504MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC410A, typeof(IE410MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC426A, typeof(IE426MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC428A, typeof(IE428And528MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC429A, typeof(IE429And529MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC431A, typeof(IE431And531MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC438A, typeof(IE438MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC451A, typeof(IE451MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC456A, typeof(IE456And556MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC460A, typeof(IE460And560MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC504C, typeof(IE404And504MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC509C, typeof(IE509MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC528C, typeof(IE428And528MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC529C, typeof(IE429And529MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC531C, typeof(IE531MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC551C, typeof(IE551MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC556C, typeof(IE456And556MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC560C, typeof(IE460And560MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC582C, typeof(IE582MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CC599C, typeof(IE599MessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CCEXTA, typeof(EXTMessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CCRCVA, typeof(RCVMessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CCREGA, typeof(REGMessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CCRFIA, typeof(RFIMessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CCRRDA, typeof(RRDMessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.Control, typeof(ControlMessageProcessor) },
		{ NLIncomingMessageSubTypeList.Codes.CCAMDA, typeof(AMDMessageProcessor) },
	});

	readonly Lazy<Dictionary<string, Type>> nctsMessageProcessors = new Lazy<Dictionary<string, Type>>(() => ObjectFactory.Get<INctsMessageProcessorProvider>().NctsMessageProcessors);

	Dictionary<string, BranchCustomsApplicationTypeMessageProcessor> cachedProcessors;
}
