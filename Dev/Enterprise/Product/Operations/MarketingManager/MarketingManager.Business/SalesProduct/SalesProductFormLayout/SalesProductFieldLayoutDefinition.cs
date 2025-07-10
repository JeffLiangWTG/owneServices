using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public abstract class SalesProductFieldLayoutDefinition : AutoSalesProductFieldLayoutDefinition
	{
		protected SalesProductFieldLayoutDefinition(OrgSalesProduct salesProduct)
			: base(salesProduct.Factory)
		{
			this.product = salesProduct;
		}

		readonly OrgSalesProduct product;

		#region GenCustomColumnDefinitionFk

		[List("Columns")]
		public override ZGuid GenCustomColumnDefinitionFk
		{
			get { return base.GenCustomColumnDefinitionFk; }
			set { base.GenCustomColumnDefinitionFk = value; }
		}

		#endregion

		#region Lists

		public OrgSalesProductCustomColumnDefinitionCollection Columns
		{
			get { return GetColumns(product); }
		}

		protected abstract OrgSalesProductCustomColumnDefinitionCollection GetColumns(OrgSalesProduct salesProduct);

		#endregion
	}
}
