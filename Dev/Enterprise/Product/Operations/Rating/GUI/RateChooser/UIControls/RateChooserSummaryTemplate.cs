using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class RateChooserSummaryTemplate : ItemTemplateControlBase
	{
		public RateChooserSummaryTemplate(RateChooserViewModel parentViewModel)
		{
			InitializeComponent();
			ParentViewModel = parentViewModel;

			if (!DesignMode)
			{
				this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

				pnlCardInfoContainer.AutoSize = true;
				pnlCardInfoContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			}

#if DEBUG
			TypeDescriptor.AddAttributes(lblCommodityCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblContainerType, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCommodityLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblContainerTypeLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblNoRateSelectedLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		void pnlEmptyRow_MouseUp(object sender, MouseEventArgs e)
		{
			ParentViewModel.SummaryRowClicked(Data);
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (Data != null)
			{
				pnlEmptyRow.Visible = Data.EmptyCardVisibility;
				pnlSelection.Visible = Data.RemoveVisibility;
				pnlCardInfoContainer.Visible = Data.SummaryCardVisibility;
			}
			else
			{
				pnlCardInfoContainer.Visible = false;
				pnlEmptyRow.Visible = true;
				pnlSelection.Visible = false;
			}
		}

		RateChooserViewModel ParentViewModel { get; }

		ChooserRateRow Data => CurrentDataItem as ChooserRateRow;

		void cbSelected_CheckedChanged(object sender, EventArgs e)
		{
			if (Data != null)
			{
				ParentViewModel.UnselectRateFromSummary(Data);
			}
		}
	}
}
