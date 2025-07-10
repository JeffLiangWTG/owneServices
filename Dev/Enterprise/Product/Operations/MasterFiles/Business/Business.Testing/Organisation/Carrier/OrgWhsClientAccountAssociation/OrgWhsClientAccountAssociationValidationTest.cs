using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgWhsClientAccountAssociationValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckOWC_WW_Warehouse_WithEmptyCarrierAccount

		public void TestCheckOWC_WW_Warehouse_WithEmptyCarrierAccount()
		{
			var carrier = Factory.New<OrgHeader>();
			var account = CreateCarrierAccount(carrier);

			var client = Factory.New<OrgHeader>();
			var accountAssociation1 = CreateClientAccountAssociation(client, account.PK);
			var accountAssociation2 = CreateClientAccountAssociation(client, ZGuid.Empty);

			var warehouse = Factory.New<IWhsWarehouse>();
			AssertNoExceptionThrown(() => accountAssociation1.OWC_WW_Warehouse = warehouse.PK);
			AssertNoErrors(accountAssociation1.OWC_WW_WarehouseInfo);

			accountAssociation1.OWC_OAN_CarrierAccount = ZGuid.Empty;
			AssertNoExceptionThrown(() => accountAssociation1.OWC_WW_Warehouse = warehouse.PK);
			AssertNoErrors(accountAssociation1.OWC_WW_WarehouseInfo);
		}

		#endregion

		#region TestCheckOWC_WW_Warehouse_UniqueCombination

		public void TestCheckOWC_WW_Warehouse_UniqueCombination()
		{
			var carrier = Factory.New<OrgHeader>();
			var can1 = CreateCarrierAccount(carrier);
			var can2 = CreateCarrierAccount(carrier);

			var client = Factory.New<OrgHeader>();
			var caa1 = CreateClientAccountAssociation(client, can1.PK);
			var caa2 = CreateClientAccountAssociation(client, can2.PK);

			var salesChannel = Factory.New<IWhsSalesChannel>();
			caa1.OWC_WSH_SalesChannel = salesChannel.PK;
			caa2.OWC_WSH_SalesChannel = salesChannel.PK;

			var warehouse1 = Factory.New<IWhsWarehouse>();
			var warehouse2 = Factory.New<IWhsWarehouse>();
			caa1.OWC_WW_Warehouse = warehouse1.PK;
			caa2.OWC_WW_Warehouse = warehouse2.PK;

			AssertNoErrors("Precondition", caa1.OWC_WW_WarehouseInfo);
			AssertNoErrors("Precondition", caa2.OWC_WW_WarehouseInfo);

			caa2.OWC_WW_Warehouse = warehouse1.PK;
			AssertHasError(caa2.OWC_WW_WarehouseInfo, "Must have unique Client, Warehouse, Carrier and Sales Channel combination.");
		}

		#endregion

		#region TestCheckOWC_OAN_CarrierAccount_UniqueCombination

		public void TestCheckOWC_OAN_CarrierAccount_UniqueCombination()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();
			var can1 = CreateCarrierAccount(carrier1);
			var can2 = CreateCarrierAccount(carrier2);
			var can3 = CreateCarrierAccount(carrier1);

			var client = Factory.New<OrgHeader>();
			var caa1 = CreateClientAccountAssociation(client, can1.PK);
			var caa2 = CreateClientAccountAssociation(client, can2.PK);

			var warehouse = Factory.New<IWhsWarehouse>();
			caa1.OWC_WW_Warehouse = warehouse.PK;
			caa2.OWC_WW_Warehouse = warehouse.PK;

			var salesChannel = Factory.New<IWhsSalesChannel>();
			caa1.OWC_WSH_SalesChannel = salesChannel.PK;
			caa2.OWC_WSH_SalesChannel = salesChannel.PK;

			AssertNoErrors("Precondition", caa1.OWC_OAN_CarrierAccountInfo);
			AssertNoErrors("Precondition", caa2.OWC_OAN_CarrierAccountInfo);

			caa2.OWC_OAN_CarrierAccount = can3.PK;
			AssertHasError(caa2.OWC_OAN_CarrierAccountInfo, "Must have unique Client, Warehouse, Carrier and Sales Channel combination.");
		}

		#endregion

		#region TestCheckOWC_WSH_SalesChannel_UniqueCombination

		public void TestCheckOWC_WSH_SalesChannel_UniqueCombination()
		{
			var carrier = Factory.New<OrgHeader>();
			var can1 = CreateCarrierAccount(carrier);
			var can2 = CreateCarrierAccount(carrier);

			var client = Factory.New<OrgHeader>();
			var caa1 = CreateClientAccountAssociation(client, can1.PK);
			var caa2 = CreateClientAccountAssociation(client, can2.PK);

			var warehouse = Factory.New<IWhsWarehouse>();
			caa1.OWC_WW_Warehouse = warehouse.PK;
			caa2.OWC_WW_Warehouse = warehouse.PK;

			var salesChannel1 = Factory.New<IWhsSalesChannel>();
			var salesChannel2 = Factory.New<IWhsSalesChannel>();
			caa1.OWC_WSH_SalesChannel = salesChannel1.PK;
			caa2.OWC_WSH_SalesChannel = salesChannel2.PK;

			AssertNoErrors("Precondition", caa1.OWC_WSH_SalesChannelInfo);
			AssertNoErrors("Precondition", caa2.OWC_WSH_SalesChannelInfo);

			caa2.OWC_WSH_SalesChannel = salesChannel1.PK;
			AssertHasError(caa2.OWC_WSH_SalesChannelInfo, "Must have unique Client, Warehouse, Carrier and Sales Channel combination.");
		}

		#endregion

		#region TestCheckOWC_OH_Client_UniqueCombination

		public void TestCheckOWC_OH_Client_UniqueCombination()
		{
			var carrier = Factory.New<OrgHeader>();
			var can = CreateCarrierAccount(carrier);

			var client1 = Factory.New<OrgHeader>();
			var client2 = Factory.New<OrgHeader>();
			var caa1 = CreateClientAccountAssociation(client1, can.PK);
			var caa2 = CreateClientAccountAssociation(client2, can.PK);

			var warehouse = Factory.New<IWhsWarehouse>();
			caa1.OWC_WW_Warehouse = warehouse.PK;
			caa2.OWC_WW_Warehouse = warehouse.PK;

			var salesChannel = Factory.New<IWhsSalesChannel>();
			caa1.OWC_WSH_SalesChannel = salesChannel.PK;
			caa2.OWC_WSH_SalesChannel = salesChannel.PK;

			AssertNoErrors("Precondition", caa1.OWC_OH_ClientInfo);
			AssertNoErrors("Precondition", caa2.OWC_OH_ClientInfo);

			caa2.OWC_OH_Client = client1.PK;
			AssertHasError(caa2.OWC_OH_ClientInfo, "Must have unique Client, Warehouse, Carrier and Sales Channel combination.");
		}

		#endregion

		#region TestCheckOWC_BillingType

		public void TestCheckOWC_OAN_BillToCarrierAccount_NoBillToAccountNumber_BillReceiver()
		{
			TestCheckOWC_OAN_BillToCarrierAccount_NoBillToAccountNumberCore(CarrierBillingType.BillReceiver);
		}

		public void TestCheckOWC_OAN_BillToCarrierAccount_NoBillToAccountNumber_BillThirdParty()
		{
			TestCheckOWC_OAN_BillToCarrierAccount_NoBillToAccountNumberCore(CarrierBillingType.BillThirdParty);
		}

		void TestCheckOWC_OAN_BillToCarrierAccount_NoBillToAccountNumberCore(string billingType)
		{
			var carrier = Factory.New<OrgHeader>();
			var account = CreateCarrierAccount(carrier);
			var client = Factory.New<OrgHeader>();
			var accountAssociation = CreateClientAccountAssociation(client, account.PK);
			accountAssociation.OWC_BillingType = billingType;

			AssertEquals("Precondition", ZGuid.Empty, accountAssociation.OWC_OAN_BillToCarrierAccount);
			accountAssociation.Validation.ValidateOWC_OAN_BillToCarrierAccount();

			AssertHasError(accountAssociation.OWC_OAN_BillToCarrierAccountInfo,
				"A Bill To Carrier Account must be specified if the billing type is Bill Receiver or Bill Third Party. Please select an appropriate Carrier Account to bill or select Bill Sender as the Billing Type.");

			var carrier2 = Factory.New<OrgHeader>();
			var account2 = CreateCarrierAccount(carrier2);
			accountAssociation.OWC_OAN_BillToCarrierAccount = account2.PK;
			AssertNoError(accountAssociation.OWC_OAN_BillToCarrierAccountInfo,
				"A Bill To Carrier Account must be specified if the billing type is Bill Receiver or Bill Third Party. Please select an appropriate Carrier Account to bill or select Bill Sender as the Billing Type.");
		}

		public void TestCheckOWC_BillingType_WarningOnBillReceiverBillingType()
		{
			var carrier = Factory.New<OrgHeader>();
			var account = CreateCarrierAccount(carrier);
			var client = Factory.New<OrgHeader>();
			var accountAssociation = CreateClientAccountAssociation(client, account.PK);
			accountAssociation.OWC_BillingType = CarrierBillingType.BillReceiver;

			var carrier2 = Factory.New<OrgHeader>();
			var account2 = CreateCarrierAccount(carrier2);
			accountAssociation.OWC_OAN_BillToCarrierAccount = account2.PK;

			AssertNotEquals("Precondition", ZGuid.Empty, accountAssociation.OWC_OAN_BillToCarrierAccount);
			AssertHasWarning(accountAssociation.OWC_BillingTypeInfo,
				"The billing type selected is Bill Receiver. Please note that the bill to party organization specified in the bill to account number will be ignored as the billing party will be taken from the job.");
		}

		public void TestCheckOWC_BillingType_InvalidCode()
		{
			var carrier = Factory.New<OrgHeader>();
			var account = CreateCarrierAccount(carrier);
			var client = Factory.New<OrgHeader>();
			var accountAssociation = CreateClientAccountAssociation(client, account.PK);
			accountAssociation.OWC_BillingType = "XXX";

			AssertEquals("Precondition: XXX not in the list of BillingTypesList.", false,
				accountAssociation.Lookups.BillingTypesList.ToArray().Any(code => code.Code == "XXX"));
			AssertHasErrors("Should have error on invalid code.", accountAssociation.OWC_BillingTypeInfo);

			accountAssociation.OWC_BillingType = "SEN";
			AssertNoErrors("Should have no more errors.", accountAssociation.OWC_BillingTypeInfo);
		}

		public void TestCheckOWC_BillingType_EmptyCode()
		{
			var carrier = Factory.New<OrgHeader>();
			var account = CreateCarrierAccount(carrier);
			var client = Factory.New<OrgHeader>();
			var accountAssociation = CreateClientAccountAssociation(client, account.PK);

			accountAssociation.OWC_BillingType = string.Empty;
			AssertHasErrors("Should have error as it's empty.", accountAssociation.OWC_BillingTypeInfo);

			accountAssociation.OWC_BillingType = "SEN";
			AssertNoErrors("Should have no more errors.", accountAssociation.OWC_BillingTypeInfo);
		}

		#endregion

		#region Helper Methods

		OrgWhsClientAccountAssociation CreateClientAccountAssociation(OrgHeader client, ZGuid carrierAccount)
		{
			var accountAssociation = Factory.New<OrgWhsClientAccountAssociation>();
			accountAssociation.OWC_OH_Client = client.PK;

			if (carrierAccount != ZGuid.Empty)
			{
				accountAssociation.OWC_OAN_CarrierAccount = carrierAccount;
			}

			return accountAssociation;
		}

		OrgCarrierAccount CreateCarrierAccount(OrgHeader carrier)
		{
			var account = Factory.New<OrgCarrierAccount>();
			account.OAN_OH_Carrier = carrier.PK;

			return account;
		}

		#endregion
	}
}
