using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Products.CACustoms.Schemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using System.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BTS;
using CargoWise.eHub.AlertService.Helpers;
using CargoWise.eHub.Core.PropertySchemas;
using System.Xml;

namespace CargoWise.eHub.Products.CACustoms.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("6EBC169A-57C8-4A91-B45F-AD30CE1CD56C")]
	public class CACXmlDissasemblerExtension : XmlDisassemblerExtension, IBaseComponent, IDisassemblerComponent, IPersistPropertyBag
	{
		private const string CanadianCustomsReply = @"<CanadianCustomsReply xmlns=""http://cargowise.com/ehub/products/canadiancustoms"" xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<Reference>{0}</Reference>
	<Content>{1}</Content>
</CanadianCustomsReply>";

		private static readonly string[] AccountHoldingRootNames = { "ZCARMSOA", "ZCARMDNOTICEAH" };
		private static readonly string[] CustomsBrokerRootNames = { "ZCARMCBSS", "ZCARMDNOTICECB" };

		public CACXmlDissasemblerExtension() : base() { }

		#region IDisassemblerComponent

		public new void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var inboxPk = pInMsg.Context.ReadPropertyString<InternalTrackingID>();
			var trackingID = pInMsg.Context.ReadPropertyString<MessageTrackingID>();

			try
			{
				string rawMessage = ReadOriginalMessageText(pInMsg);
				var customsReplyMessageString = CreateCustomsReplyMessage(rawMessage, pInMsg);
				VirtualStream persistingStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
				ReadOnlySeekableStream seekablePersistentStream = new ReadOnlySeekableStream(new MemoryStream(Encoding.UTF8.GetBytes(customsReplyMessageString)), persistingStream);
				seekablePersistentStream.Position = 0;
				pInMsg.BodyPart.Data = seekablePersistentStream;
				if (ProcessSubscriptions)
				{
					var subscribers = GetSubscribers(pInMsg);
					if (subscribers.Count == 0)
					{
						throw new Exception("Could not resolve recipient");
					}

					for (int i = 0; i < subscribers.Count; i++)
					{
						var outMessage = i == 0 ? pInMsg : CloneMessage(pContext, pInMsg);
						outMessage.Context.WriteProperty<BTS.DestinationParty>(subscribers[i]);
						MessageQueue.Enqueue(outMessage);
					}
				}
				else
				{
					MessageQueue.Enqueue(pInMsg);
				}
			}
			catch (Exception e)
			{
				GetExceptionsAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "BIZ", "Failure", ExceptionHelper.BuildExceptionDescription(pInMsg, e), new Guid(inboxPk), Guid.Empty, new Guid(trackingID), Guid.Empty, null, false);
			}
		}

		internal virtual void BaseDisassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			base.Disassemble(pContext, pInMsg);
		}

		string CreateCustomsReplyMessage(string messageText, IBaseMessage pInMsg)
		{
			XmlDocument messageDoc = new XmlDocument();
			string reference;
			string overrideEmailSubject;
			pInMsg.Context.PromoteProperty<MessageType>("http://cargowise.com/ehub/products/canadiancustoms#CanadianCustomsReply");

			messageDoc.LoadXml(messageText);
			overrideEmailSubject = "CAD";
			if (messageDoc.DocumentElement.LocalName == "DocumentMetaData" && messageDoc.DocumentElement.NamespaceURI == "urn:wco:datamodel:WCO:Declaration:1")
			{
				reference = messageDoc.SelectSingleNode("/*[local-name()='DocumentMetaData']/*[local-name()='CommunicationMetaData']/*[local-name()='ApplicationReferenceID']")?.InnerText;
			}
			else if (AccountHoldingRootNames.Contains(messageDoc.DocumentElement.LocalName))
			{
				reference = messageDoc.SelectSingleNode("/*/*[local-name()='HEADER']/*[local-name()='PARTY']/*[local-name()='ACCOUNT']")?.InnerText;
			}
			else if (CustomsBrokerRootNames.Contains(messageDoc.DocumentElement.LocalName))
			{
				reference = messageDoc.SelectSingleNode("/*/*[local-name()='HEADER']/*[local-name()='PARTY']/*[local-name()='BN9']")?.InnerText;
			}
			else
			{
				throw new InvalidOperationException("Unsupported message type");
			}

			pInMsg.Context.PromoteProperty<OverrideEmailSubject>(overrideEmailSubject);
			pInMsg.Context.Write("CanadianCustomsReplyReference", "http://cargowise.com/ehub/processing/2010/06", reference);
			return string.Format(CanadianCustomsReply, reference, Base64Encode(messageText));
		}

		static string ReadOriginalMessageText(IBaseMessage pInMsg)
		{
			string result = null;
			var orgStream = pInMsg.BodyPart.GetOriginalDataStream();
			if (orgStream != null)
			{
				orgStream.SeekBegin();
				result = orgStream.ReadToEnd();
			}
			return result;
		}

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

		IBaseMessage CloneMessage(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!message.BodyPart.Data.CanSeek)
				message.BodyPart.Data = new ReadOnlySeekableStream(message.BodyPart.Data, new VirtualStream());
			var messageCopy = pipelineContext.GetMessageFactory().CreateMessage();
			messageCopy.AddPart("Body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
			messageCopy.BodyPart.Data = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			messageCopy.Context = PipelineUtil.CloneMessageContext(message.Context);
			message.BodyPart.Data.Position = 0;
			message.BodyPart.Data.CopyTo(messageCopy.BodyPart.Data);
			message.BodyPart.Data.Position = 0;
			messageCopy.BodyPart.Data.Position = 0;
			return messageCopy;
		}

		public new IBaseMessage GetNext(IPipelineContext pContext)
		{
			if (MessageQueue.Count > 0)
				return MessageQueue.Dequeue() as IBaseMessage;
			else
				return null;
		}

		internal virtual IBaseMessage BaseGetNext(IPipelineContext pContext)
		{
			return base.GetNext(pContext);
		}

		#endregion

		#region IPersistPropertyBag

		public bool ProcessSubscriptions { get; set; }

		public new void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "ProcessSubscriptions", errorLog);
			if (var != null) ProcessSubscriptions = Convert.ToBoolean(var);

			base.Load(propertyBag, errorLog);
		}

		public new void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			base.Save(propertyBag, clearDirty, saveAllProperties);

			object val = ProcessSubscriptions;
			propertyBag.Write("ProcessSubscriptions", ref val);
		}

		object LoadProperty(IPropertyBag propertyBag, string propertyName, int errorLog)
		{
			object result = null;
			try
			{
				propertyBag.Read(propertyName, out result, errorLog);
			}
			catch { }
			return result;
		}

		#endregion

		#region IBaseComponent

		public new string Description
		{
			get { return string.Empty; }
		}

		public new string Name
		{
			get { return "Xml Disassembler Extension for CACustoms"; }
		}

		public new string Version
		{
			get { return "1.0"; }
		}

		#endregion

		public new void GetClassID(out Guid classID)
		{
			classID = new Guid("6EBC169A-57C8-4A91-B45F-AD30CE1CD56C");
		}

		string Base64Encode(string messageText)
		{
			var bytes = Encoding.UTF8.GetBytes(messageText);
			return Convert.ToBase64String(bytes);
		}

		public List<string> GetSubscribers(IBaseMessage message)
		{
			var persistingStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			message.BodyPart.Data = new ReadOnlySeekableStream(message.BodyPart.GetOriginalDataStream(), persistingStream);

			var subsAccessor = GetSubscriptionAccessor();
			var subscribers = subsAccessor.SelectSubscribedClients(message);

			return new List<string>(subscribers);
		}

		internal virtual ISubscriptionAccessor GetSubscriptionAccessor()
		{
			return DataAccessFactories.NewSubscriptionAccessorInstance();
		}

		internal virtual IExceptionsAccessor GetExceptionsAccessor()
		{
			return DataAccessFactories.NewExceptionsAccessorInstance();
		}
	}
}
