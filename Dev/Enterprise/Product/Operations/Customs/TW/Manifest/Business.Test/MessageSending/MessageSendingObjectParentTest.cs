using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectParent))]
	sealed class MessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTopLevelBusinessObject()
		{
			AssertEquals(header, sendingObjParent.TopLevelBusinessObject);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertEquals(Env.Security.TWManifestSendWithMessageErrors, sendingObjParent.SecurityCheckpointToSendWithMessageError);
		}

		public void TestSendingObjectsCollection()
		{
			AssertNotNull(sendingObjParent.SendingObjectsCollection);
		}

		[TestDate(2023, 09, 13)]
		public void TestCheckCertificateExpired()
		{
			var messageErrorCertificateExpired = TW.Business.ValidationConstants.Declaration.CertificateExpired;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTestSendingObject>();
			var credential = header.ForwarderCredential;
			credential.GP_ExpiryDate = new ZDate(2023, 09, 13);
			var bill = header.Bills.AddNew();
			var sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			var messageErrors = sendingObjParent.BizObjValidationMessageErrors;
			AssertNotContains(messageErrorCertificateExpired, messageErrors);

			credential.GP_ExpiryDate = new ZDate(2023, 09, 12);
			sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			messageErrors = sendingObjParent.BizObjValidationMessageErrors;
			AssertContains(messageErrorCertificateExpired, messageErrors);
		}

		[TestDate(2023, 09, 13)]
		public void TestCheckCertificateWillExpire()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTestSendingObject>();
			var credential = header.ForwarderCredential;
			credential.GP_ExpiryDate = new ZDate(2023, 10, 14);
			var bill = header.Bills.AddNew();
			var sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			var additionalWarnings = sendingObjParent.AdditionalWarnings;
			AssertNotContains(TW.Business.ValidationConstants.Declaration.CertificateWillExpire(credential.GP_ExpiryDate.ToString("yyyy-MM-dd")), additionalWarnings);

			credential.GP_ExpiryDate = new ZDate(2023, 10, 13);
			sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			additionalWarnings = sendingObjParent.AdditionalWarnings;
			AssertContains(TW.Business.ValidationConstants.Declaration.CertificateWillExpire(credential.GP_ExpiryDate.ToString("yyyy-MM-dd")), additionalWarnings);

			credential.GP_ExpiryDate = new ZDate(2023, 09, 12);
			sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			additionalWarnings = sendingObjParent.AdditionalWarnings;
			AssertNotContains(TW.Business.ValidationConstants.Declaration.CertificateWillExpire(credential.GP_ExpiryDate.ToString("yyyy-MM-dd")), additionalWarnings);
		}

		public void TestAdditionalWarnings()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTestSendingObject>();
			var bill = header.Bills.AddNew();
			var sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			header.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("Add Header Warning");
			};
			bill.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("Add Bill Warning");
			};
			var messageSendingObject = sendingObjParent.SendingObjectsCollection.Cast<MessageSendingObjectForTestSendingObject>().FirstOrDefault();
			messageSendingObject.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("Sending Object Warning");
			};

			var additionalWarnings = sendingObjParent.AdditionalWarnings;
			AssertContains("Add Header Warning", additionalWarnings);
			AssertContains("Add Bill Warning", additionalWarnings);
			AssertContains("Sending Object Warning", additionalWarnings);
		}

		public void TestBizObjValidationMessageErrors()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTestSendingObject>();
			var bill = header.Bills.AddNew();
			var sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			header.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("Add Header Message Error");
			};
			bill.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("Add Bill Message Error");
			};
			var messageSendingObject = sendingObjParent.SendingObjectsCollection.Cast<MessageSendingObjectForTestSendingObject>().FirstOrDefault();
			messageSendingObject.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("Sending Object Message Error");
			};

			var messageErrors = sendingObjParent.BizObjValidationMessageErrors;
			AssertContains("Add Header Message Error", messageErrors);
			AssertContains("Add Bill Message Error", messageErrors);
			AssertContains("Sending Object Message Error", messageErrors);
		}

		public void TestMenuCaption()
		{
			AssertEquals("menu Caption", sendingObjParent.MenuCaption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new MessageSendingObjectParent(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			sendingObjParent = new MessageSendingObjectParent(header, "menu Caption");
		}

		AsycudaManifestHeader header;
		MessageSendingObjectParent sendingObjParent;
	}
}
