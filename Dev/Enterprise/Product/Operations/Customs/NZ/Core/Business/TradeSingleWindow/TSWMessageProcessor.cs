using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using BaseMessageProcessor = Enterprise.Messaging.MessageProcessors.CustomsMessageProcessor;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	abstract class TSWMessageProcessor<TResponse> : BaseMessageProcessor where TResponse : TSWResponse
	{
		protected TSWMessageProcessor(LoggingInformation logger, string outgoingMessageDescription)
			: base(logger, MessageTypeList.Codes.TWR, outgoingMessageDescription + " " + MessageTypeList.Descriptions.TWR)
		{
		}

		public void ProcessMessage(EDIMessage incomingMessage, TResponse response)
		{
			Response = Argument.NotNull(response, "response");
			base.ProcessMessage(incomingMessage);
			OnMessageProcessed(incomingMessage, response);
		}

		protected virtual void OnMessageProcessed(EDIMessage incomingMessage, TResponse response)
		{ }

		#region Overrides

		protected sealed override string DoProcessingReturningStatus(EDIMessage message)
		{
			if (Response == null)
			{
				throw new InvalidOperationException("ProcessMessage(EDIMessage, TResponse) method must be used.");
			}

			try
			{
				Process(message);
				return EDIMessage.Status.Received;
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is InvalidStorageDocException))
			{
				var email = new EmailDef()
				{
					Subject = "Error Processing Response for " + Response.JobName + ": " + Response.JobID,
					Body = ex.Message +
						   System.Environment.NewLine +
						   System.Environment.NewLine +
						   message.EM_MessageText,
				};

				SendErrorReport(Response.LinkedObject, email);
				Logger.Log("Error happened while processing " + MessageFriendlyName + ": " + ex.Message + " Please contact support to help resolve this problem.");
				ErrorReporter.ReportOnce("5F265594-6991-4CCE-9B0C-8CC68E6EF245", $"{email.Subject}  Message: {message.EM_MessageNum}", ex);

				return EDIMessage.Status.Failed;
			}
			finally
			{
				Response = null;
			}
		}

		#endregion // Overrides

		#region Implementation

		protected class ReportBuilder
		{
			readonly List<Tuple<string, string>> lines = new List<Tuple<string, string>>();

			public void AddHeader(string title)
			{
				if (lines.Count > 0)
				{
					AddLine();
				}
				AddLine(title);
				AddSeparator();
			}

			public void AddLine(string line = "")
			{
				AddLine(line, null);
			}

			public void AddLine(string key, string value)
			{
				lines.Add(Tuple.Create(key, value));
			}

			public void AddSeparator()
			{
				AddLine("---------------------------------------------------------------------");
			}

			public void Clear()
			{
				lines.Clear();
			}

			public bool IsEmpty
			{
				get { return lines.Count == 0; }
			}

			public override string ToString()
			{
				var result = "";
				if (!IsEmpty)
				{
					var builder = new ZStringBuilder();
					var maxKeyLength = lines.Where(line => line.Item2 != null)
											.Max(line => line.Item1.Length);
					foreach (var line in lines)
					{
						builder.AppendLine(line.Item2 != null ? line.Item1.PadRight(maxKeyLength) + " : " + line.Item2 : line.Item1);
					}
					result = builder.ToString();
				}
				return result;
			}
		}

		protected ReportBuilder Report
		{
			get { return report ?? (report = new ReportBuilder()); }
		}

		protected TResponse Response
		{
			get;
			private set;
		}

		protected abstract void ProcessCore();

		protected abstract void SendEmail(EmailDef email);

		protected abstract void WriteEntryStatus();

		protected abstract string[] ExpectedDODocumentNames { get; }
		protected abstract ZGuid DocumentParentPK { get; }
		protected abstract ZString DocumentParentType { get; }

		protected virtual void WriteMessageStatus()
		{
		}

		protected virtual void ProcessAttachedDocuments(EDIMessage message, StorageDocsBase[] interchangeDocs)
		{
			var docFactory = (DocumentFactory)message.Interchange.DocManagerInfo.MasterFactory;
			var messageFactory = message.Factory;

			message.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			if (!messageFactory.ChildFactories.Contains(docFactory))
			{
				messageFactory.ChildFactories.Add(docFactory);
			}

			if (docFactory.ChildFactories.Contains(messageFactory))
			{
				docFactory.ChildFactories.Remove(messageFactory);
			}

			foreach (var eDoc in interchangeDocs)
			{
				try
				{
					var newEDoc = docFactory.Allocate(eDoc, DocumentParentPK);
					if (newEDoc != null)
					{
						newEDoc.SC_DocType = "MCD";
						newEDoc.SC_Desc = Core.Constants.RefDocTypeDescriptions.CustomsAuthority;
						newEDoc.SM_Type = DocumentParentType;
						foreach (ZString expectedDocName in ExpectedDODocumentNames)
						{
							if (newEDoc.SC_FileName.Contains(expectedDocName))
							{
								newEDoc.SC_FileName = expectedDocName.Replace("Delivery_Order", "DeliveryOrder") + "_" + Response.JobID;
								newEDoc.SC_DocType = Core.Constants.RefDocTypes.DeliveryOrder;
								break;
							}
						}
					}
				}
				catch (ExternalStorageException)
				{
					// TODO: Please review if this exception should be swallowed
					// The exception was swallowed before when it was using Allocate() with throwExternalStorageException = false
				}
			}
		}

		protected virtual void OutputCustomsInstruction(string instruction)
		{
			ZString customsInstruction = instruction;
			Report.AddLine(customsInstruction.Replace("seq:1,instructions:", ""));
		}

		protected virtual void OutputResponseStatus()
		{
			var statusDescriptionParts = Response.StatusDescription.Split(new[] { ',' }, 2);
			Report.AddLine("Message Status", "(" + Response.Status + ") " + statusDescriptionParts[0]);
			if (statusDescriptionParts.Length == 2)
			{
				statusDescriptionParts = statusDescriptionParts[1].Split(new[] { '-' }, 2);
				Report.AddLine("", statusDescriptionParts[0].Trim() + ".");
				if (statusDescriptionParts.Length == 2)
				{
					Report.AddLine(statusDescriptionParts[1].Trim() + ".");
				}
			}
		}

		protected virtual void OutputError(TSWResponse.ValueWithPointer error)
		{
			Report.AddLine("**Error** in {" + error.Pointer + "}:-");
			Report.AddLine("  " + error.Value);
		}

		protected virtual void ProcessCustomsInstructions()
		{
			if (!Response.CustomsInstructions.IsNullOrEmpty())
			{
				Report.AddHeader("Customs Instructions");
				Response.CustomsInstructions.ForEach(OutputCustomsInstruction);
			}
		}

		protected void OutputErrors()
		{
			if (!Response.ErrorsWithPointers.IsNullOrEmpty())
			{
				Report.AddHeader("Message Errors");
				Response.ErrorsWithPointers.ForEach(OutputError);
			}
		}

		protected bool HasBeenQueuedForReProcessing()
		{
			var result = false;
			StmALog[] reprocessLogs = Response.IncomingTSWMessage.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessagePendingProcessing.Code));
			if (reprocessLogs.Length > 0)
			{
				foreach (StmALog log in reprocessLogs)
				{
					if (log.SL_Reference == "NZC - Reprocess")
					{
						using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
						{
							log.SL_Reference = "NZC - MSG Reprocessed";
						}

						result = true;
						break;
					}
				}
			}

			return result;
		}

		ReportBuilder report;

		void Process(EDIMessage message)
		{
			Report.Clear();
			if (Response.Status != StatusList.Codes.Acknowledgement)
			{
				ProcessCore();
			}

			message.EM_MessageSubType = GetSubMessageType(message);
			WriteEntryStatus();
			WriteMessageStatus();

			if (Response.IsForSubmitter)
			{
				var reportHeader = string.Empty;
				if (Response.Status == StatusList.Codes.Acknowledgement)
				{
					reportHeader = "[" + Response.EnterpriseStatusDescription + "] Response for " + Response.MessageParentTypeName + ": " + Response.MessageParentID;
				}
				else
				{
					reportHeader = "[" + Response.EnterpriseStatusDescription + "] Response for " + Response.JobName + ": " + Response.JobID;
				}
				var reportBody = Report.ToString();
				if (!string.IsNullOrEmpty(reportBody))
				{
					message.EM_MessageInterpretation = reportHeader + System.Environment.NewLine + System.Environment.NewLine + reportBody;
					responseEmail = new EmailDef() { Subject = reportHeader, Body = reportBody };

					var interchangeDocs = message.Interchange?.DocManagerInfo.AllEDocs?.Cast<StorageDocsBase>().ToArray();
					if (interchangeDocs != null)
					{
						if (message.EM_RetryCount < MaxDocumentRetry)
						{
							foreach (var emptyDoc in interchangeDocs)
							{
								try
								{
									// To trigger SC_ImageData loads from external storage and cached
									_ = emptyDoc.SC_ImageData;
								}
								catch (ExternalStorageException ex)
								{
									throw new InvalidStorageDocException(GetExternalStorageExceptionMessage(message, emptyDoc, ex));
								}
							}
						}

						foreach (IeDoc document in interchangeDocs)
						{
							ZString docName;
							if (document.FileName.Contains("_Delivery_Order"))
							{
								var characterToStopOn = new char[] { '-' };
								docName = document.FileName.KeepCharsUntil(DocNameValues, characterToStopOn) + "_" + Response.JobID + ".pdf";
							}
							else if (document.FileName.StartsWith("PDF", StringComparison.OrdinalIgnoreCase))
							{
								docName = document.FileName.SubstringSafe(0, 13) + "_" + Response.JobID + ".pdf";
							}
							else
							{
								var extension = Path.GetExtension(document.FileName);
								ZString fileName = Path.GetFileNameWithoutExtension(document.FileName);
								docName = fileName.Left(DocumentNameMaxLength - extension.Length) + extension;
							}

							ZBlob docImageData = null;
							try
							{
								docImageData = document.ImageData;
							}
							catch (ExternalStorageException)
							{
								// TODO: The original behaviour was to swallow ExternalStorageException and return an empty ImageData
								// TestMessageWithEmptyDocumentIsReProcessedTwice requires this behaviour so I have to do this here
								// Please review and change the logic or the test accordingly as empty document won't be returned anymore and ExternalStorageException will be thrown
								// Another question is why would it continue to process after message.DataImportLogNoteCoun > MaxDocumentRetry, even if it is still going to get the exception??
								// I think this part is not necesssary and the test need to be changed
								docImageData = ZBlob.Empty;
							}
							responseEmail.Attachments.Add(new AttachmentDef(docName, docImageData.SubBlobSafe(0)));
						}

						if (!string.IsNullOrEmpty(Response.JobID))
						{
							ProcessAttachedDocuments(message, interchangeDocs);
						}
					}

					try
					{
						SendEmail(responseEmail);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ErrorReporter.ReportOnce("894778AD-905C-4B13-8511-7D857F0B71A3", "Failed to send NZ TSW notification email from TSWMessageProcessor", e);
					}
				}
				else
				{
					message.EM_MessageInterpretation = reportHeader;
				}
			}
		}

		const string DocNameValues = "DeliveryOrder_IM1EXOCR";
		const int DocumentNameMaxLength = 50;
		const int MaxDocumentRetry = 2;

		string GetExternalStorageExceptionMessage(EDIMessage message, StorageDocsBase emptyDoc, ExternalStorageException externalStorageException)
		{
			return $@"System encountered an ExternalStorageException in message {message.EM_MessageNum}.  Document Name: {emptyDoc.Name}  Document PK: {emptyDoc.PK}
External Storage Exception: {externalStorageException?.ToString() ?? "None"}";
		}

		protected virtual string GetSubMessageType(EDIMessage message)
		{
			var subMsgType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			if (Response.MessageType == TransactionTypeList.Codes.Receipt)
			{
				subMsgType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			}
			else
			{
				var agency = Response.ResponsibleGovernmentAgency;
				if (agency == ResponsibleGovernmentAgencyList.Codes.TSW)
				{
					subMsgType = TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow;
				}
				else if (agency == ResponsibleGovernmentAgencyList.Codes.MPIFOOD)
				{
					subMsgType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
				}
				else if (agency == ResponsibleGovernmentAgencyList.Codes.MPIBIO)
				{
					subMsgType = TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO;
				}
				else if (agency == ResponsibleGovernmentAgencyList.Codes.NZCS)
				{
					subMsgType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
				}
			}

			return subMsgType;
		}

		protected internal EmailDef responseEmail = new EmailDef();

		[Serializable]
		class InvalidStorageDocException : Exception
		{
			public InvalidStorageDocException(string message)
				: base(message)
			{ }

#if NETFRAMEWORK
			protected InvalidStorageDocException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{ }
#endif
		}

		#endregion // Implementation
	}
}
