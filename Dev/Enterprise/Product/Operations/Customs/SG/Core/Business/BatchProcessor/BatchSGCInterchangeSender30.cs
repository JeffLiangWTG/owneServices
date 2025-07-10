using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.Edifact;
using Enterprise.Edifact.Generic;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class BatchSGCInterchangeSender30 : BaseInterchangeSender
	{
		#region Overrides

		protected override bool IsEnvironmentDataValid()
		{
			return base.IsEnvironmentDataValid() && Helper.IsEnvironmentDataValid();
		}

		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			var charSet = new UNOACharacterSet();

			foreach (EDIMessage message in messages)
			{
				var entryHeader = message.Factory.Load<CusEntryHeader>(message.EM_LinkUniqueID);
				var glbExternalPassword = Helper.GetGlbExternalPassword(entryHeader.DeclaringBroker);
				var receiver = GetReceiver(message.EM_IsTestMessage);
				var recipientsID = GetRecipientsID(message.EM_IsTestMessage);
				var sendersID = glbExternalPassword.GP_UserID.Left(4) + "." + glbExternalPassword.GP_UserID;

				var interchange = message.Factory.New<EDIInterchange>();
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_IsActive = true;
				interchange.EI_From = sendersID;
				interchange.EI_To = receiver;
				interchange.EI_Priority = "HGH";
				interchange.EI_ApplicationCode = message.EM_ApplicationCode;
				interchange.EI_InterchangeType = message.EM_MessageType;

				var xmlMessage = message as SGXmlEDIMessage;
				if (xmlMessage != null)
				{
					PopulateSenderAndReceiver(interchange, xmlMessage, sendersID, recipientsID);
				}
				else
				{
					PopulateSenderAndReceiver(message, sendersID, recipientsID);
					interchange.EI_HeaderText = EDIInterchange.UNOAUNAString + UNB.ToString(charSet);
					interchange.EI_FooterText = UNZ.ToString(charSet);
				}

				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_BodyText = message.EM_MessageText;
				interchange.EI_GB = message.EM_GB;

				message.EM_EI = interchange.PK;
				message.EM_Status = EDIMessageStatusList.Codes.Sent;
			}
		}

		void PopulateSenderAndReceiver(EDIMessage message, string sendersID, string recipientsID)
		{
			var ediFactMessageType = message.EM_MessageType == "COO" ? "TCODEC" : "CUSDEC";
			UNB.InterchangeRecipient.RecipientIdentification = recipientsID;
			UNB.InterchangeSender.SenderIdentification = sendersID;
			UNB.ApplicationReference = ediFactMessageType;
		}

		void PopulateSenderAndReceiver(EDIInterchange interchange, SGXmlEDIMessage message, string sendersID, string recipientsID)
		{
			interchange.EI_InterchangeNumInfo.ValueChanged += (s, e) =>
			{
				var interchangeNum = interchange?.EI_InterchangeNum ?? string.Empty;

				if (!string.IsNullOrWhiteSpace(interchangeNum))
				{
					message.EM_MessageText = message.EM_MessageText.Replace(SGXmlEDIMessage.InterchangeNumberPlaceHolderXml, interchangeNum);
					interchange.EI_BodyText = message.EM_MessageText;
				}
			};

			message.EM_MessageText = message.EM_MessageText
				.Replace(SGXmlEDIMessage.SendersReferencePlaceHolderXml, sendersID.ToUpperInvariant())
				.Replace(SGXmlEDIMessage.RecipientReferencePlaceHolderXml, recipientsID.ToUpperInvariant());
		}

		protected virtual ZString GetRecipientsID(bool isForTesting)
		{
			return isForTesting
				? EDIInterchange.InterchangePartyIDs.TradeNetV4TestSystem
				: EDIInterchange.InterchangePartyIDs.TradeNetV4LiveSystem;
		}

		protected virtual ZString GetReceiver(bool isForTesting) => GetRecipientsID(isForTesting);

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			Logger.Log("Sending...");

			foreach (GlbStaff broker in Helper.GetValidBrokerMailboxes())
			{
				token.ThrowIfCancellationRequested();
				currentBrokerWrapper = SGGlbStaffWrapper.Get(broker);
				Logger.Log(string.Format(CultureInfo.InvariantCulture, "Sending messages for {0} - ({1} UserID: {2}).", broker.GS_FullName, Helper.ApplicationDescription, Helper.GetGlbExternalPassword(currentBrokerWrapper).GP_UserID));

				foreach (var applicationCode in Helper.ApplicationCodes)
				{
					SendOutboundInterchanges(applicationCode, token);
				}
			}

			Logger.Log("Sending Complete");
			Logger.AddBlankLine();
		}
		protected SGGlbStaffWrapper currentBrokerWrapper;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override bool SendInt(EDIInterchange interchange)
		{
			bool result = false;
			if (interchange != null)
			{
				if (RetryHelper.IsNextRetryLastRetry(interchange) && RetryHelper.IsLinkedMessageDelayed(interchange))
				{
					LogDelayedMessageSkipped(interchange);
					InterchangeSkipped = true;
				}
				else
				{
					try
					{
						InterchangeSkipped = false;
						result = SendAndUploadInterchange(IsInProduction, BrokerFilePath, interchange);
						if (result)
						{
							UpdateStatusToSent(interchange);
						}
					}
					catch (Exception e) when (!e.IsCriticalException()) //ensure that if a particular interchange fails, then others still get processed
					{
						ErrorReporter.ReportOnce("BatchSGCInterchangeSender30.SendInt", " SendInt Failure: " + e.Message + "; Interchange PK: " + interchange.PK + "; Exception Type: " + e.GetType(), e);
						Helper.VerboseLog(Logger, "Exception: " + e.Message);
					}
				}
			}

			return result;
		}

		public override TimeSpan TimeToSpendProcessing
		{
			get { return new TimeSpan(0, 0, 0); }
		}

		protected override void OnInterchangeSendFailed(EDIInterchange interchange)
		{
			if (!InterchangeSkipped)
			{
				interchange.EI_RetryCount++;

				if (RetryHelper.IsNextRetryLastRetry(interchange))
				{
					RetryHelper.AddDelayToLinkedEDIMessage(interchange);
				}

				if (RetryHelper.MaxRetriesExceeded(interchange))
				{
					RetryHelper.OnMaxRetriesExceeded(interchange);
				}
				else
				{
					interchange.EI_Status = EDIInterchange.Status.Queued;
				}
			}
		}

		void LogDelayedMessageSkipped(EDIInterchange interchange)
		{
			if (interchange.ContainedMessages.Count == 1)
			{
				var message = interchange.ContainedMessages[0];
				Helper.VerboseLog(Logger, $"Tried sending Interchange {interchange.EI_InterchangeNum} {interchange.EI_RetryCount} times. Final attempt is delayed until {message.EM_HeldUntilDate}.");
			}
		}

		bool InterchangeSkipped { get; set; }

		#endregion

		#region UN Segments

		UNZSegment UNZ
		{
			get
			{
				if (unz == null)
				{
					unz = new UNZSegment();
					unz.InterchangeControlCount = "1";
					unz.InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder;
				}
				return unz;
			}
		}
		UNZSegment unz;

		UNBSegment UNB
		{
			get
			{
				if (unb == null)
				{
					unb = new UNBSegment();

					var preparedTime = ZDateTime.Now;
					unb.DateTimeOfPreparation.Date = preparedTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
					unb.DateTimeOfPreparation.Time = preparedTime.ToString("HHmm", CultureInfo.InvariantCulture);
					unb.InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder;
					unb.InterchangeRecipient.PartnerIdentificationCodeQualifier = "ZZ";

					unb.InterchangeSender.PartnerIdentificationCodeQualifier = "ZZ";
					unb.SyntaxIdentifier.SyntaxIdentifier = "UNOA";
					unb.SyntaxIdentifier.SyntaxVersionNumber = "4";
				}

				return unb;
			}
		}
		UNBSegment unb;

		#endregion

		#region Check Connection/Broker Mailboxes

		protected virtual bool TestConnection()
		{
			return Helper.UseTestConnection;
		}

		protected virtual LoginCommand CheckBrokerMailbox(GlbStaff broker)
		{
			return Helper.MailboxChecker.Execute(broker);
		}

		protected virtual void Logout(LoginCommand loginCommand)
		{
			new LogoutCommand(loginCommand, new MHUBSettingsProvider(), Logger, Helper.ShowVerboseLogging).Execute();
		}

		#endregion

		#region Broker File Paths

		protected string BrokerFilePath
		{
			get
			{
				var userID = Helper.GetGlbExternalPassword(currentBrokerWrapper).GP_UserID;
				if (!BrokerFilePathDictionary.TryGetValue(userID, out var brokerFilePath))
				{
					brokerFilePath = GetBrokerFilePath(userID);
					if (!Directory.Exists(brokerFilePath))
					{
						Helper.VerboseLog(Logger, "Setting up Broker FilePaths");
						Directory.CreateDirectory(brokerFilePath);
						Helper.VerboseLog(Logger, "Broker FilePath = " + brokerFilePath);
					}
					BrokerFilePathDictionary.Add(userID, brokerFilePath);
				}
				return brokerFilePath;
			}
		}

		protected virtual string GetBrokerFilePath(ZString userID)
		{
			return Path.Combine(Path.Combine(Env.TempPath, SGBatchProcessorConstants.Directories.OuputDirectory), userID);
		}

		Dictionary<string, string> BrokerFilePathDictionary => brokerFilePathDictionary ?? (brokerFilePathDictionary = new Dictionary<string, string>());
		Dictionary<string, string> brokerFilePathDictionary;

		protected void ClearBrokerFilePathDictionary()
		{
			brokerFilePathDictionary?.Clear();
		}

		#endregion

		#region Extra Filter

		protected override ZQuery ExtraFilter
		{
			get
			{
				var glbExternalPassword = Helper.GetGlbExternalPassword(currentBrokerWrapper);
				string sendersID = glbExternalPassword.GP_UserID.Left(4) + "." + glbExternalPassword.GP_UserID;
				return new ZQuery(EDIInterchangeSchema.EI_From, SQLComparisonOperator.Equal, sendersID);
			}
		}

		#endregion

		#region Upload and Submit Interchange

		protected virtual bool SendAndUploadInterchange(bool isProduction, string brokerFilePathShutUpCodeAnalyser, EDIInterchange interchange)
		{
			Helper.VerboseLog(Logger, "Uploading interchange");
			bool result = false;
			string fileName = "";

			try
			{
				fileName = CreateInterchangeFile(brokerFilePathShutUpCodeAnalyser, interchange);

				if (!string.IsNullOrEmpty(fileName))
				{
					Helper.VerboseLog(Logger, "Uploading file: " + fileName);
					result = UploadAndSubmitFile(isProduction, fileName, interchange);
					Helper.VerboseLog(Logger, "Uploading result: " + result.ToString());
				}

				if (!result)
				{
					interchange.EI_Status = EDIInterchange.Status.Queued;
				}
			}
			finally
			{
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
			}

			return result;
		}

		string CreateInterchangeFile(string fBrokerFilePath, EDIInterchange interchange)
		{
			string result;

			var fileName = Helper.GetGlbExternalPassword(currentBrokerWrapper).GP_UserID + "_" + interchange.EI_InterchangeNum;

			if (HasSupportingDocumentAttachment(interchange))
			{
				result = PackageAttachmentInterchange(interchange, fBrokerFilePath, fileName);
			}
			else
			{
				var fileExtension = interchange.ContainedMessages[0] is SGXmlEDIMessage ? ".xml" : ".edi";
				result = SendIntToFile(interchange, fBrokerFilePath, fileName + fileExtension) ? Path.Combine(fBrokerFilePath, fileName + fileExtension) : string.Empty;
			}

			return result;
		}

		bool HasSupportingDocumentAttachment(EDIInterchange interchange)
		{
			return interchange.ContainedMessages.Count == 1 && interchange.ContainedMessages[0].EM_LinkedObject != null && interchange.ContainedMessages[0].MessageAttachments.Count > 0;
		}

		protected virtual bool UploadAndSubmitFile(bool isProduction, string fileName, EDIInterchange interchange)
		{
			return isProduction
				? UploadAndSubmitFile(isProduction, fileName, SGCustomsDataRegistry.Instance.RecipientMailbox.Value, interchange)
				: UploadAndSubmitFile(isProduction, fileName, SGBatchProcessorConstants.Test.RecipientMailBox, interchange);
		}

		protected virtual bool UploadAndSubmitFile(bool isProduction, string fileName, string mailBox, EDIInterchange interchange)
		{
			bool result = false;

			string remoteFile = isProduction
				? Path.Combine(SGCustomsDataRegistry.Instance.HomeDirectory.Value + "/upload/", Path.GetFileName(fileName))
				: Path.Combine(SGBatchProcessorConstants.Test.HomeDirectory + "/upload/", Path.GetFileName(fileName));

			bool uploadOk = UploadOneFile(fileName, remoteFile, interchange);
			if (uploadOk)
			{
				Helper.VerboseLog(Logger, "if (ftpTransfer.Upload(FileName, remoteFile))");
				if (SubmitFile(fileName, remoteFile, mailBox, interchange.EI_InterchangeNum))
				{
					Logger.Log("Interchange Submitted: " + Path.GetFileName(fileName));
					result = true;
				}
			}

			return result;
		}

		protected virtual bool UploadOneFile(string fileName, string remoteFile, EDIInterchange interchange)
		{
			return true;
		}

		protected virtual bool SubmitFile(string fileName, string remoteFile, string mailBox, string contentId)
		{
			return true;
		}

		#endregion

		#region Interchange Status

		void UpdateStatusToSent(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Sent;
			if (interchange.ContainedMessages.Count == 1)
			{
				var message = interchange.ContainedMessages[0];
				var cusEntryHeader = interchange.Factory.Load<CusEntryHeader>(message.EM_LinkUniqueID);
				if (cusEntryHeader != null)
				{
					if (cusEntryHeader.CH_Status == Core.SGConstants.DeclarationStatus.AmendmentPending)
					{
						cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
					}
					else if (cusEntryHeader.CH_Status == Core.SGConstants.DeclarationStatus.RefundPending)
					{
						cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundSent;
					}
					else if (cusEntryHeader.CH_Status == Core.SGConstants.DeclarationStatus.CancellationPending)
					{
						cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
					}
					else
					{
						cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
					}
				}
			}
		}

		#endregion

		#region Supporting Document

		protected ZString GetValidDocumentName(EDIMessageAttach ediMessageAttach)
		{
			var documentName = ediMessageAttach.EG_FileName;
			documentName = Regex.Replace(documentName, "°", string.Empty);
			documentName = documentName.ToASCII();

			return documentName;
		}

		#endregion

		#region Zip it Up

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected virtual string PackageAttachmentInterchange(EDIInterchange interchange, string outputDirectory, string fileNameNaked)
		{
			var fileName = Path.Combine(outputDirectory, fileNameNaked + ".zip");

			using (var fileStream = new FileStream(fileName, FileMode.Create))
			{
				var entryHeader = interchange.ContainedMessages[0].EM_LinkedObject as CusEntryHeader;

				if (entryHeader != null)
				{
					var sgZipEntries = new List<SGZipEntry>();

					foreach (EDIMessageAttach ediMessageAttach in interchange.ContainedMessages[0].MessageAttachments)
					{
						var supportingDocument = entryHeader.Declaration.AllEDocs.GetFromUniqueKey(ediMessageAttach.EG_StorageDocsGuid.ToGuid());

						if (supportingDocument != null)
						{
							var attachmentFileName = GetValidDocumentName(ediMessageAttach);

							if (supportingDocument.FileName.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
							{
								Stream pdfDataStream = new MemoryStream(DocumentConverter.ConvertTIFToPDF(supportingDocument.ImageData));
								sgZipEntries.Add(new SGZipEntry(new ZipStream(Path.ChangeExtension(attachmentFileName, ".pdf"), pdfDataStream), string.Empty));
							}
							else
							{
								sgZipEntries.Add(new SGZipEntry(new ZipStream(attachmentFileName, supportingDocument.GetImageDataReader().CopyToSubStreamableStreamAndCloseStream()), string.Empty));
							}
						}
					}

					var isXML = interchange.ContainedMessages[0] is SGXmlEDIMessage;
					Stream interchangeDataStream = new MemoryStream(Encoding.ASCII.GetBytes(interchange.EI_InterchangeText));

					var fileExtension = isXML ? ".xml" : ".edi";
					sgZipEntries.Add(new SGZipEntry(new ZipStream(interchange.EI_InterchangeNum + fileExtension, interchangeDataStream), "isPayLoad"));

					CreateZip(sgZipEntries, fileStream);
				}
			}

			return fileName;
		}

		void CreateZip(List<SGZipEntry> entries, Stream outputStream)
		{
			using (ZipOutputStream s = new ZipOutputStream(outputStream))
			{
				s.IsStreamOwner = false;
				s.SetLevel(6); // 0 - store only to 9 - means best compression
				foreach (SGZipEntry entry in entries)
				{
					AddEntryToZip(entry.stream, entry.fileComment, s);
				}
			}
		}

		void AddEntryToZip(ZipStream zipStream, ZString entryComment, ZipOutputStream outputStream)
		{
			ZipEntry zipEntry = new ZipEntry(Path.GetFileName(zipStream.Filename));
			zipEntry.DateTime = Env.Time.CurrentLocalDateTime;
			if (!entryComment.IsEmpty)
			{
				zipEntry.Comment = entryComment;
			}

			zipStream.Stream.Position = 0;
			zipEntry.Size = zipStream.Stream.Length;

			outputStream.PutNextEntry(zipEntry);

			using (var reader = new BinaryReader(zipStream.Stream))
			{
				byte[] buffer;
				do
				{
					buffer = reader.ReadBytes(4096);
					outputStream.Write(buffer, 0, buffer.Length);
				}
				while (buffer.Length > 0);
			}
		}

		public class SGZipEntry
		{
			public SGZipEntry(ZipStream stream, string fileComment)
			{
				this.stream = stream;
				this.fileComment = fileComment;
			}
			public readonly ZipStream stream;
			public readonly string fileComment;
		}

		#endregion

		#region Helper

		protected BatchSGInterchangeHelper Helper
		{
			get { return helper ?? (helper = CreateBatchSGInterchangeHelperCore()); }
		}
		BatchSGInterchangeHelper helper;

		protected virtual BatchSGInterchangeHelper CreateBatchSGInterchangeHelperCore()
		{
			return new BatchSG4InterchangeHelper(Logger);
		}

		SGInterchangeSubmissionRetryHelper RetryHelper
		{
			get { return interchangeSubmissionRetryHelper ?? (interchangeSubmissionRetryHelper = new SGInterchangeSubmissionRetryHelper()); }
		}
		SGInterchangeSubmissionRetryHelper interchangeSubmissionRetryHelper;

		#endregion

		#region IsProduction

		protected bool IsInProduction => !SGCustomsDataRegistry.Instance.SendTestMessages.Value;

		#endregion
	}
}
