using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class AESTIRMessageBuilder : MessageBuilder<AESInputBlockControlGenerator, AESTIREDIMessage>
	{
		public AESTIRMessageBuilder(IAESTIRMessageAttachee attachee, UpdateActionCode action)
			: base(attachee, action)
		{
		}

		protected new IAESTIRMessageAttachee messageAttachee
		{
			get { return (IAESTIRMessageAttachee)base.messageAttachee; }
		}

		protected override AESInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			return new AESInputBlockControlGenerator(messageAttachee);
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.AES.CommodityShipment; }
		}

		protected override void UpdateMessageBlocks(AESInputBlockControlGenerator block)
		{
			block.MessageBlocks.AddRange(new AESTIRMessageBlockBuilder(messageAttachee).Build(action));
		}

		protected override void SetMessageSubType(AESTIREDIMessage message)
		{
			switch (action)
			{
				case UpdateActionCode.Delete:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDDelete;
					break;
				case UpdateActionCode.Replace:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDReplace;
					break;
				default:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.SEDAdd;
					break;
			}
			message.EM_SendWithMessageErrors = messageAttachee.TopLevelBusinessObject.HasMessageErrors;
		}

		protected override List<UpdateActionCode> GetSupportedUpdateActionCodeList()
		{
			List<UpdateActionCode> supportedList = base.GetSupportedUpdateActionCodeList();
			supportedList.Add(UpdateActionCode.Add);
			supportedList.Add(UpdateActionCode.Delete);
			supportedList.Add(UpdateActionCode.Replace);
			return supportedList;
		}
	}
}
