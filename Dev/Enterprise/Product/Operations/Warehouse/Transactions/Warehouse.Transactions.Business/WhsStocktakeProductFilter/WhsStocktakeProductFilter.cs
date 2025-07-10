using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsStocktake), "ProductFilterCollection")]
	public class WhsStocktakeProductFilter : AutoWhsStocktakeProductFilter
	{
		public WhsStocktakeProductFilter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public ZString ProductCode
		{
			get { return (Product == null) ? ZString.Empty : Product.OP_PartNum; }
		}

		public ZString ProductDescription
		{
			get { return (Product == null) ? ZString.Empty : Product.OP_Desc; }
		}

		#endregion
	}
}
