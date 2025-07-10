using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class ExchangeRates : BasePage
	{
		#region DataSource

		public CustomsExchangeRatesHeader RatesHeader
		{
			get { return DataSource as CustomsExchangeRatesHeader; }
		}

		protected ZString CompanyCode
		{
			get { return GetStringFromParameter("Ref"); }
		}

		protected override BusinessObject GetNewDataSource()
		{
			CustomsExchangeRatesHeader result = null;

			if (!CompanyCode.IsEmpty)
			{
				result = new CustomsExchangeRatesHeader(Factory, CompanyCode);
			}

			return result;
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return false; }
		}

		#endregion

		#region ExchangeRatesGrid

		protected override void SetupGrids()
		{
			base.SetupGrids();
			SetupExchangeRatesGrid();
		}

		protected void SetupExchangeRatesGrid()
		{
			ExchangeRatesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("805d5788-1556-42a4-a0ee-031b9f674ad5", "Currency Code"), CustomsExchangeRate.Schema.RX_Code));
			ExchangeRatesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("23951fd9-8d6e-4c55-b577-99289915dfa2", "Description"), CustomsExchangeRate.Schema.RX_Desc));
			ExchangeRatesGrid.Columns.Add(new ZDateTimeColumn(Res.GetString("e0a8b116-4c9c-4e44-9c83-531fb7294eb2", "Expiry Date"), CustomsExchangeRate.Schema.RE_ExpiryDate));

			ZCalcEditColumn rateColumn = new ZCalcEditColumn(Res.GetString("831fc3a9-8ae5-4127-a291-abbfc9193dd1", "Customs Rate"), CustomsExchangeRate.Schema.RE_SellRate, CustomsExchangeRate.Schema.Decimals);
			rateColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
			ExchangeRatesGrid.Columns.Add(rateColumn);
		}

		#endregion
	}
}
