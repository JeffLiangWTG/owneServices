using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	public class CartageLegFilterControlTest : TestCaseWithFactory
	{
		public void TestWorkflowCustomFields()
		{
			var template = Helper.CreateWorkflowTemplate(WorkflowDescriptors.CartageLegWorkflowDescriptorCode);
			Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var legs = new CommonCartageLegCollection(Factory);
			var filterBO = new CartageLegPlannerFilterStripBusinessObject();
			using (var control = new CartageLegFilterControl(legs, filterBO))
			{
				var form = new ZForm();
				form.Controls.Add(control);
				form.Show();
				AssertGridContainsCustomField(control.FilteredGrid, "stringField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.FilteredGrid, "intField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.FilteredGrid, "dateTimeField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.FilteredGrid, "boolField", typeof(ZCheckBoxColumnStyleInfo));
				form.Dispose();
			}
		}

		void AssertGridContainsCustomField(ZGrid grid, ZString name, Type type)
		{
			bool wasFound = false;
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				if (column.Caption == name)
				{
					wasFound = true;
					Assert(column.GroupName.IsEmpty());
					AssertEquals(true, type.IsAssignableFrom(column.GetType()));
					AssertEquals(true, column.IsVisible);
				}
			}

			AssertEquals(name + " custom field was not found in the grid", true, wasFound);
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return testHelper ?? (testHelper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper testHelper;
	}
}
