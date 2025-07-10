using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AllMessages, MessageBlockDictionary.ACEApplicationCode)]
	[TopLevel(typeof(AABIX0), typeof(AABIOutputX1))]
	public class AllACEMessageFailureProcessor : ACEMessageFailureProcessor
	{
		public override void Process()
		{
			SetMessageNum();
			base.Process();
		}

		protected override void SetFailStatus(BusinessObject bizObj)
		{
			IMessageFailStatusManager messageFailStatusManager = bizObj as IMessageFailStatusManager;
			if (messageFailStatusManager != null && messageFailStatusManager.IsMessageTypeSupported(Message.EM_MessageType))
			{
				messageFailStatusManager.SetFailStatus(Message);
			}
			else
			{
				base.SetFailStatus(bizObj);
			}
		}

		void SetMessageNum()
		{
			var abiX0 = messageBlocks.OfType<AABIX0>().FirstOrDefault();

			if (abiX0 != null && abiX0.ReferenceDataTypeCode == ReferenceDataTypeBlock)
			{
				var x01 = new AABIX01();
				x01.Deserialise(AABIX01.Get80ByteStringWithMandatoryCharacter(abiX0));

				if (Message.OriginalMessage == null && !x01.UserData.IsEmpty)
				{
					Message.EM_MessageNum = x01.UserData;
				}

				if (Message.EM_MessageType.IsEmpty && !abiX0.ReferenceDataText.IsEmpty)
				{
					Message.EM_MessageType = abiX0.ReferenceDataText.SubstringSafe(12, 2);
				}

				if (Message.OriginalMessage != null)
				{
					Message.EM_MessageSubType = Message.OriginalMessage.EM_MessageSubType;
				}
			}
		}

		protected override string GetMessageTypeDescription()
		{
			var messageType = Message.OriginalMessage != null ? Message.OriginalMessage.EM_MessageType : Message.EM_MessageType;
			return new ACEApplicationIdentifierCodeList().GetDescriptionFromCode(messageType) ?? base.GetMessageTypeDescription();
		}

		const string ReferenceDataTypeBlock = "BLOCK";
	}
}
