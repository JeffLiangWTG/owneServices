using System;
using System.Reflection;
using System.Windows.Forms;
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
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class LooseMovesControlTest : TestCaseWithFactory
	{
		public void TestShowCartageLeg()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var leg = looseMove.CartageLegs.AddNew();
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.Cartage.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(cartage))
			using (var control = new LooseMovesControl())
			{
				form.Controls.Add(control);
				using (var viewForm = (ZForm)control.ShowCartageLeg(leg))
				{
					AssertEquals("ContainsCheckpoint(Env.Licence.LocalTransport)", true, viewForm.LicensedComponentManager.ContainsCheckpoint(Env.Licence.LocalTransport));
				}
			}
		}

		public void TestCalculateDistanceContextMenu()
		{
			using (LooseMovesControl control = new LooseMovesControl())
			{
				AssertNotNull(control.LooseDeliveryGrid.ContextMenu.MenuItems.FindByText("Calculate distance"));
				AssertNotNull(control.LooseCartageLegsGrid.ContextMenu.MenuItems.FindByText("Calculate distance"));
			}
		}

		public void TestGroupLooseBookedMoves()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves[0];
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove move3 = cartage.LooseBookedMoves.AddNew();
			move1.EW_BookedPackCount = 1;
			move1.EW_BookedWeight = 2;
			move1.EW_BookedVolume = 3;
			move2.EW_BookedPackCount = 4;
			move2.EW_BookedWeight = 5;
			move2.EW_BookedVolume = 6;
			move3.EW_BookedPackCount = 7;
			move3.EW_BookedWeight = 8;
			move3.EW_BookedVolume = 9;
			using (CartageForm form = new CartageForm(cartage))
			{
				form.Show();
				((TabControl)form.LooseMovesTabPage.Parent).SelectedTab = form.LooseMovesTabPage;
				LooseMovesControl looseMovesControl = form.looseMovesControl1;
				UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.OK);
				MethodInfo group_ClickMethodInfo = looseMovesControl.GetType().GetMethod("Group_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				group_ClickMethodInfo.Invoke(looseMovesControl, new object[] { null, EventArgs.Empty });
				AssertEquals("LastMessage Shown", "More than 1 row is required to be selected to use the group function.", userNotify.LastMessage.Text);
				userNotify.ClearMessagesAndAnswers();
				looseMovesControl.LooseDeliveryGrid.SelectAllElements();
				group_ClickMethodInfo.Invoke(looseMovesControl, new object[] { null, EventArgs.Empty });
				AssertEquals("LastMessage Shown", true, userNotify.LastMessage.WasNone);
				Assert(!move1.IsDeleted);
				AssertEquals(12, move1.EW_BookedPackCount);
				AssertEquals(15m, move1.EW_BookedWeight);
				AssertEquals(18m, move1.EW_BookedVolume);
				Assert(move2.IsDeleted);
				Assert(move3.IsDeleted);
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
			using (LooseMovesControl control = new LooseMovesControl())
			{
				control.SetDataBinding(cartage, "");
				AssertGridContainsCustomField(control.LooseCartageLegsGrid, "stringField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.LooseCartageLegsGrid, "intField", typeof(ZCalcEditColumnStyleInfo));
				AssertGridContainsCustomField(control.LooseCartageLegsGrid, "dateTimeField", typeof(ZDateEditColumnStyleInfo));
				AssertGridContainsCustomField(control.LooseCartageLegsGrid, "boolField", typeof(ZCheckBoxColumnStyleInfo));
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

		public void TestSelectCartageLegDoesNotThrowIfLooseDeliveryGridNotBoundToList()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var leg = looseMove.CartageLegs.AddNew();
			Factory.Save();
			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.Cartage.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(cartage))
			using (var control = new LooseMovesControl())
			{
				form.Controls.Add(control);
				using (var viewForm = (ZForm)control.ShowCartageLeg(leg))
				{
					control.LooseDeliveryGrid.IsListManagerNotNull = false;
					AssertNoExceptionThrown(() => control.SelectCartageLeg(leg));
				}
			}
		}

		public void TestSelectCartageLegDoesNotThrowIfLooseCartageLegsGridNotBoundToList()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var leg = looseMove.CartageLegs.AddNew();
			Factory.Save();
			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.Cartage.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(cartage))
			using (var control = new LooseMovesControl())
			{
				form.Controls.Add(control);
				using (var viewForm = (ZForm)control.ShowCartageLeg(leg))
				{
					control.LooseCartageLegsGrid.IsListManagerNotNull = false;
					AssertNoExceptionThrown(() => control.SelectCartageLeg(leg));
				}
			}
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
