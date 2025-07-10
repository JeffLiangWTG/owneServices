using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class ManifestUserControlTestCase : TestCaseWithFactory
	{
		internal static Control FindControl(Control control, string name)
		{
			var tabControl = control as ZTabControl;
			var controls = tabControl != null ? tabControl.AllTabPages : control.Controls.Cast<Control>();
			return (
				from childControl in controls
				let result = childControl.Name == name ? childControl : FindControl(childControl, name)
				where result != null
				select result).FirstOrDefault();
		}

		public void TestNoConveyanceEquipmentCreate_WhenValidateAllofConveyance()
		{
			var trip = Factory.New<Trip>();
			using (var form = new ZForm(trip))
			{
				using (var control = new ManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					form.FireSaveButton();
					AssertEquals("No conveyance equipment is craeated", null, trip.AllEquipmentIncludingMainConveyance.FirstOrDefault(e => e.BJ_IsConveyance && !e.IsDeleted));
				}
			}
		}

		public void TestNoValidationIsCalled_WhenSavedSuccessfully()
		{
			var trip = Factory.New<Trip>();
			using (var form = new ZForm(trip))
			{
				using (var control = new ManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					trip.AddRowError("Message-Test");
					form.FireSaveButton();
					AssertEquals("When Saving the form with row warnings, it would be failed", trip.RowNotifications.Count(), 1);
					Assert(!trip.IsInDatabase);
					trip.ClearRowNotifications();
					form.FireSaveButton();
					AssertEquals(trip.RowNotifications.Count(), 0);
					Assert(trip.IsInDatabase);
				}
			}
		}
	}
}
