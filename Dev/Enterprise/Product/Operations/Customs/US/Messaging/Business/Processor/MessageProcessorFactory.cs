using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class MessageProcessorFactory : ApplicationTypeMessageProcessor
	{
		protected MessageProcessorFactory(LoggingInformation logger, ZString applicationCode, ZString messageFriendlyName)
			: base(logger)
		{
			provider = new MessageProcessorProvider();
			Argument.NotNullOrEmpty(applicationCode, "applicationCode");
			this.applicationCode = applicationCode;
			Argument.NotNullOrEmpty(messageFriendlyName, "messageFriendlyName");
			this.messageFriendlyName = messageFriendlyName;
		}

		public static ZString[] GetImporterSecurityFilingMessageTypes() => new ZString[] { ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse, ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory, ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling };

		public static ZString[] GetReferenceFileMessageTypes()
		{
			return new ZString[]
			{
				ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse,
				ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse,
				ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse,
				ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
				ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse,
				ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
				ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
				ApplicationIdentifierCodeList.Codes.QueryQuotaResponse,
				ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse,
				ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse,
				ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse,
				ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
				ACEApplicationIdentifierCodeList.Codes.DailyStatement,
				ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse,
				ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement
			};
		}

		protected override string ApplicationCodeCore
		{
			get { return applicationCode; }
		}
		readonly ZString applicationCode;

		protected override string MessageFriendlyNameCore
		{
			get { return messageFriendlyName; }
		}
		readonly ZString messageFriendlyName;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			try
			{
				ProcessMessageInternal(message);
				message.EM_Status = CBPEDIMessage.Status.Received;
			}
			catch (InvalidMessageFormatException ex)
			{
				message.EM_Status = CBPEDIMessage.Status.Failed;
				ReportProcessingFailure(ex.Message, message.EM_MessageText, ex.Originator);
				Logger.Log("Processing message failed : " + ex.Message);
			}
			catch (MessageProcessDiscardedException ex)
			{
				message.EM_Status = CBPEDIMessage.Status.Discarded;
				Logger.Log("Message is discarded : " + ex.Message);
			}
		}

		#region Implementation

		protected IMessageProcessorProvider provider;

		void ProcessMessageInternal(EDIMessage message)
		{
			var cbpMessage = message as CBPEDIMessage;
			if (cbpMessage != null)
			{
				using (var messageTextReader = cbpMessage.GetEM_MessageTextReader(true))
				{
					var messageType = cbpMessage.EM_MessageType;

					var isNotExistOriginalMessage = cbpMessage.OriginalMessage == null;
					var isEmptyMessageType = messageType.IsEmpty;
					if (isNotExistOriginalMessage || isEmptyMessageType)
					{
						var x0Block = cbpMessage.MessageBlock.MessageBlocks.OfType<AABIX0>().FirstOrDefault();
						if (x0Block != null && x0Block.ReferenceDataTypeCode == "BLOCK")
						{
							if (isNotExistOriginalMessage)
							{
								var x01 = new AABIX01();
								x01.Deserialise(AABIX01.Get80ByteStringWithMandatoryCharacter(x0Block));

								if (!x01.UserData.IsEmpty)
								{
									message.EM_MessageNum = x01.UserData;
								}
							}

							if (isEmptyMessageType && !x0Block.ReferenceDataText.IsEmpty)
							{
								messageType = x0Block.ReferenceDataText.SubstringSafe(12, 2);
							}
						}
					}

					var reader = new BlockControlReader(messageTextReader, new string[] { cbpMessage.GetMessageBlockApplicationCode() }, messageType);
					ProcessBlock(cbpMessage, reader);
				}
			}
		}

		void ProcessBlock(CBPEDIMessage message, BlockControlReader reader)
		{
			using (var lockRefDbConnetion = Db.NewExtraConnectionToMainDb())
			using (lockRefDbConnetion.BeginTransactionWithManager())
			{
				var isDataLockSupported = IsDataLockSupported;
				string applicationIdentifier = reader.ApplicationIdentifier;
				var previousLockType = "";
				BlockControlReader.OutputMessageBlockEnumerator enumerator = reader.GetOutputMessageEnumerator();
				{
					IProcessor processor = null;
					while (enumerator.MoveNext())
					{
						var block = enumerator.Current;
						if (processor != null)
						{
							if (processor.CanAcceptBlock(block))
							{
								processor.AddMessageBlock(block);
							}
							else
							{
								processor.SetLogger(Logger);
								processor.Process();
								processor = null;
							}
						}

						if (processor == null)
						{
							var applicationCode = message.GetMessageBlockApplicationCode();
							processor = provider.GetProcessor(applicationCode, applicationIdentifier, block);
							if (processor == null)
							{
								var originalMessage = message.OriginalMessage;
								var bizO = originalMessage != null ? originalMessage.EM_LinkedObject : null;
								throw new InvalidMessageFormatException("Could not find any Processor classes that support '" + applicationIdentifier + ":" + block.GetType().Name + "'", null, bizO);
							}
							else
							{
								var databaseLockProcessor = processor as IDatabaseLockProcessor;
								if (databaseLockProcessor != null)
								{
									if (!isDataLockSupported)
									{
										var typeName = processor.GetType().FullName;
										ErrorReporter.ReportOnce(typeName + " is used in MessgeProcessorFactory that does not support lock", string.Format("EDIMessage (PK:{0}, AppCode:{1}, AppId:{2})", message.PK, ApplicationCode, applicationIdentifier));
									}
									var lockType = databaseLockProcessor.GetLockType();
									if (!string.IsNullOrEmpty(lockType) && previousLockType != lockType)
									{
										if (!string.IsNullOrEmpty(previousLockType))
										{
											ErrorReporter.ReportOnce("US Message Processor Factory Multiple Lock Types", string.Format("EDIMessage (PK:{0}, AppCode:{1}, AppId:{2}) contains data for mutliple processors which requires different reference file locks (Previous: {3}, Current: {4})", message.PK, applicationCode, applicationIdentifier, previousLockType, lockType));
										}
										if (ReferenceFileUpdateMutex.AcquireUpdateLockForSharedDatabase(lockRefDbConnetion, lockType, Core.Constants.CountryCodes.UnitedStates))
										{
											previousLockType = lockType;
										}
										else
										{
											throw new MessageProcessLockException(string.Format("US reference database needs to be updated but could not acquire lock for '{0}', skipping until data is available for update", lockType));
										}
									}
								}
							}
							processor.Message = message;
							processor.AddMessageBlock(block);
						}
					}

					if (processor != null)
					{
						processor.SetLogger(Logger);
						processor.SetBAndYBlock(reader.B, reader.Y);
						processor.Process();
						processor.Factory = null;
					}
				}

				if (!string.IsNullOrEmpty(previousLockType)) // If lock was used then data should be saved before locked is released. 
				{
					message.EM_Status = CBPEDIMessage.Status.Received;
					message.Factory.Save();
				}
			}
		}

		protected virtual bool IsDataLockSupported
		{
			get { return false; } // This should only be true on MessageProcessorFactory which process one message at a time
		}

		void ReportProcessingFailure(string reason, ZString messageText, BusinessObject linkedObject)
		{
			string uri = null;
			string humanReadableName = null;
			if (linkedObject != null)
			{
				humanReadableName = linkedObject.HumanReadableName;
				if (!string.IsNullOrEmpty(humanReadableName))
				{
					var messageAttachee = linkedObject as IMessageAttachee;
					if (messageAttachee != null)
					{
						uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(messageAttachee);
					}
				}
			}

			var generator = new HtmlResponseEmailGenerator();
			generator.ResponseDescription = HtmlResponseEmailGenerator.AResponseMessage +
				"The attached message failed to process successfully.  It will be marked as failed and no further attempt will be made to process it.<br />" +
				HtmlResponseEmailGenerator.ShownBelow;
			EmailDef email;
			if (generator.TryGenerateEmail(uri, humanReadableName, "Message", "Reason : " + reason, true, out email, GlbBranch.CurrentBranch))
			{
				var builder = new StringBuilder();
				foreach (ZString s in messageText.Split(80))
				{
					builder.AppendLine(s);
				}

				email.Attachments.Add(new AttachmentDef("Message.Txt", Encoding.ASCII.GetBytes(builder.ToString())));
				Env.OutgoingCustomsMailManager.CreateAndSave(email, Env.Registry.PostMasterGroup, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
			}
		}

		#endregion
	}
}
