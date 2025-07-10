using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradedSalesCollection : BusinessObjectCollectionView<OrgSales>
	{
		#region Constructor

		public TradedSalesCollection(OrgHeader org)
			: base(org.SalesCollection)
		{
		}

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
				sales.IsActual;
		}

		#endregion
	}
}
