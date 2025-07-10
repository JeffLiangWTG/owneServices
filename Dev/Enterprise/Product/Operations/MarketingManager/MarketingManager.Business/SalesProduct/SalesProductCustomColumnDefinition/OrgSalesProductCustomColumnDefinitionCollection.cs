using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OrgSalesProductCustomColumnDefinitionCollection : BusinessObjectCollection<OrgSalesProductCustomColumnDefinition>
	{
		public OrgSalesProductCustomColumnDefinitionCollection(OrgSalesProduct salesProduct, SalesProductDefinitionGroup definitionGroup)
			: this(salesProduct, definitionGroup, salesProduct.Factory) { }

		public OrgSalesProductCustomColumnDefinitionCollection(OrgSalesProduct salesProduct, SalesProductDefinitionGroup definitionGroup, BusinessObjectFactory factory)
		   : base(factory, new ZQuery(GenCustomColumnDefinitionSchema.XC_ParentID, salesProduct.PK))
		{
			this.salesProduct = salesProduct;
			this.definitionGroup = definitionGroup;
		}

		readonly OrgSalesProduct salesProduct;
		readonly SalesProductDefinitionGroup definitionGroup;

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var columnDefinition = (OrgSalesProductCustomColumnDefinition)child;
			columnDefinition.XC_ParentID = salesProduct.PK;
			columnDefinition.XC_ParentTableCode = salesProduct.TablePrefix;
		}

		#endregion

		#region Import

		public void Import(SalesProductFormLayoutData data)
		{
			var columnFkStrings =
				definitionGroup == SalesProductDefinitionGroup.Detail ?
				data.TradeDetailCustomColumnDefinitionFks : data.TradeLaneCustomColumnDefinitionFks;

			if (columnFkStrings != null)
			{
				foreach (var columnFkString in columnFkStrings)
				{
					ZGuid fk;
					if (ZGuid.TryParse(columnFkString, out fk))
					{
						var columnDefinition = Factory.Load<OrgSalesProductCustomColumnDefinition>(fk);
						if (columnDefinition != null)
						{
							Add(columnDefinition);
						}
					}
				}
			}
		}

		public void Export(SalesProductFormLayoutData data)
		{
			var fks = this.Select(x => x.PK.ToString()).ToArray();
			if (definitionGroup == SalesProductDefinitionGroup.Lane)
			{
				data.TradeLaneCustomColumnDefinitionFks = fks;
			}
			else if (definitionGroup == SalesProductDefinitionGroup.Detail)
			{
				data.TradeDetailCustomColumnDefinitionFks = fks;
			}
		}

		#endregion
	}
}
