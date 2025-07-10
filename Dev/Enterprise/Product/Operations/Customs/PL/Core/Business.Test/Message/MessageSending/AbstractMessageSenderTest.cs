using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class AbstractMessageSenderTest : TestCaseWithFactory
{
	protected void AssertXmlMessage(ZString xmlMessage, ZString expectedRootElement, ZString[] expectedNodes)
	{
		AssertEquals("Message cannot be empty", false, xmlMessage.IsEmpty);
		AssertEquals("Message must start with Utf-8 Declaration", true, xmlMessage.Contains("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"));
		AssertEquals("Root element exists", true, xmlMessage.Contains($"<{expectedRootElement}"));
		foreach (var node in expectedNodes)
		{
			AssertEquals($"{node} should exist in message", true, xmlMessage.Contains(node));
		}
		AssertEquals("Signature node should exist", true, xmlMessage.Contains("<ds:Signature"));
		AssertEquals("Message should end with signature and closing root element", true, xmlMessage.Contains($"</ds:Signature></{expectedRootElement}>"));
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		Factory.Save();
		factory = new BusinessObjectFactory();

		sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
		sendingObject = (BaseMessageSendingObject)sendingObjectParent.SendingObjectsCollection.First();

		SetStaffData();
	}

	void SetStaffData()
	{
		var glbStaffCertificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		glbStaffCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		glbStaffCertificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		glbStaffCertificate.Factory.Save();
	}

	protected JobDeclaration declaration;
	protected CusEntryHeader entryHeader;
	protected BusinessObjectFactory factory;
	protected BaseMessageSendingObject sendingObject;
	protected BaseMessageSendingObjectParent sendingObjectParent;
}
