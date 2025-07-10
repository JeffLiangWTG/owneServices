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
	[TestedType(typeof(HRJobOpeningsModule))]
	public class HRJobOpeningsTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.HRJobOpenings;
		}

		public void TestCheckpoints()
		{
			using (var module = new HRJobOpeningsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.HRJobOpenings, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Recruiter, module.LicenceCheckPoint);
			}
		}

		#region Properties
		public void TestGetNewFilterControl()
		{
			using (var module = new HRJobOpeningsModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is HRJobOpeningsFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new HRJobOpeningsModuleForTest())
			{
				IBusinessObjectCollection campaignCollection = module.NewGridCollection;
				Assert("Invalid type", campaignCollection is HRRecruitmentJobCampaignCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new HRJobOpeningsModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is HRJobOpeningsFilterBusinessObject);
			}
		}
		#endregion
	}
}
