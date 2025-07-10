using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SalesMatchingForm))]
	public class SalesMatchingFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = (SalesMatchingForm)GetFormToBash())
			{
				AssertEquals("", form.FormVerb);
			}
		}

		public void TestHasModeAndType_ShouldSetReadOnlyStatusOfNewModeTypeButton()
		{
			using (var form = GetFormWithModeAndType())
			{
				form.Show();
				var newModeAndTypeButton = (ZButton)form.Controls.Find("NewModeTypeButton", false).First();
				AssertEquals(false, newModeAndTypeButton.ReadOnly);
			}

			using (var form = GetFormWithModeAndType(false))
			{
				form.Show();
				var newModeAndTypeButton = (ZButton)form.Controls.Find("NewModeTypeButton", false).First();
				AssertEquals("If no mode or type exist for a sales product, this button should be disabled", true, newModeAndTypeButton.ReadOnly);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var org = Factory.New<OrgHeader>();
			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var salesHeader = new SalesHeader(org, forwardingProduct);
			var newSales = salesHeader.EntitySalesCollectionProductView.AddNew();
			var matching = new SalesMatching(org, newSales, null, forwardingProduct.SalesMatchingOptions);

			return new SalesMatchingForm(matching);
		}

		SalesMatchingForm GetFormWithModeAndType(bool hasModeAndType = true)
		{
			var org = Factory.New<OrgHeader>();
			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var salesHeader = new SalesHeader(org, forwardingProduct);
			var newSales = salesHeader.EntitySalesCollectionProductView.AddNew();
			var matching = new SalesMatching(org, newSales, null, forwardingProduct.SalesMatchingOptions);

			return new SalesMatchingForm(matching, hasModeAndType);
		}

		#endregion
	}
}
