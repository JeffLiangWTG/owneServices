using System.ComponentModel;
using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class ChargeToggleButton : ItemTemplateControlBase
	{
		public ChargeToggleButton()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(lblChargeCode, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (CurrentViewModel == null)
			{
				return;
			}

			lblChargeCode.Text = CurrentViewModel.ChargeCode;

			picErrorWarning.Visible = true;
			picErrorWarning.SetTooltip(CurrentViewModel.ChargeCodeError);
			switch (CurrentViewModel.ErrorLevel)
			{
				case ErrorLevel.Error:
					picErrorWarning.Image = Properties.Resources.ErrorDrawing;
					break;
				case ErrorLevel.Warning:
					picErrorWarning.Image = Properties.Resources.WarningDrawing;
					break;
				case ErrorLevel.None:
					picErrorWarning.Visible = false;
					break;
			}

			ForeColor = Color.Black;
			if (CurrentViewModel.IsOptional && !CurrentViewModel.IsSelected)
			{
				ForeColor = Color.DimGray;
			}
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			ApplyExtraStaticOneWayBindings();
		}

		ChargeViewModel CurrentViewModel => (ChargeViewModel)CurrentDataItem;

		void SizeToFit()
		{
			var ctrlSize = ControlDpiScalingHelper.NewScaledSize(lblChargeCode.Size.Width, Size.Height, isInStandardDpi: false);

			if (picErrorWarning.Visible)
			{
				ctrlSize = ControlDpiScalingHelper.NewScaledSize(ctrlSize.Width + picErrorWarning.Size.Width, ctrlSize.Height, isInStandardDpi: false);
				picErrorWarning.Location = ControlDpiScalingHelper.NewScaledPoint(lblChargeCode.Size.Width, picErrorWarning.Location.Y, isInStandardDpi: false);
			}

			Size = ctrlSize;
		}

		void lblChargeCode_SizeChanged(object sender, System.EventArgs e)
		{
			SizeToFit();
		}

		void picErrorWarning_VisibleChanged(object sender, System.EventArgs e)
		{
			SizeToFit();
		}

		protected void lblChargeCode_MouseHover(object sender, System.EventArgs e)
		{
			lblChargeCode.SetTooltip(CurrentViewModel.ChargeCodeDescription);
		}

		void lblChargeCode_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (CurrentViewModel.IsOptional)
			{
				CurrentViewModel.IsSelected = !CurrentViewModel.IsSelected;
			}
		}
	}
}
