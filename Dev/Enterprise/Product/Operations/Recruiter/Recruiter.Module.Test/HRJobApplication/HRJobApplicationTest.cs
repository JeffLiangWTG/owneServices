using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobApplicationModuleForTest))]
	public class HRJobApplicationTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.HRJobApplication;
		}

		public void TestCheckpoints()
		{
			using (var module = new HRJobApplicationModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.HRJobApplication, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Recruiter, module.LicenceCheckPoint);
			}
		}

		public void TestHasOperationalActionsPlugin()
		{
			using (var module = new HRJobApplicationModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestHasOperationalActions()
		{
			using (var module = new HRJobApplicationModule())
			{
				var supportable = module as IOperationalActionSupportable;
				AssertNotNull(supportable);
				AssertNotNull(supportable.OperationalActionSupporter);
			}
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = applicant.Applications.AddNew();

			AssertEquals("Should be 1 business objects with related notes", 1, application.BusinessObjectsWithRelatedNotes.Length);

			applicant.Applications.AddNew();
			AssertEquals("Should be 2 business object with related notes", 2, application.BusinessObjectsWithRelatedNotes.Length);

			applicant.Applications.AddNew();
			AssertEquals("Should be 3 business objects with related notes", 3, application.BusinessObjectsWithRelatedNotes.Length);
		}

		#region Properties
		public void TestGetNewFilterControl()
		{
			using (var module = new HRJobApplicationModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is HRJobApplicationFilterControl);
				filterControl.Dispose();
			}
		}
		#endregion
	}
}
