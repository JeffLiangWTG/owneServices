using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class ChargesItemTemplate : ItemTemplateControlBase
	{
		public ChargesItemTemplate()
		{
			InitializeComponent();
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (CurrentViewModel == null)
			{
				return;
			}

			tbvLocalAmount.Text = new ZDecimal(CurrentViewModel.LocalAmount).ToString();
			tbvLocalAmount.Error = CurrentViewModel.LocalAmountError;
			tbvLocalAmount.ErrorLevel = CurrentViewModel.LocalAmountErrorLevel;

			lblAmount.Visible = !CurrentViewModel.IsIncluded;
			lblCurrency.Visible = !CurrentViewModel.IsIncluded;
			tbvLocalAmount.Visible = !CurrentViewModel.IsIncluded;
			lblLocalCurrency.Visible = !CurrentViewModel.IsIncluded;
			lblIncluded.Visible = CurrentViewModel.IsIncluded;

			lblAmount.SetTooltip(CurrentViewModel.CalculationDescription);
			lblCurrency.SetTooltip(CurrentViewModel.CalculationDescription);
			tbvLocalAmount.SetTooltip(CurrentViewModel.CalculationDescription);
			lblLocalCurrency.SetTooltip(CurrentViewModel.CalculationDescription);
			lblIncluded.SetTooltip(CurrentViewModel.CalculationDescription);

			SyncForeColors();
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			if (e.PropertyName == nameof(CurrentViewModel.IsSelected))
			{
				SyncForeColors();
			}
		}

		void SyncForeColors()
		{
			lblAmount.ForeColor = tbCharge.ForeColor;
			lblIncluded.ForeColor = tbCharge.ForeColor;
			lblCurrency.ForeColor = tbCharge.ForeColor;
			lblLocalCurrency.ForeColor = tbCharge.ForeColor;
			tbvLocalAmount.ForeColor = tbCharge.ForeColor;
		}

		ChargeViewModel CurrentViewModel => CurrentDataItem as ChargeViewModel;
	}
}
