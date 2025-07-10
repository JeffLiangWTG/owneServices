using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public abstract class SalesProductGridColumnDefinition : AutoSalesProductGridColumnDefinition
	{
		protected SalesProductGridColumnDefinition(OrgSalesProduct salesProduct)
			: base(salesProduct.Factory)
		{
			this.product = salesProduct;
		}

		readonly OrgSalesProduct product;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsVisible = true;
		}

		#endregion

		#region Properties

		#region ColumnName

		[List("Columns")]
		public override ZGuid GenCustomColumnDefinitionFk
		{
			get { return base.GenCustomColumnDefinitionFk; }
			set { base.GenCustomColumnDefinitionFk = value; }
		}

		#endregion

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
