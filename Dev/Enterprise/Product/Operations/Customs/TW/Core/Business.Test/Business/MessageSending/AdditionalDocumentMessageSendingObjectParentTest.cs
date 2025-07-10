using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentMessageSendingObjectParent))]
	sealed class AdditionalDocumentMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new AdditionalDocumentMessageSendingObjectParent(declaration, MessageType);
		}

		[ExpectNoExceptions]
		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var testWrapper1 = new AdditionalDocumentMessageSendingObjectParent(declaration1, MessageType);
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
			var testWrapper2 = new AdditionalDocumentMessageSendingObjectParent(declaration2, MessageType);
			NUnit.Framework.Assert.That(testWrapper2.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(2));
		}

		ZString MessageType => MessageTypeList.Codes.ADM;
	}
}
