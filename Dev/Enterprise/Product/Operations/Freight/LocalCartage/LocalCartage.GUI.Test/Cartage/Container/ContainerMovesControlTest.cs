using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class ContainerMovesControlTest : TestCaseWithFactory
	{
		public void TestShowCartageLeg()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var leg = cartage.CartageLegs.AddNew();
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.Cartage.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(cartage))
			using (var control = new ContainerMovesControl())
			{
				control.SetDataBinding(cartage, "ContainerBookedMoves");
				form.Controls.Add(control);
				using (var viewForm = (ZForm)control.ShowCartageLeg(leg))
				{
					AssertEquals("ContainsCheckpoint(Env.Licence.LocalTransport)", true, viewForm.LicensedComponentManager.ContainsCheckpoint(Env.Licence.LocalTransport));
				}
			}
		}

		public void TestCalculateDistanceContextMenu()
		{
			using (ContainerMovesControl control = new ContainerMovesControl())
			{
				AssertNotNull(control.ContainerCartageLegsGrid.ContextMenu.MenuItems.FindByText("Calculate distance"));
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
			using (ContainerMovesControl control = new ContainerMovesControl())
			{
				control.SetDataBinding(cartage, "");
				AssertGridContainsCustomField(control.ContainerCartageLegsGrid, "stringField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.ContainerCartageLegsGrid, "intField", typeof(ZCalcEditColumnStyleInfo));
				AssertGridContainsCustomField(control.ContainerCartageLegsGrid, "dateTimeField", typeof(ZDateEditColumnStyleInfo));
				AssertGridContainsCustomField(control.ContainerCartageLegsGrid, "boolField", typeof(ZCheckBoxColumnStyleInfo));
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
