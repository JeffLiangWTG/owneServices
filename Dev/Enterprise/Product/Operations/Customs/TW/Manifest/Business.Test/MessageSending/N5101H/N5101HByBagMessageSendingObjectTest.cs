using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(N5101HByBagMessageSendingObject))]
	sealed class N5101HByBagMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendingObject);
			var sendingObjParent = new MessageSendingObjectParent(header);
			return new N5101HByBagMessageSendingObject(header, collection, sendingObjParent);
		}
	}
}
