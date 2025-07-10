using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SalesMigrationForm))]
	public class SalesMigrationFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = (SalesMigrationForm)GetFormToBash())
			{
				AssertEquals("", form.FormVerb);
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

			return new SalesMigrationForm(matching);
		}

		#endregion
	}
}
