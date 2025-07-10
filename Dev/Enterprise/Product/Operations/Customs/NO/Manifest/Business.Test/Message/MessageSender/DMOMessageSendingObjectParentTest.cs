using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(DMOMessageSendingObjectParent))]
sealed class DMOMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DMOMessageSendingObjectParent(null));
		AssertNoExceptionThrown("When header is not null", () => new DMOMessageSendingObjectParent(header));
	}

	public void TestTopLevelBusinessObject()
	{
		AssertType<AsycudaManifestHeader>(messageSendingObjectParent.TopLevelBusinessObject);
	}

	public void TestSecurityCheckpointToSendWithMessageError()
	{
		AssertEquals(Env.Security.GlobalManifestSendWithMessageErrors, messageSendingObjectParent.SecurityCheckpointToSendWithMessageError);
	}

	public void TestSendingObjectsCollection()
	{
		_ = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			var sendingObjectCollection = messageSendingObjectParent.SendingObjectsCollection;
			AssertNotNull(sendingObjectCollection);
			AssertType<DMOMessageSendingObjectCollection>(sendingObjectCollection);
			AssertEquals("Collection Count", 3, sendingObjectCollection.Count);
			AssertEquals("DMOManifestHeaderMessageSendingObject count", 1, sendingObjectCollection.OfType<DMOManifestHeaderMessageSendingObject>().Count());
			AssertEquals("DMOBillMessageSendingObject count", 2, sendingObjectCollection.OfType<DMOBillMessageSendingObject>().Count());
		});
	}

	public void TestMessageSendingObjectProperties()
	{
		var messageSendingObjectProperties = messageSendingObjectParent.MessageSendingObjectProperties;

		CombineAssertions(() =>
		{
			AssertEquals("Columns Count", 5, messageSendingObjectProperties.Count());

			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DMOMessageSendingObject.Schema.ShouldSend, 100, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DMOMessageSendingObject.Schema.CustomsLevel, 200, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DMOMessageSendingObject.Schema.BillNumber, 200, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DMOMessageSendingObject.Schema.Representative, 200, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DMOMessageSendingObject.Schema.Consignee, 200, true);
		});
	}

	public void TestCreateAndSaveMessage()
	{
		AssertEquals("Message sender found", expected: false, messageSendingObjectParent.CreateAndSaveMessage());
	}

	void AssertMessageSendingObjectProperty(IEnumerable<MessageSendingObjectProperty> messageSendingObjectProperties, string propertyName, int expectedColumnWidth, bool expectedMandatory)
	{
		var property = messageSendingObjectProperties.Single(i => i.PropertyName == propertyName);
		AssertNotNull($"Should contain {propertyName}", property);
		AssertEquals($"{propertyName} ColumnWidth", expectedColumnWidth, property.ColumnWidth);
		AssertEquals($"{propertyName} IsMandatory", expectedMandatory, property.IsMandatory);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return messageSendingObjectParent;
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		messageSendingObjectParent = new DMOMessageSendingObjectParent(header);
	}

	AsycudaManifestHeader header;
	DMOMessageSendingObjectParent messageSendingObjectParent;
}
