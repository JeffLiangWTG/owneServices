using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefShippingLineIntegrationsControlTest : TestCaseWithFactory
	{
		public void TestControl()
		{
			var shippingLine = Factory.New<RefShippingLine>();

			using (var form = new ZForm(shippingLine))
			using (var integrationsControl = new RefShippingLineIntegrationsControl())
			{
				form.Controls.Add(integrationsControl);
				integrationsControl.SetDataBinding(shippingLine, string.Empty);
				form.Show();

				AssertEquals(false, integrationsControl.RSL_ContainerAutomationAvailableCheckBox.Checked);
				shippingLine.RSL_ContainerAutomationAvailable = true;
				AssertEquals(true, integrationsControl.RSL_ContainerAutomationAvailableCheckBox.Checked);

				AssertEquals(false, integrationsControl.RSL_GlobalSailingScheduleAvailableCheckBox.Checked);
				shippingLine.RSL_GlobalSailingScheduleAvailable = true;
				AssertEquals(true, integrationsControl.RSL_GlobalSailingScheduleAvailableCheckBox.Checked);

				AssertEquals(false, integrationsControl.RSL_InvoiceAvailableCheckBox.Checked);
				shippingLine.RSL_InvoiceAvailable = true;
				AssertEquals(true, integrationsControl.RSL_InvoiceAvailableCheckBox.Checked);

				AssertEquals(false, integrationsControl.RSL_OceanCarrierMessagingAvailableCheckBox.Checked);
				shippingLine.RSL_OceanCarrierMessagingAvailable = true;
				AssertEquals(true, integrationsControl.RSL_OceanCarrierMessagingAvailableCheckBox.Checked);

				AssertEquals(false, integrationsControl.RSL_BookingRequestAvailableCheckBox.Checked);
				shippingLine.RSL_BookingRequestAvailable = true;
				AssertEquals(true, integrationsControl.RSL_BookingRequestAvailableCheckBox.Checked);

				AssertEquals(false, integrationsControl.RSL_ShippingInstructionAvailableCheckBox.Checked);
				shippingLine.RSL_ShippingInstructionAvailable = true;
				AssertEquals(true, integrationsControl.RSL_ShippingInstructionAvailableCheckBox.Checked);

				AssertEquals(false, integrationsControl.RSL_VerifiedGrossContainerWeightAvailableCheckBox.Checked);
				shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
				AssertEquals(true, integrationsControl.RSL_VerifiedGrossContainerWeightAvailableCheckBox.Checked);

				AssertEquals(false, integrationsControl.RSL_ShippingOrderAvailableCheckBox.Checked);
				shippingLine.RSL_ShippingOrderAvailable = true;
				AssertEquals(true, integrationsControl.RSL_ShippingOrderAvailableCheckBox.Checked);

				AssertEquals(false, integrationsControl.RSL_EManifestAvailableCheckBox.Checked);
				shippingLine.RSL_EManifestAvailable = true;
				AssertEquals(true, integrationsControl.RSL_EManifestAvailableCheckBox.Checked);
			}
		}
	}
}
