using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Rating;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RatingDocRollupOrSortLoader))]
	class OrgRatingDocRollupOrGroupLoaderTest : LoaderTestCase
	{
		public void TestGetGroupOrSubTotalWithFallbacksToRegistry()
		{
			AssertDisplay("Registry Default", DocRollupOrSortJobTypeList.Codes.Forwarding, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);

			// ENTERPRISE LEVEL REGISTRY SETTINGS
			SetupEnterpriseRegistry(DocRollupOrSortJobTypeList.Codes.All, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);
			AssertDisplay("Enterprise Registry ALL Setting", DocRollupOrSortJobTypeList.Codes.All, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);

			SetupEnterpriseRegistry(DocRollupOrSortJobTypeList.Codes.Customs, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);
			AssertDisplay("Enterprise Registry SAB Setting", DocRollupOrSortJobTypeList.Codes.Customs, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);

			SetupEnterpriseRegistry(DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertDisplay("Enterprise Registry SHP Setting", DocRollupOrSortJobTypeList.Codes.Forwarding, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			// COMPANY LEVEL REGISTRY SETTINGS
			SetupCompanyRegistry(DocRollupOrSortJobTypeList.Codes.All, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			AssertDisplay("Company Registry ALL Setting", DocRollupOrSortJobTypeList.Codes.All, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);

			SetupCompanyRegistry(DocRollupOrSortJobTypeList.Codes.Customs, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);
			AssertDisplay("Company Registry SAB Setting", DocRollupOrSortJobTypeList.Codes.Customs, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);

			SetupCompanyRegistry(DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertDisplay("Company Registry SHP Setting", DocRollupOrSortJobTypeList.Codes.Forwarding, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			// BRANCH LEVEL REGISTRY SETTINGS
			SetupBranchRegistry(DocRollupOrSortJobTypeList.Codes.All, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);
			AssertDisplay("Company Registry ALL Setting", DocRollupOrSortJobTypeList.Codes.All, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);

			SetupBranchRegistry(DocRollupOrSortJobTypeList.Codes.Customs, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertDisplay("Branch Registry SAB Setting", DocRollupOrSortJobTypeList.Codes.Customs, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			SetupBranchRegistry(DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			AssertDisplay("Branch Registry SHP Setting", DocRollupOrSortJobTypeList.Codes.Forwarding, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);

			// DEPARTMENT LEVEL REGISTRY SETTINGS
			SetupDepartmentRegistry(DocRollupOrSortJobTypeList.Codes.All, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);
			AssertDisplay("Department Registry ALL Setting", DocRollupOrSortJobTypeList.Codes.All, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);

			SetupDepartmentRegistry(DocRollupOrSortJobTypeList.Codes.Customs, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			AssertDisplay("Department Registry SAB Setting", DocRollupOrSortJobTypeList.Codes.Customs, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);

			SetupDepartmentRegistry(DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocRollupOrGroupRegistry.Schema.Display, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertDisplay("Department Registry SHP Setting", DocRollupOrSortJobTypeList.Codes.Forwarding, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			// ORGANISATION SETTINGS
			SetupOrganisation(DocRollupOrSortJobTypeList.Codes.All, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpCharges);
			AssertDisplay("Organisation ALL Default Setting", DocRollupOrSortJobTypeList.Codes.All, DocRollupOrSortDisplayList.Codes.RollUpCharges);

			SetupOrganisation(DocRollupOrSortJobTypeList.Codes.All, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence);
			AssertDisplay("Organisation ALL Setting", DocRollupOrSortJobTypeList.Codes.All, DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence);

			SetupOrganisation(DocRollupOrSortJobTypeList.Codes.Customs, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpCharges);
			AssertDisplay("Organisation SAB Default Setting", DocRollupOrSortJobTypeList.Codes.Customs, DocRollupOrSortDisplayList.Codes.RollUpCharges);

			SetupOrganisation(DocRollupOrSortJobTypeList.Codes.Customs, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence);
			AssertDisplay("Organisation SAB Setting", DocRollupOrSortJobTypeList.Codes.Customs, DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence);

			SetupOrganisation(DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpCharges);
			AssertDisplay("Organisation SHP Default Setting", DocRollupOrSortJobTypeList.Codes.Forwarding, DocRollupOrSortDisplayList.Codes.RollUpCharges);

			SetupOrganisation(DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence);
			AssertDisplay("Organisation SHP Setting", DocRollupOrSortJobTypeList.Codes.Forwarding, DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence);
		}

		#region Implementation

		void SetupOrganisation(string jobType, string propertyToSet, string value)
		{
			RatingDocumentsChargeGroupingOrRollup orgRatingDocRollupOrGroup = null;

			foreach (RatingDocumentsChargeGroupingOrRollup orgSetting in OrgRatingDocRollupOrGroupCollection)
			{
				if (orgSetting.RCG_JobType == jobType)
				{
					orgRatingDocRollupOrGroup = orgSetting;
					break;
				}
			}

			if (orgRatingDocRollupOrGroup == null)
			{
				orgRatingDocRollupOrGroup = OrgRatingDocRollupOrGroupCollection.AddNew();
			}

			orgRatingDocRollupOrGroup.RCG_Module = DocRollupOrSortModuleList.Codes.All;
			orgRatingDocRollupOrGroup.RCG_JobType = jobType;
			orgRatingDocRollupOrGroup.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			orgRatingDocRollupOrGroup.RCG_Display = DocRollupOrSortDisplayList.Codes.Default;
			orgRatingDocRollupOrGroup.RCG_Style = DocRollupOrSortStyleList.Codes.Default;

			orgRatingDocRollupOrGroup[propertyToSet] = value;
			orgRatingDocRollupOrGroup.Factory.Save();
		}

		void SetupEnterpriseRegistry(string jobType, string propertyToSet, string value)
			=> SetupRegistry(Guid.Empty, Guid.Empty, Guid.Empty, jobType, propertyToSet, value);

		void SetupCompanyRegistry(string jobType, string propertyToSet, string value)
			=> SetupRegistry(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobType, propertyToSet, value);

		void SetupBranchRegistry(string jobType, string propertyToSet, string value)
			=> SetupRegistry(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, jobType, propertyToSet, value);

		void SetupDepartmentRegistry(string jobType, string propertyToSet, string value)
			=> SetupRegistry(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), jobType, propertyToSet, value);

		void SetupRegistry(Guid companyPK, Guid branchPK, Guid departmentPK, string jobType, string propertyToSet, string value)
		{
			var registryValue = OrganisationRegistry.Instance.RatingDocRollupOrGroup.GetValueWithoutFallback(companyPK, branchPK, departmentPK);
			RatingDocRollupOrGroupRegistry invoiceOrGroupSetting = null;

			foreach (RatingDocRollupOrGroupRegistry registryRow in registryValue)
			{
				if (registryRow.JobType == jobType)
				{
					invoiceOrGroupSetting = registryRow;
					break;
				}
			}

			if (invoiceOrGroupSetting == null)
			{
				invoiceOrGroupSetting = registryValue.AddNew();
				invoiceOrGroupSetting.Module = DocRollupOrSortModuleList.Codes.All;
				invoiceOrGroupSetting.JobType = jobType;
				invoiceOrGroupSetting.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
				invoiceOrGroupSetting.Display = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
				invoiceOrGroupSetting.Style = OrgConstants.InvoiceLineGroupings.Code.None;
			}

			invoiceOrGroupSetting[propertyToSet] = value;

			OrganisationRegistry.Instance.RatingDocRollupOrGroup.SetValue(companyPK, branchPK, departmentPK, registryValue);
		}

		void AssertDisplay(string message, string jobType, string expected)
		{
			var actual = new RatingDocRollupOrSortLoader
			(
				Factory,
				OrgRatingDocRollupOrGroupCollection.Master.Header,
				GlbBranch.CurrentBranch,
				GlbDepartment.CurrentDepartment
			)
			.GetDisplay
			(
				OrgConstants.ServiceDirection.Code.Import,
				OrgConstants.ModesForGroupOrSubTotal.Codes.Air,
				OrgConstants.ModesForGroupOrSubTotal.Codes.Air,
				jobType
			);

			AssertEquals(message, expected, actual);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			OrgRatingDocRollupOrGroupCollection = org.CompanyData.RatingDocRollupOrGroups;
			OrgRatingDocRollupOrGroupCollection.RemoveAndDeleteAll();
			Factory.Save();
			OrgRatingDocRollupOrGroupCollection.RemoveAndDeleteAll();
		}

		RatingDocumentsChargeGroupingOrRollupCollection OrgRatingDocRollupOrGroupCollection;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			var org = Factory.New<OrgHeader>();
			return new RatingDocRollupOrSortLoader(Factory, org, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
		}

		#endregion
	}
}
