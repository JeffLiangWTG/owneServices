using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobApplicantModuleForTest))]
	public class HRJobApplicantTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.HRJobApplicant;
		}

		public void TestCheckpoints()
		{
			using (var module = new HRJobApplicantModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.HRJobApplicant, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Recruiter, module.LicenceCheckPoint);
			}
		}

		#region Properties
		public void TestGetNewFilterControl()
		{
			using (var module = new HRJobApplicantModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is HRJobApplicantFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new HRJobApplicantModuleForTest())
			{
				IBusinessObjectCollection applicantsCollection = module.NewGridCollection;
				Assert("Invalid type", applicantsCollection is HRJobApplicantCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new HRJobApplicantModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is HRJobApplicantFilterBusinessObject);
			}
		}
		#endregion
	}
}
