using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(MessageSendingObjectCollection))]
sealed class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingObjectCollection>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new MessageSendingObjectCollection(null));
	}

	protected override MessageSendingObjectCollection GetCollectionToTest()
	{
		return new MessageSendingObjectCollection(nctsHeader);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new MessageSendingObject(nctsHeader);
	}

	public override void TestAdd()
	{
		Assert("MessageSendingObjectCollection does not support adding.", true);
	}

	public override void TestDelete()
	{
		Assert("MessageSendingObjectCollection does not support deleting.", true);
	}

	public override void TestRemoveFromRelationship()
	{
		Assert("MessageSendingObjectCollection does not support RemoveFromRelationship.", true);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
	}

	NctsHeader nctsHeader;
}
