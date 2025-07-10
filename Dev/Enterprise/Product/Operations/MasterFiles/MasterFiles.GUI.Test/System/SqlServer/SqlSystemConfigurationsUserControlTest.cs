using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using static System.FormattableString;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed class SqlSystemConfigurationsUserControlTest : NonTransactionedTestCase
	{
		[RequiresSTA]
		public void TestApplySystemConfigurations()
		{
			// Arrange
			using (var form = new ZForm())
			{
				sqlSystemConfigurationsUserControl
					.Protected()
					.Setup("ApplyProposedConfigurations")
					.Verifiable();

				InitializeComponents(form);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.Show();
				MakeChangeToConfig();

				// Act
				ZControlExtensions.FindSingleOrDefault<ZButton>(sqlSystemConfigurationsUserControl.Object, "applyButton").PerformClick();

				// Assert
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				Assert(lastMessage.WasInformation);
				Assert(lastMessage.Contains(Invariant($"You have successfully applied proposed configurations to server: {Db.Connection.ServerName}.{Db.Connection.ServerDomain}")));

				sqlSystemConfigurationsUserControl
					.Protected()
					.Verify("ApplyProposedConfigurations", Times.Once());

				// Cleanup
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSaveSystemConfigurations()
		{
			// Arrange
			using (var form = new ZChildForm())
			{
				sqlSystemConfigurationsUserControl
					.Protected()
					.Setup("SaveSystemConfigurations")
					.Verifiable();

				InitializeComponents(form);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.Show();
				MakeChangeToConfig();

				// Act
				ZControlExtensions.FindSingleOrDefault<ZButton>(sqlSystemConfigurationsUserControl.Object, "saveButton").PerformClick();

				// Assert
				sqlSystemConfigurationsUserControl
					.Protected()
					.Verify("SaveSystemConfigurations", Times.Once());
			}
		}

		public void TestApplySystemConfigurations_HandledExceptions()
		{
			// Arrange
			var testException = new ApplicationException(nameof(TestApplySystemConfigurations_HandledExceptions));

			using (var form = new ZChildForm())
			{
				sqlSystemConfigurationsUserControl
					.Protected()
					.Setup("ApplyProposedConfigurations")
					.Callback(() => throw testException);

				InitializeComponents(form);

				form.Show();
				MakeChangeToConfig();

				// Act
				ZControlExtensions.FindSingleOrDefault<ZButton>(sqlSystemConfigurationsUserControl.Object, "applyButton").PerformClick();

				// Assert
				AssertEquals(testException, ErrorReporter.LastExceptionReported);

				// Cleanup
				ErrorReporter.Clear();
			}
		}

		public void TestCancelButtonCanCloseForm()
		{
			// Arrange
			using (var form = new ZChildForm())
			{
				InitializeComponents(form);

				form.Show();
				AssertEquals(true, form.Visible);

				// Act
				ZControlExtensions.FindSingleOrDefault<ZButton>(sqlSystemConfigurationsUserControl.Object, "cancelButton").PerformClick();

				// Assert
				AssertEquals(false, form.Visible);
			}
		}

		public void TestButtonPanelMinimumSizeSetCorrectly()
		{
			// Arrange
			using (var form = new ZForm())
			{
				form.Controls.Add(sqlSystemConfigurationsUserControl.Object);

				// Act
				var buttonPanelField = typeof(SqlSystemConfigurationsUserControl).GetField("buttonPanel", BindingFlags.Instance | BindingFlags.NonPublic);
				var buttonPanel = buttonPanelField.GetValue(sqlSystemConfigurationsUserControl.Object) as TableLayoutPanel;

				// Assert
				AssertLessThanOrEqualTo("Width larger than 225 will hide buttons when reducing size of form", 225, buttonPanel.MinimumSize.Width);
			}
		}

		void InitializeComponents(Form form)
		{
			sqlSystemConfigurationsUserControl.Object.Dock = DockStyle.Fill;
			sqlSystemConfigurationsUserControl.Object.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			sqlSystemConfigurationsUserControl.Object.Name = "tabControl";

			form.Controls.Add(sqlSystemConfigurationsUserControl.Object);
		}

		void MakeChangeToConfig()
		{
			config.HasChanges = true; // mimic user input triggers grid to inform BO of being changed
			config.ProposedValueText = "6000";
			AssertEquals(true, ZControlExtensions.FindSingleOrDefault<ZButton>(sqlSystemConfigurationsUserControl.Object, "saveButton").Enabled);
		}

		Mock<SqlSystemConfigurationsUserControl> sqlSystemConfigurationsUserControl;
		SqlSystemConfiguration config;
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
		}
	}
}
