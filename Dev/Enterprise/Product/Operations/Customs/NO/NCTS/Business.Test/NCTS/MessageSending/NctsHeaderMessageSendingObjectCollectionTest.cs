using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectCollection))]
sealed class NctsHeaderMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsHeaderMessageSendingObjectCollection>
{
	protected override NctsHeaderMessageSendingObjectCollection GetCollectionToTest() => new NctsHeaderMessageSendingObjectCollection(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection() => new NctsHeaderMessageSendingObject(CreateNewNctsHeader());

	NctsHeader CreateNewNctsHeader() => Factory.NewMoq<NctsHeader>().Object;
}
