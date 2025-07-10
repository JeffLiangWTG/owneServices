using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(MessageSendingObjectParent))]
sealed class MessageSendingObjectParentTest : NctsHeaderMessageSendingObjectParentTest
{
	public new void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new MessageSendingObjectParent(null));
	}

	public new void TestTopLevelBusinessObject()
	{
		AssertType<NctsHeader>(messageSendingObjectParent.TopLevelBusinessObject);
	}

	public void TestMessageSendingObjectProperties()
	{
		var testItem = messageSendingObjectParent.MessageSendingObjectProperties.ToList();

		CombineAssertions(() =>
		{
			const int expectedColumnsCount = 5;
			AssertEquals("Count", expectedColumnsCount, testItem.Count);

			AssertMessageSendingObjectProperty(testItem, AutoNctsHeaderMessageSendingObject.Schema.LRN, 160, true);
			AssertMessageSendingObjectProperty(testItem, MessageSendingObject.Schema.MovementReferenceNumber, 160, true);
			AssertMessageSendingObjectProperty(testItem, AutoNctsHeaderMessageSendingObject.Schema.MessageType, 100, true);
			AssertMessageSendingObjectProperty(testItem, MessageSendingObject.Schema.Description, 200, true);
			AssertMessageSendingObjectProperty(testItem, AutoNctsHeaderMessageSendingObject.Schema.ReleaseRequest, 100, true);
		});
	}

	void AssertMessageSendingObjectProperty(IEnumerable<MessageSendingObjectProperty> messageSendingObjectProperties, string propertyName, int expectedColumnWidth, bool expectedMandatory)
	{
		var property = messageSendingObjectProperties.Single(i => i.PropertyName == propertyName);
		AssertNotNull($"Should contain {propertyName}", property);
		AssertEquals($"{propertyName} ColumnWidth", expectedColumnWidth, property.ColumnWidth);
		AssertEquals($"{propertyName} IsMandatory", expectedMandatory, property.IsMandatory);
	}

	public void TestSendingObjectsCollection()
	{
		AssertType<MessageSendingObjectCollection>(messageSendingObjectParent.SendingObjectsCollection);
	}

	public void TestSendAndSaveMessages()
	{
		SetStaffData();

		var sendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().First();
		sendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.DEC;

		messageSendingObjectParent.SendAndSaveMessages();
		AssertEquals(1, nctsHeader.MovementHeader.Messages.Count);
	}

	public void TestGetBizObjValidationMessageErrorsWithSupplementary()
	{
		const string message = "Place Of Loading: [C0404] You have not entered a Place Of Loading.";

		var sendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().First();
		CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_PortOfPresentationCode = ZString.Empty;
			sendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.PRN;
			AssertContains(message, messageSendingObjectParent.BizObjValidationMessageErrors);

			sendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.INV;
			AssertNotContains(message, messageSendingObjectParent.BizObjValidationMessageErrors);

			nctsHeader.MovementHeader.BM_PortOfPresentationCode = "A";
			sendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.PRN;
			AssertNotContains(message, messageSendingObjectParent.BizObjValidationMessageErrors);
		});
	}

	public void TestSaveAndSaveMessages_ForMessageTypeAMD()
	{
		SetStaffData();
		var sendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().First();
		sendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.AMD;

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("MovementReferenceNumber for NctsHeader is empty", nctsHeader.MovementReferenceNumber);

			sendingObject.MovementReferenceNumber = "123";
			messageSendingObjectParent.SendAndSaveMessages();
			AssertEquals("NctsHeader MovementReferenceNumber is set from message sending object", "123", nctsHeader.MovementReferenceNumber);
		});
	}

	public void TestSendAndSaveMessages_ShouldCall_AssignUnassignedDeclarationGoodsItemNumbers()
	{
		SetStaffData();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var parent = new MessageSendingObjectParent(nctsHeader);
		var sendingObject = parent.SendingObjectsCollection.Cast<MessageSendingObject>().First();
		sendingObject.MessageType = DepartureMessageSendingObjectTypeList.Codes.DEC;
		var bill1 = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("The number is not assigned", 0, goodsItem1.BY_DeclarationGoodsItemNumber);
			parent.SendAndSaveMessages();
			AssertEquals("The number is assigned", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
		});
	}

	void SetStaffData()
	{
		var glbStaffCertificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		glbStaffCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		glbStaffCertificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
		certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
		certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Poland;
		certificate.XZ_ExpiryOrDueDate = new ZDateTime(2022, 1, 2);
		certificate.XZ_IssueDate = new ZDateTime(2022, 1, 1);
	}

	protected override BusinessObject GetNewBusinessObject() => messageSendingObjectParent;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObjectParent = new MessageSendingObjectParent(nctsHeader);
	}

	NctsHeader nctsHeader;
	MessageSendingObjectParent messageSendingObjectParent;
}
