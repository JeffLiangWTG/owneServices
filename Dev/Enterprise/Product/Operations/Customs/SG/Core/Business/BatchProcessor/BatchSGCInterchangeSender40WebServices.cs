using System;
using System.Collections.Generic;
using System.IO;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.MHUB.Mhx4Soap;
using Enterprise.Customs.SG.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class BatchSGCInterchangeSender40WebServices : BatchSGCInterchangeSender30
	{
		protected override bool UploadOneFile(string fileName, string remoteFile, EDIInterchange interchange)
		{
			Logger.Log("Sending via MHX4 direct SOAP...");

			MHAccessClient igorsAwesomeClient = GetIgorsClient();

			var isXML = interchange.ContainedMessages[0] is SGXmlEDIMessage;
			var messageType = PeekFile(interchange);

			try
			{
				var submitSuccess = igorsAwesomeClient.SubmitInterchangeFile(fileName, messageType, interchange.EI_InterchangeNum, isXML, attachmentsToAdd);
				attachmentsToAdd = null;
				return submitSuccess;
			}
			catch (AggregateException ex)
			{
				return LogProblem(ex);
			}
			catch (System.Net.WebException ex)
			{
				return LogProblem(ex);
			}
		}

		protected virtual MHAccessClient GetIgorsClient()
		{
			return new MHAccessClient(currentBrokerWrapper.Tradenetv4Password.GP_UserID,
				currentBrokerWrapper.Tradenetv4Password.CurrentDecryptedPassword,
				new MHUBSettingsProvider(),
				Logger);
		}

		bool LogProblem(Exception e)
		{
			var messageBody = "Problem uploading\r\n." + e.Message;
			var inner = e.InnerException;
			while (inner != null)
			{
				messageBody += inner.Message + "\r\n\r\n";
				inner = inner.InnerException;
			}

			Logger.LogWarning(messageBody);
			attachmentsToAdd = null;
			return false;
		}

		string PeekFile(EDIInterchange interchange)
		{
			if (interchange == null)
			{
				return "CUSDEC";    // TODO: how does this variable fit in with xml messaging...??
			}

			if (interchange.ContainedMessages.Count > 0)
			{
				var message = interchange.ContainedMessages[0];

				if (message is SGXmlEDIMessage)
				{
					return message.EM_MessageType + message.EM_MessageSubType;
				}
				else
				{
					var parsed = message.GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory);
					if (parsed != null)
					{
						if (parsed is Edifact.D09B.Messages.CUSDEC.CUSDECMessage)
						{
							return "CUSDEC";
						}

						if (parsed is Edifact.D09B.Messages.TCODEC.TCODECMessage)
						{
							return "TCODEC";
						}

						if (parsed is Edifact.D09B.Messages.CUSPMT.CUSPMTMessage)
						{
							return "CUSPMT";
						}
					}
				}
			}

			return "";
		}

		//PackageAttachmentInterchange
		protected override string PackageAttachmentInterchange(EDIInterchange interchange, string outputDirectory, string fileNameNaked)
		{
			attachmentsToAdd = new List<string>();

			var entryHeader = interchange.ContainedMessages[0].EM_LinkedObject as CusEntryHeader;

			if (entryHeader != null)
			{
				foreach (EDIMessageAttach ediMessageAttach in interchange.ContainedMessages[0].MessageAttachments)
				{
					var supportingDocument = entryHeader.Declaration.AllEDocs.GetFromUniqueKey(ediMessageAttach.EG_StorageDocsGuid.ToGuid());

					if (supportingDocument != null)
					{
						var attachmentFileName = GetValidDocumentName(ediMessageAttach);
						var attachmentFullFileName = Path.Combine(outputDirectory, attachmentFileName);

						attachmentsToAdd.Add(attachmentFullFileName);

						using (var fileStream = new FileStream(attachmentFullFileName, FileMode.Create))
						using (var imageDataStream = supportingDocument.GetImageDataReader())
						{
							imageDataStream.CopyTo(fileStream);
						}
					}
				}
			}

			var extension = interchange.ContainedMessages[0] is SGXmlEDIMessage ? ".xml" : ".edi";
			var fileName = Path.Combine(outputDirectory, fileNameNaked + extension);

			File.WriteAllText(fileName, interchange.EI_InterchangeText);

			return fileName;
		}

		List<string> attachmentsToAdd;

		protected override bool SubmitFile(string fileName, string remoteFile, string mailBox, string contentID)
		{
			attachmentsToAdd = null;
			return true;
		}
	}
}
