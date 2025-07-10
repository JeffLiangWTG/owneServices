using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SE.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectCollection))]
sealed class NctsHeaderMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsHeaderMessageSendingObjectCollection>
{
	protected override NctsHeaderMessageSendingObjectCollection GetCollectionToTest() => new NctsHeaderMessageSendingObjectCollection(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection() => new NctsHeaderMessageSendingObject(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<EU.NCTS.Business.NctsHeader>();
	}

	EU.NCTS.Business.NctsHeader nctsHeader;
}
