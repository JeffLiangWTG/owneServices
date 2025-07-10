using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbBranchFindingTest : TestCaseWithFactory
	{
		#region Implementation

		RefUNLOCO Kismayu
		{
			get
			{
				if (fKismayu == null)
				{
					fKismayu = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SOKMU");
					AssertNotNull("Precondition:", fKismayu);
				}
				return fKismayu;
			}
		}
		RefUNLOCO fKismayu;

		RefUNLOCO Brisbane
		{
			get
			{
				if (fBrisbane == null)
				{
					fBrisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
					AssertNotNull("Precondition:", fBrisbane);
				}
				return fBrisbane;
			}
		}
		RefUNLOCO fBrisbane;

		RefUNLOCO Singapore
		{
			get
			{
				if (fSingapore == null)
				{
					fSingapore = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
					AssertNotNull("Precondition:", fSingapore);
				}
				return fSingapore;
			}
		}
		RefUNLOCO fSingapore;

		#endregion

		public void TestPreconditionsForSucsessfullTesting()
		{
			AssertNull("Precondition: no branches should be found in Kismayu for this test", Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, Kismayu.RL_Code)));

			ZDBOnlyQuery branchesQuery = new ZDBOnlyQuery(typeof(GlbBranch));
			branchesQuery.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);

			ZDBOnlySubQuery extraPortsSubQuery = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_GB);
			extraPortsSubQuery.AddToFilter(GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, Kismayu.RL_Code);
			branchesQuery.AddSubQuery(extraPortsSubQuery, JoinCondition.And);

			AssertNull("Precondition: no branches should be found by Kismayu as related port", Factory.LoadTop1<GlbBranch>(branchesQuery));

			ZQuery ccBranchQuery = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, Brisbane.RL_Code);
			ccBranchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);
			ccBranchQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			AssertNotNull("Precondition: current company's branch should be found in Brisbane for this test", Factory.LoadTop1<GlbBranch>(ccBranchQuery));
		}

		#region TestFindControllingBranchWithFallBackToAnyCompany

		public void TestFindControllingBranchWithFallBackToAnyCompany()
		{
			AssertNull("Method handles NULLs", GlbBranch.FindControllingBranchWithFallBackToAnyCompany(null));
			AssertNull("Method handles NULLs", GlbBranch.FindControllingBranchWithFallBackToAnyCompanyIfOnlyOne(null));

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			CreateOrgCompanyData(org, false);
			Factory.Save();

			AssertNull("No branches has been set", GlbBranch.FindControllingBranchWithFallBackToAnyCompany(org));
			AssertNull("No branches has been set", GlbBranch.FindControllingBranchWithFallBackToAnyCompanyIfOnlyOne(org));

			OrgCompanyData orgCompanyData1 = CreateOrgCompanyData(org, true);
			OrgCompanyData orgCompanyData2 = CreateOrgCompanyData(org, true);
			OrgCompanyData orgCompanyData3 = CreateOrgCompanyData(org, true);
			Factory.Save();

			AssertNotNull("Falling back to a random company", GlbBranch.FindControllingBranchWithFallBackToAnyCompany(org));
			AssertNull("Return null if find more than one Controlling Branch", GlbBranch.FindControllingBranchWithFallBackToAnyCompanyIfOnlyOne(org));

			org.OH_RL_NKClosestPort = "UAAAR";
			orgCompanyData2.ControllingBranch.GB_RL_NKHomePort = "UAAAR";
			Factory.Save();
			AssertEquals("Falling back to the company that matches because of a home port", orgCompanyData2.ControllingBranch, GlbBranch.FindControllingBranchWithFallBackToAnyCompany(org));

			org.OH_RL_NKClosestPort = "NZAKL";
			GlbBranchExtraPorts extraPort = orgCompanyData2.ControllingBranch.ExtraPorts.AddNew(typeof(GlbBranchExtraPorts));
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = "NZAKL";
			Factory.Save();
			AssertEquals("Falling back to the company that matches because of a related port", orgCompanyData2.ControllingBranch, GlbBranch.FindControllingBranchWithFallBackToAnyCompany(org));

			org.OH_RL_NKClosestPort = "UAAAR";
			orgCompanyData2.ControllingBranch.GB_RL_NKHomePort = "UAIEV";
			Factory.Save();
			AssertEquals("Falling back to the company from the same country", orgCompanyData2.ControllingBranch, GlbBranch.FindControllingBranchWithFallBackToAnyCompany(org));
		}

		OrgCompanyData CreateOrgCompanyData(OrgHeader org, bool setControllingBranch)
		{
			OrgCompanyData orgCompanyData = org.CompanyDataCollection.AddNew();

			GlbCompany glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_IsActive = true;

			GlbBranch glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_IsActive = true;

			orgCompanyData.OB_GC = glbCompany.PK;

			if (setControllingBranch)
			{
				orgCompanyData.OB_GB_ControllingBranch = glbBranch.PK;
			}

			return orgCompanyData;
		}

		#endregion

		public void TestFindBranchByHomePort()
		{
			AssertNull("Method handles NULL", GlbBranch.FindByHomePort(Factory, null));

			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch kismayuBranch = company1.Branches.AddNew();
			kismayuBranch.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			AssertNull("No branch should be found", GlbBranch.FindByHomePort(Factory, Kismayu));

			kismayuBranch.GB_RL_NKHomePort = Kismayu.RL_Code;
			AssertEquals("Kismayu branch should be found", kismayuBranch, GlbBranch.FindByHomePort(Factory, Kismayu));
		}

		public void TestFindBranchByRelatedPort()
		{
			AssertNull("Method handles NULL", GlbBranch.FindByRelatedPort(Factory, null));

			GlbBranch kismayuBranch = Factory.New<GlbCompany>().Branches.AddNew();
			kismayuBranch.GB_RL_NKHomePort = Kismayu.RL_Code;

			AssertNull("Shouldn't return any branch", GlbBranch.FindByRelatedPort(Factory, Kismayu));

			GlbBranchExtraPorts port1 = Factory.New<GlbBranchExtraPorts>();
			port1.GY_GB = kismayuBranch.PK;
			port1.GY_RL_NKAdditionalBranchRelatedPort = Kismayu.RL_Code;

			GlbBranchExtraPorts port2 = Factory.New<GlbBranchExtraPorts>();
			port2.GY_GB = kismayuBranch.PK;
			port2.GY_RL_NKAdditionalBranchRelatedPort = Singapore.RL_Code;

			GlbBranchExtraPorts port3 = Factory.New<GlbBranchExtraPorts>();
			port3.GY_GB = GlbBranch.CurrentBranch.PK;
			port3.GY_RL_NKAdditionalBranchRelatedPort = Singapore.RL_Code;

			Factory.Save();

			AssertEquals("Should return Kismayu branch", kismayuBranch, GlbBranch.FindByRelatedPort(Factory, Kismayu));
			AssertEquals("Current company's branch should be preferred", GlbCompany.CurrentCompany.PK, GlbBranch.FindByRelatedPort(Factory, Singapore).GB_GC);
		}

		public void TestFindInSameCountry()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();

			GlbBranch kismayuBranch = company1.Branches.AddNew();
			kismayuBranch.GB_RL_NKHomePort = Kismayu.RL_Code;

			GlbBranch singaporeBranch = company1.Branches.AddNew();
			singaporeBranch.GB_RL_NKHomePort = Singapore.RL_Code;

			AssertEquals("Should return Kismayu branch", kismayuBranch, GlbBranch.FindAnyBranchInSameCountry(Factory, kismayuBranch.Country));
		}

		public void TestFindBranchByHomePortWithFallBackToRelatedPort()
		{
			AssertNull("Method handles NULL", GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, null));
			AssertNull("Nothig can be found by both ports", GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, Kismayu));

			GlbCompany company1 = Factory.New<GlbCompany>();
			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_RL_NKHomePort = "AUSYD";

			GlbBranchExtraPorts port1 = Factory.New<GlbBranchExtraPorts>();
			port1.GY_GB = branch1.PK;
			port1.GY_RL_NKAdditionalBranchRelatedPort = Kismayu.RL_Code;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CO2";
			GlbBranch branch3 = company2.Branches.AddNew();
			branch3.GB_Code = "GB3";
			branch3.GB_RL_NKHomePort = "AUSYD";

			GlbBranchExtraPorts port2 = Factory.New<GlbBranchExtraPorts>();
			port2.GY_GB = branch3.PK;
			port2.GY_RL_NKAdditionalBranchRelatedPort = Singapore.RL_Code;
			Factory.Save();

			AssertEquals("Nothig can be found by home port so it searches by related port", branch1, GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, Kismayu));

			GlbBranch branch2 = company1.Branches.AddNew();
			branch2.GB_RL_NKHomePort = Kismayu.RL_Code;

			AssertEquals("It searches by home port first", branch2, GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, Kismayu));

			AssertEquals("Nothig can be found by home port so it searches by related port", branch3, GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, Singapore, company2));

			GlbBranch branch4 = company2.Branches.AddNew();
			branch4.GB_RL_NKHomePort = Singapore.RL_Code;

			AssertEquals("It searches by home port first", branch4, GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, Singapore, company2));
		}
	}
}
