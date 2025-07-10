using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseMessageSendingObjectCollectionForTest : NonPersistentBusinessObjectCollection<BaseMessageSendingObjectForTest>
	{
		public BaseMessageSendingObjectCollectionForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BaseMessageSendingObjectForTest(Factory);
		}
	}
}
