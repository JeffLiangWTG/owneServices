using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(DMOManifestHeaderMessageSendingObject))]
sealed class DMOManifestHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When header is null", () => new DMOManifestHeaderMessageSendingObject(null));
		AssertNoExceptionThrown("When header is not null", () => new DMOManifestHeaderMessageSendingObject(header));
	}

	public void TestCustomsLevel() => AssertEquals("Customs Level for Header", NODMOEDIMessageTypeList.Descriptions.TRA, messageSendingObject.CustomsLevel);

	public void TestBillNumber() => AssertEquals("BillNumber for Header", ZString.Empty, messageSendingObject.BillNumber);

	public void TestRepresentative() => AssertEquals("Representative for Header", ZString.Empty, messageSendingObject.Representative);

	public void TestConsignee() => AssertEquals("Consignee for Header", ZString.Empty, messageSendingObject.Consignee);

	protected override BusinessObject GetNewBusinessObject()
	{
		return messageSendingObject;
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<AsycudaManifestHeader>();
		messageSendingObject = new DMOManifestHeaderMessageSendingObject(header);
	}

	DMOManifestHeaderMessageSendingObject messageSendingObject;
	AsycudaManifestHeader header;
}
