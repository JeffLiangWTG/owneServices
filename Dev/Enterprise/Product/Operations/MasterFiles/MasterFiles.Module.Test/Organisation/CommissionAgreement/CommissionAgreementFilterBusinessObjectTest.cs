using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CommissionAgreementFilterBusinessObject))]
	sealed class CommissionAgreementFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestIsInQueueFilter()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement1.PK;

			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.IsInCalculationQueue];
			AssertNotNull(filter);
			AssertEquals("Is in Calculation Queue", filter.MultilingualDescription);
			AssertEquals(1, filter.FlagNames.Length);
			AssertEquals("Yes", filter.FlagNames[0]);

			filter.IsActive = true;

			filter.Property0 = false;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			filter.Property0 = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestIsApprovedFilter()
		{
			var approvedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			approvedAgreement.CA0_LastApprovedDateUtc = ZDateTime.UtcNow;

			var unapprovedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			unapprovedAgreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;

			var approvedThenChangedAgreementParent = Factory.NewWithValidTestData<OrgCommissionAgreement>(); //outdated version - to ignore
			approvedThenChangedAgreementParent.CA0_LastApprovedDateUtc = ZDateTime.UtcNow;
			var approvedThenChangedAgreementChild = Factory.NewWithValidTestData<OrgCommissionAgreement>(); //actual version
			approvedThenChangedAgreementChild.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			approvedThenChangedAgreementChild.CA0_CA0_ParentVersion = approvedThenChangedAgreementParent.PK;

			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.IsApproved];
			AssertNotNull(filter);
			AssertEquals("Is Approved", filter.MultilingualDescription);
			AssertEquals(1, filter.FlagNames.Length);
			AssertEquals("Yes", filter.FlagNames[0]);

			filter.IsActive = true;

			filter.Property0 = false;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { unapprovedAgreement, approvedThenChangedAgreementChild }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			filter.Property0 = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { approvedAgreement }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestOpportunityFilter()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement1a = opportunity1.CommissionAgreements.AddNew();
			agreement1a.FillWithValidTestData();
			var agreement1b = opportunity1.CommissionAgreements.AddNew();
			agreement1b.FillWithValidTestData();

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();

			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var opportunityFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.Opportunity];
			AssertNotNull(opportunityFilter);
			AssertEquals("Opportunity", opportunityFilter.MultilingualDescription);
			opportunityFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1a, agreement1b, agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			opportunityFilter.Property = opportunity1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1a, agreement1b }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityFilter.Property = opportunity2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityFilter.Property = opportunity3.PK;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<OrgCommissionAgreement>(), Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestCustomerFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var agreement1a = Factory.New<OrgCommissionAgreement>();
			agreement1a.CA0_OH_Customer = org1.PK;
			agreement1a.FillWithValidTestData();

			var agreement1b = Factory.New<OrgCommissionAgreement>();
			agreement1b.CA0_OH_Customer = org1.PK;
			agreement1b.FillWithValidTestData();

			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.CA0_OH_Customer = org2.PK;
			agreement2.FillWithValidTestData();

			var agreement3 = Factory.New<OrgCommissionAgreement>();
			agreement3.CA0_OH_Customer = org3.PK;
			agreement3.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var customerFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.Customer];
			AssertNotNull(customerFilter);
			AssertEquals("Customer", customerFilter.MultilingualDescription);
			customerFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1a, agreement1b, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			customerFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			customerFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1a, agreement1b }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			customerFilter.Property = org2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			customerFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			customerFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestCommissionStreamFilter()
		{
			var agreement1a = Factory.New<OrgCommissionAgreement>();
			agreement1a.CA0_CommissionStream = "111";
			agreement1a.FillWithValidTestData();

			var agreement1b = Factory.New<OrgCommissionAgreement>();
			agreement1b.CA0_CommissionStream = "111";
			agreement1b.FillWithValidTestData();

			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.CA0_CommissionStream = "222";
			agreement2.FillWithValidTestData();

			var agreement3 = Factory.New<OrgCommissionAgreement>();
			agreement3.CA0_CommissionStream = "333";
			agreement3.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var commissionStreamFilter = (ModuleTextFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.CommissionStream];
			AssertNotNull(commissionStreamFilter);
			AssertEquals("Commission Stream", commissionStreamFilter.MultilingualDescription);
			commissionStreamFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1a, agreement1b, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			commissionStreamFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			commissionStreamFilter.Property = "111";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1a, agreement1b }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			commissionStreamFilter.Property = "222";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			commissionStreamFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			commissionStreamFilter.Property = "111";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestStaffIsRecipientFilter()
		{
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var risStaff = Factory.NewWithValidTestData<GlbStaff>();
			risStaff.GS_Code = "RIS";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement1.Recipients.AddNew(adlStaff);
			agreement1.Recipients.AddNew(risStaff);

			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement2.Recipients.AddNew(risStaff);
			agreement2.Recipients.AddNew(scwStaff);

			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var staffIsRecipientFilter = (ModuleNkFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.StaffIsRecipient];
			AssertNotNull(staffIsRecipientFilter);
			AssertEquals("Staff in Wolf Pack", staffIsRecipientFilter.MultilingualDescription);
			staffIsRecipientFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			staffIsRecipientFilter.Property = adlStaff.GS_Code;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			staffIsRecipientFilter.Property = risStaff.GS_Code;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			staffIsRecipientFilter.Property = scwStaff.GS_Code;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestPartyIsRecipientFilter()
		{
			var party1 = Factory.NewWithValidTestData<OrgHeader>();
			var party2 = Factory.NewWithValidTestData<OrgHeader>();
			var party3 = Factory.NewWithValidTestData<OrgHeader>();
			var proxyCompany = Factory.NewWithValidTestData<GlbCompany>();
			proxyCompany.GC_OH_OrgProxy = party3.PK;
			var proxyBranch = Factory.NewWithValidTestData<GlbBranch>();
			proxyBranch.GB_GC = proxyCompany.PK;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = proxyBranch.PK;

			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement1.Recipients.AddNew(party1);
			agreement1.Recipients.AddNew(party2);

			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement2.Recipients.AddNew(party2);
			agreement2.Recipients.AddNew(party3);

			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var agreement4 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement4.Recipients.AddNew(staff);

			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var partyIsRecipientFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.PartyIsRecipient];
			AssertNotNull(partyIsRecipientFilter);
			AssertEquals("Organization in Wolf Pack", partyIsRecipientFilter.MultilingualDescription);
			partyIsRecipientFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3, agreement4 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			partyIsRecipientFilter.Property = party1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			partyIsRecipientFilter.Property = party2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			partyIsRecipientFilter.Property = party3.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2, agreement4 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestLastApprovedDateFilter()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement1.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2, 1, 1, 1);

			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement2.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2, 12, 12, 12);

			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement3.CA0_LastApprovedDateUtc = new ZDateTime(2002, 3, 3, 1, 1, 1);

			var agreement4 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement4.CA0_LastApprovedDateUtc = ZDateTime.Empty;

			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var lastApprovedDateFilter = (ModuleDateFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.LastApprovedDate];
			AssertNotNull(lastApprovedDateFilter);
			AssertEquals("Last Approved Date", lastApprovedDateFilter.MultilingualDescription);
			lastApprovedDateFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3, agreement4 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			lastApprovedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			lastApprovedDateFilter.Property1 = new ZDateTime(2002, 2, 2, 2, 2, 2);
			lastApprovedDateFilter.Property2 = new ZDateTime(2002, 3, 3, 1, 1, 1);
			AssertContainsExactElementsInAnyOrder("Should include agreement1, but not agreement 3, as it should compare in local time", BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			lastApprovedDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			lastApprovedDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement4 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestBasisFilter()
		{
			var agreementPrv = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementPrv.CA0_CommissionBasis = "PRV";

			var agreementRev = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementRev.CA0_CommissionBasis = "REV";

			var agreementPrv2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementPrv2.CA0_CommissionBasis = "PRV";

			var agreementRev2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementRev2.CA0_CommissionBasis = "REV";
			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var basisFilter = (ModuleTextFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.Basis];
			AssertNotNull(basisFilter);

			basisFilter.IsActive = true;
			AssertEquals("Basis", basisFilter.MultilingualDescription);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementPrv, agreementRev, agreementPrv2, agreementRev2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			basisFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			basisFilter.Property = "PRV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementPrv, agreementPrv2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			basisFilter.Property = "REV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementRev, agreementRev2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			basisFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			basisFilter.Property = "PRV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementRev, agreementRev2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			basisFilter.Property = "REV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementPrv, agreementPrv2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestTriggerTypesFilter()
		{
			var agreementCCD = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementCCD.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;

			var agreementERR = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementERR.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.EarliestRevRecog;

			var agreement1AR = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement1AR.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;

			var agreementMAN = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementMAN.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;

			var agreementMAN2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementMAN2.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			Factory.Save();

			var filterBizObj = new CommissionAgreementFilterBusinessObject();
			var triggerTypesFilter = (ModuleTextFilter)filterBizObj[CommissionAgreementFilterBusinessObject.FilterDescription.TriggerTypes];
			AssertNotNull(triggerTypesFilter);

			triggerTypesFilter.IsActive = true;
			AssertEquals("Effective Trigger", triggerTypesFilter.MultilingualDescription);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementCCD, agreementERR, agreement1AR, agreementMAN, agreementMAN2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			triggerTypesFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			triggerTypesFilter.Property = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementMAN, agreementMAN2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			triggerTypesFilter.Property = CommissionTriggerTypes.Codes.EarliestRevRecog;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementERR }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			triggerTypesFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			triggerTypesFilter.Property = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementCCD, agreementERR, agreement1AR }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			var agreements = Factory.Load<OrgCommissionAgreement>(new ZQuery());
			foreach (var agreement in agreements)
			{
				agreement.Delete();
			}
			Factory.Save();
			base.SetUp();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CommissionAgreementFilterBusinessObject();
		}

		#endregion
	}
}
