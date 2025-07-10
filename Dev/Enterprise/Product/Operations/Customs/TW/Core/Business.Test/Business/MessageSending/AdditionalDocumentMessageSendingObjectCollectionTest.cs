using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentMessageSendingObjectCollection))]
	sealed class AdditionalDocumentMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalDocumentMessageSendingObjectCollection>
	{
		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			NUnit.Framework.Assert.That(!GetCollectionToTest().AllowNew, NUnit.Framework.Is.True);
		}

		protected override AdditionalDocumentMessageSendingObjectCollection GetCollectionToTest()
		{
			return new AdditionalDocumentMessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AdditionalDocumentMessageSendingObject(header);
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
