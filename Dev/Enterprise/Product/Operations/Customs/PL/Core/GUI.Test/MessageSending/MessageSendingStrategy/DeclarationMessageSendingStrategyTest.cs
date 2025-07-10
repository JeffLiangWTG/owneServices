using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.GUI.Testing;

abstract class DeclarationMessageSendingStrategyTest : BaseMessageSendingStrategyTest
{
	public void TestGetAttachmentSender()
	{
		var strategy = GetInstanceForTest(factory, declaration);
		var parent = new BaseMessageSendingObjectParent(declaration);
		AssertNotNull(strategy.GetAttachmentSender(parent));
	}

	public void TestGetMessageParent()
	{
		var strategy = GetInstanceForTest(factory, declaration);
		var parent = strategy.GetMessageParent();
		CombineAssertions(() =>
		{
			AssertNotNull(parent);
			AssertType<CustomsDeclarationMessageSendingObjectParent>(parent);
		});
	}

	public void TestGetSendingForm()
	{
		var strategy = GetInstanceForTest(factory, declaration);
		var parent = new CustomsDeclarationMessageSendingObjectParent(declaration);
		using (var form = strategy.GetSendingForm(parent))
		{
			CombineAssertions(() =>
			{
				AssertNotNull(form);
				AssertType<CustomsDeclarationSendingForm>(form);
			});
		}
	}
}
