using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbBranchExtraPortsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPortsUNLOCOandCountrySame()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			Branch.GB_GC = company.PK;

			company.GC_RN_NKCountryCode = "NZ";
			GlbBranchExtraPorts port1 = Branch.ExtraPorts.AddNew();
			port1.GY_RL_NKAdditionalBranchRelatedPort = "AUAUB";
			port1.Validation.ValidateGY_RL_NKAdditionalBranchRelatedPort();
			string wrnMsg = string.Format("The Home Port entered belongs to a different country/region to the country/region code entered on the company specified. Please ensure this is correct. If the home port is correct It is strongly recommend that you create a company for {0} as accounting information will be entered against this company", port1.AdditionalBranchRelatedPort.Country.Code);
			AssertHasWarning(port1.GY_RL_NKAdditionalBranchRelatedPortInfo, wrnMsg);

			port1.GY_RL_NKAdditionalBranchRelatedPort = "AUAUB";
			company.GC_RN_NKCountryCode = "AU";
			port1.Validation.ValidateGY_RL_NKAdditionalBranchRelatedPort();
			AssertNoWarnings(port1.GY_RL_NKAdditionalBranchRelatedPortInfo);

			company.GC_RN_NKCountryCode = "";
			port1.GY_RL_NKAdditionalBranchRelatedPort = "AUAUB";
			AssertNoWarnings(port1.GY_RL_NKAdditionalBranchRelatedPortInfo);

			company.GC_RN_NKCountryCode = "";
			port1.GY_RL_NKAdditionalBranchRelatedPort = "";
			AssertNoWarnings(port1.GY_RL_NKAdditionalBranchRelatedPortInfo);
		}

		public void TestEnteredPortsUnique()
		{
			GlbBranchExtraPorts port1 = Branch.ExtraPorts.AddNew();
			GlbBranchExtraPorts port2 = Branch.ExtraPorts.AddNew();
			port1.GY_RL_NKAdditionalBranchRelatedPort = "AUPER";
			port2.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";

			AssertNoErrors(port1.GY_RL_NKAdditionalBranchRelatedPortInfo);
			AssertNoErrors(port2.GY_RL_NKAdditionalBranchRelatedPortInfo);

			port2.GY_RL_NKAdditionalBranchRelatedPort = "AUPER";
			AssertHasErrors("Duplicate UNLOCOs should not be allowed", port2.GY_RL_NKAdditionalBranchRelatedPortInfo);
		}

		public void TestRelatedPortsNotSameAsHomePort()
		{
			GlbBranchExtraPorts port1 = Factory.New<GlbBranchExtraPorts>();
			Branch.ExtraPorts.Add(port1);

			port1.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";
			AssertNoErrors("Related ports can be different to the home port", port1.GY_RL_NKAdditionalBranchRelatedPortInfo);

			port1.GY_RL_NKAdditionalBranchRelatedPort = "AUSYD";
			AssertHasErrors("Related ports should not be the same as the home port", port1.GY_RL_NKAdditionalBranchRelatedPortInfo);
		}

		#region Implementation

		GlbBranch Branch;

		protected override void SetUp()
		{
			base.SetUp();

			Branch = Factory.New<GlbBranch>();
			Branch.GB_RL_NKHomePort = "AUSYD";
		}

		#endregion
	}
}
