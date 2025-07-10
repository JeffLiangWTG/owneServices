using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using CommercialInvoiceConstants = Enterprise.GlobalCommercialInvoice.Integration.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class GlobalCommercialInvoiceShipmentFormTest : TestCaseWithFactory
	{
		public void TestGlobalCommercialInvoicePluginVisibility()
		{
			AssertComplianceCommercialInvoicePluginVisibility(false);
			AssertComplianceCommercialInvoicePluginVisibility(true);

			void AssertComplianceCommercialInvoicePluginVisibility(bool registryValue)
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(registryValue)))
				using (var form = new ShipmentFormForTest(Factory.New<ForwardingShipment>()))
				{
					CombineAssertions(@$"Global Compliance Commercial Invoice new feature registry setting {registryValue}", () =>
					{
						var commercialInvoicePlugin = form.PlugIns.GetPlugIn(ControllerIDs.GlobalCommercialInvoicePlugin);

						if (registryValue)
						{
							AssertNotNull(commercialInvoicePlugin);
						}
						else
						{
							AssertNull(commercialInvoicePlugin);
						}
					});
				}
			}
		}

		public void TestGlobalCommercialInvoice_PluginTabPageName()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			using (var form = new ShipmentFormForTest(Factory.New<ForwardingShipment>()))
			{
				form.Show();

				((ISupportSwitchTabPage)form).SwitchTabPage(CommercialInvoiceConstants.PluginTabPageName);
				Application.DoEvents();

				AssertEquals(CommercialInvoiceConstants.PluginTabPageName, form.MainTabControl.SelectedTab.Name);
				form.Dispose();
			}
		}

		class ShipmentFormForTest : ShipmentForm, IDisposable
		{
			public ShipmentFormForTest(ForwardingShipment bO)
				: base(bO)
			{
			}

			public new ZTemplateTabControl MainTabControl
			{
				get { return base.MainTabControl; }
			}
		}
	}
}
