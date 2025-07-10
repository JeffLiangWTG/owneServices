using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business
{
	[TestedType(typeof(WhsAdHocServiceJobInvoicingSupporter))]
	class WhsAdHocServiceJobInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestConsumerType()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			IJobInvoicingPlugIn testJob = adHocServiceJob;
			AssertEquals(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob, testJob.InvoicingSupporter.ConsumerType);
		}

		public void TestGetDefaultDebtor()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_OH_Client = org.PK;
			IJobInvoicingPlugIn testJob = adHocServiceJob;

			AssertEquals(null, testJob.InvoicingSupporter.GetDefaultDebtor(null));

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			org.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, "", Core.Constants.TransportModes.All, ZString.Empty, null);

			AssertEquals(relatedParty.PK, testJob.InvoicingSupporter.GetDefaultDebtor(null)?.PK);
		}

		#region TestOperationsBranch_BasedOn_RegistrySetting

		public void TestOperationsBranch_BasedOnRegistrySettings()
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

				var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
				adHocServiceJob.WSJ_OH_Client = client.PK;
				adHocServiceJob.WSJ_WW_Whs = warehouse.PK;

				Factory.Save();

				// Default to blank
				var jobInvPlugIn = (IJobInvoicingPlugIn)adHocServiceJob;
				SetUpRegistry(1, 0, 0, 0);
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

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			return adHocServiceJob;
		}

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
	}
}
