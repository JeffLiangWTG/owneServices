using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketInvoicingSupporter))]
	public class WhsDocketInvoicingSupporterTest : WhsJobInvoicingSupporterTest<WhsDocketInvoicingSupporter, WhsDocket>
	{
		#region TestIJobInvoicingPlugIn_PostedStateChanged

		public void TestIJobInvoicingPlugIn_PostedStateChanged()
		{
			var docket = GetNewBusinessObject();
			bool wasPostedStateChangedCalled = false;
			((IBindingList)docket).ListChanged += delegate
			{ wasPostedStateChangedCalled = true; };
			docket.InvoicingSupporter.PostedStateChanged();
			AssertEquals("PostedStateChanged should be called.", true, wasPostedStateChangedCalled);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_OverriddenDepartmentPK

		public virtual void TestIJobInvoicingPlugIn_OverriddenDepartmentPK()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZGuid.Empty, jobInvPlugIn.InvoicingSupporter.OverriddenDepartmentPK);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_Origin

		public virtual void TestIJobInvoicingPlugIn_Origin()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.Origin);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_Destination

		public virtual void TestIJobInvoicingPlugIn_Destination()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.Destination);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_TranshipmentPort

		public virtual void TestIJobInvoicingPlugIn_TranshipmentPort()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.GetTranshipmentPort(CostSell.Cost));
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.GetTranshipmentPort(CostSell.Revenue));
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ConsolRateCurrency

		public virtual void TestIJobInvoicingPlugIn_ConsolRateCurrency()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.ConsolRateCurrency);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ActualChargeable

		public virtual void TestIJobInvoicingPlugIn_ActualChargeable()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(0m, jobInvPlugIn.InvoicingSupporter.ActualChargeable);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ActualChargeableUnit

		public virtual void TestIJobInvoicingPlugIn_ActualChargeableUnit()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ActualChargeableUnit);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_TransportMode

		public virtual void TestIJobInvoicingPlugIn_TransportMode()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.TransportMode);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_IsDirectShipment

		public virtual void TestIJobInvoicingPlugIn_IsDirectShipment()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(false, jobInvPlugIn.InvoicingSupporter.IsDirectShipment);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_DefaultDebtor

		public void TestIJobInvoicingPlugIn_DefaultDebtor()
		{
			if (IsIJobInvoicingPlugIn_DefaultDebtorOverriden)
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();

				Docket.WD_OH_Client = org1.PK;
				AssertNull(((IJobInvoicingPlugIn)Docket).InvoicingSupporter.GetDefaultDebtor(null));

				org1.SetRelatedParty(org2, RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, "");
				AssertEquals(org2, ((IJobInvoicingPlugIn)Docket).InvoicingSupporter.GetDefaultDebtor(null));
			}
			else
			{
				AssertNull(((IJobInvoicingPlugIn)Docket).InvoicingSupporter.GetDefaultDebtor(null));
			}
		}

		protected virtual bool IsIJobInvoicingPlugIn_DefaultDebtorOverriden
		{
			get { return false; }
		}

		#endregion

		#region TestIJobInvoicingPlugIn_SendingAgent

		public virtual void TestIJobInvoicingPlugIn_SendingAgent()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.SendingAgent);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ContainerMode

		public virtual void TestIJobInvoicingPlugIn_ContainerMode()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ContainerMode);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_IsPlugInReadOnly

		public virtual void TestIJobInvoicingPlugIn_IsPlugInReadOnly()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(false, jobInvPlugIn.InvoicingSupporter.IsPlugInReadOnly);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ReceivingAgent

		public virtual void TestIJobInvoicingPlugIn_ReceivingAgent()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.ReceivingAgent);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_IsImport

		public virtual void TestIJobInvoicingPlugIn_IsImport()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(false, jobInvPlugIn.InvoicingSupporter.IsImport);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ConsolExchangeRate

		public virtual void TestIJobInvoicingPlugIn_ConsolExchangeRate()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(0m, jobInvPlugIn.InvoicingSupporter.ConsolExchangeRate);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_Consignor

		public virtual void TestIJobInvoicingPlugIn_Consignor()
		{
			OrgHeader client = Helper.CreateClient();
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();

			Docket.WD_OH_Client = client.PK;
			AssertEquals(client, ((IJobInvoicingPlugIn)Docket).InvoicingSupporter.Consignor);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ConsumerType

		public void TestIJobInvoicingPlugIn_ConsumerType()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ExpectedConsumerType, jobInvPlugIn.InvoicingSupporter.ConsumerType);
		}

		protected virtual JobInvoicingConsumerType ExpectedConsumerType
		{
			get { return null; }  // WhsTransfer use WhsDocket implementation ... IJobInvoicing should be only implemented on classes that do, so this should be refactored, but not going to do it in this checkin
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ShipmentNumberOfColoadMaster

		public virtual void TestIJobInvoicingPlugIn_ShipmentNumberOfColoadMaster()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ShipmentNumberOfColoadMaster);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_HouseBillNumber

		public virtual void TestIJobInvoicingPlugIn_HouseBillNumber()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.HouseBillNumber);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_MasterBillNumber

		public virtual void TestIJobInvoicingPlugIn_MasterBillNumber()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.MasterBillNumber);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ETA

		public virtual void TestIJobInvoicingPlugIn_ETA()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZDateTime.Empty, jobInvPlugIn.InvoicingSupporter.ETA);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ETD

		public virtual void TestIJobInvoicingPlugIn_ETD()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZDateTime.Empty, jobInvPlugIn.InvoicingSupporter.ETD);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ActualWeight

		public virtual void TestIJobInvoicingPlugIn_ActualWeight()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(0M, jobInvPlugIn.InvoicingSupporter.ActualWeight);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ActualWeightUnit

		public virtual void TestIJobInvoicingPlugIn_ActualWeightUnit()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ActualWeightUnit);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ActualVolume

		public virtual void TestIJobInvoicingPlugIn_ActualVolume()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(0M, jobInvPlugIn.InvoicingSupporter.ActualVolume);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ActualVolumeUnit

		public virtual void TestIJobInvoicingPlugIn_ActualVolumeUnit()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ActualVolumeUnit);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_CreateAccountingJobOnSavingOfOperationsJob

		public virtual void TestIJobInvoicingPlugIn_CreateAccountingJobOnSavingOfOperationsJob()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(true, jobInvPlugIn.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		#endregion

		#region TestOperationsBranch_FallBackToWarehouseBranch_WhenConfiguredInRegistry

		public void TestOperationsBranch_FallBackToWarehouseBranch_WhenConfiguredInRegistry()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchAUSYD = company.Branches.AddNew();
			branchAUSYD.GB_Code = "AUS";
			branchAUSYD.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var client = Factory.NewWithValidTestData<OrgHeader>();
				var clientBranch = Factory.NewWithValidTestData<GlbBranch>();
				client.CompanyData.OB_GB_ControllingBranch = clientBranch.PK;

				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				var warehouseBranch = Factory.NewWithValidTestData<GlbBranch>();
				warehouse.WW_GB_RelatedCompanyBranch = warehouseBranch.PK;

				Docket.WD_OH_Client = client.PK;
				Docket.WD_WW_Whs = warehouse.PK;
				Docket.IsImportingData = true;

				var jobInvPlugIn = (IJobInvoicingPlugIn)Docket;
				SetUpRegistry(1, 0, 0, 0);
				AssertNull("Operations Branch should be Null.", jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				SetUpRegistry(0, 1, 0, 0);
				AssertEquals("OperationsBranch should set to warehouseBranch.", warehouseBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				SetUpRegistry(0, 0, 1, 0);
				AssertEquals("Operations Branch should be Client's Branch.", clientBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				SetUpRegistry(0, 0, 0, 1);
				AssertEquals("Operations Branch should be Login user's current branch.", branchAUSYD.PK, RegistryHelper.GetBillingOperationsBranch(warehouse, client).PK);

				warehouseBranch.GB_IsActive = false;
				SetUpRegistry(0, 1, 2, 3);
				AssertEquals("Operations Branch should be Client's Branch if warehouse's branch is inactive.", clientBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);
			}
		}

		#endregion

		#region TestIJobInvoicingPlugIn_OperationsBranch_BasedOnRegistrySettings

		public void TestIJobInvoicingPlugIn_OperationsBranch_BasedOnRegistrySettings()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchAUSYD = company.Branches.AddNew();
			branchAUSYD.GB_Code = "AUS";
			branchAUSYD.GB_RL_NKHomePort = "AUSYD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var client = Factory.NewWithValidTestData<OrgHeader>();
				var clientBranch = Factory.NewWithValidTestData<GlbBranch>();
				client.CompanyData.OB_GB_ControllingBranch = clientBranch.PK;

				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				var warehouseBranch = Factory.NewWithValidTestData<GlbBranch>();
				warehouse.WW_GB_RelatedCompanyBranch = warehouseBranch.PK;

				Docket.WD_OH_Client = client.PK;
				Docket.WD_WW_Whs = warehouse.PK;

				Factory.Save();

				// Default to blank
				SetUpRegistry(1, 0, 0, 0);
				var jobInvPlugIn = (IJobInvoicingPlugIn)Docket;
				AssertNull("Operations Branch should be Null.", jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				// Default to Warehouse Branch
				SetUpRegistry(0, 1, 0, 0);
				AssertEquals("Operations Branch should be Warehouse's Branch.", warehouseBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				// Default to Organisation Branch
				SetUpRegistry(0, 0, 1, 0);
				AssertEquals("Operations Branch should be Client's Branch.", clientBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				// Default to Login User Default
				SetUpRegistry(0, 0, 0, 1);
				AssertEquals("Operations Branch should be Login user's current branch.", branchAUSYD.PK, jobInvPlugIn.InvoicingSupporter.OperationsBranch.PK);

				// Default to Warehouse Branch, but however it's inactive, so should return to next level which is Organisation Branch
				warehouseBranch.GB_IsActive = false;
				SetUpRegistry(0, 1, 2, 3);
				AssertEquals("Operations Branch should be Client's Branch if warehouse's branch is inactive.", clientBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);
			}
		}

		#endregion

		#region TestIJobInvoicingPlugIn_EditSecurity

		public virtual void TestIJobInvoicingPlugIn_EditSecurity()
		{
			IJobInvoicingPlugIn testJob = GetNewBusinessObject();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_DefaultChargeGroup

		public void TestIJobInvoicingPlugIn_DefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = GetNewBusinessObject();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region TestSetDefaultsForNewCharge

		public void TestSetDefaultsForNewCharge()
		{
			var docket = GetNewDocket();
			var invoicingSupporter = GetNewSupporter(docket);
			var job = Helper.CreateRatingJob(docket);

			var charge1 = Factory.New<JobCharge>();
			var charge2 = Factory.New<JobCharge>();
			var charge3 = Factory.New<JobCharge>();
			var attrib3 = charge3.JobChargeAttributes.AddNew();
			attrib3.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			attrib3.EC_Value = "TEST";

			bool isValidationSuspended = false;
			charge1.JobChargeAttributes.CountChanged += (sender, e) => isValidationSuspended = charge1.JobChargeAttributes[0].IsValidationSuspended;

			docket.WD_ExternalReference = "";
			invoicingSupporter.SetDefaultsForNewCharge(charge1);
			AssertEquals("New charges by default should get 1 attribute (Ex. Reference).", 1, charge1.JobChargeAttributes.Count);
			AssertEquals("Docket reference should be added as a default attribute to new charges even if it is empty.", "", charge1.JobChargeAttrib_DocketReference);
			AssertEquals("Validation should be suspended for the Charge that is being added.", true, isValidationSuspended);

			docket.WD_ExternalReference = "REF-1";
			invoicingSupporter.SetDefaultsForNewCharge(charge2);
			AssertEquals("New charges by default should get 1 attribute (Ex. Reference).", 1, charge2.JobChargeAttributes.Count);
			AssertEquals("ACTUAL TEST -- Docket reference should be added as a default attribute to new charges.", "REF-1", charge2.JobChargeAttrib_DocketReference);

			invoicingSupporter.SetDefaultsForNewCharge(charge3);
			AssertEquals("Existing charge attribute should not be overriden or duplicated.", 1, charge3.JobChargeAttributes.Count);
			AssertEquals("Docket reference should not be overriden.", "TEST", charge3.JobChargeAttrib_DocketReference);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_FIN

		public void TestIJobInvoicingPlugIn_FIN()
		{
			IJobInvoicingPlugIn jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZDateTimeOffset.Empty, jobInvPlugIn.InvoicingSupporter.FIN);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_REQ

		public void TestIJobInvoicingPlugIn_REQ()
		{
			var jobInvPlugIn = GetNewBusinessObject();
			AssertEquals(ZDateTimeOffset.Empty, jobInvPlugIn.InvoicingSupporter.REQ);
		}

		#endregion

		#region SetUpRegistry

		void SetUpRegistry(ZShort defaultToBlank, ZShort defaultToBranchRelatedToPortOrWarehouseBranch, ZShort defaultToBranchOfOrganisation, ZShort defaultToLoginUserDefault)
		{
			var rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = defaultToBlank;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = defaultToBranchRelatedToPortOrWarehouseBranch;
			rule.DefaultToBranchOfOrganisation = defaultToBranchOfOrganisation;
			rule.DefaultToLoginUserDefault = defaultToLoginUserDefault;

			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
		}

		#endregion

		#region TestGetReasonNotToAllowAutoRate

		public void TestGetReasonNotToAllowAutoRate_NewDocket()
		{
			TestGetReasonNotToAllowAutoRateCore(DocketStatus.Codes.New, false);
		}

		public void TestGetReasonNotToAllowAutoRate_EnteredDocket()
		{
			TestGetReasonNotToAllowAutoRateCore(DocketStatus.Codes.Entered, false);
		}

		public void TestGetReasonNotToAllowAutoRate_AttachedToPickDocket()
		{
			TestGetReasonNotToAllowAutoRateCore(DocketStatus.Codes.AttachedToPick, false);
		}

		public void TestGetReasonNotToAllowAutoRate_FinalisedDocket()
		{
			TestGetReasonNotToAllowAutoRateCore(DocketStatus.Codes.Finalised, false);
		}

		public void TestGetReasonNotToAllowAutoRate_CancelledDocket()
		{
			TestGetReasonNotToAllowAutoRateCore(DocketStatus.Codes.Cancelled, true);
		}

		protected void TestGetReasonNotToAllowAutoRateCore(string docketStatus, bool hasErrorMessage)
		{
			var docket = GetNewDocket();
			var supporter = GetNewSupporter(docket);
			docket.WD_DocketStatus = docketStatus;

			if (hasErrorMessage)
			{
				AssertEquals($"Canceled {docket.HumanReadableName} cannot be Auto Rated.", supporter.GetReasonNotToAllowAutoRate());
			}
			else
			{
				AssertNull($"{docket.HumanReadableName} should validate with no errors.", supporter.GetReasonNotToAllowAutoRate());
			}
		}

		#endregion

		#region TestCreateAccountingJobOnSavingOfOperationsJob

		public void TestCreateAccountingJobOnSavingOfOperationsJob()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(true, docket.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		#endregion

		#region Implementation

		protected sealed override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var docket = GetNewDocket();
			return docket;
		}

		protected override WhsDocketInvoicingSupporter GetNewSupporter(WhsDocket parent)
		{
			return new WhsDocketInvoicingSupporter(parent);
		}

		protected WhsDocket Docket
		{
			get { return docket ?? (docket = GetNewDocket()); }
			set { docket = value; }
		}

		protected virtual WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsTransfer>(); // WhsTransfer use WhsDocket implementation
		}

		WhsDocket docket;

		#endregion
	}
}
