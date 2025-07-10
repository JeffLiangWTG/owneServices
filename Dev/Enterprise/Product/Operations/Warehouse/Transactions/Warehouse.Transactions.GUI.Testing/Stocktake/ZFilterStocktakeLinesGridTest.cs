using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class ZFilterStocktakeLinesGridTest : ZEditableGridWoLayotsTest
	{
		#region TestSetColumnsReadOnly

		public void TestSetColumnsReadOnly()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			using (var form = new ZForm())
			{
				using (var grid = new TestGrid())
				{
					var columnName = "WU_LastCount";
					var columnStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = columnName };
					columnStyleInfo.IsReadOnly = false;
					grid.ColumnStyles.Add(columnStyleInfo);
					form.Controls.Add(grid);
					grid.SetDataBinding(stocktake.Lines, "");

					AssertEquals(false, grid.DefaultColumnsExposed[columnName].ColumnStyle.ReadOnly);
					AssertEquals(false, grid.Columns[columnName].ColumnStyle.ReadOnly);

					grid.SetColumnsReadOnly(new string[] { columnName });
					AssertEquals(true, grid.DefaultColumnsExposed[columnName].ColumnStyle.ReadOnly);
					AssertEquals(true, grid.Columns[columnName].ColumnStyle.ReadOnly);
				}
			}
		}

		#endregion

		#region TestSetColumnsAvailability

		public void TestSetColumnsAvailability()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			using (var form = new ZForm())
			{
				using (var grid = new TestGrid())
				{
					const string defaultColumnName = WhsStocktakeLineSchema.Constants.WU_LastCount;
					var columnStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = defaultColumnName, IsReadOnly = false };
					grid.ColumnStyles.Add(columnStyleInfo);
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_LineNo });
					form.Controls.Add(grid);
					grid.SetDataBinding(stocktake.Lines, "");

					AssertEquals(2, grid.DefaultColumnsExposed.Count);
					AssertEquals(2, grid.Columns.Count);
					Assert(grid.DefaultColumnsExposed.Any(column => column.ColumnName == defaultColumnName));
					Assert(grid.Columns.Any(column => column.ColumnName == defaultColumnName));

					grid.SetColumnsAvailability(false, new string[] { defaultColumnName });
					AssertEquals(1, grid.DefaultColumnsExposed.Count);
					AssertEquals(1, grid.Columns.Count);
					Assert(grid.DefaultColumnsExposed.All(column => column.ColumnName != defaultColumnName));
					Assert(grid.Columns.All(column => column.ColumnName != defaultColumnName));

					grid.SetColumnsAvailability(true, new string[] { defaultColumnName });
					AssertEquals(2, grid.DefaultColumnsExposed.Count);
					AssertEquals(2, grid.Columns.Count);
					Assert(grid.DefaultColumnsExposed.Any(column => column.ColumnName == defaultColumnName));
					Assert(grid.Columns.Any(column => column.ColumnName == defaultColumnName));
				}
			}
		}

		#endregion

		#region TestOrderCountColumns

		public void TestOrderCountColumns()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			using (var form = new ZForm())
			{
				using (var grid = new TestGrid())
				{
					InitializeGridColumns(stocktake, form, grid);

					grid.SetAvailability(false, [WhsStocktakeLineSchema.Constants.WU_Count2, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy]);
					grid.SetAvailability(false, [WhsStocktakeLineSchema.Constants.WU_Count3, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy]);
					grid.RefreshTableStyles();

					grid.OrderCountColumns();

					AssertEquals(grid.DefaultColumnsExposed[0].ColumnName, column1Name);
					AssertEquals(grid.DefaultColumnsExposed[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.DefaultColumnsExposed[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[4].ColumnName, column2Name);
					AssertEquals(grid.DefaultColumnsExposed[5].ColumnName, column3Name);
					AssertEquals(grid.DefaultColumnsExposed[6].ColumnName, column4Name);

					AssertEquals(grid.Columns[0].ColumnName, column1Name);
					AssertEquals(grid.Columns[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.Columns[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.Columns[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.Columns[4].ColumnName, column2Name);
					AssertEquals(grid.Columns[5].ColumnName, column3Name);
					AssertEquals(grid.Columns[6].ColumnName, column4Name);

					grid.SetAvailability(true, [WhsStocktakeLineSchema.Constants.WU_Count2, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy]);

					grid.OrderCountColumns();

					AssertEquals(grid.DefaultColumnsExposed[0].ColumnName, column1Name);
					AssertEquals(grid.DefaultColumnsExposed[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.DefaultColumnsExposed[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(grid.DefaultColumnsExposed[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[7].ColumnName, column2Name);
					AssertEquals(grid.DefaultColumnsExposed[8].ColumnName, column3Name);
					AssertEquals(grid.DefaultColumnsExposed[9].ColumnName, column4Name);

					AssertEquals(grid.Columns[0].ColumnName, column1Name);
					AssertEquals(grid.Columns[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.Columns[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.Columns[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.Columns[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(grid.Columns[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(grid.Columns[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(grid.Columns[7].ColumnName, column2Name);
					AssertEquals(grid.Columns[8].ColumnName, column3Name);
					AssertEquals(grid.Columns[9].ColumnName, column4Name);

					grid.SetAvailability(true, [WhsStocktakeLineSchema.Constants.WU_Count3, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy]);

					grid.OrderCountColumns();

					AssertEquals(grid.DefaultColumnsExposed[0].ColumnName, column1Name);
					AssertEquals(grid.DefaultColumnsExposed[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.DefaultColumnsExposed[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(grid.DefaultColumnsExposed[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[7].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3);
					AssertEquals(grid.DefaultColumnsExposed[8].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[9].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[10].ColumnName, column2Name);
					AssertEquals(grid.DefaultColumnsExposed[11].ColumnName, column3Name);
					AssertEquals(grid.DefaultColumnsExposed[12].ColumnName, column4Name);

					AssertEquals(grid.Columns[0].ColumnName, column1Name);
					AssertEquals(grid.Columns[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.Columns[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.Columns[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.Columns[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(grid.Columns[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(grid.Columns[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(grid.Columns[7].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3);
					AssertEquals(grid.Columns[8].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified);
					AssertEquals(grid.Columns[9].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy);
					AssertEquals(grid.Columns[10].ColumnName, column2Name);
					AssertEquals(grid.Columns[11].ColumnName, column3Name);
					AssertEquals(grid.Columns[12].ColumnName, column4Name);
				}
			}
		}

		public void TestOrderCountColumns_WithNoCurrentCountColumns()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			using (var form = new ZForm())
			{
				using (var grid = new TestGrid())
				{
					InitializeGridColumns(stocktake, form, grid);

					grid.SetAvailability(false, [WhsStocktakeLineSchema.Constants.WU_Count2, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy]);
					grid.SetAvailability(false, [WhsStocktakeLineSchema.Constants.WU_Count3, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy]);
					grid.RefreshTableStyles();

					grid.OrderCountColumns();

					AssertEquals(grid.DefaultColumnsExposed[0].ColumnName, column1Name);
					AssertEquals(grid.DefaultColumnsExposed[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.DefaultColumnsExposed[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[4].ColumnName, column2Name);
					AssertEquals(grid.DefaultColumnsExposed[5].ColumnName, column3Name);
					AssertEquals(grid.DefaultColumnsExposed[6].ColumnName, column4Name);

					AssertEquals(grid.Columns[0].ColumnName, column1Name);
					AssertEquals(grid.Columns[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.Columns[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.Columns[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.Columns[4].ColumnName, column2Name);
					AssertEquals(grid.Columns[5].ColumnName, column3Name);
					AssertEquals(grid.Columns[6].ColumnName, column4Name);

					grid.RemoveColumns(WhsStocktakeLineSchema.Constants.WU_LastCount, WhsStocktakeLineSchema.Constants.WU_DateVerified, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					grid.SetAvailability(true, [WhsStocktakeLineSchema.Constants.WU_Count2, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy]);
					grid.OrderCountColumns();

					AssertEquals(grid.DefaultColumnsExposed[0].ColumnName, column1Name);
					AssertEquals(grid.DefaultColumnsExposed[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.DefaultColumnsExposed[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(grid.DefaultColumnsExposed[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[7].ColumnName, column2Name);
					AssertEquals(grid.DefaultColumnsExposed[8].ColumnName, column3Name);
					AssertEquals(grid.DefaultColumnsExposed[9].ColumnName, column4Name);

					AssertEquals(grid.Columns[0].ColumnName, column1Name);
					AssertEquals(grid.Columns[1].ColumnName, column2Name);
					AssertEquals(grid.Columns[2].ColumnName, column3Name);
					AssertEquals(grid.Columns[3].ColumnName, column4Name);
					AssertEquals(grid.Columns[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(grid.Columns[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(grid.Columns[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);

					grid.RemoveColumns(WhsStocktakeLineSchema.Constants.WU_Count2, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					grid.SetAvailability(true, [WhsStocktakeLineSchema.Constants.WU_Count3, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy]);

					grid.OrderCountColumns();

					AssertEquals(grid.DefaultColumnsExposed[0].ColumnName, column1Name);
					AssertEquals(grid.DefaultColumnsExposed[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(grid.DefaultColumnsExposed[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(grid.DefaultColumnsExposed[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[7].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3);
					AssertEquals(grid.DefaultColumnsExposed[8].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified);
					AssertEquals(grid.DefaultColumnsExposed[9].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy);
					AssertEquals(grid.DefaultColumnsExposed[10].ColumnName, column2Name);
					AssertEquals(grid.DefaultColumnsExposed[11].ColumnName, column3Name);
					AssertEquals(grid.DefaultColumnsExposed[12].ColumnName, column4Name);

					AssertEquals(grid.Columns[0].ColumnName, column1Name);
					AssertEquals(grid.Columns[1].ColumnName, column2Name);
					AssertEquals(grid.Columns[2].ColumnName, column3Name);
					AssertEquals(grid.Columns[3].ColumnName, column4Name);
					AssertEquals(grid.Columns[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3);
					AssertEquals(grid.Columns[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified);
					AssertEquals(grid.Columns[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy);
				}
			}
		}

		public void TestOrderCountColumns_WithoutCount1Columns()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			using (var form = new ZForm())
			{
				using (var gridWithInvisibleCount1Columns = new TestGrid())
				{
					InitializeGridColumns(stocktake, form, gridWithInvisibleCount1Columns);

					gridWithInvisibleCount1Columns.RemoveColumns(WhsStocktakeLineSchema.Constants.WU_LastCount, WhsStocktakeLineSchema.Constants.WU_DateVerified, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					gridWithInvisibleCount1Columns.SetAvailability(true, [WhsStocktakeLineSchema.Constants.WU_Count3, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy]);
					gridWithInvisibleCount1Columns.OrderCountColumns();

					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[0].ColumnName, column1Name);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[7].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[8].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[9].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[10].ColumnName, column2Name);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[11].ColumnName, column3Name);
					AssertEquals(gridWithInvisibleCount1Columns.DefaultColumnsExposed[12].ColumnName, column4Name);

					AssertEquals(gridWithInvisibleCount1Columns.Columns[0].ColumnName, column1Name);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[1].ColumnName, column2Name);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[7].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[8].ColumnName, column3Name);
					AssertEquals(gridWithInvisibleCount1Columns.Columns[9].ColumnName, column4Name);
				}
			}
		}

		public void TestOrderCountColumns_WithoutCount2Columns()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			using (var form = new ZForm())
			{
				using (var gridWithInvisibleCount2Columns = new TestGrid())
				{
					InitializeGridColumns(stocktake, form, gridWithInvisibleCount2Columns);

					gridWithInvisibleCount2Columns.RemoveColumns(WhsStocktakeLineSchema.Constants.WU_Count2, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					gridWithInvisibleCount2Columns.OrderCountColumns();

					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[0].ColumnName, column1Name);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2DateVerified);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[7].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[8].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[9].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[10].ColumnName, column2Name);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[11].ColumnName, column3Name);
					AssertEquals(gridWithInvisibleCount2Columns.DefaultColumnsExposed[12].ColumnName, column4Name);

					AssertEquals(gridWithInvisibleCount2Columns.Columns[0].ColumnName, column1Name);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[1].ColumnName, WhsStocktakeLineSchema.Constants.WU_LastCount);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[2].ColumnName, WhsStocktakeLineSchema.Constants.WU_DateVerified);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[3].ColumnName, WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[4].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[5].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3DateVerified);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[6].ColumnName, WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[7].ColumnName, column2Name);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[8].ColumnName, column3Name);
					AssertEquals(gridWithInvisibleCount2Columns.Columns[9].ColumnName, column4Name);
				}
			}
		}

		void InitializeGridColumns(WhsStocktake stocktake, ZForm form, TestGrid grid)
		{
			var dummyColumn1 = new ZTextBoxColumnStyleInfo { ColumnName = column1Name };
			var lastCountStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_LastCount };
			var dateVerifiedStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_DateVerified };
			var verifiedByStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy };
			var dummyColumn2 = new ZTextBoxColumnStyleInfo { ColumnName = column2Name };
			var count2StyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_Count2 };
			var count2DateVerifiedStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_Count2DateVerified };
			var count2VerifiedByStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy };
			var dummyColumn3 = new ZTextBoxColumnStyleInfo { ColumnName = column3Name };
			var count3StyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_Count3 };
			var count3DateVerifiedStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_Count3DateVerified };
			var count3VerifiedByStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy };
			var dummyColumn4 = new ZTextBoxColumnStyleInfo { ColumnName = column4Name };

			grid.ColumnStyles.Add(dummyColumn1);
			grid.ColumnStyles.Add(lastCountStyleInfo);
			grid.ColumnStyles.Add(dateVerifiedStyleInfo);
			grid.ColumnStyles.Add(verifiedByStyleInfo);

			grid.ColumnStyles.Add(dummyColumn2);
			grid.ColumnStyles.Add(count2StyleInfo);
			grid.ColumnStyles.Add(count2DateVerifiedStyleInfo);
			grid.ColumnStyles.Add(count2VerifiedByStyleInfo);

			grid.ColumnStyles.Add(dummyColumn3);
			grid.ColumnStyles.Add(count3StyleInfo);
			grid.ColumnStyles.Add(count3DateVerifiedStyleInfo);
			grid.ColumnStyles.Add(count3VerifiedByStyleInfo);
			grid.ColumnStyles.Add(dummyColumn4);

			form.Controls.Add(grid);
			grid.SetDataBinding(stocktake.Lines, "");

			form.Show();
		}

		#endregion

		#region TestIsDeleteMenuItemVisible

		public void TestIsDeleteMenuItemVisible()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);

			using (var form = new ZForm())
			{
				using (var grid = new TestGrid())
				{
					var columnName = "WU_Status";
					var columnStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = columnName };
					columnStyleInfo.IsReadOnly = false;
					grid.ColumnStyles.Add(columnStyleInfo);
					form.Controls.Add(grid);
					grid.SetDataBinding(stocktake.Lines, "");

					form.Show();
					AssertEquals("precondition", 1, grid.List.Count);
					AssertEquals(false, grid.IsDeleteMenuItemVisibleExposed);

					grid.SelectAllElements();// simulate righ clicking on row
					grid.OnMouseUpExposed(new MouseEventArgs(MouseButtons.Right, 1, 20, 20, -1));
					AssertEquals("Since the line is open, should be able to delete.", true, grid.IsDeleteMenuItemVisibleExposed);

					grid.Select(1); // select the row but try to open menu item outside
					grid.OnMouseUpExposed(new MouseEventArgs(MouseButtons.Right, 1, 50, 0, -1));
					AssertEquals("Since the line is open, should be able to delete.", true, grid.IsDeleteMenuItemVisibleExposed);

					line.WU_Status = StocktakeLineStatus.Codes.Closed;
					grid.SelectAllElements();
					grid.OnMouseUpExposed(new MouseEventArgs(MouseButtons.Right, 1, 20, 20, -1));
					AssertEquals("Cannot delete closed lines.", false, grid.IsDeleteMenuItemVisibleExposed);

					line.Delete();
					grid.Select(0);
					grid.OnMouseUpExposed(new MouseEventArgs(MouseButtons.Right, 1, 50, 0, -1));
					AssertEquals("Since there are no lines, it should not show deleted menu item.", false, grid.IsDeleteMenuItemVisibleExposed);
				}
			}
		}

		#endregion

		#region TestAssignSelectedLinesToUser

		#region TestAssignSelectedLinesToUser_WithStocktakeStatus

		public void TestAssignSelectedLinesToUser_WithStocktakeStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.New);

			using (var form = new StocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var contextMenu = form.StocktakeFilterStipUserControl.Grid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Assign Selected Lines to User");
				contextMenu.ShowPopupMenu();
				AssertEquals(false, menuItem.Enabled);

				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
				contextMenu.ShowPopupMenu();
				AssertEquals(false, menuItem.Enabled);
			}
		}

		#endregion

		[TestDate(2012, 05, 25)]
		public void TestAssignSelectedLinesToUser_Count1()
		{
			AssertSelectedLinesToUser(1);
		}

		[TestDate(2012, 05, 25)]
		public void TestAssignSelectedLinesToUser_Count2()
		{
			AssertSelectedLinesToUser(2);
		}

		[TestDate(2012, 05, 25)]
		public void TestAssignSelectedLinesToUser_Count3()
		{
			AssertSelectedLinesToUser(3);
		}

		void AssertSelectedLinesToUser(ZByte countColumnNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var activeStaff1 = Helper.CreateGlbStaff("01", "T1", true);
			var activeStaff2 = Helper.CreateGlbStaff("02", "T2", true);
			var inactiveStaff = Helper.CreateGlbStaff("IAU", "TIAU", false);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var openLineWithEmptyVerfiedDate = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available, countColumnNumber, activeStaff1, ZDateTime.Empty);
			var closedLineWithEmptyVerfiedDate = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, StocktakeInventoryStatus.Codes.Available, countColumnNumber, activeStaff1, ZDateTime.Empty);
			var openLineWithNoEmptyVerfiedDate = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Held, countColumnNumber, activeStaff1, ZDateTime.Now);
			var closedLineWithNoEmptyVerfiedDate = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, StocktakeInventoryStatus.Codes.Damaged, countColumnNumber, activeStaff1, ZDateTime.Now);
			var nonSelectedOpenLineWithEmptyVerfiedDate = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged, countColumnNumber, activeStaff1, ZDateTime.Now);

			using (var form = new StocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				using (var grid = (ZFilterStocktakeLinesGrid)form.StocktakeFilterStipUserControl.Grid)
				{
					var menuItem = grid.ContextMenu.MenuItems.FindByText("Assign Selected Lines to User");

					AssertEquals("pre-Condition", true, menuItem.Enabled);

					Factory.Save(); // Make sure no changes before perform search
					form.StocktakeFilterStipUserControl.FirePerformSearch();
					AssertEquals("Pre-condition", 5, form.StocktakeFilterStipUserControl.Grid.List.Count);

					grid.SelectElements(new BusinessObject[] { openLineWithEmptyVerfiedDate, closedLineWithEmptyVerfiedDate, openLineWithNoEmptyVerfiedDate, closedLineWithNoEmptyVerfiedDate });
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

					var staffCollectionPropertyInfo = typeof(ZRecordAttacher).GetField("originalFindBoxList", BindingFlags.NonPublic | BindingFlags.Instance);
					var staffCollection = (GlbStaffCollection)staffCollectionPropertyInfo.GetValue(grid.StocktakeLineAttacherForTesting);

					AssertCollectionContains("Collection should contain active staff.", activeStaff1, staffCollection);
					AssertCollectionContains("Collection should contain active staff.", activeStaff2, staffCollection);
					AssertCollectionNotContains("Collection should not contain inactive staff.", inactiveStaff, staffCollection);

					grid.LastShownStocktakeLineAttachPopupForTesting.SelectStaffForEmbeddedModuleSelection(activeStaff2);
					AssertEquals(activeStaff2.GS_Code, openLineWithEmptyVerfiedDate.CurrentCountVerifiedBy);
					AssertEquals(activeStaff1.GS_Code, closedLineWithEmptyVerfiedDate.CurrentCountVerifiedBy);
					AssertEquals(activeStaff1.GS_Code, openLineWithNoEmptyVerfiedDate.CurrentCountVerifiedBy);
					AssertEquals(activeStaff1.GS_Code, closedLineWithNoEmptyVerfiedDate.CurrentCountVerifiedBy);
					AssertEquals(activeStaff1.GS_Code, nonSelectedOpenLineWithEmptyVerfiedDate.CurrentCountVerifiedBy);

					grid.SelectElements(new[] { openLineWithNoEmptyVerfiedDate });
					grid.ContextMenu.ShowPopupMenu();
					AssertEquals("Since there is no empty verified date, context menu item should be disabled.", false, menuItem.Enabled);

					grid.SelectElements(new[] { closedLineWithEmptyVerfiedDate });
					grid.ContextMenu.ShowPopupMenu();
					AssertEquals("Since line is closed, context menu item should be disabled.", false, menuItem.Enabled);

					grid.SelectElements(new[] { openLineWithEmptyVerfiedDate });
					grid.ContextMenu.ShowPopupMenu();
					AssertEquals("Since line is with empty verified date, context menu item should be enabled.", true, menuItem.Enabled);

					grid.SelectElements(Array.Empty<BusinessObject>());
					grid.ContextMenu.ShowPopupMenu();
					AssertEquals("Since no line is selected, context menu item should be disabled.", false, menuItem.Enabled);
				}
			}
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		const string column1Name = "C1";
		const string column2Name = "C2";
		const string column3Name = "C3";
		const string column4Name = "C4";

		#endregion
	}

	class TestGrid : ZFilterStocktakeLinesGrid
	{
		public ZGridColumns DefaultColumnsExposed => DefaultColumns;

		public bool IsDeleteMenuItemVisibleExposed => base.IsDeleteMenuItemVisible;

		public void OnMouseUpExposed(MouseEventArgs e)
		{
			OnMouseUp(e);
		}

		public void RemoveColumns(params string[] columns)
		{
			foreach (var column in columns)
			{
				this.Columns.Remove(column);
			}
		}
	}
}
