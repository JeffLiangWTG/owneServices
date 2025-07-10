using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class MessageSendingActionCollectionBaseTest<T, T1, T2> : NonPersistentBusinessObjectCollectionTestCase<T>
			where T : MessageSendingActionCollectionBase<T1, T2>
			where T1 : NonPersistentBusinessObject, IMessageSendingActionBase
			where T2 : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		public void TestCollectionIsPopulated()
		{
			var coll = GetCollectionToTest();
			AssertEquals(0, coll.Count);

			HostWrapper.DISDocuments.AddNew();
			coll = GetCollectionToTest();
			AssertEquals(1, coll.Count);

			Assert("Should not allow new. System populates collection for users", !coll.AllowNew);
		}

		public abstract void TestSendMessages();

		protected abstract DISHostWrapperBase<T2> HostWrapper { get; }
		protected abstract BusinessObject JobDeclaration { get; }
	}
}
