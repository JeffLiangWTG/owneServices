using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SalesProductFormLayout : NonPersistentBusinessObject
	{
		public SalesProductFormLayout(OrgSalesProduct salesProduct)
			: base(salesProduct.Factory)
		{
			this.salesProduct = salesProduct;
		}

		readonly OrgSalesProduct salesProduct;

		#region Import / Export

		public void Import(SalesProductFormLayoutData data)
		{
			AddCustomColumnFetchHints(data);

			TradeLaneCustomColumnDefinitionCollection.Import(data);
			TradeLaneGridColumnDefinitions.Import(data);

			TradeDetailCustomColumnDefinitionCollection.Import(data);
			TradeDetailFieldLayoutDefinitions.Import(data);
			TradeDetailGridColumnDefinitions.Import(data);
		}

		void AddCustomColumnFetchHints(SalesProductFormLayoutData data)
		{
			IEnumerable<string> columnFkStrings = data.TradeLaneCustomColumnDefinitionFks ?? Enumerable.Empty<string>();
			if (data.TradeDetailCustomColumnDefinitionFks != null)
			{
				columnFkStrings = columnFkStrings.Union(data.TradeDetailCustomColumnDefinitionFks);
			}

			foreach (var columnFkString in columnFkStrings)
			{
				ZGuid fk;
				if (ZGuid.TryParse(columnFkString, out fk))
				{
					Factory.AddFetchHint(GenCustomColumnDefinitionSchema.Constants.TableName, fk);
				}
			}
		}

		public SalesProductFormLayoutData ToFormLayoutData()
		{
			var data = new SalesProductFormLayoutData();
			TradeLaneCustomColumnDefinitionCollection.Export(data);
			TradeLaneGridColumnDefinitions.Export(data);

			TradeDetailCustomColumnDefinitionCollection.Export(data);
			TradeDetailFieldLayoutDefinitions.Export(data);
			TradeDetailGridColumnDefinitions.Export(data);

			return data;
		}

		#endregion

		#region Related Business Objects

		#region TradeLane

		#region TradeLaneColumns

		[ChildEditable]
		public OrgSalesProductCustomColumnDefinitionCollection TradeLaneCustomColumnDefinitionCollection
		{
			get
			{
				if (tradeLaneCustomColumnDefinitionCollection == null)
				{
					tradeLaneCustomColumnDefinitionCollection = new OrgSalesProductCustomColumnDefinitionCollection(salesProduct, SalesProductDefinitionGroup.Lane);
					RegisterEditableChildObject(tradeLaneCustomColumnDefinitionCollection);
				}

				return tradeLaneCustomColumnDefinitionCollection;
			}
		}
		OrgSalesProductCustomColumnDefinitionCollection tradeLaneCustomColumnDefinitionCollection;

		#endregion

		#region TradeLaneGridColumnDefinitions

		[ChildEditable]
		public SalesProductTradeLaneGridColumnDefinitionCollection TradeLaneGridColumnDefinitions
		{
			get
			{
				if (tradeLaneGridColumnDefinitions == null)
				{
					tradeLaneGridColumnDefinitions = new SalesProductTradeLaneGridColumnDefinitionCollection(salesProduct);
					RegisterEditableChildObject(tradeLaneGridColumnDefinitions);
				}

				return tradeLaneGridColumnDefinitions;
			}
		}
		SalesProductTradeLaneGridColumnDefinitionCollection tradeLaneGridColumnDefinitions;

		#endregion

		#region TradeDetailFieldLayoutDefinitions

		[ChildEditable]
		public SalesProductFieldLayoutDefinitionCollection TradeDetailFieldLayoutDefinitions
		{
			get
			{
				if (tradeDetailFieldLayoutDefinitions == null)
				{
					tradeDetailFieldLayoutDefinitions = new SalesProductTradeDetailFieldLayoutDefinitionCollection(salesProduct);
					RegisterEditableChildObject(tradeDetailFieldLayoutDefinitions);
				}

				return tradeDetailFieldLayoutDefinitions;
			}
		}
		SalesProductFieldLayoutDefinitionCollection tradeDetailFieldLayoutDefinitions;

		#endregion

		#endregion

		#region TradeDetail

		#region TradeDetailColumns

		[ChildEditable]
		public OrgSalesProductCustomColumnDefinitionCollection TradeDetailCustomColumnDefinitionCollection
		{
			get
			{
				if (tradeDetailCustomColumnDefinitionCollection == null)
				{
					tradeDetailCustomColumnDefinitionCollection = new OrgSalesProductCustomColumnDefinitionCollection(salesProduct, SalesProductDefinitionGroup.Detail);
					RegisterEditableChildObject(tradeDetailCustomColumnDefinitionCollection);
				}

				return tradeDetailCustomColumnDefinitionCollection;
			}
		}
		OrgSalesProductCustomColumnDefinitionCollection tradeDetailCustomColumnDefinitionCollection;

		#endregion

		#region TradeDetailGridColumnDefinitions

		[ChildEditable]
		public SalesProductTradeDetailGridColumnDefinitionCollection TradeDetailGridColumnDefinitions
		{
			get
			{
				if (tradeDetailGridColumnDefinitions == null)
				{
					tradeDetailGridColumnDefinitions = new SalesProductTradeDetailGridColumnDefinitionCollection(salesProduct);
					RegisterEditableChildObject(tradeDetailGridColumnDefinitions);
				}

				return tradeDetailGridColumnDefinitions;
			}
		}
		SalesProductTradeDetailGridColumnDefinitionCollection tradeDetailGridColumnDefinitions;

		#endregion

		#endregion

		#endregion
	}
}
