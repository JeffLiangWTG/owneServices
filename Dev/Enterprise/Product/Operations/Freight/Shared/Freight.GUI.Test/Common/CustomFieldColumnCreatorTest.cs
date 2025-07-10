using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class CustomFieldColumnCreatorTest : TestCaseWithFactory
	{
		public void TestAddCustomColumns()
		{
			using (var grid = new ZGrid())
			{
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(),
					grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(FormatColumnInfo));

				var customFieldInfos = new List<CustomFieldInfo>
				{
					CustomFieldInfo.New(true, "Description", "", DummyBizoSchema.Z0_Description),
					CustomFieldInfo.New(true, "Decimal", "", DummyBizoSchema.Z0_Decimal),
					CustomFieldInfo.New(true, "Date", "", DummyBizoSchema.Z0_Date),
					CustomFieldInfo.New(true, "Bool", "", DummyBizoSchema.Z0_Bool),
				};

				new CustomFieldColumnCreator().Set(grid, new DummyCustomFieldsDescriptor(customFieldInfos));

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"Z0_Description|Description|ZTextBoxColumnStyleInfo|False",
					"Z0_Decimal|Decimal|ZCalcEditColumnStyleInfo|False",
					"Z0_Date|Date|ZDateEditColumnStyleInfo|False",
					"Z0_Bool|Bool|ZCheckBoxColumnStyleInfo|False"
				},
				grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(FormatColumnInfo));
			}
		}

		#region TestShowActiveOnly

		public void TestShowActiveOnly()
		{
			AssertShowActiveOnly(false);
			AssertShowActiveOnly(true);
		}

		void AssertShowActiveOnly(bool showActiveOnly)
		{
			using (var grid = new ZGrid())
			{
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(FormatColumnInfo));

				new CustomFieldColumnCreator().Set(grid, new DummyCustomFieldsDescriptor(new[]
				{
					CustomFieldInfo.New(true, "Description", "", DummyBizoSchema.Z0_Description),
					CustomFieldInfo.New(false, "Code", "", DummyBizoSchema.Z0_Code)
				}),
				"", showActiveOnly);

				if (showActiveOnly)
				{
					AssertContainsExactElementsInAnyOrder("column visibility upon creation", new[]
					{
						"Z0_Description|Description|ZTextBoxColumnStyleInfo|False"
					},
					grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(FormatColumnInfo));
				}
				else
				{
					AssertContainsExactElementsInAnyOrder("column visibility upon creation", new[]
					{
						"Z0_Description|Description|ZTextBoxColumnStyleInfo|False",
						"Z0_Code|Code|ZTextBoxColumnStyleInfo|False"
					},
					grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(FormatColumnInfo));
				}
			}
		}

		#endregion

		#region TestColumnVisible

		public void TestColumnVisible()
		{
			AssertColumnVisible(false);
			AssertColumnVisible(true);
		}

		void AssertColumnVisible(bool isVisible)
		{
			using (var grid = new ZGrid())
			{
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(FormatColumnInfo));

				new CustomFieldColumnCreator().Set(grid, new DummyCustomFieldsDescriptor(new[]
				{
					CustomFieldInfo.New(true, "Description", "", DummyBizoSchema.Z0_Description)
				}),
				"", false, isVisible);

				AssertContainsExactElementsInAnyOrder("column visibility upon creation", new[]
				{
					"Z0_Description|Description|ZTextBoxColumnStyleInfo|" + isVisible.ToString()
				},
				grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(FormatColumnInfo));
			}
		}

		#endregion

		#region Implementation

		string FormatColumnInfo(ZGridColumnInfo column)
		{
			return string.Format("{0}|{1}|{2}|{3}",
				column.ColumnName,
				column.Caption,
				column.GetType().Name,
				column.IsVisible);
		}

		#endregion
	}
}
