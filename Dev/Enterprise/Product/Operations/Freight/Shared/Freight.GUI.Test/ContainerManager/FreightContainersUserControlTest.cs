using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.GUI.Test.ContainerManager
{
	public class FreightContainersUserControlTest : TestCaseWithFactory
	{
		public void TestContractAndConsolNumberTextBoxVisibility()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var consolNumberTextBox = GetConsolNumberTextBox(containerUserControl);
				var contractNumberTextBox = GetContractNumberTextBox(containerUserControl);
				AssertEquals(true, consolNumberTextBox.Visible);
				AssertEquals(true, contractNumberTextBox.Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var consolNumberTextBox = GetConsolNumberTextBox(containerUserControl);
				var contractNumberTextBox = GetContractNumberTextBox(containerUserControl);
				AssertEquals(false, consolNumberTextBox.Visible);
				AssertEquals(false, contractNumberTextBox.Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var consolNumberTextBox = GetConsolNumberTextBox(containerUserControl);
				var contractNumberTextBox = GetContractNumberTextBox(containerUserControl);
				AssertEquals(false, consolNumberTextBox.Visible);
				AssertEquals(false, contractNumberTextBox.Visible);
			}
		}

		ZTextBox GetConsolNumberTextBox(FreightContainersUserControl containerUserControl)
		{
			return (ZTextBox)containerUserControl.Controls.Find("ConsolNumberTextBox", true)[0];
		}

		ZTextBox GetContractNumberTextBox(FreightContainersUserControl containerUserControl)
		{
			return (ZTextBox)containerUserControl.Controls.Find("ContractNumberTextBox", true)[0];
		}

		public FreightContainersUserControl CreateContainersUserControlWithBinding()
		{
			var containerUserControl = new FreightContainersUserControl();
			var container = Factory.New<CommonContainer>();
			containerUserControl.CurrentContainer = container;
			containerUserControl.SetDataBinding(container, "");
			return containerUserControl;
		}
	}
}
