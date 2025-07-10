using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class SecurityCheckpointMonitoringFormTest : TestCaseWithFactory
	{
		public void TestSecurityCheckpointShouldUnsubscribeOnCheckpointCheckedWhenFormClosed()
		{
			var autoResetEvent = new AutoResetEvent(false);
			using (var parentForm = new ZForm(null))
			{
				var thread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var monitoringForm = new SecurityCheckpointMonitoringForm())
					{
						monitoringForm.Shown += (o, e) =>
						{
							autoResetEvent.Set();
						};
						monitoringForm.ShowDialog();
					}
					autoResetEvent.Set();
				});
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				autoResetEvent.WaitOne();

				var testForm = Application.OpenForms.OfType<SecurityCheckpointMonitoringForm>().FirstOrDefault();
				testForm.ButtonToggleStartStop.PerformClick();
				testForm.Close();
				autoResetEvent.WaitOne();

				var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				var checkpoint = new SecurityCheckpoint("Code", (NoResString)"DisplayText", null, security.ZSecurityInstance);

				AssertNoExceptionThrown(() => { _ = checkpoint.IsAllowed; });
				AssertEquals("Should stop tracking", false, SecurityCheckpointMonitoringForm.IsTracking);
				thread.Join();
			}
		}
	}
}
