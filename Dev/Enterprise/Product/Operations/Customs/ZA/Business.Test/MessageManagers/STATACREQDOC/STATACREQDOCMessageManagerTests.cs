using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageManagers.Testing
{
	sealed class STATACREQDOCMessageManagerTests : TestCaseWithFactory
	{
		[TestDate(2016, 04, 22, 01, 23, 01)]
		public void TestSendTestMessage()
		{
			using (ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fANCollection))
			{
				Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
				CombineAssertions("checking is test or is waiting for response", () =>
				{
					var objectParent = new STATACREQDOCSendingObjectParent(Factory);
					objectParent.SendingObjectsCollection.OfType<STATACREQDOCSendingObject>().ForEach(x => x.ShouldSend = true);
					var notification = new MessageNotificationCollector_ForTest();
					var messageManager = new STATACREQDOCMessageManager(objectParent, notification);
					notification.NextAnswer = false;
					messageManager.SendMessages();
					AssertEquals(@"This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.", notification.LastMessage);
					AssertEquals("Continue sending with warning?", notification.LastCaption);
				});
				Factory.Save();
				CombineAssertions("Successful Send of Test message", () =>
				{
					var objectParent = new STATACREQDOCSendingObjectParent(Factory);
					objectParent.SendingObjectsCollection.OfType<STATACREQDOCSendingObject>().ForEach(x => x.ShouldSend = true);
					var notification = new MessageNotificationCollector_ForTest();
					var messageManager = new STATACREQDOCMessageManager(objectParent, notification);
					notification.NextAnswer = true;
					messageManager.SendMessages();
					AssertEquals(@"Customs Statement Request (REQDOC) message for 3234002346 queued for sending", notification.LastMessage);
					AssertEquals("Message Sending Result", notification.LastCaption);
				});
				Factory.Save();
				AssertSentMessage(isTestMessage: true);
			}
		}

		[TestDate(2016, 04, 22, 01, 23, 01)]
		public void TestSendRealMessage()
		{
			using (ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fANCollection))
			{
				Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, false);
				CombineAssertions("Successful Send of real message", () =>
				{
					var objectParent = new STATACREQDOCSendingObjectParent(Factory);
					objectParent.SendingObjectsCollection.OfType<STATACREQDOCSendingObject>().ForEach(x => x.ShouldSend = true);
					var notification = new MessageNotificationCollector_ForTest();
					var messageManager = new STATACREQDOCMessageManager(objectParent, notification);
					messageManager.SendMessages();
					AssertEquals(@"Customs Statement Request (REQDOC) message for 3234002346 queued for sending", notification.LastMessage);
					AssertEquals("Message Sending Result", notification.LastCaption);
				});
				Factory.Save();
				AssertSentMessage(isTestMessage: false);
			}
		}

		void AssertSentMessage(ZBool isTestMessage)
		{
			var query = new ZQuery(ZArchitecture.Schema.EDIMessageSchema.EM_ApplicationCode, "ZAC");
			query.AddToFilter(ZArchitecture.Schema.EDIMessageSchema.EM_MessageType, "REQ");
			query.AddToFilter(ZArchitecture.Schema.EDIMessageSchema.EM_ReceiveTransmit, "TRX");
			query.AddToFilter(ZArchitecture.Schema.EDIMessageSchema.EM_Status, "QUE");
			query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
			var sentMessage = new BusinessObjectFactory().LoadTop1<REQDOCEDIMessage>(query);
			AssertNotNull("Sent Message", sentMessage);
			AssertEquals("EM_IsTestMessage", isTestMessage, sentMessage.EM_IsTestMessage);
			AssertEquals("UNH+1+REQDOC:D:99B:UN:ZZZ01'BGM+493+3234002346+9'DOC+493+3234002346'DTM+318:20160422:102'DTM+90:20160401:102'DTM+91:20160422:102'NAD+MS+51051342TST'LIN+1'UNT+9+1'", sentMessage.EM_MessageText);
			AssertEquals("51051342TST", ((IInterchangeSenderIdProvider)sentMessage).SenderID);
		}

		FinancialAccountNumberPortMapCollection fANCollection;
		protected override void SetUp()
		{
			base.SetUp();
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "TST", Core.Constants.CountryCodes.SouthAfrica);
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			Factory.Save();
			fANCollection = new FinancialAccountNumberPortMapCollection();
			var mapping = fANCollection.AddNew();
			mapping.OrganizationPK = testAgent.PK;
			mapping.CustomsOfficeCode = "BFN";
			mapping.FinancialAccountNumber = "3234002346";
			mapping.ImporterPays = true;
			mapping.AccountStartDay = 1;
		}
	}
}
