using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesMatchingGrid : ZUserControl
	{
		public SalesMatchingGrid()
		{
			InitializeComponent();
		}

		public ZGrid SalesGrid
		{
			get { return MatchedSalesGrid; }
		}

		public void Init(SalesMatching matching)
		{
			var grid = MatchedSalesGrid;

			grid.GridId += "|" + matching.Product.MP_Code;

			var columnStylesForTradeDetailProperties = matching.SalesMatchingOptions.TradeDetailPropertiesForMatching.Select(property => GetColumnStyleInfo(matching, property, true)).ToArray();
			grid.ColumnStyles.InsertRange(3, columnStylesForTradeDetailProperties);

			var columnStylesForSalesProperties = matching.SalesMatchingOptions.SalesPropertiesForMatching.Select(property => GetColumnStyleInfo(matching, property)).ToArray();
			grid.ColumnStyles.InsertRange(1, columnStylesForSalesProperties);
		}

		static ZTextBoxColumnStyleInfo GetColumnStyleInfo(SalesMatching matching, Tuple<string, Type> property, bool isTradeDetailProperty = false)
		{
			var propertyName = property.Item1;
			if (propertyName == OrgSalesSchema.Constants.OW_OriginID)
			{
				var result = new ZTextBoxColumnStyleInfo();
				result.ColumnName = "OriginDescription";
				result.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

				var product = matching.Product;
				if (product != null && product.LocationArrangement == OrgSalesProductLocationArrangement.SingleLocation)
				{
					result.CaptionResourceString = Res.GetData("61f30312-62ee-4aa5-99a3-e3356d3e8029", "Location");
				}

				return result;
			}
			else if (propertyName == OrgSalesSchema.Constants.OW_DestinationID)
			{
				var result = new ZTextBoxColumnStyleInfo();
				result.ColumnName = "DestinationDescription";
				result.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
				return result;
			}
			else
			{
				var propertyType = property.Item2;
				var prefix = isTradeDetailProperty ? "TradeDetail+" : "Sales+";
				var columnName = prefix + propertyName;
				if (propertyType.IsAssignableFrom(typeof(ZGuid)))
				{
					return new ZGuidFindBoxColumnStyleInfo() { ColumnName = columnName };
				}
				else
				{
					return new ZTextBoxColumnStyleInfo() { ColumnName = columnName };
				}
			}
		}
	}
}
