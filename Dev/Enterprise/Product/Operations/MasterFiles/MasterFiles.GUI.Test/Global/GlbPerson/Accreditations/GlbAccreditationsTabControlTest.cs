using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbAccreditationsTabControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestEditSecurity()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";
			person.PER_MobilePhone = "0455555555";
			person.PER_HomeAddress1 = "Bourke Street3";
			person.PER_EmailAddress = "XXXXXX@wisetech.com";
			Factory.Save();

			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;

			using (var form = new GlbPersonForm(person))
			using (var control = new GlbAccreditationsTabControl())
			{
				form.Controls.Add(control);
				form.Show();

				var attemptsGrid = (ZGrid)control.Controls.Find("attemptsGrid", true).FirstOrDefault();
				AssertNotNull(attemptsGrid);
				var completionDueDateColumn = attemptsGrid.Columns.FirstOrDefault(x => x.ColumnName == GlbAccreditationAttemptSchema.Constants.HAA_CompletionDueDate);
				AssertNotNull(completionDueDateColumn);
				AssertEquals("The column should be editable when security is enabled", false, completionDueDateColumn.ColumnStyle.ReadOnly);

				var attemptDeleteMenu = GetAttemptDeleteMenu(control);
				AssertEquals("The menu item should be visible when security is enabled", true, attemptDeleteMenu.Visible);
			}

			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = false;

			using (var form = new GlbPersonForm(person))
			using (var control = new GlbAccreditationsTabControl())
			{
				form.Controls.Add(control);
				form.Show();

				var attemptsGrid = (ZGrid)control.Controls.Find("attemptsGrid", true).FirstOrDefault();
				AssertNotNull(attemptsGrid);
				var completionDueDateColumn = attemptsGrid.Columns.FirstOrDefault(x => x.ColumnName == GlbAccreditationAttemptSchema.Constants.HAA_CompletionDueDate);
				AssertNotNull(completionDueDateColumn);
				AssertEquals("The column should be read only when security is disabled", true, completionDueDateColumn.ColumnStyle.ReadOnly);

				var attemptDeleteMenu = GetAttemptDeleteMenu(control);
				AssertEquals("The menu item should be visible when security is disabled", true, attemptDeleteMenu.Visible);
			}
		}

		public void TestSetupControls()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = false;
			AssertControls(person, true);

			Env.Security.GlbAccreditationAttemptView.IsAllowed = false;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = false;
			AssertControls(person, true);

			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = false;
			person.ReadOnly = false;
			AssertControls(person, false);

			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = true;
			AssertControls(person, false);

			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = false;
			AssertControls(person, true);
		}

		void AssertControls(GlbPerson person, bool isEditable)
		{
			using (var form = new GlbPersonForm(person))
			using (var control = new GlbAccreditationsTabControl())
			{
				form.Controls.Add(control);
				form.Show();

				var attemptsGrid = (ZGrid)control.Controls.Find("attemptsGrid", true).FirstOrDefault();
				AssertNotNull(attemptsGrid);
				var completionDueDateColumn = attemptsGrid.Columns.FirstOrDefault(x => x.ColumnName == GlbAccreditationAttemptSchema.Constants.HAA_CompletionDueDate);
				AssertNotNull(completionDueDateColumn);

				var attemptDeleteButton = GetAttemptDeleteButton(control);
				var attemptDeleteMenu = GetAttemptDeleteMenu(control);

				if (person.ReadOnly)
				{
					AssertEquals(false, attemptDeleteButton.Enabled);
					AssertEquals(false, attemptDeleteMenu.Enabled);
				}
				else
				{
					AssertEquals(true, attemptDeleteButton.Enabled);
					AssertEquals(true, attemptDeleteMenu.Enabled);

					if (isEditable)
					{
						UnitTestUserNotification.Instance.ClearMessages();
						attemptDeleteButton.PerformClick();
						attemptDeleteMenu.PerformClick();
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage?.Text);
					}
					else
					{
						UnitTestUserNotification.Instance.ClearMessages();
						attemptDeleteButton.PerformClick();
						AssertStartsWith("Permission Denied", "You do not have the appropriate security rights to run this function", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessages();
						attemptDeleteMenu.PerformClick();
						AssertStartsWith("Permission Denied", "You do not have the appropriate security rights to run this function", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		static ToolStripItem GetAttemptDeleteButton(GlbAccreditationsTabControl glbAccreditationsTabControl) => (glbAccreditationsTabControl.Controls.Find("attemptGridToolStrip", true).Single() as ZToolStrip).Items.Find("attemptDeleteButton", true).Single();
		static MenuItem GetAttemptDeleteMenu(GlbAccreditationsTabControl glbAccreditationsTabControl) => (glbAccreditationsTabControl.Controls.Find("attemptsGrid", true).Single() as ZGrid).ContextMenu.MenuItems.FindByText("&Delete");
	}
}
