using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class CartageLegsControlTest : TestCaseWithFactory
	{
		public void TestSendToVehicleMenuDoesntExist()
		{
			using (CartageLegsControl control = new CartageLegsControl())
			{
				AssertNull("Send to the vehicle menu is added from the GPSVehicleMonitor plugin, don't add it on the control", control.CartageLegsGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Send to the vehicle", true));
			}
		}

		public void TestContextMenus()
		{
			using (CartageLegsControl control = new CartageLegsControl())
			{
				AssertNotNull(control.CartageLegsGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Calculate distance"));
			}
		}

		public void TestWorkflowCustomFields()
		{
			var template = Helper.CreateWorkflowTemplate(WorkflowDescriptors.CartageLegWorkflowDescriptorCode);
			Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirExport, 2);
			using (CartageLegsControl control = new CartageLegsControl())
			{
				control.SetDataBinding(cartage, "");
				AssertGridContainsCustomField(control.CartageLegsGrid.InnerGrid, "stringField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.CartageLegsGrid.InnerGrid, "intField", typeof(ZCalcEditColumnStyleInfo));
				AssertGridContainsCustomField(control.CartageLegsGrid.InnerGrid, "dateTimeField", typeof(ZDateEditColumnStyleInfo));
				AssertGridContainsCustomField(control.CartageLegsGrid.InnerGrid, "boolField", typeof(ZCheckBoxColumnStyleInfo));
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
					AssertContains(string.Format("__{0}__prop", name.ToUpperInvariant()), column.ColumnName);
					AssertEquals("Workflow Custom Fields", column.GroupName.Caption);
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
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
