using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectCollection))]
	sealed class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingObjectCollection>
	{
		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			NUnit.Framework.Assert.That(!GetCollectionToTest().AllowNew, NUnit.Framework.Is.True);
		}

		protected override MessageSendingObjectCollection GetCollectionToTest()
		{
			return new MessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageSendingObjectForTest(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
		}

		JobDeclaration declaration;
		CusEntryHeader header;
	}
}
