using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsFilterControlDBHitsTestCase<TCollection, TFilterBizO> : WhsEnvFilterControlDBHitsTestCase<TCollection, TFilterBizO>
		where TCollection : IBusinessObjectCollection
		where TFilterBizO : FilterBusinessObject
	{
		#region TestDoesNotCastToLower

		public void TestDoesNotCastToLower()
		{
			using (var control = GetNewFilterStripControl())
			{
				CombineAssertions("No column in CW1 should be cast to Lower case.", () =>
				{
					foreach (var column in control.Grid.ColumnStyles)
					{
						var columnStyle = column as ZGridColumnInfo;
						if (columnStyle != null && columnStyle.CharacterCasing == CharacterCasing.Lower)
						{
							Assert(columnStyle.ColumnName, false);
						}
					}

					Assert("All good.", true);
				});
			}
		}

		#endregion

		#region TestWorkflowCustomFields

		public void TestWorkflowCustomFields()
		{
			if (!string.IsNullOrEmpty(WorkflowDescriptorCode))
			{
				var template = Helper.CreateWorkflowTemplate("T1", WorkflowDescriptorCode);
				Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
				Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
				Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
				Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);

				Factory.Save();

				using (var control = GetNewFilterStripControl())
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
			else
			{
				// Workflow Custom Fields not supported
				Assert(true);
			}
		}

		protected virtual string WorkflowDescriptorCode => "";

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
				}
			}

			AssertEquals(name + " custom field was not found in the grid", true, wasFound);
		}

		#endregion

		#region Implementation

		protected abstract ZFilterStripControl GetNewFilterStripControl();

		protected new WhsTestHelperFunctions Helper => new WhsTestHelperFunctions(Factory);

		protected override IEnumerable<string> GetTableNamesToIgnoreForUnusedFetchHints(string columnName)
		{
			var tableNamesToIgnore = new List<string>();
			tableNamesToIgnore.Add(GenCustomAddOnValueSchema.Constants.TableName);
			if (columnName == WhsDocketSchema.Constants.WD_OH_Client || columnName == nameof(WhsDocket.ClientName))
			{
				tableNamesToIgnore.Add(OrgCompanyDataSchema.Constants.TableName);
			}

			return tableNamesToIgnore;
		}

		#endregion
	}
}
