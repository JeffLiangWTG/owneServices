using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TranshipmentMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendMessages()
		{
			SendAllMessage();
			foreach (TranshipmentMessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertEquals("message.EM_ApplicationCode is TWC", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("message.EM_Status is QUE", Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit is TRX", Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType is TRA", MessageTypeList.Codes.TRA, message.EM_MessageType);
				AssertEquals("message.EM_LinkUniqueID is not null", entry.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_LinkTable is CusInBondHeader", CusInBondHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("message.EM_ApplicationReference is " + cusHeader.BH_CustomsProfile, cusHeader.BH_CustomsProfile, message.EM_ApplicationReference);
				AssertEquals("message.EM_IsTestMessage is true", true, message.EM_IsTestMessage);
			}
		}

		public void TestSendMessagesWhenGenerateEntryNumberException()
		{
			cusHeader.EntryNumber = ZString.Empty;
			cusHeader.ReceiptOffice = "A";
			cusHeader.TW_BoxNumber = "123";
			cusHeader.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			using (cusHeader.GetGenerateEntryNumberExceptionSupporter())
			{
				notification.Clear();
				parent = new TranshipmentMessageSendingObjectParent(cusHeader, MessageTypeList.Codes.TRA);
				foreach (TranshipmentMessageSendingObject action in parent.SendingObjectsCollection)
				{
					action.ShouldSend = true;
				}
				var manager = new TranshipmentMultiMessageManager(parent, MessageTypeList.Codes.TRA, notification);
				manager.SendMessages(cusHeader.MessageInitiator);
				CombineAssertions(() =>
				{
					AssertEquals("Generate entry number error.", notification.LastMessage);
					AssertEquals("Cannot Send Message", notification.LastCaption);
				});
			}
		}

		void SendAllMessage()
		{
			parent = new TranshipmentMessageSendingObjectParent(cusHeader, MessageTypeList.Codes.TRA);
			AssertEquals("coll has 1", 1, parent.SendingObjectsCollection.Count);
			foreach (TranshipmentMessageSendingObject action in parent.SendingObjectsCollection)
			{
				action.ShouldSend = true;
			}

			var manager = new TranshipmentMultiMessageManager(parent, MessageTypeList.Codes.TRA, notification);
			manager.SendMessagesWithoutSaving(cusHeader.MessageInitiator);
		}

		readonly MessageNotificationCollector_ForTest notification = new MessageNotificationCollector_ForTest();
		TranshipmentMessageSendingObjectParent parent;
		CusInBondHeader cusHeader;
		CusEntryNumber cusNumber;
		protected override void SetUp()
		{
			base.SetUp();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001TEST";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			cusNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNumber.CE_ParentID = cusHeader.PK;
			cusNumber.CE_Category = "CUS";
			cusNumber.CE_EntryType = "TRS";
			cusNumber.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNumber.CE_EntryNum = "NO1";
			cusHeader.BH_GS_NKCusAgent = "TT";
			cusHeader.BH_CustomsProfile = "123-3";
			cusHeader.ArrivalBill.B0_ReferenceID = "1234";
			cusHeader.MovementBill.B0_ReferenceID = "1234";
			cusHeader.ReceiptOffice = "AA";
			cusHeader.UnladingOffice = "BB";
			cusHeader.TW_BoxNumber = "123";
			Factory.Save();
		}
	}
}
