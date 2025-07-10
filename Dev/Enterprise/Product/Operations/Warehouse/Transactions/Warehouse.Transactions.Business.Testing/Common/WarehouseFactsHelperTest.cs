using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseFactsHelperTest : TestCaseWithFactory
	{
		public void TestNullObject_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => WarehouseFactsHelper.GetOrganisationFact(null));
		}

		public void TestPK()
		{
			var organisation = Factory.NewWithPrimaryKey<OrgHeader>(ZGuid.BrettsGuid.ToGuid());

			var organisationFact = WarehouseFactsHelper.GetOrganisationFact(organisation);
			AssertEquals(nameof(OrganisationFact.PK), ZGuid.BrettsGuid, organisationFact.PK);
			AssertEquals(nameof(IOrganisationFact.PK), ZGuid.BrettsGuid, ((IOrganisationFact)organisationFact).PK);
		}

		public void TestCode()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "ABC";

			var organisationFact = WarehouseFactsHelper.GetOrganisationFact(organisation);
			AssertEquals(nameof(OrganisationFact.Code), "ABC", organisationFact.Code);
			AssertEquals(nameof(IOrganisationFact.Code), "ABC", ((IOrganisationFact)organisationFact).Code);
		}

		public void TestIsProxyOrgOfCurrentCompany()
		{
			var org = Factory.New<OrgHeader>();
			var organisationFact_Org = WarehouseFactsHelper.GetOrganisationFact(org);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), false, organisationFact_Org.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, ((IOrganisationFact)organisationFact_Org).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, organisationFact_Org.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, ((IOrganisationFact)organisationFact_Org).IsProxyOrgOfCurrentCompany);

			var orgProxy = Factory.New<OrgHeader>();
			var company = GlbCompany.CurrentCompany;
			company.GC_OH_OrgProxy = orgProxy.PK;
			var organisationFact_OrgProxy = WarehouseFactsHelper.GetOrganisationFact(orgProxy);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, organisationFact_OrgProxy.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, ((IOrganisationFact)organisationFact_OrgProxy).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), true, organisationFact_OrgProxy.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, ((IOrganisationFact)organisationFact_OrgProxy).IsProxyOrgOfCurrentCompany);
		}

		public void TestIsProxyOrgOfCurrentCompany_BranchProxy()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var company = GlbCompany.CurrentCompany;
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = orgHeader.PK;

			var organisationFact1 = WarehouseFactsHelper.GetOrganisationFact(orgHeader);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, organisationFact1.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, ((IOrganisationFact)organisationFact1).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), true, organisationFact1.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, ((IOrganisationFact)organisationFact1).IsProxyOrgOfCurrentCompany);
		}

		public void TestIsProxyOrgOfAnyCompany()
		{
			var org = Factory.New<OrgHeader>();
			var organisationFact_Org = WarehouseFactsHelper.GetOrganisationFact(org);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), false, organisationFact_Org.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, ((IOrganisationFact)organisationFact_Org).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, organisationFact_Org.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, ((IOrganisationFact)organisationFact_Org).IsProxyOrgOfCurrentCompany);

			var orgProxy = Factory.New<OrgHeader>();
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = orgProxy.PK;
			var organisationFact_OrgProxy = WarehouseFactsHelper.GetOrganisationFact(orgProxy);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, organisationFact_OrgProxy.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, ((IOrganisationFact)organisationFact_OrgProxy).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, organisationFact_OrgProxy.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, ((IOrganisationFact)organisationFact_OrgProxy).IsProxyOrgOfCurrentCompany);
		}

		public void TestIsProxyOrgOfAnyCompany_BranchProxy()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = orgHeader.PK;

			var organisationFact = WarehouseFactsHelper.GetOrganisationFact(orgHeader);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, organisationFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, ((IOrganisationFact)organisationFact).IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, organisationFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, ((IOrganisationFact)organisationFact).IsProxyOrgOfCurrentCompany);
		}
	}
}
