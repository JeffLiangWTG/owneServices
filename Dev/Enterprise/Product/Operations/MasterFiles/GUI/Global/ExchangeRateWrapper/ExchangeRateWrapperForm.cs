using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ExchangeRateWrapperForm : ZForm, IButtonNewTextOverride
	{
		public ExchangeRateWrapperForm(BulkExchangeRateUpdater updater) : base(updater)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		#region GUI Setup

		#region IButtonTextOverride members

		string IButtonNewTextOverride.NewButtonText
		{
			get { return Res.GetString("ExchangeRateWrapperForm|CancelButtonText", "Cancel"); }
		}

		#endregion

		public override string FormVerb
		{
			get { return ""; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				ZCalcEditColumnStyleInfo buyRateColumnStyle = (ZCalcEditColumnStyleInfo)ExchangeRateWrapperGrid.GetColumnStyle("BuyRate");
				buyRateColumnStyle.Decimals = ((BulkExchangeRateUpdater)BusinessEntity).ExchangeRateDecimalPlaces;
				ZCalcEditColumnStyleInfo sellRateColumnStyle = (ZCalcEditColumnStyleInfo)ExchangeRateWrapperGrid.GetColumnStyle("SellRate");
				sellRateColumnStyle.Decimals = ((BulkExchangeRateUpdater)BusinessEntity).ExchangeRateDecimalPlaces;
			}
		}

		#endregion
	}
}
