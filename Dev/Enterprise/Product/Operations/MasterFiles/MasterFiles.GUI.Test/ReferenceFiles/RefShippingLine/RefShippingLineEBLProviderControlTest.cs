using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefShippingLineEBLProviderControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestEBlProviderTabShouldBeVisible()
		{
			var shippingLine = Factory.New<RefShippingLine>();

			var requirements = shippingLine.ShippingLineMessagingRequirements.AddNew();
			shippingLine.RSL_OceanCarrierMessagingAvailable = true;
			requirements.RSR_RST_NKType = "BLP";
			requirements.RSR_IsBookingRequest = true;
			requirements.RSR_IsShippingInstruction = true;
			requirements.RSR_IsShippingOrder = true;

			var eBLProviders = shippingLine.ShippingLineEBLProviders.AddNew();
			eBLProviders.RSE_IsAvailable = true;
			eBLProviders.RSE_IsDefault = true;
			eBLProviders.RSE_Name = "Cargo X";

			using (var form = new RefShippingLineForm(shippingLine))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("DetailTemplateTabControl", true).FirstOrDefault();
				AssertNotNull(tabControl);
				var tabPage = (ZTabPage)tabControl.TabPages["EBLProviderTabPage"];
				AssertEquals(true, tabPage.TabVisible);

				tabControl.SelectedIndex = 2;

				var eBLProviderGrid = (ZGrid)tabPage.Controls.Find("EBLProviderGrid", true).FirstOrDefault();
				AssertNotNull(eBLProviderGrid);
				AssertEquals(3, eBLProviderGrid.Columns.Count);

				CombineAssertions(() =>
				{
					AssertNotNull(eBLProviderGrid.Columns["RSE_IsAvailable"]);
					AssertNotNull(eBLProviderGrid.Columns["RSE_Name"]);
					AssertNotNull(eBLProviderGrid.Columns["RSE_IsDefault"]);
				});
			}
		}

		[RequiresSTA]
		public void TestEBLProviderTabShouldBeInvisibleIfMessagingRequirementHasNoBLPType()
		{
			var shippingLine = Factory.New<RefShippingLine>();

			var requirements = shippingLine.ShippingLineMessagingRequirements.AddNew();
			shippingLine.RSL_OceanCarrierMessagingAvailable = true;
			requirements.RSR_RST_NKType = "DOG";
			requirements.RSR_IsBookingRequest = true;
			requirements.RSR_IsShippingInstruction = true;
			requirements.RSR_IsShippingOrder = true;

			var eBLProviders = shippingLine.ShippingLineEBLProviders.AddNew();
			eBLProviders.RSE_IsAvailable = true;
			eBLProviders.RSE_IsDefault = true;
			eBLProviders.RSE_Name = "Cargo X";

			using (var form = new RefShippingLineForm(shippingLine))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("DetailTemplateTabControl", true).FirstOrDefault();
				AssertNotNull(tabControl);
				var tabPage = (ZTabPage)tabControl.TabPages["EBLProviderTabPage"];
				AssertNull(tabPage);
			}
		}
	}
}
