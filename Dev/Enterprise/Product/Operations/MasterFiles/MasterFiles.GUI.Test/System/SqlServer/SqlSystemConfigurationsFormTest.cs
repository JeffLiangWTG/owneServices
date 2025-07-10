using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(SqlSystemConfigurationsForm))]
	sealed partial class SqlSystemConfigurationsFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestAllTabPageSubControlsAreTypeofITabPageContentHolder()
		{
			// Arrange
			using (var form = new SqlSystemConfigurationsForm())
			{
				// Act
				form.Show();

				// Assert
				AssertEquals(
					"All tab page controls need to be type of ITabPageContentHolder",
					false,
					form.FindSingleOrDefault<ZTabControl>("tabControl")
						.Controls
						.Cast<ZTabPage>()
						.Select(x => x.Controls[0])
						.Any(x => !(x is ITabPageContentHolder)));
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new SqlSystemConfigurationsForm();
		}
	}
}
