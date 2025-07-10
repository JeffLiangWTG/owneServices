using System;
using System.ComponentModel;
using System.Drawing;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser
{
	public partial class RateChooserCardTemplate : ItemTemplateControlBase
	{
		public RateChooserCardTemplate(ChooserContainerCommodityViewModel parentViewModel) : base()
		{
			InitializeComponent();

			ParentViewModel = parentViewModel;
			originalBackColor = BackColor;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Data != null && Visible)
			{
				pnlApply.Visible = true;

				btnApply.Visible = Data.SelectRelatedRatesApplyButtonVisibility;
				lblSelectRelatedRatesText.Text = Data.SelectRelatedRatesText;
				lblSelectRelatedRatesText.Visible = !string.IsNullOrEmpty(Data.SelectRelatedRatesText);
				cbIsSelected.Checked = Data.IsSelected;

				pnlApply.Visible = btnApply.Visible || lblSelectRelatedRatesText.Visible;
			}

#if DEBUG
			TypeDescriptor.AddAttributes(lblSelectRelatedRatesText, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		void btnApply_Click(object sender, EventArgs e)
		{
			if (Data != null)
			{
				Data.IsSelected = true;
			}

			ParentViewModel?.RaiseSelectRelatedRates();
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();
			if (Data == null)
			{
				return;
			}

			pnlApply.Visible = true;

			btnApply.Visible = Data.SelectRelatedRatesApplyButtonVisibility;
			lblSelectRelatedRatesText.Text = Data.SelectRelatedRatesText;
			lblSelectRelatedRatesText.Visible = !string.IsNullOrEmpty(Data.SelectRelatedRatesText);
			cbIsSelected.Checked = Data.IsSelected;

			pnlApply.Visible = btnApply.Visible || lblSelectRelatedRatesText.Visible;
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			pnlApply.Visible = true;

			switch (e.PropertyName)
			{
				case nameof(Data.SelectRelatedRatesApplyButtonVisibility):
					btnApply.Visible = Data.SelectRelatedRatesApplyButtonVisibility;
					break;

				case nameof(Data.SelectRelatedRatesText):
					lblSelectRelatedRatesText.Text = Data.SelectRelatedRatesText;
					lblSelectRelatedRatesText.Visible = !string.IsNullOrEmpty(Data.SelectRelatedRatesText);
					break;

				case nameof(Data.IsSelected):
					cbIsSelected.Checked = Data.IsSelected;
					break;
			}

			pnlApply.Visible = btnApply.Visible || lblSelectRelatedRatesText.Visible;
		}

		ChooserRateRow Data => CurrentDataItem as ChooserRateRow;
		ChooserContainerCommodityViewModel ParentViewModel { get; }

		void RateChooserCardTemplate_MouseHover(object sender, EventArgs e)
		{
			this.BackColor = SystemColors.GradientActiveCaption;
		}

		readonly Color originalBackColor;

		void RateChooserCardTemplate_MouseLeave(object sender, EventArgs e)
		{
			this.BackColor = originalBackColor;
		}
	}
}
