using System.IO;
using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.SG.MHUB;
using Enterprise.Customs.SG.MHUB.MHX;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class BatchSGCInterchangeSender40Script : BatchSGCInterchangeSender30
	{
		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			Logger.Log("Sending via MHX4 scripting...");
			Helper.VerboseLog(Logger, "Get Valid Broker Mailboxes...");

			foreach (var broker in Helper.GetValidBrokerMailboxes())
			{
				Helper.VerboseLog(Logger, "Broker: " + broker.GS_FullName);

				currentBrokerWrapper = SGGlbStaffWrapper.Get(broker);
				Logger.Log("Sending messages for " + broker.GS_FullName + " - (TradeNet UserID: " + currentBrokerWrapper.Tradenetv4Password.GP_UserID + ").");

				SendOutboundInterchanges(EDIInterchange.ApplicationCodes.SingaporeTradenet4, token);
				SendOutboundInterchanges(EDIInterchange.ApplicationCodes.SGCustomsTradenetXML, token);
			}

			Logger.Log("Sending Complete");
			Logger.AddBlankLine();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		protected override bool UploadAndSubmitFile(bool isProduction, string fileNameToUpload, EDIInterchange ignoreInt)
		{
			// We do not do the folder permissions checking stuff here.  We assume that Retrieve is done before Send.
			var mailbox = isProduction ? SGCustomsDataRegistry.Instance.RecipientMailbox.Value : SGBatchProcessorConstants.Test.RecipientMailBox;

			var creator = new Mhx4ProfileCreator(currentBrokerWrapper.Tradenetv4Password);
			creator.CreateInputScript_Upload(new FileInfo(fileNameToUpload).Name, mailbox, Logger);
			creator.CreateProfileFile(new MHUBSettingsProvider());

			var process = new System.Diagnostics.Process();
			process.EnableRaisingEvents = false;
			process.StartInfo.FileName = Helper.GetPathToMhaccessTwoWays();
			process.StartInfo.WorkingDirectory = creator.ProfileFile.DirectoryName;
			process.StartInfo.Arguments = creator.ProfileFile.FullName;

			process.Start();
			process.WaitForExit(SGCustomsDataRegistry.Instance.FTPProcessTimeout.Value);

			var responseChecker = new MhxScriptSuccessChecker(creator, MHUBConstants.CommandType.Submit);
			var responseFlag = responseChecker.ResponseSuccessFlag;
			if (responseFlag != MHUBConstants.ResponseStatusCodes.CompleteSuccess)
			{
				Logger.LogWarning(responseChecker.AllOutput);
			}

			return responseFlag == MHUBConstants.ResponseStatusCodes.CompleteSuccess;
		}

		protected override string GetBrokerFilePath(ZString userID)
		{
			return Path.Combine(Env.TempPath, userID);
		}

		protected override string PackageAttachmentInterchange(EDIInterchange interchange, string outputDirectory, string fileNameNaked)
		{
			// Don't zip 'em, just write 'em out to disk. MHAccess will do the zipping.
			var extension = interchange.ContainedMessages[0] is SGXmlEDIMessage ? ".xml" : ".edi";
			var fileName = Path.Combine(outputDirectory, fileNameNaked + extension);

			if (SendIntToFile(interchange, outputDirectory, fileNameNaked + extension))
			{
				var entryHeader = interchange.ContainedMessages[0].EM_LinkedObject as CusEntryHeader;

				if (entryHeader != null)
				{
					foreach (EDIMessageAttach ediMessageAttach in interchange.ContainedMessages[0].MessageAttachments)
					{
						var attachmentsFolder = fileName + Mhx4ProfileCreator.AttachmentsFolderName;
						Directory.CreateDirectory(attachmentsFolder);

						var supportingDocument = entryHeader.Declaration.AllEDocs.GetFromUniqueKey(ediMessageAttach.EG_StorageDocsGuid.ToGuid());

						if (supportingDocument != null)
						{
							// Version 4 doesn't bother to convert TIF to PDF, BP said it's not (no longer?) necessary.
							var attachmentFileName = GetValidDocumentName(ediMessageAttach);
							attachmentFileName = attachmentFileName.Replace(Mhx4ProfileCreator.DelimiterForMultipleFilenames, string.Empty); // Comma is delimiter in the script file

							using (var fileStream = new FileStream(Path.Combine(attachmentsFolder, attachmentFileName), FileMode.Create))
							using (var imageDataStream = supportingDocument.GetImageDataReader())
							{
								imageDataStream.CopyTo(fileStream);
							}
						}
					}
				}
			}

			return fileName;
		}

		protected new BatchSG4InterchangeHelper Helper => (BatchSG4InterchangeHelper)base.Helper;
	}
}
