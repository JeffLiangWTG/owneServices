using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HMessageManagerTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("N5101HMessage", messageManager.MessageFriendlyName);
		}

		public void TestBusinessObject()
		{
			AssertEquals(messageSendingObject, messageManager.BusinessObject);
		}

		public void TestCanSendOriginal()
		{
			Assert(messageManager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			Assert(!messageManager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			Assert(!messageManager.IsWaitingForResponse);
		}

		public void TestGenerateMessage()
		{
			var expected_EM_ApplicationReference = "TFD0386";
			var glbExternalPassword = Factory.NewWithValidTestData<GlbCompanyCredential>();
			glbExternalPassword.GP_PasswordType = PasswordTypesList.Codes.TVF;
			glbExternalPassword.GP_MailBoxID = expected_EM_ApplicationReference;
			glbExternalPassword.GP_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			messageSendingObject.ShouldSend = true;

			var messages = messageManager.GenerateOriginalMessages(messageSendingObject);
			var message = messages[0];
			var xml = new N5101HMessageBuilder().PopulateXml(messageSendingObject);

			CombineAssertions("EDIMessage", () =>
			{
				AssertEquals("Messages Count", 1, messages.Length);
				AssertEquals("EM_MessageText", xml, message.EM_MessageText);
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.FHM, message.EM_MessageType);
				AssertEquals("EM_LinkUniqueID", messageSendingObject.Header.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", messageSendingObject.Header.TableName, message.EM_LinkTable);
				AssertEquals("EM_IsTestMessage", true, message.EM_IsTestMessage);
				AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
				AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
				AssertEquals("EM_ApplicationReference", expected_EM_ApplicationReference, message.EM_ApplicationReference);
				AssertEquals("Bill Count", messageSendingObject.Bills.Count(), ((AsycudaMessage)message).Bills.Count());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendingObject);
			var sendingObjParent = new MessageSendingObjectParent(header);
			messageSendingObject = new N5101HMessageSendingObject(header, collection, sendingObjParent);
			messageManager = new N5101HMessageManager(messageSendingObject);
			SetUpCurrentCompanyOrgProxyTWVATOrgCusCode();
		}

		void SetUpCurrentCompanyOrgProxyTWVATOrgCusCode()
		{
			var orgCusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			orgCusCode.FillWithValidTestData();
			orgCusCode.OK_CustomsRegNo = "52889317";
			orgCusCode.OK_CodeType = "VAT";
			orgCusCode.OK_RN_NKCodeCountry = "TW";
		}

		N5101HMessageSendingObject messageSendingObject;
		N5101HMessageManager messageManager;
	}
}
