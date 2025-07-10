using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class ContainerDetailsUserControlTest : TestCaseWithFactory // BaseFreightTest
	{
		MenuItem FindMenu(ZGrid grid, ZString menuText)
		{
			foreach (MenuItem menu in grid.ContextMenu.MenuItems)
			{
				if (menu.Text == menuText)
				{
					return menu;
				}
			}

			return null;
		}

		[ExpectNoExceptions()]
		public void TestExportToExcelDoesNotThrowException()
		{
			CommonCartage cartage = Factory.NewWithValidTestData<CommonCartage>();
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc";
			JobSailing sailing = (new LocalCartageTestHelper(Factory)).CreateSailing(vessel, "123", "AUSYD", "NZAKL", ZDateTime.Empty);
			cartage.JJ_JX_Sailing = sailing.PK;
			cartage.LocalClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			CommonContainer container1 = cartage.ContainerBookedMoves.AddNew().Container;
			CommonContainer container2 = cartage.ContainerBookedMoves.AddNew().Container;
			container1.JC_ContainerNum = "CONT1";
			container2.JC_ContainerNum = "CONT2";
			Factory.Save();
			using (ZForm form = new ZForm(cartage))
			{
				using (ContainerDetailsControl containerDetailsControl = new ContainerDetailsControl())
				{
					form.Controls.Add(containerDetailsControl);
					containerDetailsControl.ContainersGrid.SetDataBinding(cartage, "ContainerBookedMoves");
					AssertEquals("Precondition: Grid should contain 2 containers", 2, containerDetailsControl.ContainersGrid.List.Count);
					containerDetailsControl.ContainersGrid.SelectAllElements();
					try
					{
						var menu = FindMenu(containerDetailsControl.ContainersGrid, "Export All Columns To Excel");
						menu.PerformClick();
					}
					finally
					{
						DeleteIfExists(ExcelExporter.LastExportedFileNameStaticForTest);
					}

					try
					{
						var menu = FindMenu(containerDetailsControl.ContainersGrid, "Export Visible Columns To Excel");
						menu.PerformClick();
					}
					finally
					{
						DeleteIfExists(ExcelExporter.LastExportedFileNameStaticForTest);
					}
				}
			}
		}

		public void TestCreatingAndViewingLoadListForSelectedContainersDoesNotThrowException()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc";
			var sailing = (new LocalCartageTestHelper(Factory)).CreateSailing(vessel, "123", "AUSYD", "NZAKL", ZDateTime.Empty);
			cartage.JJ_JX_Sailing = sailing.PK;
			cartage.LocalClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "CONT1";
			Factory.Save();
			using (var form = new ZForm(cartage))
			{
				using (ContainerDetailsControl containerDetailsControl = new ContainerDetailsControl())
				{
					form.Controls.Add(containerDetailsControl);
					containerDetailsControl.ContainersGrid.SetDataBinding(cartage, "ContainerBookedMoves");
					AssertEquals("Precondition: Grid should contain 1 containers", 1, containerDetailsControl.ContainersGrid.List.Count);
					containerDetailsControl.ContainersGrid.Select(0); // first row
					var createMenuItem = FindMenu(containerDetailsControl.ContainersGrid, "Create Load List from selected containers");
					var viewMenuItem = FindMenu(containerDetailsControl.ContainersGrid, "View Load List of selected container");
					AssertNoExceptionThrown("Should not return unable to cast object exception", () =>
					{
						createMenuItem.PerformClick();
						viewMenuItem.PerformClick();
					});
				}
			}
		}

		public void TestSelectCartageLeg()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var move1 = cartage.ContainerBookedMoves.AddNew();
			var container1 = move1.Container;
			container1.JC_ContainerNum = "CNT1";
			var container1Leg = move1.CartageLegs.AddNew();
			AssertEquals("Pre-condition:", container1Leg.Container, container1);
			var move2 = cartage.ContainerBookedMoves.AddNew();
			var container2 = move2.Container;
			container2.JC_ContainerNum = "CNT2";
			var container2Leg = move2.CartageLegs.AddNew();
			AssertEquals("Pre-condition:", container2Leg.Container, container2);
			Factory.Save();
			using (var form = new ZForm(cartage))
			{
				using (ContainerDetailsControl containerDetailsControl = new ContainerDetailsControl())
				{
					form.Controls.Add(containerDetailsControl);
					containerDetailsControl.ContainersGrid.SetDataBinding(cartage, "ContainerBookedMoves");
					containerDetailsControl.ContainerMovesControl.ContainerCartageLegsGrid.SetDataBinding(cartage.ContainerBookedMoves, "CartageLegs");
					AssertEquals("Precondition: Grid should contain 2 containers", 2, containerDetailsControl.ContainersGrid.List.Count);
					containerDetailsControl.SelectCartageLeg(container2Leg);
					AssertEquals("Should have selected 1 row.", 1, containerDetailsControl.ContainersGrid.SelectedRowCount);
					var selectedMove = (CommonBookedCtgMove)containerDetailsControl.ContainersGrid.SelectedElements[0];
					AssertEquals("The current selected container should be Container2.", container2, selectedMove.Container);
				}
			}
		}
	}
}
