using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.US.Testing
{
	class WhsOrderDocAddressValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region Validation

		#region TSA Validation

		public void TestMissingTransportCoErrorAndWarning()
		{
			var warehouse = Helper.CreateWarehouse("TST WHS");
			var client = Helper.CreateClient();
			var transportCo = Helper.CreateClient();
			var order = Helper.CreateWhsOrder(client, warehouse);
			var pick = Factory.New<WhsPick>();
			var errorMessage = "The Transport Company has no address entered. A Transport Company with a valid address is required when finalizing orders in a TSA known warehouse.";
			var warningMessage = "The Transport Company has no address entered";

			var availableStatuses = new DocketStatus();
			foreach (CodeDescriptionPair status in availableStatuses)
			{
				BeforeValidationSetupAndValidation(warehouse, Environment.CodeLists.US.TSAStatus.Codes.Known, null, null, order, pick, status);
				AssertErrorOrWarningMessages(order, errorMessage, warningMessage);

				BeforeValidationSetupAndValidation(warehouse, Environment.CodeLists.US.TSAStatus.Codes.Unknown, null, null, order, pick, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertHasWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(warehouse, Environment.CodeLists.US.TSAStatus.Codes.Known, null, null, order, null, status);
				AssertErrorOrWarningMessages(order, errorMessage, warningMessage);

				BeforeValidationSetupAndValidation(warehouse, Environment.CodeLists.US.TSAStatus.Codes.Unknown, null, null, order, null, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertHasWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(null, null, null, null, order, pick, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertHasWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(null, null, null, null, order, null, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertHasWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(warehouse, Environment.CodeLists.US.TSAStatus.Codes.Known, transportCo, null, order, pick, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertNoWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(warehouse, Environment.CodeLists.US.TSAStatus.Codes.Unknown, transportCo, null, order, pick, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertNoWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(warehouse, Environment.CodeLists.US.TSAStatus.Codes.Known, transportCo, null, order, null, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertNoWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(warehouse, Environment.CodeLists.US.TSAStatus.Codes.Unknown, transportCo, null, order, null, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertNoWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(null, null, transportCo, null, order, pick, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertNoWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);

				BeforeValidationSetupAndValidation(null, null, transportCo, null, order, null, status);
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertNoWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);
			}
		}

		void AssertErrorOrWarningMessages(WhsOrder order, ZString errorMessage, ZString warningMessage)
		{
			if (order.IsFinalised || order.IsFinalising)
			{
				AssertHasError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertNoWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);
			}
			else
			{
				AssertNoError(order.TransportCoDocAddress.OrganisationPKInfo, errorMessage);
				AssertHasWarning(order.TransportCoDocAddress.OrganisationPKInfo, warningMessage);
			}
		}

		protected void SetOrderStatus(WhsOrder order, CodeDescriptionPair orderStatus)
		{
			if (order != null)
			{
				order.WD_DocketStatus = DocketStatus.Codes.Entered;
				if (orderStatus.Code != DocketStatus.Codes.Finalised)
				{
					order.WD_FinalisedDate = ZDateTimeOffset.Empty;
					order.WD_DocketStatus = orderStatus.Code;
					AssertEquals("Precondition: Incorrect Docket Status", orderStatus.Code, order.WD_DocketStatus);
				}
				else
				{
					order.WD_FinalisedDate = ZDateTimeOffset.Now;
				}
			}
		}

		protected void BeforeValidationSetupAndValidation(WhsWarehouse warehouse, string warehouseTSA, OrgHeader transportCo, string transportCoTSA, WhsOrder order, WhsPick pick)
		{
			BeforeValidationSetupAndValidation(warehouse, warehouseTSA, transportCo, transportCoTSA, order, pick, null);
		}

		protected void BeforeValidationSetupAndValidation(WhsWarehouse warehouse, string warehouseTSA, OrgHeader transportCo, string transportCoTSA, WhsOrder order, WhsPick pick, CodeDescriptionPair status)
		{
			BeforeValidationSetupAndValidation(warehouse, warehouseTSA, transportCo, transportCoTSA, order, pick, status, false);
		}

		protected void BeforeValidationSetupAndValidation(WhsWarehouse warehouse, string warehouseTSA, OrgHeader transportCo, string transportCoTSA, WhsOrder order, WhsPick pick, CodeDescriptionPair status, bool isFinalising)
		{
			if (pick != null)
			{
				order.WD_WP = pick.PK;
			}
			else
			{
				order.WD_WP = ZGuid.Empty;
			}
			if (transportCo != null)
			{
				order.TransportCoPK = transportCo.PK;
				Helper.SetOrgAddressTSAStatus(transportCo.MainAddress, transportCoTSA);
			}
			else
			{
				order.TransportCoPK = ZGuid.Empty;
			}
			if (warehouse != null)
			{
				order.WD_WW_Whs = warehouse.PK;
				Helper.SetWarehouseTSAStatus(warehouse, warehouseTSA);
			}
			else
			{
				order.WD_WW_Whs = ZGuid.Empty;
			}
			if (status != null)
			{
				SetOrderStatus(order, status);
			}
			if (isFinalising)
			{
				using (new SemaphoreManager(order.FinaliseDocketSemaphore))
				{
					order.TransportCoDocAddress.Validation.ValidateOrganisationPK();
				}
			}
			else
			{
				order.TransportCoDocAddress.Validation.ValidateOrganisationPK();
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsUS(Factory);
		}

		protected new WhsTestHelperFunctionsUS Helper
		{
			get { return (WhsTestHelperFunctionsUS)base.Helper; }
		}

		#endregion
	}
}
