using System.Collections;
using System.Linq;
using Enterprise.Core.Forms;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class GlbBranchFilterControlTest : TestCase
	{
		[RequiresSTA]
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = new GlbBranchFilterControl())
			{
				var columns = filterControl.Grid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "BaseCountry+Code");
				});
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);

			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
