using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectParent))]
sealed class NctsHeaderMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageSendingObjectProperties_Arrival()
	{
		var parent = (NctsHeaderMessageSendingObjectParent)GetNewBusinessObject();
		var properties = parent.MessageSendingObjectProperties;

		CombineAssertions("MessageSendingObjectProperties", () =>
		{
			AssertEquals("Properties count", 4, properties.Count());
			AssertEquals("LRN", properties.ElementAt(0).PropertyName);
			AssertEquals("MRN", properties.ElementAt(1).PropertyName);
			AssertEquals("MessageType", properties.ElementAt(2).PropertyName);
			AssertEquals("MessageTypeDescription", properties.ElementAt(3).PropertyName);
		});
	}

	public void TestMessageSendingObjectProperties_Departure()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var parent = (NctsHeaderMessageSendingObjectParent)GetNewBusinessObject();
		var properties = parent.MessageSendingObjectProperties;

		CombineAssertions("MessageSendingObjectProperties", () =>
		{
			AssertEquals("Properties count", 5, properties.Count());
			AssertEquals("LRN", properties.ElementAt(0).PropertyName);
			AssertEquals("MRN", properties.ElementAt(1).PropertyName);
			AssertEquals("MessageType", properties.ElementAt(2).PropertyName);
			AssertEquals("MessageTypeDescription", properties.ElementAt(3).PropertyName);
			AssertEquals("MessageStatus", properties.ElementAt(4).PropertyName);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObjectParent(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
	}

	NctsHeader nctsHeader;
}
