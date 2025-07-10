using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingContainersUserControlTest : TestCaseWithFactory
	{
		public void TestCarrierContractLookupsConditionalVisibility()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var popupFindBox = GetPopupFindBox(containerUserControl);
				AssertEquals(true, popupFindBox.Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var popupFindBox = GetPopupFindBox(containerUserControl);
				AssertEquals(false, popupFindBox.Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var popupFindBox = GetPopupFindBox(containerUserControl);
				AssertEquals(false, popupFindBox.Visible);
			}
		}

		public void TestCO2eForEmptyVisible()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var propInfo = containerUserControl.GetType().GetProperty("CO2eForEmptyVisible", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertEquals(true, propInfo.GetValue(containerUserControl, null));
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var propInfo = containerUserControl.GetType().GetProperty("CO2eForEmptyVisible", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertEquals(false, propInfo.GetValue(containerUserControl, null));
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var propInfo = containerUserControl.GetType().GetProperty("CO2eForEmptyVisible", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertEquals(false, propInfo.GetValue(containerUserControl, null));
			}
		}

		public void TestImportTabPage()
		{
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var container = Factory.New<ForwardingContainer>();
				container.JC_DeliveryMode = "CFS/CFS";

				containerUserControl.CurrentContainer = container;

				var importTabPage = GetTabPage(containerUserControl, "ImportTabPage");
				importTabPage.NotifyBindingOrShowing();

				var importLabel = GetLabel(importTabPage, "ImportProcessTabLabel");
				AssertEquals("Carrier Haulage", importLabel.CaptionResourceString.Caption);

				container.JC_DeliveryMode = "CFS/CY";
				AssertEquals("Merchant Haulage", importLabel.CaptionResourceString.Caption);
			}
		}

		public void TestExportTabPage()
		{
			using (var containerUserControl = CreateContainersUserControlWithBinding())
			{
				var container = Factory.New<ForwardingContainer>();
				container.JC_DeliveryMode = "CY/CFS";

				containerUserControl.CurrentContainer = container;

				var exportTabPage = GetTabPage(containerUserControl, "ExportTabPage");
				exportTabPage.NotifyBindingOrShowing();

				var exportLabel = GetLabel(exportTabPage, "ExportProcessTabLabel");
				var exportEmptyReqByDateEdit = GetDateEdit(exportTabPage);
				AssertEquals("Empty Pickup Required By", exportEmptyReqByDateEdit.CaptionResourceString.Caption);
				AssertEquals("Merchant Haulage", exportLabel.CaptionResourceString.Caption);

				container.JC_DeliveryMode = "CFS/CFS";
				AssertEquals("Empty Required at Door", exportEmptyReqByDateEdit.CaptionResourceString.Caption);
				AssertEquals("Carrier Haulage", exportLabel.CaptionResourceString.Caption);
			}
		}

		ZLabel GetLabel(ZTabPage tabPage, string labelName)
		{
			return (ZLabel)tabPage.Controls.Find(labelName, true)[0];
		}

		ZTabPage GetTabPage(ForwardingContainersUserControl containerUserControl, string tabPageName)
		{
			return (ZTabPage)containerUserControl.Controls.Find(tabPageName, true)[0];
		}

		ZDateEdit GetDateEdit(ZTabPage tabPage)
		{
			return (ZDateEdit)tabPage.Controls.Find("ExportEmptyReqByDateEdit", true)[0];
		}

		ContractAllocationGuidFindBox GetPopupFindBox(ForwardingContainersUserControl containerUserControl)
		{
			return (ContractAllocationGuidFindBox)containerUserControl.Controls.Find("JC_RCA_AllocationRouteCodeFindBox", true)[0];
		}

		public ForwardingContainersUserControl CreateContainersUserControlWithBinding()
		{
			var containerUserControl = new ForwardingContainersUserControl();
			var container = Factory.New<ForwardingContainer>();
			containerUserControl.CurrentContainer = container;
			containerUserControl.SetDataBinding(container, "");
			return containerUserControl;
		}
	}
}
