using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5167;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5167MessageSendingObject))]
	sealed class N5167MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			return new N5167MessageSendingObject(entryHeader);
		}

		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new N5167MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Create).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMessageType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new N5167MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.MessageType, NUnit.Framework.Is.EqualTo(MessageTypeList.Codes.IEA).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestActionList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var action = new N5167MessageSendingObject(entryHeader);
			var list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Create));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestGetNewValidation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new N5167MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Validation.GetType(), NUnit.Framework.Is.EqualTo(typeof(N5167MessageSendingObjectValidation)));
		}

		public void TestSerializeToMessageString()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var messageSendingObject = new N5167MessageSendingObject(entryHeader);
			var messageString = messageSendingObject.SerializeToMessageString();
			AssertXMLContains("<FunctionCode>9</FunctionCode>", messageString);
		}
	}
}
