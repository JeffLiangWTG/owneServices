using System.Collections;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefContainerISOTypesFilterControlTest : TestCase
	{
		[RequiresSTA]
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = new RefContainerISOTypesFilterControl())
			{
				var columns = filterControl.Grid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "ISOCode");
					AssertHasColumn(columns, "Description");
				});
			}
		}

		[RequiresSTA]
		public void TestInfoLabelIsDockedOnTop()
		{
			using (var filterControl = new RefContainerISOTypesFilterControl())
			{
				var infoLabelControls = filterControl.Controls.Find("InfoLabel", false);
				Assert(infoLabelControls != null);
				Assert(infoLabelControls.Length > 0);
				var infoLabel = infoLabelControls[0];
				Assert(infoLabel is ZLabel);
				Assert(infoLabel.Dock == System.Windows.Forms.DockStyle.Top);
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
