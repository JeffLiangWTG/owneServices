using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(MessageManager))]
sealed class MessageManagerTest : TestCaseWithFactory
{
	public void TestGetAllMessageManagers()
	{
		var messageSendingObjectParent = new MessageSendingObjectParent(declaration);
		AssertEquals(1, messageSendingObjectParent.SendingObjectsCollection.Count);
		AssertEquals(ZBool.True, messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend);
		var manager = new MessageManagerForTesting(messageSendingObjectParent);
		AssertSame("The TopLevelBizObjToManage should be the jobDeclaration", declaration, manager.TopLevelBizObjToManage);
		var sendingObject = manager.GetAllMessageManagers_Exposed()[0];
		AssertType<CUSDECMessageManager>("A CUSDECMessageManager should been created for sending object", sendingObject);
	}

	public void TestSendMessage()
	{
		const string bgmReferenceWithoutVersion = "11122233320240601000001";
		const string bgmRefVersion1 = $"{bgmReferenceWithoutVersion}01";
		const string bgmRefVersion2 = $"{bgmReferenceWithoutVersion}02";
		header.CH_BGMReference = bgmRefVersion1;

		var messageSendingObjectParent = new MessageSendingObjectParent(declaration);
		var manager = new MessageManagerForTesting(messageSendingObjectParent);
		manager.SendMessages();
		var finalMessage = header.Messages[0];
		CombineAssertions(() =>
		{
			AssertContains("Message contains BGMref with new version", bgmRefVersion2, finalMessage.EM_MessageText);
			AssertEquals("Header updated with BGMref with new version", bgmRefVersion2, header.CH_BGMReference);
		});
	}

	[TestDate(2023, 10, 19, 09, 30, 00)]
	public void TestSendMessage_DeclarantId()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgCusCode = orgHeader.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.OrganizationNumber;
		orgCusCode.OK_CustomsRegNo = "111222333";

		declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;

		AssertSendMessage_DeclarantId(ZDateTime.Empty, ZString.Empty, ZDateTime.Now, "1112223332023101900000101");

		var newDateForDuty = new ZDateTime(2024, 10, 19, 09, 30, 00);
		AssertSendMessage_DeclarantId(newDateForDuty, ZString.Empty, newDateForDuty, "1112223332024101900000101");
		AssertSendMessage_DeclarantId(newDateForDuty, "11122233320231019000001", newDateForDuty, "1112223332024101900000201");

		void AssertSendMessage_DeclarantId(ZDateTime dateForDuty, ZString bgmReference, ZDateTime expDateForDuty, ZString expBGMReference)
		{
			entryInstruction.CEI_DateForDuty = dateForDuty;
			header.CH_BGMReference = bgmReference;

			var messageSendingObjectParent = new MessageSendingObjectParent(declaration);
			var manager = new MessageManagerForTesting(messageSendingObjectParent);
			manager.SendMessages();

			var finalMessage = header.Messages[0];
			CombineAssertions($"When CEI_DateForDuty.Empty = {dateForDuty.IsEmpty} and CH_BGMReference: {bgmReference.IsEmpty}", () =>
			{
				AssertEquals("Header CH_BGMReference", expBGMReference, header.CH_BGMReference);
				AssertEquals("EntryInstruction CEI_DateForDuty", expDateForDuty, entryInstruction.CEI_DateForDuty);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		header = declaration.CustomsEntryHeaders.AddNew();
		header.EntryNumber = ZString.Empty;
		header.CH_EntryStatus = ZString.Empty;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		header.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = header.MergedLines.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}
	JobDeclaration declaration;
	CusEntryHeader header;
	CusEntryInstruction entryInstruction;

	#region Implementation

	class MessageManagerForTesting : MessageManager
	{
		public MessageManagerForTesting(MessageSendingObjectParent declarationWrapper) : base(declarationWrapper, new MessageNotificationCollector_ForTest())
		{
		}

		public SingleMessageManager[] GetAllMessageManagers_Exposed()
		{
			return GetAllMessageManagers();
		}
	}

	#endregion

}
