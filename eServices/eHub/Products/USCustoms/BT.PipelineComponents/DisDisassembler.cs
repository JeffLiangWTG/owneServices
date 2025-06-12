using System;
using System.Collections.Generic;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Products.USCustoms.BT.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	public class DisDisassembler : BaseComponent, IDisassemblerComponent
	{
		protected override Guid ClassID { get { return new Guid("20b501e3-15ea-4e25-a03a-de6d9169f454"); } }
		protected override string DisplayName { get { return "USC: DIS Disassembler"; } }

		public bool Production { get; set; }

		public DisDisassembler()
		{
			this.Production = true;
		}

		public void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			IBaseMessage outMsg = DisassembleXml(pContext, pInMsg);
			outMsg.BodyPart.Data = new ReadOnlySeekableStream(outMsg.BodyPart.Data, new VirtualStream());

			var subscriptionAccessor = GetSubscriptionAccessor();
			List<string> clients = new List<string>(subscriptionAccessor.SelectSubscribedClients(outMsg));
			if (clients.Count == 0)
			{
				string preparerID = (string)outMsg.Context.Read("DestinationPartyID", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
				if (!String.IsNullOrWhiteSpace(preparerID))
					clients.AddRange(subscriptionAccessor.SelectUSCustomsClientsForFilerCode(preparerID, this.Production));
			}

			outMsg.Context.Promote("NextStep", "http://cargowise.com/ehub/routing/2010/06", "DeliverToOutbox");
			if (clients.Count > 0)
				outMsg.Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", clients[0]);
			this.messageQueue.Enqueue(outMsg);

			for (int i = 1; i < clients.Count; i++)
			{
				var msgCopy = pContext.GetMessageFactory().CreateMessage();
				msgCopy.AddPart("Body", pContext.GetMessageFactory().CreateMessagePart(), true);
				msgCopy.BodyPart.Data = new VirtualStream();
				msgCopy.Context = PipelineUtil.CloneMessageContext(outMsg.Context);
				msgCopy.Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", clients[i]);
				msgCopy.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", Guid.NewGuid().ToString());
				long dataStartPos = outMsg.BodyPart.Data.Position;
				outMsg.BodyPart.Data.CopyTo(msgCopy.BodyPart.Data);
				outMsg.BodyPart.Data.Position = dataStartPos;
				msgCopy.BodyPart.Data.Position = 0;
				this.messageQueue.Enqueue(msgCopy);
			}
		}

		internal virtual IBaseMessage DisassembleXml(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var xmlDasm = new XmlDasmComp();
			xmlDasm.Disassemble(pContext, pInMsg);
			return xmlDasm.GetNext(pContext);
		}

		internal virtual ISubscriptionAccessor GetSubscriptionAccessor()
		{
			return DataAccessFactories.NewSubscriptionAccessorInstance();
		}

		public IBaseMessage GetNext(IPipelineContext pContext)
		{
			return messageQueue.Count > 0 ? messageQueue.Dequeue() : null;
		}

		Queue<IBaseMessage> messageQueue = new Queue<IBaseMessage>();
	}
}
