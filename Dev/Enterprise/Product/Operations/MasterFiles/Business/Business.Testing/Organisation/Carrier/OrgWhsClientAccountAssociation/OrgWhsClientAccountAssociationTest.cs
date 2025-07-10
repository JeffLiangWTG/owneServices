using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgWhsClientAccountAssociation))]
	sealed class OrgWhsClientAccountAssociationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetCarrierAccountNumber()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TRANSP1";

			var can1 = carrier.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "TR1SONYSYD";

			var can2 = carrier.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "TR1SONYMEL";

			var can3 = carrier.CarrierAccounts.AddNew();
			can3.OAN_AccountNumber = "TR1SONYALL";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "SONY";

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var whsSyd = (IWhsWarehouse)helper.CreateWarehouse("SYD", "S");
			var whsMel = (IWhsWarehouse)helper.CreateWarehouse("MEL", "M");
			var whsPer = (IWhsWarehouse)helper.CreateWarehouse("PER", "P");

			var salesChannel1 = (IWhsSalesChannel)helper.CreateWhsSalesChannel("1", "111");
			var salesChannel2 = (IWhsSalesChannel)helper.CreateWhsSalesChannel("2", "222");
			var salesChannel3 = (IWhsSalesChannel)helper.CreateWhsSalesChannel("3", "334");

			Factory.Save();

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_OAN_CarrierAccount = can1.PK;
			whsCANSonySyd.OWC_WW_Warehouse = whsSyd.PK;

			var whsCANSonyMel = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonyMel.OWC_OAN_CarrierAccount = can2.PK;
			whsCANSonyMel.OWC_WW_Warehouse = whsMel.PK;

			var whsCANSonyAll = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonyAll.OWC_OAN_CarrierAccount = can3.PK;

			var whsCANSonyAll_Sales1 = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonyAll_Sales1.OWC_OAN_CarrierAccount = can3.PK;
			whsCANSonyAll_Sales1.OWC_WSH_SalesChannel = salesChannel1.PK;

			var whsCANSonySyd_Sales2 = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd_Sales2.OWC_OAN_CarrierAccount = can1.PK;
			whsCANSonySyd_Sales2.OWC_WW_Warehouse = whsSyd.PK;
			whsCANSonySyd_Sales2.OWC_WSH_SalesChannel = salesChannel2.PK;

			var caa_salesChannel3 = client.OrgWhsClientAccountAssociations.AddNew();
			caa_salesChannel3.OWC_WSH_SalesChannel = salesChannel3.PK;
			caa_salesChannel3.OWC_OAN_CarrierAccount = can2.PK;

			CombineAssertions(() =>
			{
				var canTest1 = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(carrier, client, whsMel);
				AssertSame(nameof(canTest1), can2, canTest1);

				var canTest2 = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(carrier, client, null);
				AssertSame(nameof(canTest2), can3, canTest2);

				var canTest3 = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(null, client, whsSyd, salesChannel3);
				AssertNull(nameof(canTest3), canTest3);

				var canTest4 = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(carrier, null, whsSyd);
				AssertNull(nameof(canTest4), canTest4);

				var canTest5 = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(carrier, client, whsPer);
				AssertSame(nameof(canTest5), can3, canTest5);

				var canTest6 = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(carrier, client, whsPer, salesChannel3);
				AssertSame(nameof(canTest6), can2, canTest6);

				var canTest7 = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(carrier, client, whsSyd, salesChannel2);
				AssertSame(nameof(canTest7), can1, canTest7);

				var canTest8 = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(carrier, client, null, salesChannel1);
				AssertSame(nameof(canTest8), can3, canTest8);
			});
		}

		public void TestGetWhsClientAccountAssociation()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TRANSP1";

			var can1 = carrier.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "TR1SONYSYD";

			var can2 = carrier.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "TR1SONYMEL";

			var can3 = carrier.CarrierAccounts.AddNew();
			can3.OAN_AccountNumber = "TR1SONYALL";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "SONY";

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var whsSyd = (IWhsWarehouse)helper.CreateWarehouse("SYD", "S");
			var whsMel = (IWhsWarehouse)helper.CreateWarehouse("MEL", "M");
			var whsPer = (IWhsWarehouse)helper.CreateWarehouse("PER", "P");

			var salesChannel1 = (IWhsSalesChannel)helper.CreateWhsSalesChannel("1", "111");
			var salesChannel2 = (IWhsSalesChannel)helper.CreateWhsSalesChannel("2", "222");
			var salesChannel3 = (IWhsSalesChannel)helper.CreateWhsSalesChannel("3", "334");

			Factory.Save();

			var whsCANSonySyd = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd.OWC_OAN_CarrierAccount = can1.PK;
			whsCANSonySyd.OWC_WW_Warehouse = whsSyd.PK;

			var whsCANSonyMel = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonyMel.OWC_OAN_CarrierAccount = can2.PK;
			whsCANSonyMel.OWC_WW_Warehouse = whsMel.PK;

			var whsCANSonyAll = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonyAll.OWC_OAN_CarrierAccount = can3.PK;

			var whsCANSonyAll_Sales1 = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonyAll_Sales1.OWC_OAN_CarrierAccount = can3.PK;
			whsCANSonyAll_Sales1.OWC_WSH_SalesChannel = salesChannel1.PK;

			var whsCANSonySyd_Sales2 = client.OrgWhsClientAccountAssociations.AddNew();
			whsCANSonySyd_Sales2.OWC_OAN_CarrierAccount = can1.PK;
			whsCANSonySyd_Sales2.OWC_WW_Warehouse = whsSyd.PK;
			whsCANSonySyd_Sales2.OWC_WSH_SalesChannel = salesChannel2.PK;

			var caa_salesChannel3 = client.OrgWhsClientAccountAssociations.AddNew();
			caa_salesChannel3.OWC_WSH_SalesChannel = salesChannel3.PK;
			caa_salesChannel3.OWC_OAN_CarrierAccount = can2.PK;

			CombineAssertions(() =>
			{
				var whsCANTest1 = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(carrier, client, whsMel, null);
				AssertSame(nameof(whsCANTest1), whsCANSonyMel, whsCANTest1);

				var whsCANTest2 = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(carrier, client, null, null);
				AssertSame(nameof(whsCANTest2), whsCANSonyAll, whsCANTest2);

				var whsCANTest3 = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(null, client, whsSyd, salesChannel3);
				AssertNull(nameof(whsCANTest3), whsCANTest3);

				var whsCANTest4 = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(carrier, null, whsSyd, null);
				AssertNull(nameof(whsCANTest4), whsCANTest4);

				var whsCANTest5 = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(carrier, client, whsPer, null);
				AssertSame(nameof(whsCANTest5), whsCANSonyAll, whsCANTest5);

				var whsCANTest6 = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(carrier, client, whsPer, salesChannel3);
				AssertSame(nameof(whsCANTest6), caa_salesChannel3, whsCANTest6);

				var whsCANTest7 = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(carrier, client, whsSyd, salesChannel2);
				AssertSame(nameof(whsCANTest7), whsCANSonySyd_Sales2, whsCANTest7);

				var whsCANTest8 = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(carrier, client, null, salesChannel1);
				AssertSame(nameof(whsCANTest8), whsCANSonyAll_Sales1, whsCANTest8);
			});
		}

		public void TestGetReadOnlySecurity()
		{
			var orgWhsClientAccountAssociation = Factory.NewWithValidTestData<DummyOrgWhsClientAccountAssociation>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			security.OrgWarehouseModify.IsAllowed = true;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				orgWhsClientAccountAssociation.OWC_OH_Client = ZGuid.Empty;
				AssertEquals("When Client is null, readonly should return false.", false, orgWhsClientAccountAssociation.GetReadOnlySecurityForTesting);

				orgWhsClientAccountAssociation.OWC_OH_Client = client.PK;
				AssertEquals("When Client is defined but not in the database, readonly should return false.", false, orgWhsClientAccountAssociation.GetReadOnlySecurityForTesting);

				Factory.Save();
				AssertEquals("When Client is defined and in the database and OrgWarehouseModify is allowed, readonly should return false.", false, orgWhsClientAccountAssociation.GetReadOnlySecurityForTesting);
			}

			orgWhsClientAccountAssociation = Factory.NewWithValidTestData<DummyOrgWhsClientAccountAssociation>();
			client = Factory.NewWithValidTestData<OrgHeader>();
			security.OrgWarehouseModify.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				orgWhsClientAccountAssociation.OWC_OH_Client = ZGuid.Empty;
				AssertEquals("When Client is null, readonly should return false.", false, orgWhsClientAccountAssociation.GetReadOnlySecurityForTesting);

				orgWhsClientAccountAssociation.OWC_OH_Client = client.PK;
				AssertEquals("When Client is defined but not in the database, readonly should return false.", false, orgWhsClientAccountAssociation.GetReadOnlySecurityForTesting);

				Factory.Save();
				AssertEquals("When Client is defined and in the database and OrgWarehouseModify is not allowed, readonly should return true.", true, orgWhsClientAccountAssociation.GetReadOnlySecurityForTesting);
			}
		}

		public void TestBillToCarrierAccountReadOnly()
		{
			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			AssertEquals("Precondition", orgWhsClientAccountAssociation.OWC_BillingType, CarrierBillingType.BillSender);
			AssertEquals("OWC_OAN_BillToCarrierAccount is readonly", true, orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccountInfo.ReadOnly);

			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillReceiver;
			AssertEquals("OWC_OAN_BillToCarrierAccount is not readonly", false, orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccountInfo.ReadOnly);

			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillSender;
			AssertEquals("OWC_OAN_BillToCarrierAccount is readonly", true, orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccountInfo.ReadOnly);

			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillThirdParty;
			AssertEquals("OWC_OAN_BillToCarrierAccount is not readonly", false, orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccountInfo.ReadOnly);
		}

		public void TestBillSenderBillingTypeClearsBillToCarrierAccount()
		{
			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			AssertEquals("Precondition", orgWhsClientAccountAssociation.OWC_BillingType, CarrierBillingType.BillSender);
			AssertEquals("OWC_OAN_BillToCarrierAccount is readonly", true, orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccountInfo.ReadOnly);

			var orgCarrierAccount = Factory.NewWithValidTestData<OrgCarrierAccount>();
			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillReceiver;
			orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount = orgCarrierAccount.PK;

			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillThirdParty;
			AssertEquals("OWC_OAN_BillToCarrierAccount is retained", orgCarrierAccount.PK, orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount);

			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillSender;
			AssertEquals("OWC_OAN_BillToCarrierAccount is cleared", ZGuid.Empty, orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount);
		}

		public void TestBillToCarrierAccount()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TRANSP1";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLPAYER";
			billToParty.OH_FullName = "Bill Payer";

			var can1 = carrier.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "ACCT1";
			can1.OAN_OH_BillToParty = carrier.PK;

			var can2 = carrier.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "ACCT2";
			can2.OAN_OH_BillToParty = billToParty.PK;

			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can1.PK;
			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillReceiver;
			orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount = can2.PK;

			AssertEquals("BILLPAYER", orgWhsClientAccountAssociation.BillToCarrierCode);
			AssertEquals("Bill Payer", orgWhsClientAccountAssociation.BillToCarrierName);
		}

		public void TestBillToCarrierAccount_NoBillToParty()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TRANSP1";

			var can1 = carrier.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "ACCT1";
			can1.OAN_OH_BillToParty = carrier.PK;

			var can2 = carrier.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "TR1SONYSYD";
			can2.OAN_OH_BillToParty = ZGuid.Empty;

			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can1.PK;
			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillReceiver;
			orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount = can2.PK;

			AssertEquals("", orgWhsClientAccountAssociation.BillToCarrierCode);
			AssertEquals("", orgWhsClientAccountAssociation.BillToCarrierName);
		}

		public void TestCarrierAccountResetsBillToCarrierAccount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "TRANSP1";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLPAYER";
			billToParty.OH_FullName = "Bill Payer";

			var can1 = carrier1.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "ACCT1";
			can1.OAN_OH_BillToParty = carrier1.PK;

			var can2 = carrier1.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "ACCT2";
			can2.OAN_OH_BillToParty = billToParty.PK;

			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can1.PK;
			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillReceiver;
			orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount = can2.PK;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "TRANSP2";

			var canCarrier2 = carrier2.CarrierAccounts.AddNew();
			canCarrier2.OAN_AccountNumber = "ACCT1";

			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = canCarrier2.PK;
			Assert("Carrier Acct Number from another carrier, bill to carrier acct number should be reset.", orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount.IsEmpty);
		}

		public void TestCarrierAccountDoesNotResetBillToCarrierAccountIfSameCarrier()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "TRANSP1";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLPAYER";
			billToParty.OH_FullName = "Bill Payer";

			var can1 = carrier1.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "ACCT1";

			var can2 = carrier1.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "ACCT2";
			can2.OAN_OH_BillToParty = billToParty.PK;

			var can3 = carrier1.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "ACCT3";

			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can1.PK;
			orgWhsClientAccountAssociation.OWC_BillingType = CarrierBillingType.BillReceiver;
			orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount = can2.PK;

			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can3.PK;
			AssertEquals("Carrier Acct Number from same carrier, bill to carrier acct number should not be reset.",
				can2.PK,
				orgWhsClientAccountAssociation.OWC_OAN_BillToCarrierAccount);
		}

		public void TestDutyBillToCarrierAccount()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TRANSP1";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "DUTYPAYER";
			billToParty.OH_FullName = "Duty Payer";

			var can1 = carrier.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "ACCT1";
			can1.OAN_OH_BillToParty = carrier.PK;

			var can2 = carrier.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "ACCT2";
			can2.OAN_OH_BillToParty = billToParty.PK;

			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can1.PK;
			orgWhsClientAccountAssociation.OWC_OAN_DutyBillToCarrierAccount = can2.PK;

			AssertEquals("DUTYPAYER", orgWhsClientAccountAssociation.DutyBillToCarrierCode);
			AssertEquals("Duty Payer", orgWhsClientAccountAssociation.DutyBillToCarrierName);
		}

		public void TestDutyBillToCarrierAccount_NoBillToParty()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TRANSP1";

			var can1 = carrier.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "ACCT1";
			can1.OAN_OH_BillToParty = carrier.PK;

			var can2 = carrier.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "TR1SONYSYD";
			can2.OAN_OH_BillToParty = ZGuid.Empty;

			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can1.PK;
			orgWhsClientAccountAssociation.OWC_OAN_DutyBillToCarrierAccount = can2.PK;

			AssertEquals("", orgWhsClientAccountAssociation.DutyBillToCarrierCode);
			AssertEquals("", orgWhsClientAccountAssociation.DutyBillToCarrierName);
		}

		public void TestCarrierAccountResetsDutyBillToCarrierAccount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "TRANSP1";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLPAYER";
			billToParty.OH_FullName = "Bill Payer";

			var can1 = carrier1.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "ACCT1";
			can1.OAN_OH_BillToParty = carrier1.PK;

			var can2 = carrier1.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "ACCT2";
			can2.OAN_OH_BillToParty = billToParty.PK;

			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can1.PK;
			orgWhsClientAccountAssociation.OWC_OAN_DutyBillToCarrierAccount = can2.PK;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "TRANSP2";

			var canCarrier2 = carrier2.CarrierAccounts.AddNew();
			canCarrier2.OAN_AccountNumber = "ACCT1";

			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = canCarrier2.PK;
			Assert("Carrier Acct Number from another carrier, bill to carrier acct number should be reset.", orgWhsClientAccountAssociation.OWC_OAN_DutyBillToCarrierAccount.IsEmpty);
		}

		public void TestCarrierAccountDoesNotResetDutyBillToCarrierAccountIfSameCarrier()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "TRANSP1";

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLPAYER";
			billToParty.OH_FullName = "Bill Payer";

			var can1 = carrier1.CarrierAccounts.AddNew();
			can1.OAN_AccountNumber = "ACCT1";

			var can2 = carrier1.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "ACCT2";
			can2.OAN_OH_BillToParty = billToParty.PK;

			var can3 = carrier1.CarrierAccounts.AddNew();
			can2.OAN_AccountNumber = "ACCT3";

			var orgWhsClientAccountAssociation = Factory.New<DummyOrgWhsClientAccountAssociation>();
			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can1.PK;
			orgWhsClientAccountAssociation.OWC_OAN_DutyBillToCarrierAccount = can2.PK;

			orgWhsClientAccountAssociation.OWC_OAN_CarrierAccount = can3.PK;
			AssertEquals("Carrier Acct Number from same carrier, bill to carrier acct number should not be reset.",
				can2.PK,
				orgWhsClientAccountAssociation.OWC_OAN_DutyBillToCarrierAccount);
		}
	}
}
