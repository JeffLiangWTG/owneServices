using System.IO;
using System.IO.Compression;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.PL.Business.Constants;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.PL.Business;

public class AttachmentMessageSender : PLMessageSender
{
	public AttachmentMessageSender(BusinessObjectFactory factory, BaseMessageSendingObjectParent sendingObjectParent)
		: base(factory, sendingObjectParent)
	{
		this.declaration = Argument.NotNull(sendingObjectParent.ParentDeclaration, $"{nameof(sendingObjectParent)}.{nameof(BaseMessageSendingObjectParent.ParentDeclaration)}");
	}

	readonly JobDeclaration declaration;

	protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.PLCustomsPUESCEmailSystem;

	protected override ZString ApplicationReference => declaration.JE_DeclarationReference;

	protected override ZString MessageType => EdiMessageMessageType.Attachment;

	protected override void SendCore(EDIMessage message)
	{
		base.SendCore(message);

		message.EM_IsActive = true;
		message.EM_LinkedObject = declaration;
		message.EM_LinkUniqueID = declaration.PK;
		var source = new MemoryStream(GetZipMessage());
		message.SetEM_MessageTextOrDataSource(source);

		var declarationForUpdate = factory.Load<JobDeclaration>(declaration.PK);
		declarationForUpdate.AttachmentMessages.Add(message);
	}

	byte[] GetZipMessage()
	{
		byte[] byteArray;

		using (var memoryStream = new MemoryStream())
		{
			using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
			{
				AddXmlMessageToZipArchive(archive);
				AddEDocsToZipArchive(archive);
			}

			byteArray = memoryStream.ToArray();
		}
		return byteArray;
	}

	void AddXmlMessageToZipArchive(ZipArchive archive)
	{
		var xmlMessageFileName = "eZalaczniki.xml";
		var xmlMessageFile = archive.CreateEntry(xmlMessageFileName);

		IXmlMessageBuilder messageBuilder = new AttachmentsMessageBuilder(new AttachmentProvider(SendingObjectParent));
		var attachmentMessage = messageBuilder.GenerateXmlMessage().GetSerializedString();

		using (var entryStream = xmlMessageFile.Open())
		using (var streamWriter = new StreamWriter(entryStream))
		{
			streamWriter.Write(attachmentMessage);
		}
	}

	void AddEDocsToZipArchive(ZipArchive archive)
	{
		foreach (JobDeclarationMessageSendingEDocs eDoc in SendingObjectParent.EDocs)
		{
			var newEDocZipFileName = eDoc.Filename;
			var newEDocZipItem = archive.CreateEntry(newEDocZipFileName);

			foreach (IeDoc jobDeclarationEDocFile in declaration.DocManagerInfo.AllEDocs)
			{
				if (jobDeclarationEDocFile.FileName == newEDocZipFileName)
				{
					using (var entryStream = newEDocZipItem.Open())
					{
						jobDeclarationEDocFile.GetImageDataReader().CopyTo(entryStream);
					}
					break;
				}
			}
		}
	}
}
