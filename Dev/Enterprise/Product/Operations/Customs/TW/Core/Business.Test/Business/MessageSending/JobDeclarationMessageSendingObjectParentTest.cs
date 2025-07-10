using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectParent))]
	sealed class JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new JobDeclarationMessageSendingObjectParent(declaration, MessageType);
		}

		[ExpectNoExceptions]
		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var testWrapper1 = new JobDeclarationMessageSendingObjectParent(declaration1, MessageType);
			NUnit.Framework.Assert.That(testWrapper1.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(0));
			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			NUnit.Framework.Assert.That(testWrapper1.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(0));
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration2.CusEntryInstruction;
			var header1 = declaration2.CustomsEntryHeaders.AddNew();
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header1.CH_CEI_Instruction = entryInstruction.PK;
			header2.CH_CEI_Instruction = entryInstruction.PK;
			var testWrapper2 = new JobDeclarationMessageSendingObjectParent(declaration2, MessageType);
			NUnit.Framework.Assert.That(testWrapper2.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestSendingIEAObjectsCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryStatus = ZString.Empty;
			entry1.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryStatus = EntryStatusCodeList.Codes.IEM;
			entry2.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var testWrapper = new JobDeclarationMessageSendingObjectParent(declaration, MessageTypeList.Codes.IEA);
			NUnit.Framework.Assert.That(testWrapper.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestMessageSendingObjectICD()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CusEntryInstruction;
			header.CH_CEI_Instruction = instruction.PK;
			header.MergedLines.AddNew().InvoiceLines.AddNew();
			var messageSendingObject = new JobDeclarationMessageSendingObjectParent(declaration, MessageType);
			NUnit.Framework.Assert.That(messageSendingObject.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(1));
			var wrapper = messageSendingObject.SendingObjectsCollection[0];
			NUnit.Framework.Assert.That(wrapper, NUnit.Framework.Is.TypeOf(typeof(NX5105MessageSendingObject)));
		}

		[TestDate(2019, 03, 18)]
		[ExpectNoExceptions]
		public void TestPreSendValidation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var header = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CusEntryInstruction;
			header.CH_CEI_Instruction = instruction.PK;
			var parent = new JobDeclarationMessageSendingObjectParent(declaration, MessageType);
			parent.SelectedSendingObjects.Cast<MessageSendingObject>().ForEach(x =>
			{
				x.ShouldSend = true;
				x.Action = ActionCodeList.Codes.Create;
			}

			);
			instruction.CEI_DateForDuty = new ZDateTime(2019, 03, 17);
			parent.UpdateEntryInstructionDataForDutyIfNeeded();
			NUnit.Framework.Assert.That(instruction.CEI_DateForDuty, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 03, 17)));
			instruction.CEI_DateForDuty = new ZDateTime(2019, 03, 19);
			parent.UpdateEntryInstructionDataForDutyIfNeeded();
			NUnit.Framework.Assert.That(instruction.CEI_DateForDuty, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 03, 19)));
			parent.SelectedSendingObjects.Cast<MessageSendingObject>().ForEach(x =>
			{
				x.ShouldSend = false;
				x.Action = ActionCodeList.Codes.Create;
			}

			);
			instruction.CEI_DateForDuty = new ZDateTime(2019, 03, 17);
			parent.UpdateEntryInstructionDataForDutyIfNeeded();
			NUnit.Framework.Assert.That(instruction.CEI_DateForDuty, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 03, 17)));
			parent.SelectedSendingObjects.Cast<MessageSendingObject>().ForEach(x =>
			{
				x.ShouldSend = true;
				x.Action = ActionCodeList.Codes.Delete;
			}

			);
			instruction.CEI_DateForDuty = new ZDateTime(2019, 03, 17);
			parent.UpdateEntryInstructionDataForDutyIfNeeded();
			NUnit.Framework.Assert.That(instruction.CEI_DateForDuty, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 03, 17)));
		}

		[ExpectNoExceptions]
		public void TestSendingADMObjectsCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryStatus = ZString.Empty;
			entry1.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryStatus = "ADD";
			entry2.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var testWrapper = new JobDeclarationMessageSendingObjectParent(declaration, MessageTypeList.Codes.ADM);
			NUnit.Framework.Assert.That(testWrapper.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestMessageType()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var testWrapper1 = new JobDeclarationMessageSendingObjectParent(declaration1, MessageType);
			NUnit.Framework.Assert.That(MessageType, NUnit.Framework.Is.EqualTo(testWrapper1.MessageType));
		}

		[ExpectNoExceptions]
		public void TestBizObjValidationMessageErrors()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTestSendingObject>();
			declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("MessageType Add Message Error");
			}

			;
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_DeclarationIncoterm = "CFR";
			var instruction = declaration.CusEntryInstruction;
			header.CH_CEI_Instruction = instruction.PK;
			var testWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration, MessageType);
			var messageSendingObject = testWrapper.SendingObjectsCollection.Cast<MessageSendingObjectForTesting>().FirstOrDefault();
			messageSendingObject.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("Blue Message Error");
			}

			;
			messageSendingObject.ShouldSend = false;
			NUnit.Framework.Assert.That(testWrapper.BizObjValidationMessageErrors.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			messageSendingObject.ShouldSend = true;
			NUnit.Framework.Assert.That(testWrapper.BizObjValidationMessageErrors.ToString(), NUnit.Framework.Does.Contain("Blue Message Error"));
			NUnit.Framework.Assert.That(testWrapper.BizObjValidationMessageErrors.ToString(), NUnit.Framework.Does.Contain("MessageType Add Message Error"));
		}

		[ExpectNoExceptions]
		public void TestAdditionalWarnings()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTestSendingObject>();
			declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("MessageType Add Warning");
			}

			;
			var header = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CusEntryInstruction;
			header.CH_CEI_Instruction = instruction.PK;
			var testWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration, MessageType);
			var messageSendingObject = testWrapper.SendingObjectsCollection.Cast<MessageSendingObjectForTesting>().FirstOrDefault();
			messageSendingObject.ShouldSend = false;
			NUnit.Framework.Assert.That(testWrapper.AdditionalWarnings.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			messageSendingObject.ShouldSend = true;
			NUnit.Framework.Assert.That(testWrapper.AdditionalWarnings.ToString(), NUnit.Framework.Does.Contain("MessageType Add Warning"));
		}

		[ExpectNoExceptions]
		public void TestAllowSendWithError()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTestSendingObject>();
			declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("MessageType Add Warning");
			}

			;
			var testWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration, MessageType);
			testWrapper.AllowSendWithError = true;
			NUnit.Framework.Assert.That(testWrapper.AllowSendWithError, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			testWrapper.AllowSendWithError = false;
			NUnit.Framework.Assert.That(!testWrapper.AllowSendWithError, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		ZString MessageType => "ICD";
	}
}
