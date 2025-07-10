using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Testing.Module
{
	abstract class BaseRatingHeaderFilterControlTest : TestCaseWithFactory
	{
		public void TestLastUpdatedColumnIsSupercededByLastEditColumn()
		{
			using (var control = GetNewFilterStripControl())
			{
				var form = new ZForm();
				form.Controls.Add(control);
				form.Show();

				CombineAssertions("'Last Updated' should be superceded by 'Last Edit'", () =>
				{
					var lastUpdatedColumn = GetColumn(control.FilteredGrid, "Last Updated");
					AssertNull("'Last Updated' should be null", lastUpdatedColumn);

					var lastEditColumn = GetColumn(control.FilteredGrid, "Last Edit");
					AssertColumn(lastEditColumn, typeof(ZTextBoxColumnStyleInfo), "'Last Edit' should exist");
				});

				form.Dispose();
			}
		}

		public void AssertColumn(ZGridColumnInfo column, Type type, string message = default)
		{
			AssertNotNull($"{message}: column should exist", column);
			AssertEquals($"{message}: column type", true, type.IsAssignableFrom(column.GetType()));
		}

		#region Implementation

		protected abstract ZFilterStripControl GetNewFilterStripControl();

		static ZGridColumnInfo GetColumn(ZGrid grid, ZString caption)
		{
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				if (column.CaptionResourceString.Caption == caption || column.Caption == caption)
				{
					return column;
				}
			}

			return null;
		}

		#endregion
	}
}
