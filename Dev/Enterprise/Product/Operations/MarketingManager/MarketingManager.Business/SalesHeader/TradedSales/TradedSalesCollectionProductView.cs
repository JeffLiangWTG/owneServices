using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradedSalesCollectionProductView : BusinessObjectCollectionView<OrgSales>
	{
		#region Constructor

		public TradedSalesCollectionProductView(TradedSalesCollection collection, OrgSalesProduct salesProduct)
			: base(collection)
		{
			this.salesProduct = salesProduct;

			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// defer rebuild as salesProduct not populated yet
		}

		public OrgSalesProduct SalesProduct
		{
			get { return salesProduct; }
		}
		readonly OrgSalesProduct salesProduct;

		#endregion

		#region Relationship

		protected override bool ShouldWeAddBusinessObjectStraightToView(BusinessObject businessObject)
		{
			return true;
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var sales = (OrgSales)element;
			return
				sales.OW_MP_Product == (salesProduct != null ? salesProduct.PK : ZGuid.Empty);
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			var sales = (OrgSales)child;
			sales.OW_MP_Product = (salesProduct != null) ? salesProduct.PK : ZGuid.Empty;
		}

		#endregion
	}
}
