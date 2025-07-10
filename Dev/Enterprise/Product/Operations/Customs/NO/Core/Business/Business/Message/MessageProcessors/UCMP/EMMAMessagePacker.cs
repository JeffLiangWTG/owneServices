using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

[assembly: UniversalCustomsEDIMessagePacker(ApplicationCodeList.Codes.NOCustomsEmma, typeof(Enterprise.Customs.NO.Business.EMMAMessagePacker))]

namespace Enterprise.Customs.NO.Business;

sealed class EMMAMessagePacker : IUniversalCustomsEDIMessagePacker
{
	static ZString PackCore(CusEntryHeader entryHeader, JobDeclaration declaration, EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		var headerAttributes = new Dictionary<string, string>();
		AttachInvoiceEDocsAndMostRecentSadhEDoc(entryHeader, message, headerAttributes, logger);
		if (AssignXmlFileNameAttribute(message, declaration, headerAttributes, logger) is { IsEmpty: false } errorMessage)
		{
			return errorMessage;
		}

		PopulateInterchangeHeaders(message, interchange, logger);
		PopulateInterchangeAttributes(interchange, headerAttributes, logger);
		PopulateInterchangeBody(entryHeader, message, interchange, headerAttributes, logger);

		logger.Log($"{LoggerPrefix} successfully for EDIMessage with PK: [{message.PK}]");
		return ZString.Empty;
	}

	static void AttachInvoiceEDocsAndMostRecentSadhEDoc(CusEntryHeader entryHeader, EDIMessage message,
		Dictionary<string, string> headerAttributes,
		LoggingInformation logger)
	{
		var documentProvider = new EmmaDocumentProvider(entryHeader);

		if (documentProvider.GetDocuments().ToArray() is not { Length: > 0 } eDocs)
		{
			return;
		}
		var filenamesToAdd = new List<ZString>();
		foreach (var eDoc in eDocs)
		{
			var filename = eDoc.FileName;
			logger.DebugLog($"{LoggerPrefix} adding eDoc attachment with FileName: [{filename}] to EDIMessage with PK: [{message.PK}]");
			filenamesToAdd.Add(filename);
			var messageAttach = message.MessageAttachments.AddNew();
			messageAttach.EG_StorageDocsGuid = eDoc.DocumentId;
		}
		headerAttributes.Add(AttachmentFilenamesAttribute, string.Join(", ", filenamesToAdd));
	}

	static void PopulateInterchangeHeaders(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		logger.DebugLog($"{LoggerPrefix} adding headers to EDIInterchange with PK: [{interchange.PK}]");
		var sessionGuid = ZGuid.NewZGuid();
		EDIMessagePackerUtils.PopulateInterchange(
			interchange,
			ApplicationCodeList.Codes.NOCustomsEmma,
			message.EM_MessageType,
			message.ExternalPassword?.Company?.LicenceKeyIdentifier ?? GlbCompany.CurrentCompany.LicenceKeyIdentifier,
			EmmaMessageSupportedTarget,
			message.EM_GB,
			message.EM_GP,
			sessionGuid);
	}

	static void PopulateInterchangeAttributes(EDIInterchange interchange, Dictionary<string, string> headerAttributes, LoggingInformation logger)
	{
		logger.DebugLog($"{LoggerPrefix} adding attributes to EDIInterchange with PK: [{interchange.PK}]");
		interchange.SetHeaderTextWithAttributeDictionary(headerAttributes);
	}

	static void PopulateInterchangeBody(CusEntryHeader entryHeader, EDIMessage message, EDIInterchange interchange,
		Dictionary<string, string> headerAttributes,
		LoggingInformation logger)
	{
		logger.DebugLog($"{LoggerPrefix} adding body to EDIInterchange with PK: [{interchange.PK}]");

		interchange.ContainedMessages.Add(message);

		var stream = new MemoryStream();
		var writer = new StreamWriter(stream);

		if (!message.EM_MessageText.IsEmpty)
		{
			writer.Write(ConstructXMLHeaderMIMEText(headerAttributes[FileNameAttribute]));
			writer.Flush();
			writer.Write(message.EM_MessageText);
			writer.WriteLine();
			writer.Flush();
		}

		var attachmentsContent = new List<string>();
		AttachInvoiceEDocsDataAndMostRecentSadhEDocData(entryHeader, attachmentsContent);
		foreach (var attachmentContent in attachmentsContent)
		{
			writer.Write(attachmentContent);
			writer.Flush();
		}

		writer.WriteLine();
		writer.WriteLine("--MIME_boundary--");

		writer.Flush();
		stream.Position = 0;

		interchange.SetEI_BodyDataSource(new StreamSource(stream));
	}

	static void AttachInvoiceEDocsDataAndMostRecentSadhEDocData(CusEntryHeader entryHeader, List<string> attachmentsContent)
	{
		var documentProvider = new EmmaDocumentProvider(entryHeader);

		if (documentProvider.GetDocuments().ToArray() is not { Length: > 0 } eDocs)
		{
			return;
		}

		foreach (var eDoc in eDocs)
		{
			var filename = eDoc.FileName;
			using var stream = eDoc.Document.GetImageDataReader();
			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			byte[] fileData = ms.ToArray();
			var attachmentContent = ConstructDocMIMEAttachment(filename, eDoc.DocumentId.ToString(), fileData);
			attachmentsContent.Add(attachmentContent);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "MIME strings")]
	static string ConstructDocMIMEAttachment(string fileName, string fileId, byte[] fileData)
	{
		var result = new ZStringBuilder();
		var attachmentText = Convert.ToBase64String(fileData);

		result.AppendLine();
		result.AppendLine("--MIME_boundary");
		result.AppendLine("Content-Type: application/pdf");
		result.AppendLine($"Content-ID: <{fileId}>");
		result.AppendLine($"Content-Disposition: attachment; filename={fileName} ");
		result.AppendLine("Content-Transfer-Encoding: base64");
		result.AppendLine();
		result.AppendLine(attachmentText);

		return (result.ToString());
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "MIME strings")]
	static string ConstructXMLHeaderMIMEText(string fileName)
	{
		var result = new ZStringBuilder();

		result.AppendLine("MIME-Version: 1.0");
		result.AppendLine("Content-Type: Multipart/Related; boundary=\"MIME_boundary\"");
		result.AppendLine();
		result.AppendLine("--MIME_boundary");
		result.AppendLine("Content-Type: Application/xml");
		result.AppendLine("Content-ID: <xml_manifest>");
		result.AppendLine($"Content-Disposition: inline; filename={fileName}");
		result.AppendLine();

		return result.ToString();
	}

	static ZString AssignXmlFileNameAttribute(EDIMessage message, JobDeclaration declaration, Dictionary<string, string> headerAttributes, LoggingInformation logger)
	{
		if (declaration.DeclarantCode.IsEmpty)
		{
			var errorMessage = $"{LoggerPrefix} found empty DeclarantCode for EDIMessage with PK: [{message.PK}]";
			logger.LogError(errorMessage);
			return errorMessage;
		}

		headerAttributes.Add(FileNameAttribute, $"{declaration.DeclarantCode}.xml");
		return ZString.Empty;
	}

	#region IUniversalCustomsEDIMessagePacker

	bool IUniversalCustomsEDIMessagePacker.AllowEmptyMessageBody => false;

	ZString IUniversalCustomsEDIMessagePacker.Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		_ = Argument.NotNull(message, nameof(message));
		_ = Argument.NotNull(interchange, nameof(interchange));
		_ = Argument.NotNull(logger, nameof(logger));
		using var traceLogger = new TraceLogger($"{LoggerPrefix} for EDIMessage with PK: [{message.PK}]", logger);

		if (!SanityCheckLinkedObject(message, logger, out var entryHeader, out var declaration))
		{
			return (NoResString)"Linked object should be a CusEntryHeader with Declaration";
		}

		return PackCore(entryHeader, declaration, message, interchange, logger);
	}

	static bool SanityCheckLinkedObject(EDIMessage message, LoggingInformation logger, out CusEntryHeader entryHeader, out JobDeclaration declaration)
	{
		entryHeader = message.EM_LinkedObject as CusEntryHeader;
		if (entryHeader is null)
		{
			declaration = null;
			logger.LogError($"{LoggerPrefix} Linked object is not a type of CusEntryHeader for EDIMessage with PK: [{message.PK}]");
			return false;
		}
		declaration = entryHeader.Declaration;
		if (declaration is null)
		{
			logger.LogError($"{LoggerPrefix} Linked CusEntryHeader: [{entryHeader.PK}] is not linked to a JobDeclaration for EDIMessage with PK: [{message.PK}]");
			return false;
		}
		return true;
	}

	#endregion

	static string LoggerPrefix => (NoResString)"NO EMMA Message Packer";
	const string AttachmentFilenamesAttribute = "custom.NO.AttachmentFilenames";
	const string EmmaMessageSupportedTarget = "NOCEmma";
	const string FileNameAttribute = "custom.NO.XmlFileName";
}
