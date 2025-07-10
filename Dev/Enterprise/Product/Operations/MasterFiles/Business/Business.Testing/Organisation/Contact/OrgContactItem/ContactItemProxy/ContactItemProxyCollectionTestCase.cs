using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal abstract class ContactItemProxyCollectionTestCase<T, U> : NonPersistentBusinessObjectCollectionTestCase<T>
			where T : ContactItemProxyCollection<U>
			where U : ContactItemProxy
	{
	}
}
