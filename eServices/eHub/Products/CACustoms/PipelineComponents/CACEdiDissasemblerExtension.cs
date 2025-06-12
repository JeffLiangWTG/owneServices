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
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.CACustoms.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("D2F4B0A2-775D-4D13-84A4-2F2738B155A7")]
	public class CACEdiDissasemblerExtension : EdiDissasemblerExtension, IBaseComponent, IDisassemblerComponent, IPersistPropertyBag
	{
		const string D13AGOVCBR = "http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D13A_GOVCBR";
		private const string CanadianCustomsReply = @"<CanadianCustomsReply xmlns=""http://cargowise.com/ehub/products/canadiancustoms"" xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<Reference>{0}</Reference>
	<Content>{1}</Content>
</CanadianCustomsReply>";
		private static readonly Regex[] EDIFACTProcessingFilterList = {
			new Regex("'(\r\n|\r|\n)*UNG\\+CUSDEC\\+DN\\+[^']+S:99B", RegexOptions.Compiled),
			new Regex("'(\r\n|\r|\n)*UNG\\+CUSDEC\\+SOA\\+[^']+S:99B", RegexOptions.Compiled),
			new Regex("'(\r\n|\r|\n)*UNG\\+GOVCBR\\+IID[TP]\\+[^']+D:13A", RegexOptions.Compiled),
			new Regex("'(\r\n|\r|\n)*UNG\\+GOVCBR\\+CCR\\+[^']+D:13A", RegexOptions.Compiled),
		};
		private static readonly string[] AccountHoldingRootNames = { "ZCARMSOA", "ZCARMDNOTICEAH" };
		private static readonly string[] CustomsBrokerRootNames = { "ZCARMCBSS", "ZCARMDNOTICECB" };
		private const string CACustomsClientSystemRegistrationTypeID = "CACustomsSystemReference";

		public CACEdiDissasemblerExtension() : base() { }

		#region IDisassemblerComponent

		public new void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			// Encoding chosen because biztalk needs a single byte encoding and Entrust (CA Encryption Library) outputs in ISO-8859-1
			var CAEncoding = Encoding.GetEncoding("ISO-8859-1", new EncoderReplacementFallback(""), new DecoderReplacementFallback("")); // dont fallback to '?' as that is the edifact escape character
			var inboxPk = pInMsg.Context.ReadPropertyString<InternalTrackingID>();
			var trackingID = pInMsg.Context.ReadPropertyString<MessageTrackingID>();

			try
			{
				string rawMessage = ReadOriginalMessageText(pInMsg);
				if (IsEDIFACTProcessingMessage(rawMessage))
				{
					var rawMessageISO88591 = new MemoryStream(CAEncoding.GetBytes(rawMessage));
					pInMsg.BodyPart.Data = rawMessageISO88591;
					ProcessSubscriptions = false;

					BaseDisassemble(pContext, pInMsg);

					var messageDebatched = BaseGetNext(pContext);
					var messageReference = messageDebatched.Context.ReadPropertyString("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema");
					pInMsg.Context.Write("CanadianCustomsReplyReference", "http://cargowise.com/ehub/processing/2010/06", messageDebatched.Context.ReadPropertyString("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema"));
					FindFirstActiveClientOrSubscribers(pContext, messageDebatched);
					var destinationParty = messageDebatched.Context.ReadPropertyString<BTS.DestinationParty>();

					while (messageDebatched != null)
					{
						if(messageReference != messageDebatched.Context.ReadPropertyString("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema"))
						{
							pInMsg.Context.Write("CanadianCustomsReplyReference", "http://cargowise.com/ehub/processing/2010/06", messageDebatched.Context.ReadPropertyString("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema"));
							FindFirstActiveClientOrSubscribers(pContext, messageDebatched);
						}
						else
						{
							messageDebatched.Context.WriteProperty<BTS.DestinationParty>(destinationParty);
						}
						var messageType = messageDebatched.Context.ReadPropertyString<BTS.MessageType>();
						if (messageType == D13AGOVCBR)
						{
							messageDebatched.Context.WriteProperty<RawMessage>(rawMessage);
						}

						MessageQueue.Enqueue(messageDebatched);
						messageDebatched = BaseGetNext(pContext);
					}
				}
				else
				{
					var recipient = ExtractRecipient(rawMessage);
					var customsReplyMessageString = string.Format(CanadianCustomsReply, recipient, Base64Encode(rawMessage));
					pInMsg.Context.PromoteProperty<MessageType>("http://cargowise.com/ehub/products/canadiancustoms#CanadianCustomsReply");
					VirtualStream persistingStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
					ReadOnlySeekableStream seekablePersistentStream = new ReadOnlySeekableStream(new MemoryStream(Encoding.UTF8.GetBytes(customsReplyMessageString)), persistingStream);
					seekablePersistentStream.Position = 0;
					pInMsg.BodyPart.Data = seekablePersistentStream;

					if (ProcessSubscriptions)
					{
						pInMsg.Context.Write("CanadianCustomsReplyReference", "http://cargowise.com/ehub/processing/2010/06", recipient);
						FindFirstActiveClientOrSubscribers(pContext, pInMsg, recipient);
						MessageQueue.Enqueue(pInMsg);
					}
					else
					{
						MessageQueue.Enqueue(pInMsg);
					}
				}
			}
			catch (Exception e)
			{
				GetExceptionsAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "BIZ", "Failure", ExceptionHelper.BuildExceptionDescription(pInMsg, e), new Guid(inboxPk), Guid.Empty, new Guid(trackingID), Guid.Empty, null, false);
			}
		}

		private void FindFirstActiveClientOrSubscribers(IPipelineContext pContext, IBaseMessage message, string reference = null)
		{
			var messageReference = message.Context.ReadPropertyString("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema");
			var clientSystemID = GetPartyAccessor().GetClientSystemIDFromClientSystemRegistration(CACustomsClientSystemRegistrationTypeID, messageReference ?? reference);	
			var firstActiveClient =
						string.IsNullOrEmpty(clientSystemID) ? string.Empty :
						GetTransformAccessor().CallActionProcedure("SelectFirstActiveeHubClientPerSystem", null,
						"@enterpriseCode", clientSystemID.Substring(0, 3),
						"@serverCode", clientSystemID.Substring(clientSystemID.Length - 3));

			if (string.IsNullOrWhiteSpace(firstActiveClient))
			{
				var subscribers = GetSubscribers(message);

				var activeDistinctClients =
							subscribers.Count == 0 ? new List<string>() :
							subscribers.Count == 1 ? new List<string>() { subscribers[0] } :
							subscribers.Select(x => new { EnterpriseCode = x.Substring(0, 3), ServerCode = x.Substring(x.Length - 3) })
								.Distinct()
								.Select(x => GetTransformAccessor()
									.CallActionProcedure("SelectFirstActiveeHubClientPerSystem", null,
										"@enterpriseCode", x.EnterpriseCode,
										"@serverCode", x.ServerCode))
								.Where(x => !string.IsNullOrEmpty(x))
								.ToList();

				if (activeDistinctClients.Count > 0)
				{
					message.Context.WriteProperty<BTS.DestinationParty>(activeDistinctClients[0]);

					for (int i = 1; i < activeDistinctClients.Count; i++)
					{
						var messageCopy = CloneMessage(pContext, message);
						messageCopy.Context.WriteProperty<BTS.DestinationParty>(activeDistinctClients[i]);
						MessageQueue.Enqueue(messageCopy);
					}
				}
				else if(subscribers.Count > 0)
				{
					message.Context.WriteProperty<BTS.DestinationParty>(subscribers[0]);
				}
				else
				{
					throw new Exception("Could not resolve recipient");
				}
			}
			else
			{
				message.Context.WriteProperty<BTS.DestinationParty>(firstActiveClient);
			}
		}

		internal virtual void BaseDisassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			base.Disassemble(pContext, pInMsg);
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

		#region IBaseComponent

		public new string Description
		{
			get { return string.Empty; }
		}

		public new string Name
		{
			get { return "EDI Disassembler Extension for CACustoms"; }
		}

		public new string Version
		{
			get { return "1.0"; }
		}

		#endregion

		public new void GetClassID(out Guid classID)
		{
			classID = new Guid("D2F4B0A2-775D-4D13-84A4-2F2738B155A7");
		}

		bool IsEDIFACTProcessingMessage(string messageText)
		{
			return EDIFACTProcessingFilterList.Any(filter => filter.IsMatch(messageText));
		}

		string ExtractRecipient(string messageText)
		{
			var senderText = GetEdifactParts(messageText).Skip(2).Take(1).FirstOrDefault();
			var recipientText = GetEdifactParts(messageText).Skip(3).Take(1).FirstOrDefault();

			if (string.IsNullOrWhiteSpace(senderText))
			{
				throw new ArgumentException("Unable to find sender.");
			}

			if (string.IsNullOrWhiteSpace(recipientText))
			{
				throw new ArgumentException("Unable to find recipient id.");
			}

			return $"{recipientText} - {senderText}";
		}

		IEnumerable<string> GetEdifactParts(string text)
		{
			const string splitter = "+";
			int currentPosition = 0;

			while (true)
			{
				var newPosition = text.IndexOf(splitter, currentPosition, StringComparison.InvariantCulture);
				if (newPosition == -1) yield break;
				yield return text.Substring(currentPosition, newPosition - currentPosition);
				currentPosition = newPosition + splitter.Length;
			}
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

		internal virtual ITransformAccessor GetTransformAccessor()
		{
			return DataAccessFactories.NewTransformAccessorInstance();
		}

		public virtual IPartyAccessor GetPartyAccessor()
		{
			return DataAccessFactories.NewPartyAccessorInstance();
		}
	}
}
