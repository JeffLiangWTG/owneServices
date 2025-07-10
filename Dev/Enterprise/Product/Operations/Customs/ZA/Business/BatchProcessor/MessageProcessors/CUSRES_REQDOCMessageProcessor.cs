using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	class CUSRES_REQDOCMessageProcessor : ZACApplicationTypeMessageProcessor
	{
		public CUSRES_REQDOCMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "CUSRES-REQDOC Message";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { SARSEDIMessage.MessageTypes.CUSRES_REQDOC };

		protected override bool RequiresPreProcessingCore => true;

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason, MessageHelper Helper) TryFindLinkedObject(EDIMessage message)
		{
			var branchPK = message.EM_GB;
			BusinessObject linkedObject = null;
			MultilingualString discardReason = (NoResString)string.Empty;
			CUSRESMessageHelper helper = null;

			if (message is not CUSRES_REQDOCEDIMessage incomingMessage)
			{
				discardReason = GetMessageProcessorCannotProcessMessage("CUSRES-REQDOC", message);
			}
			else
			{
				var jobNumber = ZString.Empty;
				helper = incomingMessage.CUSRESHelper;
				if (helper != null && !helper.LRNNumber.IsEmpty)
				{
					jobNumber = helper.LRNNumber;
					var query = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, jobNumber);
					var targetHeader = message.Factory.LoadTop1<CusEntryHeader>(query);
					if (targetHeader != null)
					{
						linkedObject = targetHeader;
					}
					else if (CUSRESMessageProcessor.LoadOutgoingMessageFromCommonAccessReference(incomingMessage, helper) is EDIMessage outGoingMessage)
					{
						branchPK = outGoingMessage.EM_GB;
						if (outGoingMessage.EM_LinkedObject is IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider linkedEDIFACTMessageAttachee)
						{
							jobNumber = linkedEDIFACTMessageAttachee.JobIdentification;
							linkedObject = outGoingMessage.EM_LinkedObject;
						}
					}
				}

				if (linkedObject is not null)
				{
					Logger.Log(Res.GetString("61590CEB-B010-4E7A-BF2E-E7F0F308AD5F", "Linking {2} Message: #{0}/{3} to job: {1}", message.EM_MessageNum, jobNumber, "CUSRES-REQDOC", message.Interchange?.EI_InterchangeNum));
				}
				else
				{
					discardReason = GetUnableToFindTheLinkedJobMessage("CUSRES-REQDOC", message);
				}
			}
			return (branchPK, linkedObject, discardReason, helper);
		}

		protected override void ProcessMessageMain(EDIMessage message)
		{
			if (message.EM_LinkedObject is IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider linkedEDIFACTMessageAttachee)
			{
				linkedEDIFACTMessageAttachee.AddMessage(message);
				message.EM_Status = EDIMessage.Status.ProcessedOK;
			}
			else
			{
				Logger.Log(Res.GetString("56853827-19E9-4851-91D6-0222CC99F206", "No business object linked to Message: #{0}/{1}", message.EM_MessageNum, message.Interchange?.EI_InterchangeNum, "CUSRES-REQDOC"));
				message.EM_Status = EDIMessage.Status.Discarded;
			}
		}
	}
}
