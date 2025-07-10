using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class GlbGroupFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCanAcceptDataCore()
		{
			using (var filterControl = new GlbGroupFilterControlForTesting())
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
					var glbStaffCollection = new GlbStaffCollection(Factory);

					var staff1 = glbStaffCollection.AddNew();
					staff1.GS_FullName = "Staff1";

					var staff2 = glbStaffCollection.AddNew();
					staff2.GS_FullName = "Staff2";

					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(GlbStaffSchema.Constants.GS_FullName, 80));
					form.Controls.Add(grid);
					grid.SetDataBinding(glbStaffCollection, "");

					dataObject = new GridRowsDataObject(grid, new BusinessObject[] { staff1, staff2 });

					Assert("Can accept data", filterControl.CanAcceptDataCoreExposed(dataObject));
				}
			}
		}

		[RequiresSTA]
		public void TestAcceptDataCore()
		{
			var staffFactory = Factory;
			var groupFactory = new BusinessObjectFactory();

			using (var filterControl = new GlbGroupFilterControlForTesting())
			{
				using (var form = new ZForm())
				using (var grid = new ZGrid())
				{
					var glbStaffCollection = new GlbStaffCollection(staffFactory);

					var staff1 = glbStaffCollection.AddNew();
					staff1.GS_FullName = "Staff1";

					var staff2 = glbStaffCollection.AddNew();
					staff2.GS_FullName = "Staff2";

					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(GlbStaffSchema.Constants.GS_FullName, 80));
					form.Controls.Add(grid);
					grid.SetDataBinding(glbStaffCollection, "");

					var dataObject = new GridRowsDataObject(grid, new BusinessObject[] { staff1, staff2 });
					var glbGroup = groupFactory.New<GlbGroup>();

					AssertEquals("Empty List", 0, glbGroup.Staff.Count);

					filterControl.AcceptDataCoreExposed(dataObject, glbGroup);

					AssertEquals("Should contains two staff", 2, glbGroup.Staff.Count);
					AssertEquals("Staff1", glbGroup.Staff[0].GS_FullName);
					AssertEquals("Staff2", glbGroup.Staff[1].GS_FullName);

					AssertEquals("Added staff should belong to the same factory as group", glbGroup.Factory, glbGroup.Staff[0].Factory);
					AssertEquals("Added staff should belong to the same factory as group", glbGroup.Factory, glbGroup.Staff[1].Factory);
				}
			}
		}

		class GlbGroupFilterControlForTesting : GlbGroupFilterControl
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
	}
}
