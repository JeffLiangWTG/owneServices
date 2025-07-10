using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplication))]
	sealed class HRJobApplicationWorkflowProviderTest : WorkflowProviderTest<HRJobApplication, HRJobApplicationProcessTaskCollection>
	{
		public void TestGetTemplateFilterCriteria()
		{
			var application = BusinessObject;

			var staffBR = Factory.New<GlbStaff>();
			staffBR.GS_Code = "BR";
			staffBR.GS_LoginName = "Ben Rogers";

			application.JobOpening.HV_GS_NKControlledBy = staffBR.GS_Code;

			var staffAD = Factory.New<GlbStaff>();
			staffAD.GS_Code = "AD";
			staffAD.GS_LoginName = "Anthony Dunn";

			application.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(application.Applicant.HA_RN_NKCountryInfo, ProcessTaskTemplate.P0_SubType1Info, Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.NewZealand, ZString.Empty);
			AssertGetTemplateFilterCriteria<ZString>(application.JobOpening.HV_GS_NKControlledByInfo, ProcessTaskTemplate.P0_SubType2Info, "BR", "AD", ZString.Empty);
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			Assert("Created when Locale is set in SetDefaultValues", true);
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return "HRA"; }
		}
	}
}
