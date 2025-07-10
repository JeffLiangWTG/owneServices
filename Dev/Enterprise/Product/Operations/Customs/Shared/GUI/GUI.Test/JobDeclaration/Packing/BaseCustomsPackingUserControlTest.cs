using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseCustomsPackingUserControlTest : TestCaseWithFactory
	{
		public void TestCW_ContainerNoOrEquipmentNoAvailabilityAndColumnCaption()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.ContainersRequired).Returns(true);
			declarationMock.Setup(m => m.EquipmentsRequired).Returns(true);
			var declaration = declarationMock.Object;

			using (var form = new ZForm(declaration))
			using (var userControl = new BaseCustomsBrokerageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				userControl.JobDeclaration = declaration;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;

				var packing = (BaseCustomsPackingUserControl)userControl.Packing;
				CombineAssertions("When ContainersRequired and EquipmentsRequired are both true", () =>
				{
					AssertNotNull(packing.PackingDetailsGrid.Columns[BasePackage.Schema.CW_ContainerNoOrEquipmentNo]);
					AssertEquals("Container/Equipment", packing.PackingDetailsGrid.GetColumnStyle(BasePackage.Schema.CW_ContainerNoOrEquipmentNo).Caption);
				});

				declarationMock.Setup(m => m.EquipmentsRequired).Returns(false);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				CombineAssertions("When ContainersRequired is true and EquipmentsRequired is false", () =>
				{
					AssertNotNull(packing.PackingDetailsGrid.Columns[BasePackage.Schema.CW_ContainerNoOrEquipmentNo]);
					AssertEquals("Container No", packing.PackingDetailsGrid.GetColumnStyle(BasePackage.Schema.CW_ContainerNoOrEquipmentNo).Caption);
				});

				declarationMock.Setup(m => m.ContainersRequired).Returns(false);
				declarationMock.Setup(m => m.EquipmentsRequired).Returns(true);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				CombineAssertions("When ContainersRequired is false and EquipmentsRequired is true", () =>
				{
					AssertNotNull(packing.PackingDetailsGrid.Columns[BasePackage.Schema.CW_ContainerNoOrEquipmentNo]);
					AssertEquals("Equipment", packing.PackingDetailsGrid.GetColumnStyle(BasePackage.Schema.CW_ContainerNoOrEquipmentNo).Caption);
				});

				declarationMock.Setup(m => m.EquipmentsRequired).Returns(false);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertNull("When ContainersRequired and EquipmentsRequired are both false", packing.PackingDetailsGrid.Columns[BasePackage.Schema.CW_ContainerNoOrEquipmentNo]);
			}
		}

		public void TestParentColumnDisplayed()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var userControl = new BaseCustomsBrokerageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.JobDeclaration = declaration;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packing = ((BaseCustomsPackingUserControl)userControl.Packing);
				Assert("Should not have Parent column in BaseControl", !packing.PackingDetailsGrid.Columns.Contains("CW_CW_ParentPackage"));
			}
		}
	}
}
