using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseCustomsCusContainersUserControlTest : TestCaseWithFactory
	{
		public void TestColumnStyleInfo_CO_RN_NKOwnerCountry()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.ContainerTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.ContainerUserControl)
				{
					var columnStyleInfo = userControl.CusContainersBoundGrid.ColumnStyles.OfType<ZCodeFindBoxColumnStyleInfo>().FirstOrDefault(x => x.ColumnName == "CO_RN_NKOwnerCountry");
					CombineAssertions(() =>
					{
						AssertNotNull(columnStyleInfo);
						AssertEquals("Lookups.CountryList", columnStyleInfo.BindToList);
						AssertEquals("Owner Country", columnStyleInfo.CaptionResourceString.Caption);
						AssertEquals(ZArchitecture.Modules.ModuleIDs.RefCountry, columnStyleInfo.ModuleID);
						AssertEquals(false, columnStyleInfo.IsVisible);
						AssertEquals(90, columnStyleInfo.Width);
					});
				}
			}
		}

		public void TestCusContainersBoundGridWithOverrideFreightDefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON001";
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_OverrideFreightDefaults = false;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.JE_JS = shipment.PK;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				Application.DoEvents();
				form.PlugIns.SelectPlugInTabPage(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				var plugin = (BaseBrokeragePlugIn)form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				plugin.OnGUIShown();
				var brokerageUserControl = (BaseCustomsBrokerageUserControl)plugin.UserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.ContainerTabPage;
				brokerageUserControl.LoadContainerTabPage();
				using (var userControl = brokerageUserControl.ContainerUserControl)
				{
					var grid = userControl.CusContainersBoundGrid;
					AssertEquals(1, grid.InnerGrid.ListManager.Count);
					Assert("Should not allow new when the synchronisation from shipment is active and relevant shipment is not null.", grid.ReadOnly);
					declaration.JE_OverrideFreightDefaults = true;
					AssertEquals(1, grid.InnerGrid.ListManager.Count);
					Assert("Should allow new when the synchronisation from shipment is inactive.", !grid.ReadOnly);
				}
			}
		}

		public void TestWarnUsersWhenDeletingContainers()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			Bill bill = declaration.Bills.AddNew();
			BasePackingGroup packGroup = bill.PackingGroups.AddNew();
			BaseCusContainer container = declaration.CusContainers.AddNew();
			packGroup.CR_CO_Container = container.PK;
			using (BaseJobDeclarationForm form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.ContainerTabPage;
				AssertNotNull(form.CustomsBrokerageUserControl.ContainerUserControl);
				using (BaseCustomsCusContainersUserControl userControl = form.CustomsBrokerageUserControl.ContainerUserControl)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals(1, userControl.CusContainersBoundGrid.InnerGrid.ListManager.Count);
					SelectTheFirstContainerAndPostDeleteKeyToGrid(userControl.CusContainersBoundGrid.InnerGrid);
					AssertNull("No pop-up is expected as no packing group is going to be deleted", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("container is deleted", true, container.IsDeleted);
					ZString containerNumber = "CRUX1234562";
					container = declaration.CusContainers.AddNew();
					container.CO_ContainerNumber = containerNumber;
					packGroup.CR_CO_Container = container.PK;
					BasePackingGroup nonContainerisedPackGroup = bill.PackingGroups.AddNew();
					AssertEquals(true, container.PackingGroups.HasElementsToBeDeletedWhenContainerDeleted);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					SelectTheFirstContainerAndPostDeleteKeyToGrid(userControl.CusContainersBoundGrid.InnerGrid);
					AssertEquals(string.Format("This container, {0} is about to be deleted and all the packages linked to this container will be deleted as well. Alternatively, you can clear the container number from the packages you want to retain and then, delete the container. Do you wish to delete the container now?", containerNumber), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Container is not deleted yet as users answered No", false, container.IsDeleted);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					SelectTheFirstContainerAndPostDeleteKeyToGrid(userControl.CusContainersBoundGrid.InnerGrid);
					AssertEquals(string.Format("This container, {0} is about to be deleted and all the packages linked to this container will be deleted as well. Alternatively, you can clear the container number from the packages you want to retain and then, delete the container. Do you wish to delete the container now?", containerNumber), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Container is not deleted yet as users answered No", true, container.IsDeleted);
				}
			}
		}

		void SelectTheFirstContainerAndPostDeleteKeyToGrid(ZGrid containerGrid)
		{
			containerGrid.Focus();
			containerGrid.Select(0);
			Application.DoEvents();
			KeySender.PostKeyDown(containerGrid, Keys.Delete);
			Application.DoEvents();
		}
	}
}
