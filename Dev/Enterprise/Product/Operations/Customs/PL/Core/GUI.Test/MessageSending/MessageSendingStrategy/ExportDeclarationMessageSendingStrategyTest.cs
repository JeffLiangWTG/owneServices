using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ExportDeclarationMessageSendingStrategyTest : DeclarationMessageSendingStrategyTest
{
	public void TestGetMessageSender()
	{
		var strategy = GetInstanceForTest(factory, declaration);

		var header = declaration.CustomsEntryHeaders.AddNew();
		var sendingObject = new BaseMessageSendingObject(header);
		var parent = new BaseMessageSendingObjectParent(declaration);

		var messageSender = strategy.GetMessageSender(sendingObject, parent);
		CombineAssertions(() =>
		{
			AssertNotNull(messageSender);
			AssertType<AESMessageSender>(messageSender);
		});
	}

	protected override BaseMessageSendingStrategy GetInstanceForTest(BusinessObjectFactory factory, JobDeclaration declaration)
		=> new ExportDeclarationMessageSendingStrategy(factory, declaration);
}
