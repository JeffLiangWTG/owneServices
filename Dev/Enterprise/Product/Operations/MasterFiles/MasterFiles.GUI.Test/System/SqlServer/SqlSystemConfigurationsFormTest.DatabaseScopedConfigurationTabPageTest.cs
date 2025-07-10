using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed partial class SqlSystemConfigurationsFormTest
	{
		sealed class DatabaseScopedConfigurationTabPageTest : TestCase, ITabPageContentHolderTest
		{
			[RequiresSTA]
			public void TestLoad()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				const int CfgValue = 10;
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				using (DatabaseScopedConfigurationExtensions.CreateConfigurationProtectionScope<int>(CfgName))
				{
					DatabaseScopedConfigurationExtensions.WriteConfiguration(connection, CfgName, CfgValue.ToString());

					// Act
					var controlDetails = ShowFormAndSwitchTab();

					// Assert
					AssertEquals(CfgValue, controlDetails.MaxDop.CurrentValue);
					AssertEquals(false, controlDetails.MaxDop.IsValueDefault);
					AssertEquals(false, controlDetails.FindConfigByName("LEGACY_CARDINALITY_ESTIMATION").CurrentValue);
				}
			}

			[RequiresSTA]
			public void TestApply()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				const int CfgValue = 100;
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				using (DatabaseScopedConfigurationExtensions.CreateConfigurationProtectionScope<int>(CfgName))
				{
					var controlDetails = ShowFormAndSwitchTab();

					AssertEquals("Value is default to begin with", true, controlDetails.MaxDop.IsValueDefault);
					controlDetails.MaxDop.MakeChangeToProposedValueAsUIDoes(CfgValue.ToString());

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					// Act
					controlDetails.ApplyButton.PerformClick();
					Application.DoEvents();

					// Assert
					AssertEquals("Value is saved", CfgValue, DatabaseScopedConfigurationExtensions.ReadConfiguration(connection, CfgName));
					AssertEquals("Value is reloaded", CfgValue, controlDetails.MaxDop.CurrentValue);
					AssertEquals(false, controlDetails.MaxDop.IsValueDefault);

					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals(true, lastMessage.WasInformation);
					AssertEquals("You have successfully applied proposed database scoped configurations", lastMessage.Text);

					// Cleanup
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			[RequiresSTA]
			public void TestApply_HandledException()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				using (DatabaseScopedConfigurationExtensions.CreateConfigurationProtectionScope<int>(CfgName))
				{
					var controlDetails = ShowFormAndSwitchTab();
					controlDetails.MaxDop.MakeChangeToProposedValueAsUIDoes("-1");

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					// Act
					controlDetails.ApplyButton.PerformClick();
					Application.DoEvents();

					// Assert
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals(true, lastMessage.WasError);
					AssertEquals("Failed to apply one or more proposed configurations to database, please see details in grid row notifications", lastMessage.Text);

					// Cleanup
					ErrorReporter.Clear();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			[RequiresSTA]
			public void TestCancelButtonCanCloseForm()
			{
				// Arrange
				var controlDetails = ShowFormAndSwitchTab();
				AssertEquals(true, form.Visible);

				// Act
				controlDetails.CancelButton.PerformClick();
				Application.DoEvents();

				// Assert
				AssertEquals(false, form.Visible);
			}

			[RequiresSTA]
			public void TestFormIsClosed()
			{
				// Arrange
				var controlDetails = ShowFormAndSwitchTab();
				AssertEquals(true, form.Visible);

				// Act
				form.Close();
				Application.DoEvents();

				// Assert
				AssertEquals(false, form.Visible);
			}

			[RequiresSTA]
			public void TestClosingFormCanLeadToSaveIfUserAgrees()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				const int CfgValue = 100;
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				using (DatabaseScopedConfigurationExtensions.CreateConfigurationProtectionScope<int>(CfgName))
				{
					var controlDetails = ShowFormAndSwitchTab();

					AssertEquals("Value is default to begin with", true, controlDetails.MaxDop.IsValueDefault);
					controlDetails.MaxDop.MakeChangeToProposedValueAsUIDoes(CfgValue.ToString());

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // confirm
					form.Close();
					Application.DoEvents();

					// Assert
					AssertEquals(false, form.Visible);
					AssertEquals("Value is saved", CfgValue, DatabaseScopedConfigurationExtensions.ReadConfiguration(connection, CfgName));
					AssertEquals(
						"You have successfully applied proposed database scoped configurations",
						UnitTestUserNotification.Instance.LastMessage.Text);

					// Cleanup
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			[RequiresSTA]
			public void TestClosingFormDoNotLeadToSaveIfUserDoesNotAgree()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				const int CfgValue = 100;
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				using (DatabaseScopedConfigurationExtensions.CreateConfigurationProtectionScope<int>(CfgName))
				{
					var controlDetails = ShowFormAndSwitchTab();

					AssertEquals("Value is default to begin with", true, controlDetails.MaxDop.IsValueDefault);
					controlDetails.MaxDop.MakeChangeToProposedValueAsUIDoes(CfgValue.ToString());

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.Close();
					Application.DoEvents();

					// Assert
					AssertEquals(false, form.Visible);
					AssertNotEquals("Value is not saved", CfgValue, DatabaseScopedConfigurationExtensions.ReadConfiguration(connection, CfgName));

					// Cleanup
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			[RequiresSTA]
			public void TestClosingFormCanBeCanceled()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				const int CfgValue = 100;
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				using (DatabaseScopedConfigurationExtensions.CreateConfigurationProtectionScope<int>(CfgName))
				{
					var controlDetails = ShowFormAndSwitchTab();

					AssertEquals("Value is default to begin with", true, controlDetails.MaxDop.IsValueDefault);
					controlDetails.MaxDop.MakeChangeToProposedValueAsUIDoes(CfgValue.ToString());

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					form.Close();
					Application.DoEvents();

					// Assert
					AssertEquals(true, form.Visible);
					AssertNotEquals("Value is not saved", CfgValue, DatabaseScopedConfigurationExtensions.ReadConfiguration(connection, CfgName));

					// Cleanup
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			[RequiresSTA]
			public void TestSwitchTab()
			{
				// Arrange
				var controlDetails = ShowFormAndSwitchTab();

				// Act
				form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex = 1;
				Application.DoEvents();

				// Assert
				AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Length);
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[0].WasNone);
			}

			[RequiresSTA]
			public void TestSwitchingTabCanLeadToSaveIfUserAgrees()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				const int CfgValue = 100;
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				using (DatabaseScopedConfigurationExtensions.CreateConfigurationProtectionScope<int>(CfgName))
				{
					var controlDetails = ShowFormAndSwitchTab();
					controlDetails.MaxDop.MakeChangeToProposedValueAsUIDoes(CfgValue.ToString());

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex = 0;
					Application.DoEvents();

					// Assert
					AssertEquals(0, form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex);
					AssertEquals("Value is saved", CfgValue, DatabaseScopedConfigurationExtensions.ReadConfiguration(connection, CfgName));
					AssertEquals(
						"You have successfully applied proposed database scoped configurations",
						UnitTestUserNotification.Instance.LastMessage.Text);

					// Cleanup
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			[RequiresSTA]
			public void TestSwitchingTabDoNotSaveIfUserDoesNotAgree()
			{
				// Arrange
				const string CfgName = "MAXDOP";
				const int CfgValue = 100;
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				using (DatabaseScopedConfigurationExtensions.CreateConfigurationProtectionScope<int>(CfgName))
				{
					var controlDetails = ShowFormAndSwitchTab();
					controlDetails.MaxDop.MakeChangeToProposedValueAsUIDoes(CfgValue.ToString());

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex = 0;
					Application.DoEvents();

					// Assert
					AssertEquals(0, form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex);
					AssertNotEquals(CfgValue, DatabaseScopedConfigurationExtensions.ReadConfiguration(connection, CfgName));
					AssertNotEquals(
						"You have successfully applied proposed database scoped configurations",
						UnitTestUserNotification.Instance.LastMessage.Text);

					// Cleanup
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			ControlDetail ShowFormAndSwitchTab()
			{
				form.Show();
				form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex = 1;
				Application.DoEvents();

				return new ControlDetail(form.FindSingleOrDefault<DatabaseScopedConfigurationUserControl>("databaseScopedConfigurationUserControl"));
			}

			SqlSystemConfigurationsForm form;
			protected override void SetUp()
			{
				base.SetUp();
				form = new SqlSystemConfigurationsForm();
			}

			protected override void TearDown()
			{
				form.Dispose();
				base.TearDown();
			}

			readonly struct ControlDetail
			{
				public DatabaseScopedConfigurationUserControl Control { get; }
				public ZButton ApplyButton => Control.FindSingleOrDefault<ZButton>("applyButton");
				public ZButton CancelButton => Control.FindSingleOrDefault<ZButton>("cancelButton");

				public DatabaseScopedConfigurationViewModel VM => (DatabaseScopedConfigurationViewModel)Control.BindingSource.DataSource;
				public DatabaseScopedConfiguration MaxDop => FindConfigByName("MAXDOP");

				public ControlDetail(DatabaseScopedConfigurationUserControl control)
				{
					AssertNotEquals(null, control);
					Control = control;
				}

				public DatabaseScopedConfiguration FindConfigByName(string name)
				{
					return VM.SelectedDatabaseContainer.Configurations.Find(x => x.Name == name).Single();
				}
			}
		}
	}
}
