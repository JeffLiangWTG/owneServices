using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(DMOMessageSendingObjectCollection))]
sealed class DMOMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DMOMessageSendingObjectCollection>
{
	public void TestAllowNew()
	{
		Assert("AllowNew should be false", !GetCollectionToTest().AllowNew);
	}

	protected override DMOMessageSendingObjectCollection GetCollectionToTest()
	{
		return new DMOMessageSendingObjectCollection(Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		return new DMOManifestHeaderMessageSendingObject(header);
	}
}
