using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AttachmentMessageSenderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Factory", "Value cannot be null.\r\nParameter name: factory", () => new AttachmentMessageSender(factory: null, sendingObjectParent: msgObjectParent));
			AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObjectParent", "Value cannot be null.\r\nParameter name: sendingObjectParent", () => new AttachmentMessageSender(Factory, sendingObjectParent: null));
		});
	}

	public void TestSend()
	{
		var factory = new BusinessObjectFactory();
		msgObjectParent.AllowSendWithError = false;
		new AttachmentMessageSender(factory, msgObjectParent).Send();

		var updatedDeclaration = factory.Load<JobDeclaration>(declaration.PK);
		var attachmentMessage = updatedDeclaration.AttachmentMessages[0];
		CombineAssertions(() =>
		{
			AssertEquals("1 Attachment Message should be generated for Declaration.AttachmentMessages", 1, updatedDeclaration.AttachmentMessages.Count);
			AssertNotNullOrEmpty("EM_MessageData should not be empty", attachmentMessage.EM_MessageData.ToUTF8());
			AssertEquals("EM_ApplicationCode should be PLM", EDIMessage.ApplicationCodes.PLCustomsPUESCEmailSystem, attachmentMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageOwner should be empty", ZString.Empty, attachmentMessage.EM_MessageOwner);
			AssertEquals("EM_MessageType should be ATT", EdiMessageMessageType.Attachment, attachmentMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should be empty for attachment", ZString.Empty, attachmentMessage.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit should be TRX", EDIInterchange.Direction.Transmit, attachmentMessage.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationReference should be same as JE_DeclarationReference", updatedDeclaration.JE_DeclarationReference, attachmentMessage.EM_ApplicationReference);
			AssertEquals("EM_Status should be QUE", EDIMessage.Status.Queued, attachmentMessage.EM_Status);
			AssertEquals("EM_SystemLastEditUser should be same as CurrentUser.GS_Code", GlbStaff.CurrentUser.GS_Code, attachmentMessage.EM_SystemLastEditUser);
			AssertNotNull("EM_SystemLastEditTimeUtc should not be empty", attachmentMessage.EM_SystemLastEditTimeUtc);
			AssertEquals("EM_SystemCreateUser should be same as CurrentUser.GS_Code", GlbStaff.CurrentUser.GS_Code, attachmentMessage.EM_SystemCreateUser);
			AssertNotNull("EM_SystemCreateTimeUtc should not be empty", attachmentMessage.EM_SystemCreateTimeUtc);
			AssertEquals("EM_GB should be same as CurrentBranch.PK", GlbBranch.CurrentBranch.PK, attachmentMessage.EM_GB);
			AssertEquals("EM_GE should be same as CurrentDepartment.PK", GlbDepartment.CurrentDepartment.PK, attachmentMessage.EM_GE);
			AssertEquals("EM_LinkedObject should be same as declaration", updatedDeclaration, attachmentMessage.EM_LinkedObject);
			AssertEquals("EM_LinkUniqueID should be same as declaration.PK", updatedDeclaration.PK, attachmentMessage.EM_LinkUniqueID);
			AssertEquals("EM_IsActive should be true", true, attachmentMessage.EM_IsActive);
			AssertEquals("EM_IsTestMessage should be true", true, attachmentMessage.EM_IsTestMessage);
			AssertEquals("EM_SendWithMessageErrors should be false", false, attachmentMessage.EM_SendWithMessageErrors);
		});
	}

	public void TestGetZipMessage()
	{
		var pdfFile1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "pdfFile1.pdf", Core.Constants.RefDocTypes.DocumentOfOrigin);
		declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "pdfFile2.pdf", Core.Constants.RefDocTypes.MiscellaneousDocument);
		var pdfFile3 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "pdfFile3.pdf", Core.Constants.RefDocTypes.RequestDocument);

		var msgObjectParentEdoc1 = msgObjectParent.EDocs.AddNew();
		msgObjectParentEdoc1.EDoc = pdfFile1.UniqueKey;
		var msgObjectParentEdoc2 = msgObjectParent.EDocs.AddNew();
		msgObjectParentEdoc2.EDoc = pdfFile3.UniqueKey;

		Factory.Save();

		var factory = new BusinessObjectFactory();
		msgObjectParent.AllowSendWithError = false;
		new AttachmentMessageSender(factory, msgObjectParent).Send();

		var updatedDeclaration = factory.Load<JobDeclaration>(declaration.PK);
		var zipMessage = updatedDeclaration.AttachmentMessages[0].EM_MessageData;

		using (var memoryStream = new MemoryStream(zipMessage))
		using (var zipArchive = new ZipArchive(memoryStream))
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZipArchive should have 3 files", 3, zipArchive.Entries.Count);
				AssertEquals("ZipArchive should contain eZalaczniki.xml and file should not be empty"
					, true, zipArchive.Entries.Any(entry => entry.Name == "eZalaczniki.xml" && entry.Length != 0));
				AssertEquals("ZipArchive should contain pdfFile1.pdf and file should not be empty"
					, true, zipArchive.Entries.Any(entry => entry.Name == "pdfFile1.pdf" && entry.Length != 0));
				AssertEquals("ZipArchive should contain pdfFile3.pdf and file should not be empty"
					, true, zipArchive.Entries.Any(entry => entry.Name == "pdfFile3.pdf" && entry.Length != 0));
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.Factory.Save();
		msgObjectParent = new CustomsDeclarationMessageSendingObjectParent(declaration);

		var glbStaffCertificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		glbStaffCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		glbStaffCertificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		glbStaffCertificate.Factory.Save();
	}

	JobDeclaration declaration;
	CustomsDeclarationMessageSendingObjectParent msgObjectParent;
}
