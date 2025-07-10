using System.Reflection;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed class DatabaseScopedConfigurationUserControlTest : TestCase
	{
		public void TestButtonPanelMinimumSizeSetCorrectly()
		{
			// Arrange
			DatabaseScopedConfigurationUserControl databaseScopedConfigurationUserControl;
			using (var form = new ZForm())
			{
				databaseScopedConfigurationUserControl = new DatabaseScopedConfigurationUserControl();
				form.Controls.Add(databaseScopedConfigurationUserControl);

				// Act
				var buttonPanelField = typeof(DatabaseScopedConfigurationUserControl).GetField("buttonPanel", BindingFlags.Instance | BindingFlags.NonPublic);
				var buttonPanel = buttonPanelField.GetValue(databaseScopedConfigurationUserControl) as TableLayoutPanel;

				// Assert
				AssertLessThanOrEqualTo("Width larger than 150 will hide buttons when reducing size of form", 150, buttonPanel.MinimumSize.Width);
			}
		}
	}
}
