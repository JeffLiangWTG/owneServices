using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class RetrospectiveQuotaRequestMessageSendingStrategyTest : BaseMessageSendingStrategyTest
{
	public void TestGetAttachmentSender()
	{
		var strategy = GetInstanceForTest(factory, declaration);
		var parent = new BaseMessageSendingObjectParent(declaration);
		AssertNull(strategy.GetAttachmentSender(parent));
	}

	public void TestGetMessageParent()
	{
		var strategy = GetInstanceForTest(factory, declaration);
		var parent = strategy.GetMessageParent();
		CombineAssertions(() =>
		{
			AssertNotNull(parent);
			AssertType<RetrospectiveQuotaRequestMessageSendingObjectParent>(parent);
		});
	}

	public void TestGetMessageSender()
	{
		var strategy = GetInstanceForTest(factory, declaration);
		var header = declaration.CustomsEntryHeaders.AddNew();
		var sendingObject = new BaseMessageSendingObject(header);
		var parent = new RetrospectiveQuotaRequestMessageSendingObjectParent(declaration);

		var messageSender = strategy.GetMessageSender(sendingObject, parent);
		CombineAssertions(() =>
		{
			AssertNotNull(messageSender);
			AssertType<AISRetrospectiveQuotaMessageSender>(messageSender);
		});
	}

	public void TestGetSendingForm()
	{
		var strategy = GetInstanceForTest(factory, declaration);
		var parent = new RetrospectiveQuotaRequestMessageSendingObjectParent(declaration);
		using (var form = strategy.GetSendingForm(parent))
		{
			CombineAssertions(() =>
			{
				AssertNotNull(form);
				AssertType<RetrospectiveQuotaRequestSendingForm>(form);
			});
		}
	}

	protected override BaseMessageSendingStrategy GetInstanceForTest(BusinessObjectFactory factory, JobDeclaration declaration)
		=> new RetrospectiveQuotaRequestMessageSendingStrategy(factory, declaration);
}
