using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(MessageSendingActionCollection))]
sealed class MessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingActionCollection>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new MessageSendingActionCollection(null));
	}

	public void TestAddMessageSendingActions_Departure()
	{
		var collection = new MessageSendingActionCollection(nctsHeader);
		AssertType<NctsDepartureMovementHeader>(collection[0].NctsHeader.MovementHeader);
	}

	public void TestAddMessageSendingActions_Arrival()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var collection = new MessageSendingActionCollection(nctsHeader);
		AssertType<NctsArrivalMovementHeader>(collection[0].NctsHeader.ArrivalMovementHeader);
	}

	public override void TestAdd()
	{
		Assert("MessageSendingActionCollection does not support adding.", true);
	}

	public override void TestDelete()
	{
		Assert("MessageSendingActionCollection does not support deleting.", true);
	}

	public override void TestRemoveFromRelationship()
	{
		Assert("MessageSendingActionCollection does not support RemoveFromRelationship.", true);
	}

	protected override MessageSendingActionCollection GetCollectionToTest() => new MessageSendingActionCollection(nctsHeader);

	protected override BusinessObject GetNewElementToAddToTheCollection() => new MessageSendingActionParent(nctsHeader).SendingObjectsCollection.AddNew();

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
	}

	NctsHeader nctsHeader;
}
