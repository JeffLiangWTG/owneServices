using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ContractManagement.GUI.Testing
{
	[TestedType(typeof(ContractAndAllocationsAttachForm))]
	public class ContractAndAllocationsAttachFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ContractAndAllocationsAttachForm(configurationMock.Object);
		}

		[RequiresSTA]
		public void TestPerformSearch()
		{
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractType = "PRO";

			Factory.Save();

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				var gridCount = filterControl.Grid.List.Count;
				AssertEquals("Should contain 1 contract", gridCount, 1);
			}
		}

		[RequiresSTA]
		public void TestSelectContractButton_Valid()
		{
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();
				filterControl.Grid.Select(0);

				var selectContractButton = GUITestHelper.FindControl<ZButton>(form.Controls, "SelectContractButton");
				selectContractButton.PerformClick();

				var latestMessage = ((UnitTestUserNotification)Globals.Message).LastMessage.Text;
				AssertNullOrEmpty(latestMessage);
			}
		}

		[RequiresSTA]
		public void TestSelectContractButton_ContractError()
		{
			var errorMessage = "No can do. Got an 8:30 res at Dorsia.";
			var notification = new Notification(NotificationType.Error, errorMessage);

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			notificationProviderMock.Setup(notificationProvider => notificationProvider.GetContractAllocationNotification(It.IsAny<RatingContract>())).Returns(notification);

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				var selectContractButton = GUITestHelper.FindControl<ZButton>(form.Controls, "SelectContractButton");
				selectContractButton.PerformClick();

				var latestMessage = ((UnitTestUserNotification)Globals.Message).LastMessage.Text;
				AssertContains("Should display contract error", errorMessage, latestMessage);
			}
		}

		[RequiresSTA]
		public void TestSelectContractButton_FormStaysOpenWhenSelectionFails()
		{
			formActionsMock.Setup(formActions => formActions.TryAllocateToContract(It.IsAny<IRatingContract>())).Returns(false);

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				var selectContractButton = GUITestHelper.FindControl<ZButton>(form.Controls, "SelectContractButton");
				selectContractButton.PerformClick();

				AssertEquals("Form should not have closed, so no dialog result expected.", DialogResult.None, form.DialogResult);
			}
		}

		[RequiresSTA]
		public void TestSelectAllocationRouteButton_Valid()
		{
			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var contract = route.Contract;
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				var selectRouteButton = GUITestHelper.FindControl<ZButton>(form.Controls, "SelectAllocationRouteButton");
				selectRouteButton.PerformClick();

				var latestMessage = ((UnitTestUserNotification)Globals.Message).LastMessage.Text;
				AssertNullOrEmpty(latestMessage);
			}
		}

		[RequiresSTA]
		public void TestSelectAllocationRouteButton_RouteError()
		{
			var errorMessage = "No can do. Got an 8:30 res at Dorsia.";
			var notification = new Notification(NotificationType.Error, errorMessage);

			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var contract = route.Contract;
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			notificationProviderMock.Setup(notificationProvider => notificationProvider.GetRouteAllocationNotification(It.IsAny<RatingContractAllocationLine>())).Returns(notification);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				var selectRouteButton = GUITestHelper.FindControl<ZButton>(form.Controls, "SelectAllocationRouteButton");
				selectRouteButton.PerformClick();

				var latestMessage = ((UnitTestUserNotification)Globals.Message).LastMessage.Text;
				AssertContains("Should display route error", errorMessage, latestMessage);
			}
		}

		[RequiresSTA]
		public void TestSelectAllocationRouteButton_ContractError()
		{
			var errorMessage = "No can do. Got an 8:30 res at Dorsia.";
			var notification = new Notification(NotificationType.Error, errorMessage);

			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var contract = route.Contract;
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			notificationProviderMock.Setup(notificationProvider => notificationProvider.GetContractAllocationNotification(It.IsAny<RatingContract>())).Returns(notification);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				var selectRouteButton = GUITestHelper.FindControl<ZButton>(form.Controls, "SelectAllocationRouteButton");
				selectRouteButton.PerformClick();

				var latestMessage = ((UnitTestUserNotification)Globals.Message).LastMessage.Text;
				AssertContains("Should display contract error", errorMessage, latestMessage);
			}
		}

		[RequiresSTA]
		public void TestSelectAllocationRouteButton_ContractAndRouteError()
		{
			var routeErrorMessage = "No can do. Got an 8:30 res at Dorsia.";
			var routeNotification = new Notification(NotificationType.Error, routeErrorMessage);

			var contractErrorMessage = "Let's see Paul Allen's card.";
			var contractNotification = new Notification(NotificationType.Error, contractErrorMessage);

			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var contract = route.Contract;
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			notificationProviderMock.Setup(notificationProvider => notificationProvider.GetRouteAllocationNotification(It.IsAny<RatingContractAllocationLine>())).Returns(routeNotification);
			notificationProviderMock.Setup(notificationProvider => notificationProvider.GetContractAllocationNotification(It.IsAny<RatingContract>())).Returns(contractNotification);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				Application.DoEvents();
				var selectRouteButton = GUITestHelper.FindControl<ZButton>(form.Controls, "SelectAllocationRouteButton");
				selectRouteButton.PerformClick();

				var latestMessage = ((UnitTestUserNotification)Globals.Message).LastMessage.Text;
				AssertContains("Should display route error", routeErrorMessage, latestMessage);
			}
		}

		[RequiresSTA]
		public void TestSelectAllocationRouteButton_FormStaysOpenWhenSelectionFails()
		{
			formActionsMock.Setup(formActions => formActions.TryAllocateToAllocationRoute(It.IsAny<IRatingContractAllocationLine>())).Returns(false);

			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var contract = route.Contract;
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				var selectRouteButton = GUITestHelper.FindControl<ZButton>(form.Controls, "SelectAllocationRouteButton");
				selectRouteButton.PerformClick();

				AssertEquals("Form should not have closed, so no dialog result expected.", DialogResult.None, form.DialogResult);
			}
		}

		[RequiresSTA]
		public void TestShouldNotShowDeletedContracts()
		{
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractType = "PRO";
			Factory.Save();

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = GetFormToBashCore() as ContractAndAllocationsAttachForm)
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<ZFilterStripCommonControl>(form.Controls, "ContractFilterStripControl");
				filterControl.FirePerformSearch();

				var gridCount = filterControl.Grid.List.Count;
				AssertEquals("Should show the contract", 1, gridCount);

				contract.RCT_IsActive = false;
				Factory.Save();

				filterControl.FirePerformSearch();
				gridCount = filterControl.Grid.List.Count;
				AssertEquals("Should not show the deleted contract", 0, gridCount);

				contract.RCT_IsActive = true;
				contract.RCT_ContractType = "CLI";
				Factory.Save();

				filterControl.FirePerformSearch();
				gridCount = filterControl.Grid.List.Count;
				AssertEquals("Should not show the client contract", 0, gridCount);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			formActionsMock = new Mock<IRatingContractSimulationFormActions>();
			formActionsMock.Setup(formActions => formActions.IsEnabledAllocationToContract).Returns(true);
			formActionsMock.Setup(formActions => formActions.IsEnabledAllocationToRoute).Returns(true);

			filterDefaultsMock = new Mock<IRatingContractSimulationFilterDefaults>();
			quantityProviderMock = new Mock<IRatingContractSimulationQuantityProvider>();
			notificationProviderMock = new Mock<IRatingContractSimulationNotificationProvider>();

			configurationMock = new Mock<IContractSimulationFormConfiguration>();
			configurationMock.Setup(configurationFactoryMock => configurationFactoryMock.FormActions).Returns(formActionsMock.Object);
			configurationMock.Setup(configurationFactoryMock => configurationFactoryMock.FilterDefaults).Returns(filterDefaultsMock.Object);
			configurationMock.Setup(configurationFactoryMock => configurationFactoryMock.QuantityProvider).Returns(quantityProviderMock.Object);
			configurationMock.Setup(configurationFactoryMock => configurationFactoryMock.NotificationProvider).Returns(notificationProviderMock.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();

			quantityProviderMock = null;
			formActionsMock = null;
			filterDefaultsMock = null;
		}

		Mock<IContractSimulationFormConfiguration> configurationMock;
		Mock<IRatingContractSimulationFormActions> formActionsMock;
		Mock<IRatingContractSimulationFilterDefaults> filterDefaultsMock;
		Mock<IRatingContractSimulationQuantityProvider> quantityProviderMock;
		Mock<IRatingContractSimulationNotificationProvider> notificationProviderMock;
	}
}
