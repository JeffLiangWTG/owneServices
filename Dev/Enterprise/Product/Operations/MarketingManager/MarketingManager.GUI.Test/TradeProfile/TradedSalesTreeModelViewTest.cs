using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradedSalesTreeModelView))]
	sealed class TradedSalesTreeModelViewTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);
			var model = new TradedSalesTreeModel(salesAnalysis);
			return new TradedSalesTreeModelView(model);
		}
	}
}
