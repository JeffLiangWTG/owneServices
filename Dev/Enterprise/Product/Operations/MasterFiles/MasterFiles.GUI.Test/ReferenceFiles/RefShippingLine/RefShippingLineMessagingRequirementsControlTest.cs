using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefShippingLineMessagingRequirementsControlTest : TestCaseWithFactory
	{
		public void TestControl()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			var requirements = shippingLine.ShippingLineMessagingRequirements.AddNew();
			shippingLine.RSL_OceanCarrierMessagingAvailable = true;
			requirements.RSR_RST_NKType = "CON";
			requirements.RSR_IsBookingRequest = true;
			requirements.RSR_IsShippingInstruction = true;
			requirements.RSR_IsShippingOrder = true;

			using (var form = new RefShippingLineForm(shippingLine))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("DetailTemplateTabControl", true)[0];
				var tabPage = (ZTabPage)tabControl.TabPages["MessagingRequirementsTabPage"];
				AssertEquals(true, tabPage.TabVisible);

				shippingLine.RSL_OceanCarrierMessagingAvailable = false;
				AssertEquals(false, tabPage.TabVisible);

				shippingLine.RSL_OceanCarrierMessagingAvailable = true;
				AssertEquals(true, tabPage.TabVisible);

				tabControl.SelectedIndex = 1;

				var messagingRequirementsGrid = (ZGrid)tabPage.Controls.Find("MessagingRequirementsGrid", true)[0];
				AssertEquals(7, messagingRequirementsGrid.Columns.Count);

				CombineAssertions(() =>
				{
					AssertNotNull(messagingRequirementsGrid.Columns["RSR_RST_NKType"]);
					AssertNotNull(messagingRequirementsGrid.Columns["RequirementType+RST_Description"]);
					AssertNotNull(messagingRequirementsGrid.Columns["RSR_IsBookingRequest"]);
					AssertNotNull(messagingRequirementsGrid.Columns["RSR_IsShippingInstruction"]);
					AssertNotNull(messagingRequirementsGrid.Columns["RSR_IsShippingOrder"]);
					AssertNotNull(messagingRequirementsGrid.Columns["RSR_IsEManifest"]);
					AssertNotNull(messagingRequirementsGrid.Columns["RSR_IsVerifiedGrossContainerWeight"]);
				});
			}

			shippingLine.RSL_OceanCarrierMessagingAvailable = false;

			using (var form = new RefShippingLineForm(shippingLine))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("DetailTemplateTabControl", true)[0];
				var tabPage = (ZTabPage)tabControl.TabPages["MessagingRequirementsTabPage"];
				AssertNull(tabPage);
			}
		}
	}
}
