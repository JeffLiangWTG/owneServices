using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefCurrencyFilterControl : ZFilterStripControl
	{
		public RefCurrencyFilterControl()
		{
			InitializeComponent();
		}

		public RefCurrencyFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetNumOfDecimalsByCompany();
		}

		#region Decimal Places

		void SetNumOfDecimalsByCompany()
		{
			foreach (ZGridColumnInfo column in FilteredGrid.ColumnStyles)
			{
				if (column.GroupName.Equals(Enterprise.MasterFiles.Module.Res.GetData("RefCurrencyFilterControl|d901a7cf-f5f3-4fc4-8039-732c72cc63d2", "Current Exchange Rates")) && column is ZCalcEditColumnStyleInfo calcEditColumn)
				{
					calcEditColumn.Decimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
				}
			}
		}

		#endregion
	}
}
