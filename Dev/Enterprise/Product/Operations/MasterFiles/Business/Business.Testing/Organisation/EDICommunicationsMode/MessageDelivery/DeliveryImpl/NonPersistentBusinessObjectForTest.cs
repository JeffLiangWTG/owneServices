using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Testing
{
	sealed class NonPersistentBusinessObjectForTest : NonPersistentBusinessObject
	{
		public NonPersistentBusinessObjectForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
