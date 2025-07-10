using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class TextWithValidation : ZUserControl
	{
		public TextWithValidation()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(lblText, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		[Browsable(false)]
		public new string Text
		{
			get { return lblText.Text; }
			set
			{
				lblText.Text = value;
				lblText.RealignCenter(this);
			}
		}

		[Browsable(false)]
		public string Error
		{
			get { return error; }
			set
			{
				if (error != value)
				{
					error = value;
					pbValidationImage.SetTooltip(error);
				}
			}
		}

		string error;

		[Browsable(false)]
		public ErrorLevel ErrorLevel
		{
			get { return errorLevel; }
			set
			{
				if (errorLevel != value)
				{
					errorLevel = value;

					switch (errorLevel)
					{
						case ErrorLevel.None:
							pbValidationImage.Visible = false;
							break;

						case ErrorLevel.Warning:
							pbValidationImage.Visible = !string.IsNullOrWhiteSpace(Error);
							pbValidationImage.Image = Properties.Resources.WarningDrawing;
							break;

						case ErrorLevel.Error:
							pbValidationImage.Visible = !string.IsNullOrWhiteSpace(Error);
							pbValidationImage.Image = Properties.Resources.ErrorDrawing;
							break;
					}
				}
			}
		}

		ErrorLevel errorLevel;

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(OFontTypes.Normal)]
		public OFontTypes FontType
		{
			get
			{
				return OFont.FindFontType(lblText.Font);
			}
			set
			{
				lblText.Font = OFont.GetFont(value);

				if (value == OFontTypes.Largest)
				{
					pbValidationImage.Size = ControlDpiScalingHelper.NewScaledSize(20, 20, true);
				}
				else
				{
					pbValidationImage.Size = ControlDpiScalingHelper.NewScaledSize(15, 15, true);
				}

				lblText.RealignCenter(this);
				pbValidationImage.RealignCenter(this);
			}
		}
	}
}
