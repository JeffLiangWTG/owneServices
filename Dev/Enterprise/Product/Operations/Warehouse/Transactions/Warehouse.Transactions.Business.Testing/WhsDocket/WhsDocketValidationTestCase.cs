using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketValidationTestCase<TDocket> : WhsBusinessObjectValidationTestCase
		where TDocket : WhsDocket
	{
		#region TestBondNotSupportedCountryErrorMsg

		public void TestBondNotSupportedCountryErrorMsg()
		{
			AssertEquals("This Country/Region does not have Customs Integration.",
				GetNewBusinessObject().Validation.BondNotSupportedCountryErrorMsg);
		}

		#endregion

		#region TestValidateWD_PL_NKCarrierServiceLevel

		public void TestValidateWD_PL_NKCarrierServiceLevel()
		{
			var docket = GetNewBusinessObject();
			docket.WD_PL_NKCarrierServiceLevel = "";
			AssertNoNotifications(docket.WD_PL_NKCarrierServiceLevelInfo);

			docket.WD_PL_NKCarrierServiceLevel = "STD";
			AssertHasWarnings(docket.WD_PL_NKCarrierServiceLevelInfo);

			var transportCo = Factory.New<OrgHeader>();
			var serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "ABC";

			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoPK = transportCo.PK;
				docket.WD_PL_NKCarrierServiceLevel = "ABC";
				AssertNoWarnings(docket.WD_PL_NKCarrierServiceLevelInfo);

				docket.WD_PL_NKCarrierServiceLevel = "STD";
				AssertNoWarnings("STD is available for all TransportCo's.", docket.WD_PL_NKCarrierServiceLevelInfo);
			}
			else
			{
				docket.WD_PL_NKCarrierServiceLevel = "ABC";
				AssertNoWarnings(docket.WD_PL_NKCarrierServiceLevelInfo);

				docket.WD_PL_NKCarrierServiceLevel = "STD";
				AssertNoWarnings(docket.WD_PL_NKCarrierServiceLevelInfo);
			}

			docket.WD_PL_NKCarrierServiceLevel = "XXX";
			AssertHasWarnings(docket.WD_PL_NKCarrierServiceLevelInfo);
		}

		#endregion

		#region TestValidateWD_WW_Whs

		public void TestValidateWD_WW_Whs()
		{
			Docket.Delete();
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = Helper.CreateClient().PK;
			docket.Validation.ValidateWD_WW_Whs();
			AssertMandatoryValidationError(docket.WD_WW_WhsInfo, true);

			docket.WD_WW_Whs = data.Whs1.PK;
			AssertNoError(docket.WD_WW_WhsInfo, WhsDocketValidation.CannotSelectInactiveWarehouse);

			Factory.Save();
			data.Whs1.WW_IsActive = false;
			docket.Validation.ValidateWD_WW_Whs();
			AssertHasError(docket.WD_WW_WhsInfo, WhsDocketValidation.CannotSelectInactiveWarehouse);

			TestValidateWD_WW_WhsCore();
		}

		protected virtual void TestValidateWD_WW_WhsCore()
		{
		}

		#endregion

		#region TestValidateWD_DocketSubType

		public void TestValidateWD_DocketSubType()
		{
			TestValidateWD_DocketSubType_BondedWarehouse();
			TestValidateWD_DocketSubType_IsNotEmptyAndValidType();
		}

		#region TestValidateWD_DocketSubType_BondedWarehouse

		protected virtual IEnumerable<string> ValidSubTypeForBondedWarehouse
			=> Enumerable.Empty<string>();

		protected virtual void TestValidateWD_DocketSubType_BondedWarehouse()
		{
			var docket = GetNewBusinessObject();
			var isBondedWarehouseSubTypesExist = false;
			var freeStoreDocketSubType = docket.WD_DocketSubType;
			foreach (var subType in ValidSubTypeForBondedWarehouse)
			{
				isBondedWarehouseSubTypesExist = true;
				if (IsDocketSubTypesSupported)
				{
					var whs = Helper.CreateWarehouse("1");
					docket.WD_WW_Whs = whs.PK;

					docket.WD_DocketSubType = subType;
					AssertHasError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.BondNotEnabledErrorMsg);

					Helper.EnableWarehouseForBond(docket.Warehouse, true);
					AssertBondedSupportedByCountry(docket, Constants.CountryCodes.Singapore, isBondedSupported: false);
					AssertBondedSupportedByCountry(docket, Constants.CountryCodes.Australia, isBondedSupported: true);
					AssertBondedSupportedByCountry(docket, Constants.CountryCodes.NewZealand, isBondedSupported: false);
					AssertBondedSupportedByCountry(docket, Constants.CountryCodes.UnitedStates, isBondedSupported: true);

					Helper.EnableWarehouseForFreeStore(docket.Warehouse, false);
					docket.WD_DocketSubType = "XXX";
					AssertHasError(docket.WD_DocketSubTypeInfo, "Enter a valid Docket Sub Type.");
					AssertNoError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.FreeStoreNotEnabledErrorMsg);

					docket.WD_DocketSubType = freeStoreDocketSubType;
					AssertHasError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.FreeStoreNotEnabledErrorMsg);

					// excise warehouse
					Helper.EnableWarehouseForBond(docket.Warehouse, false);
					Helper.EnableWarehouseForExcise(docket.Warehouse, true);
					docket.Validation.ValidateWD_DocketSubType();
					AssertNoErrors(docket.WD_DocketSubTypeInfo);

					docket.WD_DocketSubType = subType;
					AssertNoError(docket.WD_DocketSubTypeInfo, docket.Validation.BondNotSupportedCountryErrorMsg);
					AssertBondedSupportedByCountry(docket, Constants.CountryCodes.Singapore, isBondedSupported: false);
					AssertBondedSupportedByCountry(docket, Constants.CountryCodes.Australia, isBondedSupported: true);
					AssertBondedSupportedByCountry(docket, Constants.CountryCodes.NewZealand, isBondedSupported: false);
					AssertBondedSupportedByCountry(docket, Constants.CountryCodes.UnitedStates, isBondedSupported: true);

					docket.WD_DocketSubType = "XXX";
					AssertNoError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.BondNotEnabledErrorMsg);
					AssertNoError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.FreeStoreNotEnabledErrorMsg);

					docket.WD_DocketSubType = "";
					AssertNoError("Empty sub type should not be validated", docket.WD_DocketSubTypeInfo, WhsDocketValidation.BondNotEnabledErrorMsg);
					AssertNoError("Empty sub type should not be validated", docket.WD_DocketSubTypeInfo, WhsDocketValidation.FreeStoreNotEnabledErrorMsg);
				}
			}
			if (!isBondedWarehouseSubTypesExist)
			{
				var warehouse = Helper.CreateWarehouse("1");
				Helper.EnableWarehouseForBond(warehouse, true);
				docket.WD_WW_Whs = warehouse.PK;
				Helper.EnableWarehouseForFreeStore(warehouse, false);
				docket.Validation.ValidateAll();

				if (docket.IsCustomsTransaction)
				{
					AssertNoErrors(docket.WD_WW_WhsInfo);
					AssertNoErrors(docket.WD_DocketSubTypeInfo);
				}
				else
				{
					AssertHasError(docket.WD_WW_WhsInfo, WhsDocketValidation.FreeStoreNotEnabledErrorMsg);
					AssertHasError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.FreeStoreNotEnabledErrorMsg);
				}
			}
		}

		void AssertBondedSupportedByCountry(WhsDocket docket, string countryCode, bool isBondedSupported)
		{
			docket.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(countryCode).RL_Code;
			docket.Validation.ValidateWD_DocketSubType();

			if (!isBondedSupported)
			{
				AssertHasWarning(docket.WD_DocketSubTypeInfo, docket.Validation.BondNotSupportedCountryErrorMsg);
			}
			else
			{
				AssertNoWarning(docket.WD_DocketSubTypeInfo, docket.Validation.BondNotSupportedCountryErrorMsg);
			}
		}

		#endregion

		#region TestValidateWD_DocketSubType_IsNotEmptyAndValidType

		void TestValidateWD_DocketSubType_IsNotEmptyAndValidType()
		{
			if (IsDocketSubTypesSupported)
			{
				TestCodePairList(Docket.WD_DocketSubTypeInfo, ErrorCheckType.HasErrors, false, Docket.Lookups.SubTypes);
			}
			else
			{
				Assert("Subtypes are not supported, no need to test.", true);
			}
		}

		protected virtual bool IsDocketSubTypesSupported
		{
			get { return true; }
		}

		#endregion

		#region TestValidateWD_DocketSubType_InwardsProcessing

		public void TestValidateWD_DocketSubType_InwardsProcessing()
		{
			var isBondedWarehouseSubTypesExist = false;

			foreach (var subType in ValidSubTypeForInwardProcessing)
			{
				isBondedWarehouseSubTypesExist = true;

				var docket = GetNewBusinessObject();
				var whs1 = Helper.CreateWarehouse("1");
				var whs2 = Helper.CreateWarehouse("2");
				whs1.WW_IsVirtualWarehouse = true;
				whs2.WW_IsVirtualWarehouse = true;
				var iprArea = Helper.CreateArea(whs2, "IPR", AreaTypes.Codes.InwardProcessing);

				docket.WD_WW_Whs = whs1.PK;
				docket.WD_IsInwardsProcessingJob = true;
				docket.WD_DocketSubType = subType; // Set last to test setter runs validation
				AssertHasError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
				AssertHasError(docket.WD_WW_WhsInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
				AssertHasError(docket.WD_IsInwardsProcessingJobInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);

				docket.WD_WW_Whs = whs2.PK;
				AssertNoErrors(docket.WD_DocketSubTypeInfo);
				AssertNoErrors(docket.WD_WW_WhsInfo);
				AssertNoErrors(docket.WD_IsInwardsProcessingJobInfo);

				// Test Warehouse setter validation
				docket.WD_WW_Whs = whs1.PK;
				AssertHasError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
				AssertHasError(docket.WD_WW_WhsInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
				AssertHasError(docket.WD_IsInwardsProcessingJobInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);

				// Test inward processing setter validation
				docket.WD_IsInwardsProcessingJob = false;
				AssertNoError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
				AssertNoError(docket.WD_WW_WhsInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
				AssertNoError(docket.WD_IsInwardsProcessingJobInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);

				docket.WD_IsInwardsProcessingJob = true;
				AssertHasError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
				AssertHasError(docket.WD_WW_WhsInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
				AssertHasError(docket.WD_IsInwardsProcessingJobInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);
			}

			if (!isBondedWarehouseSubTypesExist)
			{
				Assert(true);
			}
		}

		protected virtual IEnumerable<string> ValidSubTypeForInwardProcessing => Enumerable.Empty<string>();

		#endregion

		#endregion

		#region TestValidateWD_DropMode

		public virtual void TestValidateWD_DropMode()
		{
			AssertNoNotifications(Docket.WD_DropModeInfo);

			AssertCollectionNotContains("Precondition: Test code not in the current drop mode selection", "TST", Docket.Lookups.DropModes);
			Docket.WD_DropMode = "TST";
			AssertHasErrors(Docket.WD_DropModeInfo);

			AssertNotEquals("Precondition: Should have valid selection codes", 0, Docket.Lookups.DropModes.Count);
			Docket.WD_DropMode = Docket.Lookups.DropModes[0].Code;
			AssertNoErrors(Docket.WD_DropModeInfo);
		}

		#endregion

		#region TestValidateWD_OH_Client

		#region TestValidateWD_OH_Client

		public virtual void TestValidateWD_OH_Client()
		{
			AssertWD_OH_ClientCreditCheck(false);
		}

		#endregion

		#region TestValidateWD_OH_Client_ForWeb

		public virtual void TestValidateWD_OH_Client_ForWeb()
		{
			AssertWD_OH_ClientCreditCheck(true);
		}

		protected virtual void AssertWD_OH_ClientCreditCheck(bool isForWeb)
		{
			Globals.IsWeb = isForWeb;

			//no client assigned to the docket so no warnings
			Docket.WD_OH_Client = ZGuid.Empty;
			AssertNoWarnings(Docket.WD_OH_ClientInfo);

			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Docket.WD_WW_Whs = whs.PK;

			//assigning a client with credit not on hold
			OrgHeader clientNotOnHold = Helper.CreateClient("1", "NotOnHold");
			clientNotOnHold.CompanyData.OB_AROnCreditHold = false;
			clientNotOnHold.OH_IsDebtor = true;
			Docket.WD_OH_Client = TestObjectCreator.AALSHI.PK;

			Factory.Save();

			Job job = TestObjectCreator.Job1;

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = clientNotOnHold.PK;
			aRInv.AH_InvoiceAmount = 51m;
			aRInv.AH_OutstandingAmount = 51m;

			ARInvoiceLine line = (ARInvoiceLine)aRInv.Lines.AddNew();
			AccChargeCode chargeCode = TestObjectCreator.CC1;
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_LocalExTaxAmount = 51m;

			JobCharge jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_OSSellAmt = 51m;

			Factory.Save();

			clientNotOnHold.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			Docket.WD_OH_Client = clientNotOnHold.PK;

			AssertNoWarnings("Credit on hold.", Docket.WD_OH_ClientInfo);

			//assigning a client with credit on hold
			OrgHeader clientOnHold = Helper.CreateClient("2", "OnHold");
			clientOnHold.CompanyData.OB_AROnCreditHold = true;
			Docket.WD_OH_Client = clientOnHold.PK;
			if (isForWeb)
			{
				AssertNoWarnings("Credit on hold.", Docket.WD_OH_ClientInfo);
			}
			else
			{
				AssertHasWarning(Docket.WD_OH_ClientInfo, "Credit on hold.");
			}

			// assert CreditChecker.ValidateIsCreditLimitExceeded is executed

			OrgHeader client = Helper.CreateClient("3", "Credit limit exceeded");
			client.CompanyData.OB_AROnCreditHold = false;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_ARCreditLimit = 33m;
			Docket.WD_OH_Client = TestObjectCreator.AALSHI.PK;

			aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = client.PK;
			aRInv.AH_InvoiceAmount = 51m;
			aRInv.AH_OutstandingAmount = 51m;

			line = (ARInvoiceLine)aRInv.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_LocalExTaxAmount = 51m;

			jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_OSSellAmt = 51m;

			Factory.Save();

			Docket.WD_OH_Client = ZGuid.Empty;
			AssertNoWarnings("Precondition", Docket.WD_OH_ClientInfo);
			Docket.WD_OH_Client = client.PK;
			if (isForWeb)
			{
				AssertNoWarnings(Docket.WD_OH_ClientInfo);
			}
			else
			{
				AssertEquals(1, Docket.WD_OH_ClientInfo.GetWarnings().GetUniqueMessageList().Length);
			}
			client.CompanyData.OB_AROnCreditHold = true;

			// assert both credit on hold and credit limit exceeded is checked
			Docket.WD_OH_Client = ZGuid.Empty;
			Docket.WD_OH_Client = client.PK;
			if (isForWeb)
			{
				AssertNoWarnings(Docket.WD_OH_ClientInfo);
			}
			else
			{
				AssertHasWarning(Docket.WD_OH_ClientInfo, "Credit on hold.");
				AssertEquals(2, Docket.WD_OH_ClientInfo.GetWarnings().GetUniqueMessageList().Length);
			}
		}

		#endregion

		#region TestValidateWD_OH_Client_CreditCheckWithOrgNotDebtor

		public void TestValidateWD_OH_Client_CreditCheckWithOrgNotDebtor()
		{
			AssertNoWarnings(Docket.WD_OH_ClientInfo);

			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Docket.WD_WW_Whs = whs.PK;

			Job job = TestObjectCreator.Job1;

			OrgHeader client = Helper.CreateClient("1", "Org Debtor");
			client.OH_IsDebtor = true;
			client.CompanyData.OB_ARCreditLimit = 33m;
			OrgHeader client2 = Helper.CreateClient("2", "Org Not Debtor");
			client2.CompanyData.OB_AROnCreditHold = false;
			client2.OH_IsDebtor = false;
			client2.CompanyData.OB_ARCreditLimit = 33m;
			Docket.WD_OH_Client = TestObjectCreator.AALSHI.PK;

			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = client.PK;
			aRInv.AH_InvoiceAmount = 51m;
			aRInv.AH_OutstandingAmount = 51m;

			ARInvoiceLine line = (ARInvoiceLine)aRInv.Lines.AddNew();
			AccChargeCode chargeCode = TestObjectCreator.CC1;
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_LocalExTaxAmount = 51m;

			JobCharge jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_OSSellAmt = 51m;

			Factory.Save();

			client.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			client2.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			if (Docket is WhsAdjustment || Docket is WhsTransfer)
			{
				AssertNoWarnings("No Warning", Docket.WD_OH_ClientInfo);
			}
			else
			{
				Docket.WD_OH_Client = ZGuid.Empty;
				AssertNoWarnings("Precondition", Docket.WD_OH_ClientInfo);

				Docket.WD_OH_Client = ZGuid.Empty;
				Docket.WD_OH_Client = client.PK;
				if (!Globals.IsWeb)
				{
					AssertHasWarningContaining(Docket.WD_OH_ClientInfo, "over the credit limit");
				}
				else
				{
					AssertNoWarnings(Docket.WD_OH_ClientInfo);
				}

				Docket.WD_OH_Client = ZGuid.Empty;
				Docket.WD_OH_Client = client2.PK;
				AssertNoWarnings(Docket.WD_OH_ClientInfo);
			}
		}

		#endregion

		#region TestValidateWD_OH_Client_ReservedQty

		public void TestValidateWD_OH_Client_ReservedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient("CLIENT2");
			var expectedErrorMessage = "Some of the lines are Cross Docked, you cannot change the Client.";

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = client2.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();
			AssertNoError("Precondition", docket.WD_OH_ClientInfo, expectedErrorMessage);

			docket.WD_OH_Client = data.Org1.PK;
			Factory.Save();
			AssertNoError("Changing client will not cause any issues, since nothing is reserved for previous client.", docket.WD_OH_ClientInfo, expectedErrorMessage);

			// reserving some inventory
			var line = docket.Lines.AddNew();
			if (docket is WhsPickableDocket)
			{
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Helper.CreateReservePickLine((WhsPickableDocketLine)line, receive.Inventory[0], 5m);
			}
			else
			{
				// temporary remove when removing inventory table
				_ = line.Inventory.Count > 0 ? line.Inventory[0] : line.Inventory.AddNew();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
				Helper.CreateReservePickLine(order.Lines[0], line.Inventory[0], 5m);
			}
			docket.WD_OH_Client = client2.PK;
			AssertHasError("Changing client while having reserved qty should cause error.", docket.WD_OH_ClientInfo, expectedErrorMessage);

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			docket.Validation.ValidateWD_OH_Client();
			AssertNoError("Finalised dockets should not run validation for performance reasons.", docket.WD_OH_ClientInfo, expectedErrorMessage);
		}

		#endregion

		#endregion

		#region TestValidateWD_ExternalReference

		public virtual void TestValidateWD_ExternalReference()
		{
			Docket.WD_ExternalReferenceSplit = 0;
			Docket.WD_ExternalReference = "1";
			AssertNoErrors(Docket.WD_ExternalReferenceInfo);

			WhsDocket docket2 = GetNewBusinessObject();
			docket2.WD_ExternalReferenceSplit = 0;
			docket2.WD_ExternalReference = "1";

			if (docket2.IsUniqueExternalReferenceCreatedOnSave)
			{
				AssertNoErrors(docket2.WD_ExternalReferenceInfo);
			}
			else
			{
				AssertHasError(docket2.WD_ExternalReferenceInfo, docket2.Validation.DuplicateReferenceErrorMsg());
			}

			docket2.WD_ExternalReferenceSplit = 1;
			docket2.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(docket2.WD_ExternalReferenceInfo);

			WhsDocket docket3 = GetNewBusinessObject();
			docket3.WD_ExternalReference = "";
			docket3.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(docket3.WD_ExternalReferenceInfo);

			// setup for Factory.Save()
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			OrgHeader org = Helper.CreateClient();
			docket2.WD_WW_Whs = whs.PK;
			docket2.WD_OH_Client = org.PK;
			Docket.Delete();
			docket3.Delete();
			Factory.Save();

			docket2.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(docket2.WD_ExternalReferenceInfo);
		}

		public void TestValidateWD_ExternalReference_SkipIfAutoCreatingWorkOrders()
		{
			var docket1 = Docket;
			docket1.WD_ExternalReference = "Same";

			var docket2 = GetNewBusinessObject();

			if (docket2.WD_DocketType == DocketType.Codes.WorkOrder)
			{
				var workOrder = docket2 as WhsWorkOrder;
				using (workOrder.BOM.SuspendAutoCreatingWorkOrdersSemaphoreForTest())
				{
					docket2.WD_ExternalReference = "Same";
					AssertNoErrors(workOrder.WD_ExternalReferenceInfo);
				}
			}
			else if (docket2.IsUniqueExternalReferenceCreatedOnSave)
			{
				docket2.WD_ExternalReference = "Same";
				AssertNoErrors(docket2.WD_ExternalReferenceInfo);
			}
			else
			{
				docket2.WD_ExternalReference = "Same";
				AssertHasError(docket2.WD_ExternalReferenceInfo, docket2.Validation.DuplicateReferenceErrorMsg());
			}
		}

		#endregion

		#region TestValidateWD_WSH_SalesChannel

		public void TestValidateWD_WSH_SalesChannel()
		{
			var docket = GetNewBusinessObject();
			AssertNoErrors("Empty Sales Channels are allowed on all WhsDockets.", docket.WD_WSH_SalesChannelInfo);

			docket.WD_WSH_SalesChannel = Factory.New<WhsSalesChannel>().PK;
			if (docket.IsSalesChannelAllowed)
			{
				AssertNoErrors("Sales Channels should be allowed on this Docket.", docket.WD_WSH_SalesChannelInfo);
			}
			else
			{
				AssertHasError("Sales Channels are not allowed on this Docket.", docket.WD_WSH_SalesChannelInfo, "Sales Channels are only allowed on Warehouse Orders.");
			}
		}

		#endregion

		// totals

		#region TestValidateWD_TotalUnits

		public virtual void TestValidateWD_TotalUnits()
		{
			TestMinDecimal(Docket.WD_TotalUnitsInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestValidateWD_TotalWeight

		public void TestValidateWD_TotalWeight()
		{
			TestMinDecimal(Docket.WD_TotalWeightInfo, ErrorCheckType.HasErrors, 0);
		}

		public void TestCheckWD_TotalWeight_MatchesLinesLevelWeight()
		{
			AssertValidationForDocketTotalVsLinesTotal("Weight", WhsDocketSchema.WD_TotalWeight, WhsDocketSchema.WD_TotalWeightUnit, OrgSupplierPartSchema.OP_Weight, OrgSupplierPartSchema.OP_WeightUQ, Constants.Weight.Kilograms);

			var weightMeasure = new WeightMeasure("Weight", Constants.Weight.Kilograms);
			AssertValidationForDocketTotalVsLinesTotal_HandlesRounding(weightMeasure, WhsDocketSchema.WD_TotalWeight, WhsDocketSchema.WD_TotalWeightUnit, OrgSupplierPartSchema.OP_Weight, OrgSupplierPartSchema.OP_WeightUQ, Constants.Weight.Pounds);
		}

		#region TestCheckWD_TotalWeight_DecimalOverflowException

		public void TestCheckWD_TotalWeight_DecimalOverflowException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 1000m;
			data.Part1.OP_WeightUQ = Constants.Weight.Kilograms;

			var docket = GetNewBusinessObject();
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var expectedRowError = "Attempt to overflow capacity of Weight. Please validate your setup and restart the process.";

				docket.WD_OH_Client = data.Org1.PK;
				docket.WD_WW_Whs = data.Whs1.PK;
				docket.WD_TotalWeightUnit = Constants.Weight.Kilograms;

				AddLineForCheckWD_TotalWeightOrCubicValidation(docket, data.Part1, decimal.MaxValue);

				docket.RemoveRowError(expectedRowError); // ensure error is not added in business layer but during validation
				AssertNoExceptionThrown("Re-calculation of weight from lines during validation should not cause exceptions.", () => docket.Validation.ValidateWD_TotalWeight());
				AssertHasRowError(docket, expectedRowError);
			}
			else
			{
				Assert("Total Weight is not calculated for the Docket Type.", true);
			}
		}

		#endregion

		#endregion

		#region TestValidateWD_TotalWeightUnit

		public void TestValidateWD_TotalWeightUnit()
		{
			TestCodePairList(Docket.WD_TotalWeightUnitInfo, ErrorCheckType.HasErrors, false, Docket.TotalWeightUnits);
		}

		#endregion

		#region TestValidateWD_TotalCubic

		public void TestValidateWD_TotalCubic()
		{
			TestMinDecimal(Docket.WD_TotalCubicInfo, ErrorCheckType.HasErrors, 0);
		}

		public void TestValidateWD_TotalCubic_MatchesLinesLevelCubic()
		{
			AssertValidationForDocketTotalVsLinesTotal("Volume", WhsDocketSchema.WD_TotalCubic, WhsDocketSchema.WD_TotalCubicUnit, OrgSupplierPartSchema.OP_Cubic, OrgSupplierPartSchema.OP_CubicUQ, Constants.Volume.CubicMetres);

			var volumeMeasure = new VolumeMeasure("Volume", Constants.Volume.CubicMetres);
			AssertValidationForDocketTotalVsLinesTotal_HandlesRounding(volumeMeasure, WhsDocketSchema.WD_TotalCubic, WhsDocketSchema.WD_TotalCubicUnit, OrgSupplierPartSchema.OP_Cubic, OrgSupplierPartSchema.OP_CubicUQ, Constants.Volume.CubicFeet);
		}

		#region TestCheckWD_TotalCubic_DecimalOverflowException

		public void TestCheckWD_TotalCubic_DecimalOverflowException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Cubic = 1000m;
			data.Part1.OP_CubicUQ = Constants.Volume.Litre;

			var docket = GetNewBusinessObject();
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var expectedRowError = "Attempt to overflow capacity of Volume. Please validate your setup and restart the process.";

				docket.WD_OH_Client = data.Org1.PK;
				docket.WD_WW_Whs = data.Whs1.PK;
				docket.WD_TotalCubicUnit = Constants.Volume.Litre;

				AddLineForCheckWD_TotalWeightOrCubicValidation(docket, data.Part1, decimal.MaxValue);

				docket.RemoveRowError(expectedRowError); // ensure error is not added in business layer but during validation
				AssertNoExceptionThrown("Re-calculation of volume from lines during validation should not cause exceptions.", () => docket.Validation.ValidateWD_TotalCubic());
				AssertHasRowError(docket, expectedRowError);
			}
			else
			{
				Assert("Total Volume is not calculated for the Docket Type.", true);
			}
		}

		#endregion

		#endregion

		#region TestValidateWD_TotalCubicUnit

		public void TestValidateWD_TotalCubicUnit()
		{
			TestCodePairList(Docket.WD_TotalCubicUnitInfo, ErrorCheckType.HasErrors, false, Docket.TotalCubicUnits);
		}

		#endregion

		#region AssertValidationForDocketTotalVsLinesTotal

		void AssertValidationForDocketTotalVsLinesTotal(
			ZString docketTotalDescription,
			SchemaDecimalColumn docketTotalColumn,
			SchemaStringColumn docketTotalUQColumn,
			SchemaDecimalColumn partWeightOrVolumeColumn,
			SchemaStringColumn partWeightOrVolumeUQColumn,
			ZString docketUQ)
		{
			var docket = GetNewBusinessObject();
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);

				// setup the docket
				docket.WD_OH_Client = data.Org1.PK;
				docket.WD_WW_Whs = data.Whs1.PK;
				docket[docketTotalUQColumn] = docketUQ;

				SetUpProductForTotal(data, partWeightOrVolumeColumn, partWeightOrVolumeUQColumn, docketUQ);

				var expectedWarningPrefix = ZString.Format("Original {0} of product calculated from the product master file was", docketTotalDescription);

				AddLineForCheckWD_TotalWeightOrCubicValidation(docket, data.Part1, 10m);

				var info = docket.ZPropertyInfoHash[docketTotalColumn.Name];
				AssertTotalInfo(docket, expectedWarningPrefix, docketTotalColumn, info);
			}
			else
			{
				Assert("Total Weight or Volume is not used.", true);
			}
		}

		protected virtual void SetUpProductForTotal(TestDataSimpleEnvironment data, SchemaDecimalColumn partWeightOrVolumeColumn, SchemaStringColumn partWeightOrVolumeUQColumn, ZString docketUQ)
		{
			data.Part1[partWeightOrVolumeColumn] = 5m;
			data.Part1[partWeightOrVolumeUQColumn] = docketUQ;
		}

		protected virtual void AssertTotalInfo(WhsDocket whsDocket, ZString expectedWarningPrefix, SchemaDecimalColumn docketTotalColumn, ZPropertyInfo info)
		{
			AssertEquals("Precondition: Weight/Volume of a docket should be populated from lines.", 50m, whsDocket[docketTotalColumn]);
			AssertEquals(false, info.Notifications.Any(n => n.Message.Contains(expectedWarningPrefix)));

			whsDocket[docketTotalColumn] = 70m;
			info.Notifications.Single(n => n.Message.Contains(expectedWarningPrefix));

			whsDocket[docketTotalColumn] = -10m;
			AssertHasErrors(info);
			AssertEquals(false, info.Notifications.Any(n => n.Message.Contains(expectedWarningPrefix)));

			whsDocket[docketTotalColumn] = 50m;
			AssertEquals(false, info.Notifications.Any(n => n.Message.Contains(expectedWarningPrefix)));
		}

		void AssertValidationForDocketTotalVsLinesTotal_HandlesRounding(
			IUnitOfMeasure unitOfMeasure,
			SchemaDecimalColumn docketTotalColumn,
			SchemaStringColumn docketTotalUQColumn,
			SchemaDecimalColumn partWeightOrVolumeColumn,
			SchemaStringColumn partWeightOrVolumeUQColumn,
			ZString productUQ)
		{
			// ensure rounding works
			var docket = GetNewBusinessObject();
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);

				// setup the docket
				docket.WD_OH_Client = data.Org1.PK;
				docket.WD_WW_Whs = data.Whs1.PK;
				docket[docketTotalUQColumn] = unitOfMeasure.TotalUQ;

				// setup the product with a different Unit to the Docket
				data.Part1[partWeightOrVolumeColumn] = 5m;
				data.Part1[partWeightOrVolumeUQColumn] = productUQ;

				// add a single line to the docket
				AddLineForCheckWD_TotalWeightOrCubicValidation(docket, data.Part1, 10m);

				// ensure no warning due to rounding (WD_TotalWeight/Cubic rounds to 2 and 3 decimals respectively)
				var docketTotalInfo = docket.ZPropertyInfoHash[docketTotalColumn.Name];
				var docketTotalUQInfo = docket.ZPropertyInfoHash[docketTotalUQColumn.Name];
				var totalFromLines = (ZDecimal)UnitOfMeasureConverter.GetTotalQuantityFromLines(docket, new WrappingUnitOfMeasure(unitOfMeasure), GetLinesForWeightAndVolumeCalculation(docket).Select(l => new LineWithProductAndQuantity(l.WE_OP, l.WE_TransactionQuantity)));
				AssertEquals("Precondition - The line total decimals must exceed the docket DB field decimals to prove that rounding works.", true, totalFromLines.Normalize().DecimalPlaces > docketTotalColumn.Scale);

				AssertNoWarnings(docketTotalInfo);
			}
		}

		protected virtual IEnumerable<WhsDocketLine> GetLinesForWeightAndVolumeCalculation(TDocket docket) => docket.Lines;

		class WrappingUnitOfMeasure : IUnitOfMeasure
		{
			public WrappingUnitOfMeasure(IUnitOfMeasure wrappedMeasure)
			{
				WrappedMeasure = wrappedMeasure;
			}

			readonly IUnitOfMeasure WrappedMeasure;

			public ZString Name => WrappedMeasure.Name;
			public ZString TotalUQ => WrappedMeasure.TotalUQ;

			public int DecimalPlacesForRounding => 10;

			public ZDecimal GetQuantityFromProduct(OrgSupplierPart part) => WrappedMeasure.GetQuantityFromProduct(part);
			public ZString GetUQFromProduct(OrgSupplierPart part) => WrappedMeasure.GetUQFromProduct(part);
		}

		protected virtual void AddLineForCheckWD_TotalWeightOrCubicValidation(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			var line1 = docket.Lines.AddNew();
			line1.WE_OP = part.PK;
			line1.WE_TransactionQuantity = units;
		}

		#endregion

		//

		#region TestValidateWD_PickPriority

		public void CheckWD_PickPriority()
		{
			Docket.WD_PickPriority = (ZByte)1;
			AssertNoErrors(Docket.WD_PickPriorityInfo);

			Docket.WD_PickPriority = (ZByte)21;
			AssertHasErrors(Docket.WD_PickPriorityInfo);
		}
		#endregion

		#region TestValidateWD_BookingDate

		public void TestValidateWD_BookingDate()
		{
			TestMandatoryDateTimeOffset(Docket.WD_BookingDateInfo, ErrorCheckType.HasErrors);
		}

		#endregion

		#region TestValidateWD_PackagesSent

		public void TestValidateWD_PackagesSent()
		{
			TestMinInt(Docket.WD_PackagesSentInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestValidateWD_PalletsSent

		public void TestValidateWD_PalletsSent()
		{
			TestMinShort(Docket.WD_PalletsSentInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestValidateWD_RSServiceLevel

		public void TestValidateWD_RSServiceLevel()
		{
			Docket.WD_RS_NKServiceLevel = "";
			AssertNoNotifications(Docket.WD_RS_NKServiceLevelInfo);

			Docket.WD_RS_NKServiceLevel = "YYY";
			AssertHasWarnings(Docket.WD_RS_NKServiceLevelInfo);

			var serviceLevel = Docket.Lookups.ServiceLevels.AddNew();
			serviceLevel.RS_Code = "ABC";
			Docket.WD_RS_NKServiceLevel = "ABC";
			AssertNoWarnings(Docket.WD_RS_NKServiceLevelInfo);

			Docket.WD_RS_NKServiceLevel = "XXX";
			AssertHasWarnings(Docket.WD_RS_NKServiceLevelInfo);
		}

		#endregion

		#region TestValidateWD_UnitsSent

		public void TestValidateWD_UnitsSent_MinValue()
		{
			TestMinDecimal(Docket.WD_UnitsSentInfo, ErrorCheckType.HasErrors, 0);
		}

		public void TestValidateWD_UnitsSent_WarningMaxValue()
		{
			var maxValue = int.MaxValue;
			var warningMessage = "Please do not enter a quantity greater than 2147483647 it will not export.";
			var docket = GetNewBusinessObject();

			docket.WD_UnitsSent = new decimal(maxValue / 2);
			AssertNoWarningContaining(docket.WD_UnitsSentInfo, warningMessage);

			docket.WD_UnitsSent = new decimal(-2);
			AssertNoWarningContaining(docket.WD_UnitsSentInfo, warningMessage);

			docket.WD_UnitsSent = new decimal(maxValue) + 2;
			AssertHasWarning(docket.WD_UnitsSentInfo, warningMessage);

			docket.WD_UnitsSent = new decimal(0);
			AssertNoWarningContaining(docket.WD_UnitsSentInfo, warningMessage);
		}

		#endregion

		#region TestValidateWD_CustomAttributes

		public void TestValidateWD_CustomAttributes()
		{
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = Helper.CreateClient().PK;

			if (((ICustomLabelsConfigOrgProvider)docket).ConfigOrg != null)
			{
				new CustomLabelsTestCase(docket, new WhsDocket.CustomLabelsProvider(docket)).TestCustomLabelsMandatoryValidation();
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestTransportCoNameOrPKValidation

		public void TestTransportCoNameOrPKValidation()
		{
			TestTransportCoNameOrPKValidationCore();
		}

		protected virtual void TestTransportCoNameOrPKValidationCore()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				var transportCo = Factory.New<OrgHeader>();
				transportCo.OH_IsTransportClient = true;

				job.TransportCoDocAddress.E2_AddressOverride = false;
				ValidateTransportCoNameOrPK(docket);
				AssertNoErrors(job.TransportCoNameOrPKInfo);

				job.TransportCoDocAddress.OrganisationPK = transportCo.PK;
				ValidateTransportCoNameOrPK(docket);
				AssertNoErrors(job.TransportCoNameOrPKInfo);

				job.TransportCoDocAddress.E2_AddressOverride = true;
				job.TransportCoDocAddress.E2_CompanyName = "Blah";
				ValidateTransportCoNameOrPK(docket);
				AssertNoErrors(job.TransportCoNameOrPKInfo);

				job.TransportCoDocAddress.E2_CompanyName = "";
				ValidateTransportCoNameOrPK(docket);
				AssertHasErrors(job.TransportCoNameOrPKInfo);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual void ValidateTransportCoNameOrPK(WhsDocket docket)
		{
			throw new InvalidOperationException("Override ValidateTransportCoNameOrPK in test if your Job uses Transport Companies.");
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			Docket.AddRowError("TEST ERROR");
			AssertEquals(true, Docket.HasRowErrors);

			Docket.Validation.ValidateAll();
			AssertEquals("Should not clear row errors", true, Docket.HasRowErrors);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var docket = GetNewBusinessObject();
			var validation = new TestWhsDocketValidation(docket);

			var list = new string[]
			{
				WhsDocketSchema.Constants.WD_WL_CrossDock,
				WhsDocketSchema.Constants.WD_WL_InboundDockDoor,
				WhsDocketSchema.Constants.WD_WLO_PlannedLoad,
				WhsDocketSchema.Constants.WD_WSH_SalesChannel,
				WhsDocketSchema.Constants.WD_F3_NKTotalPackType,
				WhsDocketSchema.Constants.WD_PL_NKCarrierServiceLevel,
				WhsDocketSchema.Constants.WD_P9_PackingTask,
			};

			foreach (var propertyInfo in docket.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsDocketValidation

		class TestWhsDocketValidation : WhsDocketValidation
		{
			public TestWhsDocketValidation(WhsDocket parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;

		protected TDocket GetNewBusinessObject()
		{
			return Factory.New<TDocket>();
		}

		protected TDocket Docket
		{
			get => docket ?? (docket = GetNewBusinessObject());
		}

		TDocket docket;

		#endregion
	}
}
