using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Edifact.D98A.Messages.CUSRES;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using NZCMessage = Enterprise.Customs.NZ.Business.Declaration.NZCMessage;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	public class MessageProcessorFactory : ApplicationTypeMessageProcessor
	{
		public MessageProcessorFactory(LoggingInformation logger)
			: base(logger)
		{
		}

		string messageFriendlyName;
		protected override string MessageFriendlyNameCore
		{
			get { return messageFriendlyName; }
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.NewZealandCustoms; }
		}

		protected override void ProcessMessageCore(EDIMessage ediMessage)
		{
			messageFriendlyName = "Unknown";
			var tswMessage = ediMessage as TSWMessage;
			if (tswMessage != null)
			{
				ProcessTSWMessage(tswMessage);
			}
			else
			{
				var nzcMessage = ediMessage as NZCMessage;
				if (nzcMessage != null)
				{
					ProcessCusmodMessage(nzcMessage);
				}
			}
		}

		void ProcessCusmodMessage(NZCMessage nzcMessage)
		{
			try
			{
				CUSRESMessage cusresMessage = nzcMessage.MessageAsCUSRESD98A;
				if (cusresMessage == null)
				{
					Edifact.D96B.Messages.CUSRES.CUSRESMessage unsolicitedMsg = nzcMessage.MessageAsCUSRESD96B
						?? throw new MessageProcessingException("Corrupted or Malformed response message. Message does not conform to UN-EDIFACT standard. Cannot process.");
				}

				IProcessorDelegator[] delegators = new IProcessorDelegator[] { new ECIManifestDelegator(), new DeclarationDelegator(), new ExpressECIDelegator(), new ConsolDelegator() };
				bool attemptedToProcess = false;
				foreach (IProcessorDelegator delegator in delegators)
				{
					if (delegator.CanProcess(nzcMessage))
					{
						messageFriendlyName = delegator.MessageFriendlyName;
						attemptedToProcess = true;
						delegator.Process(Logger, nzcMessage);
						break;
					}
				}

				if (!attemptedToProcess)
				{
					try // processing EdiFact unsolicited message here...
					{
						Edifact.D96B.Messages.CUSRES.CUSRESMessage unsolicitedMsg = nzcMessage.MessageAsCUSRESD96B;
						if (unsolicitedMsg != null)
						{
							DeclarationDelegator unsolicitedDelegator = new DeclarationDelegator();
							if (unsolicitedDelegator.CanProcessUnsolicitedMessage(nzcMessage))
							{
								messageFriendlyName = unsolicitedDelegator.MessageFriendlyName;
								unsolicitedDelegator.ProcessUnsolicitedDeliveryOrder(Logger, nzcMessage);
							}
							else
							{
								ZString sendersReference = cusresMessage.UNH[0].CommonAccessReference;
								Logger.Log("Response message appears to be destined for another system. (Reference: " + sendersReference + " is not a " + Core.Constants.ProductName + " reference.)");
								nzcMessage.EM_Status = EDIMessage.Status.Discarded;
							}
						}
						else
						{
							ZString sendersReference = cusresMessage.UNH[0].CommonAccessReference;
							Logger.Log("Response message appears to be destined for another system. (Reference: " + sendersReference + " is not a " + Core.Constants.ProductName + " reference.)");
							nzcMessage.EM_Status = EDIMessage.Status.Discarded;
						}
					}
					catch (MessageProcessingException messageProcessingException)
					{
						ZString sendersReference = cusresMessage.UNH[0].CommonAccessReference;
						Logger.Log(messageProcessingException.Message);
						Logger.Log("Response message appears to be destined for another system. (Reference: " + sendersReference + " is not a " + Core.Constants.ProductName + " reference.)");
						nzcMessage.EM_Status = EDIMessage.Status.Discarded;
					}
				}
			}
			catch (MessageProcessingException messageProcessingException)
			{
				nzcMessage.EM_Status = EDIMessage.Status.Error;
				Logger.Log(messageProcessingException.Message);
			}
		}

		void ProcessTSWMessage(TSWMessage tswMessage)
		{
			string error = null;
			BaseTSWResponse response;
			if (BaseTSWResponse.TryParse(tswMessage, out response))
			{
				if (!string.IsNullOrEmpty(response.SendersReference))
				{
					if (response.OutgoingMessage != null)
					{
						if (response.IsForSubmitter)
						{
							error = ProcessMatchedMessage(tswMessage, response);
						}
						else
						{
							ProcessNotificationResponse(tswMessage, response);
						}
					}
					else
					{
						try
						{
							if (response.IsForSubmitter)
							{
								error = ProcessUnsolicitedMessage(tswMessage, response);
							}
							else
							{
								ProcessNotificationResponse(tswMessage, response);
							}
						}
						catch (MessageProcessingException messageProcessingException)
						{
							Logger.Log(messageProcessingException.Message);
							Logger.Log("Could not find outgoing message for inbound Trade Single Window message.");
							Logger.Log("Response message appears to be destined for another system. (Reference: " + response.SendersReference + " is not a " + Core.Constants.ProductName + " reference.)");
							tswMessage.EM_Status = EDIMessage.Status.Discarded;
						}
					}
				}
				else
				{
					error = "Could not find reference number identifying this information exchange.";
				}
			}
			else
			{
				error = "Message contains invalid XML";
			}

			if (error != null)
			{
				if (tswMessage.EM_Status != EDIMessage.Status.Discarded)
				{
					tswMessage.EM_Status = EDIMessage.Status.Failed;
				}

				Logger.Log(error);
			}
		}

		string ProcessMatchedMessage(TSWMessage tswMessage, BaseTSWResponse response)
		{
			string error = null;
			if (IsDuplicateEntryResponse(response))
			{
				tswMessage.EM_Status = EDIMessage.Status.Error;
				tswMessage.Notes.AddNew(true, "Senders Ref Duplicate", "This unsolicited duplicate message error was detected and ignored due to the lack of any pending outgoing messages.");
			}
			else
			{
				error = ProcessMessage(tswMessage, response);
			}

			return error;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]   // case statement
		string ProcessMessage(TSWMessage tswMessage, BaseTSWResponse response)
		{
			string error = null;
			// Link incoming message
			tswMessage.EM_ApplicationReference = response.SendersReference;
			tswMessage.EM_LinkTable = response.OutgoingMessage.EM_LinkTable;
			tswMessage.EM_LinkUniqueID = response.OutgoingMessage.EM_LinkUniqueID;

			if (tswMessage.EM_LinkedObject == null)
			{
				tswMessage.EM_Status = EDIMessage.Status.Discarded;
				tswMessage.Notes.AddNew(true, "Senders Ref sending object", "This message was discarded due to the outgoing message business object missing.");
				error = "Response for " + tswMessage.EM_ApplicationReference + " expecting business object for " + tswMessage.EM_LinkTable + " PK key '" + tswMessage.EM_LinkUniqueID.ToString() + "' cannot be processed as the linked object cannot be found.";
				tswMessage.Notes.AddNew(true, "Senders Ref missing linked object", error);
			}
			else
			{
				// Select processor and process message
				if (tswMessage.EM_LinkTable == ForwardingConsol.Schema.TableName && response.OutgoingMessage.EM_MessageType == MessageTypeList.Codes.IPI)
				{
					new IPIMessageProcessor(Logger).ProcessMessage(tswMessage, new IM1Response(response));
				}
				else
				{
					switch (response.OutgoingMessage.EM_MessageType)
					{
						case MessageTypeList.Codes.OCR:
							new OCRMessageProcessor(Logger).ProcessMessage(tswMessage, new OCRResponse(response));
							break;
						case MessageTypeList.Codes.E40:
						case MessageTypeList.Codes.E41:
							new EX1MessageProcessor(Logger).ProcessMessage(tswMessage, new EX1Response(response));
							break;
						case MessageTypeList.Codes.I10:
						case MessageTypeList.Codes.I11:
						case MessageTypeList.Codes.I51:
						case MessageTypeList.Codes.I52:
						case MessageTypeList.Codes.I53:
						case MessageTypeList.Codes.IPI:
							new IM1MessageProcessor(Logger).ProcessMessage(tswMessage, new IM1Response(response));
							break;
						case MessageTypeList.Codes.CRE:
							var creResponse = GetWriteOffResponse(response);
							if (creResponse != null)
							{
								new WriteOffMessageProcessor(Logger, MessageTypeList.Descriptions.CRE).ProcessMessage(tswMessage, creResponse);
							}
							else if (response.SendersReference.StartsWith(NZDiagnosticConsol.NzDiagnosticMark, StringComparison.Ordinal))
							{
								tswMessage.EM_Status = EDIMessage.Status.Recognised;
								Logger.DebugLog("Diagnostic message response has been received and processed.");
							}
							else
							{
								error = "CRE response for " + response.LinkedObject.GetType().Name + " cannot be processed.";
							}
							break;
						case MessageTypeList.Codes.ICR:
							var icrResponse = GetWriteOffResponse(response);
							if (icrResponse != null)
							{
								new WriteOffMessageProcessor(Logger, MessageTypeList.Descriptions.ICR).ProcessMessage(tswMessage, icrResponse);
							}
							else
							{
								error = "ICR response for " + response.LinkedObject.GetType().Name + " cannot be processed.";
							}
							break;
						default:
							error = "Could not find message processor for inbound Trade Single Window message.";
							break;
					}
				}
			}

			return error;
		}

		string ProcessUnsolicitedMessage(TSWMessage tswMessage, BaseTSWResponse response)
		{
			new UnsolicitedMessageProcessor(Logger).ProcessMessage(tswMessage, new NZCResponse(response));
			return null;
		}

		void ProcessNotificationResponse(TSWMessage tswMessage, BaseTSWResponse response)
		{
			new NotificationMessageProcessor(Logger).ProcessMessage(tswMessage, new TSWNotificationResponse(response));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]   // case statement
		bool IsDuplicateEntryResponse(BaseTSWResponse response)
		{
			var result = false;
			if (NeedingToCheckMessage(response))
			{
				TSWResponse response1 = null;
				switch (response.OutgoingMessage.EM_MessageType)
				{
					case MessageTypeList.Codes.OCR:
						response1 = new OCRResponse(response);
						break;
					case MessageTypeList.Codes.E40:
					case MessageTypeList.Codes.E41:
						response1 = new EX1Response(response);
						break;
					case MessageTypeList.Codes.I10:
					case MessageTypeList.Codes.I11:
					case MessageTypeList.Codes.I51:
					case MessageTypeList.Codes.I52:
					case MessageTypeList.Codes.I53:
					case MessageTypeList.Codes.IPI:
						response1 = new IM1Response(response);
						break;
					case MessageTypeList.Codes.CRE:
					case MessageTypeList.Codes.ICR:
						response1 = GetWriteOffResponse(response);
						break;
				}

				if (response1 != null)
				{
					result = response1.ErrorCodes.FirstOrDefault(x => x == ErrorList.Codes.SendersReferenceNumberDuplicatesNotAllowed) != null;
				}
			}

			return result;
		}

		bool NeedingToCheckMessage(BaseTSWResponse response)
		{
			var entryHeader = response.OutgoingMessage.EM_LinkedObject as CusEntryHeader;
			return ((entryHeader != null && !entryHeader.IsWaitingForResponse)
				|| (response.OutgoingMessage.EM_MessageType == MessageTypeList.Codes.OCR)
				|| (response.OutgoingMessage.EM_MessageType == MessageTypeList.Codes.CRE)
				|| (response.OutgoingMessage.EM_MessageType == MessageTypeList.Codes.ICR));
		}

		WriteOffResponse GetWriteOffResponse(BaseTSWResponse response)
		{
			if (response.LinkedObject is CusMAWB)
			{
				return new WriteOffOResponseMAWB(response);
			}
			else if (response.LinkedObject is Declaration.ECIWriteOff.Manifesting.CusEntryHeader)
			{
				return new WriteOffResponseManifesting(response);
			}
			else if (response.LinkedObject is Declaration.ECIWriteOff.CusEntryHeader)
			{
				return new WriteOffResponseDeclaration(response);
			}
			else if (response.LinkedObject is ForwardingConsol)
			{
				return new WriteOffResponseConsol(response);
			}
			else if (response.LinkedObject is CusSCAOceanBill)
			{
				return new WriteOffResponseSeaCargo(response);
			}
			else if (response.LinkedObject is TranshipmentRequest)
			{
				return new WriteOffResponseTranshipmentRequest(response);
			}
			else if (response.LinkedObject is Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader)
			{
				return (WriteOffResponse)Activator.CreateInstance(ObjectFactory.GetType("NZAsycudaManifestHeaderWriteOffResponse"), new object[] { response });
			}
			else
			{
				return null;
			}
		}
	}
}
