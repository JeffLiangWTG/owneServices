using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlobalCreditControlledDocumentsApprovalMissingExRateEmailTest : AccountingEmailDefTest
	{
		protected override Type EmailDefType => typeof(GlobalCreditControlledDocumentsApprovalMissingExRateEmail);

		public void TestSendEmail()
		{
			var email = new GlobalCreditControlledDocumentsApprovalMissingExRateEmail(OrgHeader, BO, MenuItemPK);
			email.Send();

			AssertRecipients(email);
			AssertEquals("Email Subject", "Missing Exchange Rate while determining Global Credit Limit for OH1", email.Subject);
			AssertEquals(@"Global outstanding transactions balance cannot be calculated for the global credit group.<br />It requires valid exchange rates for today to be entered (using ‘GCB’ or ‘PER’ exchange rate type) in all system companies where the Global Credit Group has transactions and/or local credit limits.<br />For a list of system companies and currency codes, please refer to Organization (OH1) > A/R > Credit Control and Settlement > Global > Companies Local Credit Control and Settlement Details.<br />
<br />
System could not determine the Global Credit Limit for OH1 while delivering the document: docName for S0001.", email.Body);
		}

		public void TestSendEmailWithWrongMenuItem()
		{
			var email = new GlobalCreditControlledDocumentsApprovalMissingExRateEmail(OrgHeader, BO, ZGuid.Empty);
			email.Send();

			AssertRecipients(email);
			AssertEquals("Email Subject", "Missing Exchange Rate while determining Global Credit Limit for OH1", email.Subject);
			AssertEquals(@"Global outstanding transactions balance cannot be calculated for the global credit group.<br />It requires valid exchange rates for today to be entered (using ‘GCB’ or ‘PER’ exchange rate type) in all system companies where the Global Credit Group has transactions and/or local credit limits.<br />For a list of system companies and currency codes, please refer to Organization (OH1) > A/R > Credit Control and Settlement > Global > Companies Local Credit Control and Settlement Details.<br />
<br />
System could not determine the Global Credit Limit for OH1 while delivering the document:  for S0001.", email.Body);
		}

		public void TestSendEmailWithWrongBusinessObject()
		{
			// DummyBusinessObject could be any type that does not implement ICreditControlledDocumentDelivery
			var email = new GlobalCreditControlledDocumentsApprovalMissingExRateEmail(OrgHeader, Factory.New<DummyBusinessObject>(), MenuItemPK);
			email.Send();

			AssertRecipients(email);
			AssertEquals("Email Subject", "Missing Exchange Rate while determining Global Credit Limit for OH1", email.Subject);
			AssertEquals(@"Global outstanding transactions balance cannot be calculated for the global credit group.<br />It requires valid exchange rates for today to be entered (using ‘GCB’ or ‘PER’ exchange rate type) in all system companies where the Global Credit Group has transactions and/or local credit limits.<br />For a list of system companies and currency codes, please refer to Organization (OH1) > A/R > Credit Control and Settlement > Global > Companies Local Credit Control and Settlement Details.<br />
<br />
System could not determine the Global Credit Limit for OH1 while delivering the document: docName for .", email.Body);
		}

		void AssertRecipients(GlobalCreditControlledDocumentsApprovalMissingExRateEmail email)
		{
			AssertEquals("Email Recipient count", 2, email.Recipients.Count);
			Assert("Email Recipient", email.Recipients.Contains("pointyhairedguy1@shippingco.com"));
			Assert("Email Recipient", email.Recipients.Contains("pointyhairedguy2@shippingco.com"));
		}

		protected override void SetUp()
		{
			var pointy1 = Factory.NewWithValidTestData<GlbStaff>();
			pointy1.GS_FullName = "pointyhairedguy1";
			pointy1.GS_EmailAddress = "pointyhairedguy1@shippingco.com";
			var pointy2 = Factory.NewWithValidTestData<GlbStaff>();
			pointy2.GS_FullName = "pointyhairedguy2";
			pointy2.GS_EmailAddress = "pointyhairedguy2@shippingco.com";
			var notifyGroup = Factory.New<GlbGroup>();
			notifyGroup.Staff.Add(pointy1);
			notifyGroup.Staff.Add(pointy2);
			AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notifyGroup.PK.ToGuid());

			OrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader.OH_Code = "OH1";

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			BO = shipment as BusinessObject;

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path2";
			menuItem.SU_MenuName = "docName";
			MenuItemPK = menuItem.PK;

			Factory.Save();
		}

		OrgHeader OrgHeader;
		BusinessObject BO;
		ZGuid MenuItemPK;
	}
}
