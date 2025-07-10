using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class ChargesItemTemplate : ItemTemplateControlBase
	{
		public ChargesItemTemplate()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(lblCalculatedAmountString, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCalculatedFormula, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblHandlingOffice, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			if (Data == null)
			{
				return;
			}

			cbIsActive.Enabled = Data.IsActiveEnabled;
			cbIsActive.Visible = Data.IsActiveVisibility;
			pnlError.Visible = Data.ChargeCodeErrorVisibility;
			lblCalculatedFormula.Visible = Data.CalculatedFormulaVisibility;
			lblCalculatedAmountString.Visible = Data.CalculatedFormulaVisibility;

			// As PictureBox is not bindable, we have to set it's image here.
			if (Data.ChargeCodeErrorType == ChargeCodeErrorType.Error && Data.ErrorIcon != null)
			{
				pbErrorIcon.Image = Data.ErrorIcon;
			}
			else if (Data.ChargeCodeErrorType == ChargeCodeErrorType.Warning && Data.WarningIcon != null)
			{
				pbErrorIcon.Image = Data.WarningIcon;
			}

			AdjustHeight();
		}

		void AdjustHeight()
		{
			int CalculateAppropriateHeightForLabel(ZLabel label, string text)
			{
				var result = 19;
				if (!string.IsNullOrEmpty(text))
				{
					var neededWidth = TextRenderer.MeasureText(text, label.Font).Width;
					if (neededWidth > label.Width)
					{
						result = ((neededWidth / label.Width) + 1) * 20;
					}
				}
				return result;
			}

			var appropriateHeight = Math.Max(CalculateAppropriateHeightForLabel(lblCalculatedFormula, Data?.CalculatedFormula), CalculateAppropriateHeightForLabel(lblHandlingOffice, Data?.HandlingOffice));
			var appropriateHeightScaled = ControlDpiScalingHelper.ScaleToCurrentDpiY(appropriateHeight);
			// isInStandardDpi = false because width and height have been scaled to user's DPI
			Size = ControlDpiScalingHelper.NewScaledSize(this.Width, appropriateHeightScaled, isInStandardDpi: false);
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			if (e.PropertyName == nameof(Data.IsActive))
			{
				cbIsActive.Checked = Data.IsActive;
			}
		}

		#region Tooltips
		void lblCode_MouseHover(object sender, EventArgs e)
		{
			lblCode.SetTooltip(Data?.Description);
		}

		void pbErrorIcon_MouseHover(object sender, EventArgs e)
		{
			pbErrorIcon.SetTooltip(Data?.ChargeCodeErrorToolTipText);
		}

		void lblHandlingOffice_MouseHover(object sender, EventArgs e)
		{
			lblHandlingOffice.SetTooltip(Data?.HandlingOffice);
		}

		void lblCalculatedFormula_MouseHover(object sender, EventArgs e)
		{
			lblCalculatedFormula.SetTooltip(Data?.CalculatedFormulaToolTip);
		}
		#endregion

		ChargeViewModel Data => CurrentDataItem as ChargeViewModel;
	}
}
