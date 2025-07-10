using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWMessage))]
	sealed class TWMessageTest : EDIMessageTest
	{
		[TestDate(2022, 12, 30)]
		[ExpectNoExceptions]
		public void TestSetFunctionalReferenceIDForLicensingMessage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Taiwan;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var message = GetNXMMessage(controllingMessageHeader.PK, MessageTypeList.Codes._101);
			Factory.Save();
			message.Reload();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.EM_MessageText, NUnit.Framework.Is.EqualTo("<a>Hello World 96944490002212300001 Hello World</a>").Using(CustomComparers.TypeComparison), "EM_MessageText");
				NUnit.Framework.Assert.That(((CusTWControllingMessageHeader)message.EM_LinkedObject).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002212300001").Using(CustomComparers.TypeComparison), "TW1_FunctionalReferenceId");
			});
		}

		[TestDate(2025, 01, 09)]
		[ExpectNoExceptions]
		public void TestSetCMLicensingReferenceIDForLicensingMessage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Taiwan;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			var controllingMessageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message.EM_Status = Status.Queued;
			message.EM_ReceiveTransmit = Direction.Transmit;
			message.EM_MessageType = MessageTypeList.Codes.ICD;
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			message.EM_IsTestMessage = TWCustomsDataRegistry.IsTestMode;
			message.EM_MessageText = $"<a>Hello World &lt;&lt;FUNCTIONAL REFERENCE ID PLACE HOLDER {controllingMessageHeader1.PK}&gt;&gt; Hello World &lt;&lt;FUNCTIONAL REFERENCE ID PLACE HOLDER {controllingMessageHeader2.PK}&gt;&gt; Hello World</a>";
			Factory.Save();
			message.Reload();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.EM_MessageText, NUnit.Framework.Is.EqualTo("<a>Hello World 96944490SW2501090001 Hello World 96944490SW2501090002 Hello World</a>").Using(CustomComparers.TypeComparison), "EM_MessageText");
				NUnit.Framework.Assert.That(controllingMessageHeader1.TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490SW2501090001").Using(CustomComparers.TypeComparison), "controllingMessageHeader1.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That(controllingMessageHeader2.TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490SW2501090002").Using(CustomComparers.TypeComparison), "controllingMessageHeader2.TW1_FunctionalReferenceId");
			});
		}

		[ExpectNoExceptions]
		public void TestEM_MessageInterpretationWithTWCAndTRX()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageType = "ECD";
			testMessage.EM_MessageText = N5203MessageHelperTest.TestXml;
			Factory.Save();
			var testHtml = N5203MessageHelperTest.TestHtml;
			var testXml = N5203MessageHelperTest.TestXml;
			NUnit.Framework.Assert.That(testMessage.EM_MessageInterpretation, NUnit.Framework.Is.EqualTo(testHtml).Using(CustomComparers.TypeComparison));
			testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageType = "ECD";
			testMessage.EM_MessageText = testXml;
			Factory.Save();
			NUnit.Framework.Assert.That(testMessage.EM_MessageInterpretation, NUnit.Framework.Is.Not.EqualTo(testHtml).Using(CustomComparers.TypeComparison));
			testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageType = "IEA";
			testMessage.EM_MessageText = testXml;
			Factory.Save();
			NUnit.Framework.Assert.That(testMessage.EM_MessageInterpretation, NUnit.Framework.Is.Not.EqualTo(testHtml).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 8, 5)]
		[ExpectNoExceptions]
		public void TestFillEntryNumberPlaceHolder()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var message1 = entryHeader.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message1.EM_Status = EDIMessage.Status.Sent;
			message1.EM_MessageType = "ICD";
			message1.EM_MessageText = "<A><!-- placeholder:EntryNumber --></A>";
			declaration.EntryNumber = "AAA";
			Factory.Save();
			NUnit.Framework.Assert.That(message1.EM_MessageText, NUnit.Framework.Is.EqualTo("<A>AAA</A>").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declaration.EntryNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			entryHeader.EntryNumber = "";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			message1 = entryHeader.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message1.EM_Status = EDIMessage.Status.Sent;
			message1.EM_MessageType = "ICD";
			message1.EM_MessageText = "<A><!-- placeholder:EntryNumber --></A>";
			Factory.Save();
			NUnit.Framework.Assert.That(message1.EM_MessageText, NUnit.Framework.Is.EqualTo("<A>BB  0912300001</A>").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("BB  0912300001").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_Style = "";
			entryHeader.EntryNumber = "";
			message1 = entryHeader.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message1.EM_Status = EDIMessage.Status.Sent;
			message1.EM_MessageType = "ICD";
			message1.EM_MessageText = "<A><!-- placeholder:EntryNumber --></A>";
			Factory.Save();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 8, 5)]
		[ExpectNoExceptions]
		public void TestFillEntryNumberPlaceHolder_WhenSendingLicensingMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";

			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders.AddNew();
			var message = controllingMessageHeaders.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageType = MessageTypeList.Codes._401;
			message.EM_MessageText = "<A><!-- placeholder:EntryNumber --></A>";
			Factory.Save();
			NUnit.Framework.Assert.That(message.EM_MessageText, NUnit.Framework.Is.EqualTo("<A>BB  0912300001</A>").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("BB  0912300001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestShouldLicensingMessageAllocateEntryNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._301), NUnit.Framework.Is.True, "301");
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._31A), NUnit.Framework.Is.True, "31A");
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._31D), NUnit.Framework.Is.True, "31D");
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._401), NUnit.Framework.Is.True, "401");
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._601), NUnit.Framework.Is.True, "601");
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._603), NUnit.Framework.Is.True, "603");
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._101), NUnit.Framework.Is.False, "101");
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._201), NUnit.Framework.Is.False, "201");
				NUnit.Framework.Assert.That(TWMessage.ShouldLicensingMessageAllocateEntryNumber(MessageTypeList.Codes._207), NUnit.Framework.Is.False, "207");
			});
		}

		[ExpectNoExceptions]
		public void TestApplicationCode()
		{
			var message = (TWMessage)GetNewBusinessObject();
			NUnit.Framework.Assert.That(message.EM_ApplicationCode, NUnit.Framework.Is.EqualTo(TWMessage.ApplicationCodes.TaiwanCustoms).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNewAndLoadType()
		{
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = "BBB";
			testMessage.EM_MessageType = "CCC";
			NUnit.Framework.Assert.That(testMessage.GetType(), NUnit.Framework.Is.EqualTo(typeof(EDIMessage)), "EDIMessage");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			testMessage = newFactory.Load<EDIMessage>(testMessage.PK);
			NUnit.Framework.Assert.That(testMessage.GetType(), NUnit.Framework.Is.EqualTo(typeof(TWMessage)), "TWMessage");
		}

		[ExpectNoExceptions]
		public void TestShouldUseNText()
		{
			var messageText = "<a>你好</a>";
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN2";
			testMessage.EM_MessageText = messageText;
			testMessage.EM_MessageType = "CCC";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			testMessage = newFactory.Load<TWMessage>(testMessage.PK);
			NUnit.Framework.Assert.That(testMessage.EM_MessageText, NUnit.Framework.Is.EqualTo(messageText).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsTranshipment()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.TRN;
			NUnit.Framework.Assert.That(testMessage.IsTranshipment, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = "XX";
			NUnit.Framework.Assert.That(!testMessage.IsTranshipment, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsTransferApplication()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.TRA;
			NUnit.Framework.Assert.That(testMessage.IsTransferApplication, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = "XX";
			NUnit.Framework.Assert.That(!testMessage.IsTransferApplication, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsLicensingMessageResponse()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes._102;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._202;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._302;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._32A;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._32D;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._402;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._602;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._901;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._902;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._903;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = "XX";
			NUnit.Framework.Assert.That(!testMessage.IsLicensingMessageResponse, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsLicensingMessageDeliveryNotification()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes._101;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._201;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._207;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._301;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._31A;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._31D;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._401;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._601;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = MessageTypeList.Codes._603;
			NUnit.Framework.Assert.That(testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
			testMessage.EM_MessageType = "XX";
			NUnit.Framework.Assert.That(!testMessage.IsLicensingMessageDeliveryNotification, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsCustomsDeliveryNotification()
		{
			var testMap = new Dictionary<string, bool>()
			{
				{ "ARM", false },
				{ "IEM", false },
				{ "RFM", false },
				{ "TRN", false },
				{ "UHC", false },
				{ "ECD", true },
				{ "ICD", true },
				{ "ADM", true },
				{ "IEA", true }
			};
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			foreach (var messageTypeAndExpected in testMap)
			{
				testMessage.EM_MessageType = messageTypeAndExpected.Key;
				NUnit.Framework.Assert.That(testMessage.IsCustomsDeliveryNotification, NUnit.Framework.Is.EqualTo(messageTypeAndExpected.Value), $"when {testMessage.EM_MessageType}");
			}
		}

		[ExpectNoExceptions]
		public void TestCusEntryHeader()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var message1 = entryHeader.Messages.AddNew();
			NUnit.Framework.Assert.That(message1.CusEntryHeader, NUnit.Framework.Is.Not.EqualTo(default(CusEntryHeader)));
			var message2 = Factory.New<TWMessage>();
			NUnit.Framework.Assert.That(message2.CusEntryHeader, NUnit.Framework.Is.EqualTo(default(CusEntryHeader)));
		}

		[ExpectNoExceptions]
		public void TestMessageCode()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.ARM;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("NX5106").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes.IEM;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("N5109").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes.RFM;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("N5107").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes.TRN;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("N5302").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes.UHC;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("N5168").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes._302;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("NX302").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes._402;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("NX402").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes._602;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("NX602").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes._32D;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("NX302_DN").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes._902;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("NX902").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes._903;
			NUnit.Framework.Assert.That(testMessage.MessageCode, NUnit.Framework.Is.EqualTo("NX903").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMessageTypeDescription()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.ARM;
			NUnit.Framework.Assert.That(testMessage.MessageTypeDescription, NUnit.Framework.Is.EqualTo("Agency Response Message").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes.IEM;
			NUnit.Framework.Assert.That(testMessage.MessageTypeDescription, NUnit.Framework.Is.EqualTo("Examination Required Notice").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes.RFM;
			NUnit.Framework.Assert.That(testMessage.MessageTypeDescription, NUnit.Framework.Is.EqualTo("Required Formalities Message").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes.TRN;
			NUnit.Framework.Assert.That(testMessage.MessageTypeDescription, NUnit.Framework.Is.EqualTo("Transshipment/Transit Permit").Using(CustomComparers.TypeComparison));
			testMessage.EM_MessageType = MessageTypeList.Codes.UHC;
			NUnit.Framework.Assert.That(testMessage.MessageTypeDescription, NUnit.Framework.Is.EqualTo("Unable to Handle Container").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			NUnit.Framework.Assert.That(TWMessage.TypeDecider.GetType(), NUnit.Framework.Is.EqualTo(typeof(TWMessageTypeDecider)));
		}

		[ExpectNoExceptions]
		public void TestEntryNumberAndEventType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew();
			message.EM_MessageType = MessageTypeList.Codes.ECD;
			message.EM_MessageText = TWXmlTestCaseWithFactory.GetTWNotification("SNT", "IMP", "TEST01", "NUM1");
			NUnit.Framework.Assert.That(message.EntryNumber, NUnit.Framework.Is.EqualTo("TEST01").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EntryType, NUnit.Framework.Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EventType, NUnit.Framework.Is.EqualTo("SNT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.InterchangeNumber, NUnit.Framework.Is.EqualTo("NUM1").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2019, 12, 31)]
		[ExpectNoExceptions]
		public void TestGetNumberFountainNumbersAndFillInPlaceHolders()
		{
			var expectedMessage = "<a>Hello World BBAA081230000100001 Hello World</a>";
			var messageText = "<a>Hello World &lt;&lt;FUNCTIONAL REFERENCE ID PLACE HOLDER&gt;&gt; Hello World</a>";
			var classification = Factory.NewWithValidTestData<Customs.Business.BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_TariffNum = "0000.00.00.00Y";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_CustomsOffice = "AA";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 05, 15);
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CC = classification.PK;
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CC = classification.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			Factory.Save();
			var message = entryHeader.Messages.AddNew();
			message.EM_MessageType = MessageTypeList.Codes.IEA;
			message.EM_ReceiveTransmit = Enterprise.Customs.TW.Business.TWMessage.Direction.Transmit;
			message.EM_ApplicationCode = "TWC";
			message.EM_Status = "QUE";
			message.EM_MessageNum = "TWIN2";
			message.EM_MessageText = messageText;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			message = newFactory.Load<TWMessage>(message.PK);
			NUnit.Framework.Assert.That(message.EM_MessageText, NUnit.Framework.Is.EqualTo(expectedMessage).Using(CustomComparers.TypeComparison));
			expectedMessage = "<a>Hello World BBAA081230000100002 Hello World</a>";
			message = entryHeader.Messages.AddNew();
			message.EM_MessageType = MessageTypeList.Codes.ADM;
			message.EM_ReceiveTransmit = Enterprise.Customs.TW.Business.TWMessage.Direction.Transmit;
			message.EM_ApplicationCode = "TWC";
			message.EM_Status = "QUE";
			message.EM_MessageNum = "TWIN2";
			message.EM_MessageText = messageText;
			Factory.Save();
			message = newFactory.Load<TWMessage>(message.PK);
			NUnit.Framework.Assert.That(message.EM_MessageText, NUnit.Framework.Is.EqualTo(expectedMessage).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2022, 06, 30)]
		[ExpectNoExceptions]
		public void TestGetNumberFountainNumbersAndFillInPlaceHolders_TRA()
		{
			var messageText = "<a>Hello World <!-- placeholder:EntryNumber --> Hello World</a>";

			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "BB";
			header.TW_BoxNumber = "123";
			Factory.Save();
			NUnit.Framework.Assert.That(header.EntryNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "EntryNumber should be empty. - should be [null] or [empty]");

			var message = header.Messages.AddNew();
			message.EM_MessageType = MessageTypeList.Codes.TRA;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message.EM_ApplicationCode = "TWC";
			message.EM_Status = "QUE";
			message.EM_MessageNum = "TWIN2";
			message.EM_MessageText = messageText;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			message = newFactory.Load<TWMessage>(message.PK);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(header.EntryNumber.ToString(), NUnit.Framework.Does.Contain("AABB11123"));
				NUnit.Framework.Assert.That(message.EM_MessageText.ToString(), NUnit.Framework.Does.Contain("<a>Hello World AABB11123"));
			});
		}

		[TestDate(2024, 1, 1)]
		[ExpectNoExceptions]
		public void TestGetNumberFountainNumbersAndFillInPlaceHolders_NXM()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Taiwan;
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			var nx101MessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var nx20101MessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var nx20107MessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var nx301MessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var nx301AXMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var nx301DNMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var nx401MessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var nx601MessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var nx603MessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();

			var nx101Message = GetNXMMessage(nx101MessageHeader.PK, MessageTypeList.Codes._101);
			var nx20101Message = GetNXMMessage(nx20101MessageHeader.PK, MessageTypeList.Codes._201);
			var nx20107Message = GetNXMMessage(nx20107MessageHeader.PK, MessageTypeList.Codes._207);
			var nx301Message = GetNXMMessage(nx301MessageHeader.PK, MessageTypeList.Codes._301);
			var nx301AXMessage = GetNXMMessage(nx301AXMessageHeader.PK, MessageTypeList.Codes._31A);
			var nx301DNMessage = GetNXMMessage(nx301DNMessageHeader.PK, MessageTypeList.Codes._31D);
			var nx401Message = GetNXMMessage(nx401MessageHeader.PK, MessageTypeList.Codes._401);
			var nx601Message = GetNXMMessage(nx601MessageHeader.PK, MessageTypeList.Codes._601);
			var nx603Message = GetNXMMessage(nx603MessageHeader.PK, MessageTypeList.Codes._603);
			Factory.Save();

			nx101Message.Reload();
			nx20101Message.Reload();
			nx20107Message.Reload();
			nx301Message.Reload();
			nx301AXMessage.Reload();
			nx301DNMessage.Reload();
			nx401Message.Reload();
			nx601Message.Reload();
			nx603Message.Reload();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That((nx101Message.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010001").Using(CustomComparers.TypeComparison), "nx101Message.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That((nx20101Message.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010002").Using(CustomComparers.TypeComparison), "nx20101Message.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That((nx20107Message.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010003").Using(CustomComparers.TypeComparison), "nx20107Message.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That((nx301Message.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010004").Using(CustomComparers.TypeComparison), "nx301Message.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That((nx301AXMessage.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010005").Using(CustomComparers.TypeComparison), "nx301AXMessage.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That((nx301DNMessage.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010006").Using(CustomComparers.TypeComparison), "nx301DNMessage.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That((nx401Message.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010007").Using(CustomComparers.TypeComparison), "nx401Message.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That((nx601Message.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010008").Using(CustomComparers.TypeComparison), "nx601Message.TW1_FunctionalReferenceId");
				NUnit.Framework.Assert.That((nx603Message.EM_LinkedObject as CusTWControllingMessageHeader).TW1_FunctionalReferenceId, NUnit.Framework.Is.EqualTo("96944490002401010009").Using(CustomComparers.TypeComparison), "nx603Message.TW1_FunctionalReferenceId");
			});
		}

		[ExpectNoExceptions]
		public void TestEM_Calc_MessageTypeCode()
		{
			var list = Factory.GetCachedValue<MessageTypeCodeList>();
			foreach (var code in list.GetAllCodes())
			{
				message.EM_MessageType = code;
				NUnit.Framework.Assert.That(message.EM_Calc_MessageTypeCode, NUnit.Framework.Is.EqualTo(list.GetDescriptionFromCode(code)).Using(CustomComparers.TypeComparison));
			}

			message.EM_MessageType = ZString.Empty;
			NUnit.Framework.Assert.That(message.EM_Calc_MessageTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestEM_Calc_MessageTypeDescription()
		{
			AssertMessageTypeDescription("", "", "");
			AssertMessageTypeDescription("X", "", "");
			AssertMessageTypeDescription("", "X", "");
			AssertMessageTypeDescription("ECD", "TRX", "出口報單");
			AssertMessageTypeDescription("ECD", "RCV", "訊息傳送狀態通知 - 出口報單");
			AssertMessageTypeDescription("ICD", "TRX", "單證合一進口報單");
			AssertMessageTypeDescription("ICD", "RCV", "訊息傳送狀態通知 - 單證合一進口報單");
			AssertMessageTypeDescription("ADM", "TRX", "檢附申辦文件訊息");
			AssertMessageTypeDescription("ADM", "RCV", "訊息傳送狀態通知 - 檢附申辦文件訊息");
			AssertMessageTypeDescription("IEA", "TRX", "進口貨物查驗申請書");
			AssertMessageTypeDescription("IEA", "RCV", "訊息傳送狀態通知 - 進口貨物查驗申請書");
			AssertMessageTypeDescription("ARM", "RCV", "單證合一核覆訊息");
			AssertMessageTypeDescription("ERM", "RCV", "出口貨物放行通知");
			AssertMessageTypeDescription("IEM", "RCV", "查驗貨物通知");
			AssertMessageTypeDescription("RFM", "RCV", "應補辦事項通知");
			AssertMessageTypeDescription("UHC", "RCV", "無法吊櫃通知");
			AssertMessageTypeDescription("TPC", "RCV", "進口貨物稅費繳納證兼匯款申請書");
			AssertMessageTypeDescription("TAD", "RCV", "國庫專戶存款收款書兼匯款申請書");
			AssertMessageTypeDescription("IRM", "RCV", "進口貨物放行通知");
			AssertMessageTypeDescription("RIA", "RCV", "審核結果核覆訊息");
			AssertMessageTypeDescription("RQA", "RCV", "檢疫核覆訊息");
			AssertMessageTypeDescription("RFD", "RCV", "食藥署查驗結果回覆訊息");
			AssertMessageTypeDescription("RWA", "RCV", "酒類查驗核覆訊息");
			AssertMessageTypeDescription("NLA", "RCV", "簽審通知訊息");
			AssertMessageTypeDescription("EIN", "RCV", "錯誤或異常通知訊息");
			AssertMessageTypeDescription("CAA", "TRX", "單證合一進口報單 (航材)");
		}

		[ExpectNoExceptions]
		void AssertMessageTypeDescription(ZString messageType, ZString receiveTransmit, ZString expected)
		{
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = receiveTransmit;
			NUnit.Framework.Assert.That(message.EM_Calc_MessageTypeDescription, NUnit.Framework.Is.EqualTo(expected));
		}

		[TestDate(2020, 8, 5)]
		[ExpectNoExceptions]
		public void TestCustomsNumberEnteredEventLogged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader.EntryNumber = "";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			var message1 = entryHeader.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message1.EM_Status = EDIMessage.Status.Sent;
			message1.EM_MessageType = "ICD";
			message1.EM_MessageText = "<A><!-- placeholder:EntryNumber --></A>";
			Factory.Save();
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("BB  0912300001").Using(CustomComparers.TypeComparison));
			var filteredLog = entryHeader.Logs.Find((x) => x.SL_SE_NKEvent == Events.CustomsNumberEntered.Code && x.IsInDatabase && x.SL_Reference == entryHeader.EntryNumber);
			NUnit.Framework.Assert.That(filteredLog.Count(), NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestMessageStreamFormatterType()
		{
			var message = Factory.NewWithValidTestData<TWMessageForTest>();
			NUnit.Framework.Assert.That(message.MessageStreamFormatterExposed, NUnit.Framework.Is.TypeOf<EDIMessageStreamFormatterForXml>());
		}

		[ExpectNoExceptions]
		public void TestFunctionalReferenceID()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108.xml");
			testMessage.EM_MessageType = "FHR";

			NUnit.Framework.Assert.That(testMessage.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("11233527SW1812100001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsManifestMessage()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			CombineAssertions(() =>
			{
				testMessage.EM_MessageType = "FHR";
				NUnit.Framework.Assert.That(testMessage.IsManifestMessage, NUnit.Framework.Is.True, "FHR");

				testMessage.EM_MessageType = "ICD";
				NUnit.Framework.Assert.That(!testMessage.IsManifestMessage, NUnit.Framework.Is.True, "ICD");

				testMessage.EM_MessageType = ZString.Empty;
				NUnit.Framework.Assert.That(!testMessage.IsManifestMessage, NUnit.Framework.Is.True, "Empty");
			});
		}

		[ExpectNoExceptions]
		public void TestIsManifestDeliveryNotification()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			CombineAssertions(() =>
			{
				testMessage.EM_MessageType = MessageTypeList.Codes.FHM;
				NUnit.Framework.Assert.That(testMessage.IsManifestDeliveryNotification, NUnit.Framework.Is.True, "FHM");

				testMessage.EM_MessageType = MessageTypeList.Codes.FCF;
				NUnit.Framework.Assert.That(testMessage.IsManifestDeliveryNotification, NUnit.Framework.Is.True, "FCF");

				testMessage.EM_MessageType = "ICD";
				NUnit.Framework.Assert.That(!testMessage.IsManifestDeliveryNotification, NUnit.Framework.Is.True, "ICD");

				testMessage.EM_MessageType = ZString.Empty;
				NUnit.Framework.Assert.That(!testMessage.IsManifestDeliveryNotification, NUnit.Framework.Is.True, "Empty");
			});
		}

		[ExpectNoExceptions]
		public void TestGetAdditionalRegisteredLinkedObjectTypes()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessageForTest>();
			NUnit.Framework.Assert.That(testMessage.AdditionalRegisteredLinkedObjectTypesExposed, NUnit.Framework.Is.EquivalentTo(new[] { typeof(CusInBondHeader), typeof(CusTWControllingMessageHeader), typeof(CusEntryNumber) }));
		}

		[ExpectNoExceptions]
		public void TestGoodsShipmentValidationCode()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessageForTest>();
			NUnit.Framework.Assert.That(testMessage.GoodsShipmentValidationCode.Any(), NUnit.Framework.Is.EqualTo(false));

			testMessage.IncomingMessageKeyInfomation.GoodsShipmentValidationCode.Add("A");
			NUnit.Framework.Assert.That(testMessage.GoodsShipmentValidationCode.Contains("A"), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestGoodsShipmentNameCode()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessageForTest>();
			NUnit.Framework.Assert.That(testMessage.GoodsShipmentNameCode.Any(), NUnit.Framework.Is.EqualTo(false));

			testMessage.IncomingMessageKeyInfomation.GoodsShipmentNameCode.Add("B");
			NUnit.Framework.Assert.That(testMessage.GoodsShipmentNameCode.Contains("B"), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestStatementCode()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessageForTest>();
			NUnit.Framework.Assert.That(testMessage.StatementCode.Any(), NUnit.Framework.Is.EqualTo(false));

			testMessage.IncomingMessageKeyInfomation.StatementCode.Add("C");
			NUnit.Framework.Assert.That(testMessage.StatementCode.Contains("C"), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestClearanceStatus()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessageForTest>();
			testMessage.EM_MessageType = "ERM";
			testMessage.IncomingMessageKeyInfomation.StatusNameCode = "C1";
			NUnit.Framework.Assert.That(testMessage.ClearanceStatus, NUnit.Framework.Is.EqualTo("C1").Using(CustomComparers.TypeComparison));

			testMessage.EM_MessageType = "FHM";
			testMessage.IncomingMessageKeyInfomation.StatusNameCode = "C2";
			NUnit.Framework.Assert.That(testMessage.ClearanceStatus, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			testMessage.EM_MessageType = "IRM";
			testMessage.IncomingMessageKeyInfomation.StatusNameCode = "C3M";
			NUnit.Framework.Assert.That(testMessage.ClearanceStatus, NUnit.Framework.Is.EqualTo("C3M").Using(CustomComparers.TypeComparison));
		}

		TWMessage GetNXMMessage(ZGuid linkUniqueID, ZString messageType)
		{
			var message = Factory.New<TWMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message.EM_Status = Status.Queued;
			message.EM_ReceiveTransmit = Direction.Transmit;
			message.EM_MessageType = messageType;
			message.EM_LinkUniqueID = linkUniqueID;
			message.EM_LinkTable = CusTWControllingMessageHeaderSchema.Constants.TableName;
			message.EM_IsTestMessage = TWCustomsDataRegistry.IsTestMode;
			message.EM_MessageText = "<a>Hello World &lt;&lt;FUNCTIONAL REFERENCE ID PLACE HOLDER&gt;&gt; Hello World</a>";
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.NewWithValidTestData<TWMessage>();
		}

		TWMessage message;
	}
}
