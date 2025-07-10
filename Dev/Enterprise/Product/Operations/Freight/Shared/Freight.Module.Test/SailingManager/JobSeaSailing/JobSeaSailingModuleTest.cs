using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI.OnlineSailingSchedules;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobSeaSailingModule))]
	sealed class JobSeaSailingModuleTest : ZModuleBasherTest
	{
		public void TestIdentifierForPersistingForm()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage.GenerateSailings();

			Factory.Save();

			var sailing = voyage.Sailings[0];

			using (var module = new JobSeaSailingModule())
			{
				using (var form = ((IFilterModuleInternalsForTesting)module).ShowEditForm(sailing))
				{
					AssertEquals("Must be PK of the TypeOfTopLevelBusinessObject (JobVoyage)", voyage.PK, form.IdentifierForPersistingForm);
				}
			}
		}

		public void TestImportOnlineSchedules_Availability_DependsFrom_EnableScheduleFeedServiceRegistry()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new JobSeaSailingModule())
			{
				AssertNull(module.FormActionMenu.FindByText("Import Global Schedules", true));
			}

			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new JobSeaSailingModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Import Global Schedules", true));
			}
		}

		public void TestImportOnlineSchedules_ShowsOnlineSailingSchedulesForm()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new JobSeaSailingModule())
			{
				var importMenuItem = module.FormActionMenu.FindByText("Import Global Schedules", true);
				importMenuItem.PerformClick();

				var onlineSchedulesForm = Application.OpenForms.OfType<OnlineSchedulesForm>().SingleOrDefault();

				AssertNotNull("Global Schedules have been shown", onlineSchedulesForm);
				onlineSchedulesForm.Dispose();
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobSeaSailing;
		}

		#endregion
	}
}
