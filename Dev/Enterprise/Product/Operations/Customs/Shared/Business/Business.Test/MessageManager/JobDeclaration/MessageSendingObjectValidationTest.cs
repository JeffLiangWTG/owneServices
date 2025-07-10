using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class MessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public virtual void TestParent()
		{
			var testItem = new JobDeclarationMessageSendingObjectValidation(new JobDeclarationMessageSendingObject(Factory.New<CusEntryHeader>()));
			AssertNotNull(testItem.Parent);
			Assert(testItem.Parent is JobDeclarationMessageSendingObject);
		}
	}
}
