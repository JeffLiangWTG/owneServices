using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(JobRequiredDocumentAddInfoModule))]
	sealed class JobRequiredDocumentAddInfoModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobRequiredDocumentAddInfo;
		}

		public void TestJobRequiredDocumentAddInfoModuleAllows()
		{
			using (var module = new JobRequiredDocumentAddInfoModule())
			{
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", false, module.AllowEdit);
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
				AssertEquals("module.HasActions", false, module.HasActions);
			}
		}

		protected override bool HasController()
		{
			return false;
		}

		[RequiresSTA]
		public void TestSearchForJobRequiredDocumentAddInfoModule()
		{
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_Code = "US1";
			var branch2 = uSCompany.Branches.AddNew();
			branch2.GB_Code = "US1";

			var addInfo1 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo1.EX_ReferenceNumber = "REF1";
			addInfo1.EX_GC_Company = GlbCompany.CurrentCompany.PK;
			addInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;

			var addInfo2 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_GC_Company = GlbCompany.CurrentCompany.PK;
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

			var addInfo3 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo3.EX_ReferenceNumber = "REF3";
			addInfo3.EX_GC_Company = uSCompany.PK;
			addInfo3.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			Factory.Save();

			using (var module = new JobRequiredDocumentAddInfoModule())
			{
				var moduleTesting = (IFilterModuleInternalsForTesting)module;
				moduleTesting.PerformSearch();
				AssertEquals("Will show two records in current company", 2, moduleTesting.GridCollection.Count);
			}
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;
	}
}
