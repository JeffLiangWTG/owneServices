using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class MessageSenderTest : TestCaseWithFactory
	{
		public void TestGetNotificationsAsWarnings()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var action = new MessageSendingAction(header, ActionCode.Creating, true);
			var notifications = MessageSender.GetNotifications(action);

			AssertEquals(1, notifications.Count);
			var notification = notifications[0];
			Assert(notification.IsWarning);
			Assert(!notification.IsError);
		}

		public void TestGetNotificationsAsErrors()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var action = new MessageSendingAction(header, ActionCode.Creating, false);
			var notifications = MessageSender.GetNotifications(action);

			AssertEquals(5, notifications.Count);
			var notification = notifications.Last();
			Assert(!notification.IsWarning);
			Assert(notification.IsError);
		}

		public void TestSendManifestWithMessageErrors()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			header.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var cachedValue = Env.Security.USAMSSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.USAMSSendWithMessageErrors.IsAllowed = true;
				var sender = new MessageSender();
				var result = sender.SendManifest(header.PK);
				AssertSendingResult(1, MessageSender.Constants.Message.MessageSentNotification(1), result);

				result = sender.SendManifest(header.PK, null, false);
				AssertEquals(0, result.Item1);
				AssertContains("There are message errors on this job and you don't have security rights to send with message errors", result.Item2);
			}
			finally
			{
				Env.Security.USAMSSendWithMessageErrors.IsAllowed = cachedValue;
			}
		}

		public void TestUSAMSMessageSenderWithInvalidEnviroment()
		{
			Integration.Customs.US.USAMS.IUSAMSMessageSender sender = new MessageSender();
			var result = sender.SendManifest(ZGuid.NewZGuid());
			AssertSendingResult(0, MessageSender.Constants.Message.NoHeaderNotification, result);

			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			Factory.Save();
			result = sender.SendManifest(header.PK);
			AssertSendingResult(0, MessageSender.Constants.Message.NoOrgProxySCACNotification(header), result);
		}

		public void TestUSAMSMessageSenderSuccessfulAmendment()
		{
			Integration.Customs.US.USAMS.IUSAMSMessageSender sender = new MessageSender();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.MovementHeader.FillWithValidTestData();
			var bill1 = header.Bills.AddNew();
			bill1.FillWithValidTestData();
			bill1.MovementDetail.FillWithValidTestData();

			var bill2 = header.Bills.AddNew();
			bill2.FillWithValidTestData();
			bill2.MovementDetail.FillWithValidTestData();

			var bill3 = header.Bills.AddNew();
			bill3.FillWithValidTestData();
			bill3.MovementDetail.FillWithValidTestData();

			var bill4 = header.Bills.AddNew();
			bill4.FillWithValidTestData();
			bill4.MovementDetail.FillWithValidTestData();

			var company = Factory.Load<OrgHeader>(header.Company.OrgProxy.PK);
			company.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);

			Factory.Save();

			var amendments = new[]
			{
				new USAMSManifestBillAmendment() { ActionCode = "A", AmendmentCode = "A", BillPK = bill1.PK.ToGuid() },
				new USAMSManifestBillAmendment() { ActionCode = "A", AmendmentCode = "A", BillPK = bill3.PK.ToGuid() },
			};

			var result = sender.SendManifest(header.PK, amendments);
			AssertSendingResult(2, MessageSender.Constants.Message.MessageSentNotification(2), result);
		}

		public void TestUSAMSMessageSenderSuccessful()
		{
			Integration.Customs.US.USAMS.IUSAMSMessageSender sender = new MessageSender();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.MovementHeader.FillWithValidTestData();
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			var moveDetail = bill.MovementDetail;
			moveDetail.FillWithValidTestData();

			var company = Factory.Load<OrgHeader>(header.Company.OrgProxy.PK);
			company.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);

			Factory.Save();

			var result = sender.SendManifest(header.PK);
			AssertSendingResult(1, MessageSender.Constants.Message.MessageSentNotification(1), result);
		}
		public void TestNoOrgProxySCACNotification()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var message = MessageSender.Constants.Message.NoOrgProxySCACNotification(header);
			AssertNotContains("Maintain > System > Companies > Current Company > Company Info. > Organization > Details > Config > Registration Numbers / Codes.", message);
			AssertContains("Maintain > User Admin > Companies > Current Company > Company Info. > Organization Proxy > Details > Config > Registration Numbers / Codes.", message);
		}
		public void TestUSAMSMessageSenderWithProxySetOnBranchAndCompany()
		{
			Integration.Customs.US.USAMS.IUSAMSMessageSender sender = new MessageSender();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();

			Factory.Save();
			var result = sender.SendManifest(header.PK);
			AssertSendingResult(0, MessageSender.Constants.Message.NoOrgProxySCACNotification(header), result);

			header.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			result = sender.SendManifest(header.PK);
			AssertSendingResult(1, MessageSender.Constants.Message.MessageSentNotification(1), result);

			header.Company.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			header.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			result = sender.SendManifest(header.PK);
			AssertSendingResult(1, MessageSender.Constants.Message.MessageSentNotification(1), result);
		}

		static void AssertSendingResult(int messageSent, string message, Tuple<int, string> sendingResult)
		{
			AssertEquals(messageSent, sendingResult.Item1);
			AssertEquals(message, sendingResult.Item2);
		}
	}
}
