using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed partial class SqlSystemConfigurationsFormTest
	{
		sealed class SqlSystemConfigurationsTabPageTest : TestCase, ITabPageContentHolderTest
		{
			public void TestFormIsClosed()
			{
				// Arrange
				using (var form = sqlSystemConfigurationsForm.Object)
				{
					sqlSystemConfigurationsUserControl
						.Protected()
						.Setup("SaveSystemConfigurations")
						.Verifiable();

					form.Show();

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.Close();
					Application.DoEvents();

					// Assert
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("SaveSystemConfigurations", Times.Never());
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("ApplyProposedConfigurations", Times.Never());
					AssertEquals("Form is closed", false, form.Visible);
				}
			}

			[RequiresSTA]
			public void TestClosingFormCanLeadToSaveIfUserAgrees()
			{
				// Arrange
				using (var form = sqlSystemConfigurationsForm.Object)
				{
					sqlSystemConfigurationsUserControl
						.Protected()
						.Setup("SaveSystemConfigurations")
						.Verifiable();

					form.Show();
					MakeChangeToConfig();

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.Close();
					Application.DoEvents();

					// Assert
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("SaveSystemConfigurations", Times.Once());
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("ApplyProposedConfigurations", Times.Never());
					AssertEquals("Form is closed", false, form.Visible);
				}
			}

			[RequiresSTA]
			public void TestClosingFormDoNotLeadToSaveIfUserDoesNotAgree()
			{
				// Arrange
				using (var form = sqlSystemConfigurationsForm.Object)
				{
					sqlSystemConfigurationsUserControl
						.Protected()
						.Setup("SaveSystemConfigurations")
						.Verifiable();

					form.Show();
					MakeChangeToConfig();

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.Close();
					Application.DoEvents();

					// Assert
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("SaveSystemConfigurations", Times.Never());
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("ApplyProposedConfigurations", Times.Never());
					AssertEquals("Form is closed", false, form.Visible);
				}
			}

			[RequiresSTA]
			public void TestClosingFormCanBeCanceled()
			{
				// Arrange
				using (var form = sqlSystemConfigurationsForm.Object)
				{
					form.Show();
					MakeChangeToConfig();

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					form.Close();
					Application.DoEvents();

					// Assert
					AssertEquals("Form is still visible rather than being closed", true, form.Visible);
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("SaveSystemConfigurations", Times.Never());
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("ApplyProposedConfigurations", Times.Never());
				}
			}

			[ExpectNoExceptions]
			[RequiresSTA]
			public void TestSwitchTab()
			{
				// Arrange
				using (var form = sqlSystemConfigurationsForm.Object)
				{
					sqlSystemConfigurationsUserControl
						.Protected()
						.Setup("SaveSystemConfigurations")
						.Verifiable();

					form.Show();

					// Act
					form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex = 1;
					Application.DoEvents();

					// Assert
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("SaveSystemConfigurations", Times.Never());
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("ApplyProposedConfigurations", Times.Never());
				}
			}

			[RequiresSTA]
			public void TestSwitchingTabCanLeadToSaveIfUserAgrees()
			{
				// Arrange
				using (var form = sqlSystemConfigurationsForm.Object)
				{
					sqlSystemConfigurationsUserControl
						.Protected()
						.Setup("SaveSystemConfigurations")
						.Verifiable();

					form.Show();
					MakeChangeToConfig();

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex = 1;
					Application.DoEvents();

					// Assert
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("SaveSystemConfigurations", Times.Once());
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("ApplyProposedConfigurations", Times.Never());
				}
			}

			[RequiresSTA]
			public void TestSwitchingTabDoNotSaveIfUserDoesNotAgree()
			{
				// Arrange
				using (var form = sqlSystemConfigurationsForm.Object)
				{
					sqlSystemConfigurationsUserControl
						.Protected()
						.Setup("SaveSystemConfigurations")
						.Verifiable();

					form.Show();
					MakeChangeToConfig();

					// Act
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.FindSingleOrDefault<ZTabControl>("tabControl").SelectedIndex = 1;
					Application.DoEvents();

					// Assert
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("SaveSystemConfigurations", Times.Never());
					sqlSystemConfigurationsUserControl
						.Protected()
						.Verify("ApplyProposedConfigurations", Times.Never());
				}
			}

			void MakeChangeToConfig()
			{
				config.HasChanges = true; // mimic user input triggers grid to inform BO of being changed
				config.ProposedValueText = "6000";
				AssertEquals(true, ZControlExtensions.FindSingleOrDefault<ZButton>(sqlSystemConfigurationsUserControl.Object, "saveButton").Enabled);
			}

			Mock<SqlSystemConfigurationsUserControl> sqlSystemConfigurationsUserControl;
			SqlSystemConfiguration config;
			Mock<SqlSystemConfigurationsForm> sqlSystemConfigurationsForm;
			protected override void SetUp()
			{
				base.SetUp();

				config = new SqlSystemConfiguration(106)
				{
					Name = "locks",
					MinValue = 5000,
					MaxValue = 2147483647,
					ConfiguredValue = 5100,
					IsDynamic = false
				};
				sqlSystemConfigurationsUserControl = new Mock<SqlSystemConfigurationsUserControl>() { CallBase = true };
				sqlSystemConfigurationsUserControl
					.Protected()
					.Setup<SqlSystemConfigurationsCollection>("GetSqlServerConfigurations", ItExpr.IsAny<DbConnection>())
					.Returns(new SqlSystemConfigurationsCollection { config });

				sqlSystemConfigurationsUserControl
					.Protected()
					.Setup("ApplyProposedConfigurations")
					.Verifiable();
				sqlSystemConfigurationsUserControl
					.Protected()
					.Setup("SaveSystemConfigurations")
					.Verifiable();

				sqlSystemConfigurationsForm = new Mock<SqlSystemConfigurationsForm>() { CallBase = true };
				sqlSystemConfigurationsForm.Protected()
					.Setup<SqlSystemConfigurationsUserControl>("CreateSqlSystemConfigurationsUserControl")
					.Returns(sqlSystemConfigurationsUserControl.Object);
			}
		}
	}
}
