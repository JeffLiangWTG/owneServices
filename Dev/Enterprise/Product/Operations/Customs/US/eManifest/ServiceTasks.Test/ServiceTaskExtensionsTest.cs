using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.eManifest.ServiceTasks.Testing
{
	abstract class ServiceTaskExtensionsTest<T> : ServiceTaskTestCase<T>
		where T : Customs.ServiceTasks.CustomsServiceTask, new()
	{
		public void TestServiceTaskRunsForUSCompanies()
		{
			AssertServiceTaskRunsForCompanies(Constants.CountryCodes.UnitedStates);
		}

		public void TestServiceTaskRunsForOtherCompanies()
		{
			AssertServiceTaskRunsForCompanies(Constants.CountryCodes.Australia);
		}

		void AssertServiceTaskRunsForCompanies(string companyCountryCode)
		{
			var currentCompany = GlbCompany.CurrentCompany;
			using (new TempDirectory())
			{
				currentCompany.GC_RN_NKCountryCode = companyCountryCode;
				var serviceTask = new T();
				var log = InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
				AssertContains("Service task should be run for US company with valid e-Manifest license", ProcessedLog(), log.ToString());
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			foreach (var company in Factory.Load<GlbCompany>(new ZQuery()).Where(c => c.PK != GlbCompany.CurrentCompany.PK))
			{
				company.GC_IsActive = false;
			}

			MessagingTestHelper.SetupPostMasterEmailGroup(Factory);
			PrepareMessagesToBeProcessed();
			Factory.Save();
		}

		protected abstract void PrepareMessagesToBeProcessed();

		protected abstract string ProcessedLog();
	}
}
