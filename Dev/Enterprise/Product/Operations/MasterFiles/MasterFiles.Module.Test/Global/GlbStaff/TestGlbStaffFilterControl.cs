using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class TestGlbStaffFilterControl : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = new GlbStaffFilterControl())
			{
				var columns = filterControl.Grid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "GS_DepartureDate");
					AssertHasColumn(columns, "GS_Gender");
					AssertHasColumn(columns, "Country+Code");
					AssertHasColumn(columns, "LastPasswordChangeDate");
				});
			}
		}

		[RequiresSTA]
		public void TestCanAcceptDataCore()
		{
			using (var filterControl = new GlbStaffFilterControlForTesting())
			{
				var dataObject = new DataObject(DataFormats.FileDrop, new[] { "Test.txt" });
				Assert("Cannot accept data as DataObject is not GridRowsDataObject", !filterControl.CanAcceptDataCoreExposed(dataObject));

				using (var form = new ZForm())
				using (var grid = new ZGrid())
				{
					var collection = new DummyBusinessObjectCollection(Factory);

					var dummy1 = collection.AddNew();
					dummy1.Z0_Code = "ABC";
					dummy1.Z0_Number = 100;
					dummy1.Z0_Description = "One";

					var dummy2 = collection.AddNew();
					dummy2.Z0_Code = "XYZ";
					dummy2.Z0_Number = 999;
					dummy2.Z0_Description = "Two";

					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 80));
					grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_Number, 80, 0));
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
					form.Controls.Add(grid);
					grid.SetDataBinding(collection, "");

					dataObject = new GridRowsDataObject(grid, new BusinessObject[] { dummy1, dummy2 });

					Assert("Cannot accept data as bizo is not GlbGroup type", !filterControl.CanAcceptDataCoreExposed(dataObject));
				}

				using (var form = new ZForm())
				using (var grid = new ZGrid())
				{
					var glbGroupCollection = new GlbGroupCollection(Factory);

					var group1 = glbGroupCollection.AddNew();
					group1.GG_Code = "Group1";

					var group2 = glbGroupCollection.AddNew();
					group2.GG_Code = "Group2";

					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(GlbGroupSchema.Constants.GG_Code, 80));
					form.Controls.Add(grid);
					grid.SetDataBinding(glbGroupCollection, "");

					dataObject = new GridRowsDataObject(grid, new BusinessObject[] { group1, group2 });

					Assert("Can accept data", filterControl.CanAcceptDataCoreExposed(dataObject));
				}
			}
		}

		[RequiresSTA]
		public void TestAcceptDataCore()
		{
			var groupFactory = Factory;
			var staffFactory = new BusinessObjectFactory();

			using (var filterControl = new GlbStaffFilterControlForTesting())
			{
				using (var form = new ZForm())
				using (var grid = new ZGrid())
				{
					var glbGroupCollection = new GlbGroupCollection(groupFactory);

					var group1 = glbGroupCollection.AddNew();
					group1.GG_Code = "Group1";

					var group2 = glbGroupCollection.AddNew();
					group2.GG_Code = "Group2";

					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(GlbGroupSchema.Constants.GG_Code, 80));
					form.Controls.Add(grid);
					grid.SetDataBinding(glbGroupCollection, "");

					var dataObject = new GridRowsDataObject(grid, new BusinessObject[] { group1, group2 });
					var glbStaff = staffFactory.New<GlbStaff>();

					AssertEquals("Default group ALL", 1, glbStaff.Groups.Count);
					AssertEquals("ALL", glbStaff.Groups[0].GG_Code);

					filterControl.AcceptDataCoreExposed(dataObject, glbStaff);

					AssertEquals("Should contains three groups", 3, glbStaff.Groups.Count);
					AssertEquals("Group1", glbStaff.Groups[1].GG_Code);
					AssertEquals("Group2", glbStaff.Groups[2].GG_Code);

					AssertEquals("Added group should belong to the same factory as staff", glbStaff.Factory, glbStaff.Groups[1].Factory);
					AssertEquals("Added group should belong to the same factory as staff", glbStaff.Factory, glbStaff.Groups[2].Factory);
				}
			}
		}

		class GlbStaffFilterControlForTesting : GlbStaffFilterControl
		{
			public bool CanAcceptDataCoreExposed(IDataObject dataObject)
			{
				return CanAcceptDataCore(dataObject);
			}

			public void AcceptDataCoreExposed(IDataObject dataObject, BusinessObject targetBizO)
			{
				AcceptDataCore(dataObject, targetBizO);
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);

			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
