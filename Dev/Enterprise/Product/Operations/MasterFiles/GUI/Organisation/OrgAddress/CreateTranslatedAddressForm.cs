using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CreateTranslatedAddressForm : ZChildForm
	{
		public CreateTranslatedAddressForm(AddressMapper mapper, bool shouldValidateAddress)
			: base(mapper)
		{
			AddressDetailsControlWithLanguage.Address = mapper.Address1;
			AddressDetailsControlWithLanguage2.Address = mapper.Address2;
			AddressDetailsControlWithLanguage2.LanguageControlReadonly = true;
			AddressDetailsControlWithLanguage2.CountryControl.ReadOnly = true;

			if (!DesignModeFinder.IsDesigning && !shouldValidateAddress)
			{
				AddressDetailsControlWithLanguage.ValidateButton.Visible = false;
				AddressDetailsControlWithLanguage2.ValidateButton.Visible = false;
			}

#if !WINZOR
			AddressGroupBox.Paint += PaintBorderlessGroupBox;
			AddressGroupBox2.Paint += PaintBorderlessGroupBox;
#endif
			AddressMapper = mapper;
		}

		readonly AddressMapper AddressMapper;

#if !WINZOR

		void PaintBorderlessGroupBox(object sender, PaintEventArgs e)
		{
			GroupBox box = sender as GroupBox;
			DrawGroupBox(box, e.Graphics, Color.Black, Color.White);
		}

		void DrawGroupBox(GroupBox box, Graphics g, Color textColor, Color borderColor)
		{
			if (box != null)
			{
				using (var textBrush = new SolidBrush(textColor))
				using (var borderBrush = new SolidBrush(borderColor))
				using (var borderPen = new Pen(borderBrush))
				{
					var strSize = g.MeasureString(box.Text, box.Font);
					var rect = new Rectangle(
						ControlDpiScalingHelper.ScaleToCurrentDpiX(box.ClientRectangle.X),
						ControlDpiScalingHelper.ScaleToCurrentDpiY(box.ClientRectangle.Y + (int)(strSize.Height / 2)),
						ControlDpiScalingHelper.ScaleToCurrentDpiX(box.ClientRectangle.Width - 1),
						ControlDpiScalingHelper.ScaleToCurrentDpiY(box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1));

					// Clear text and border
					g.Clear(this.BackColor);

					// Draw text
					g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left, 0);

					// Drawing Border
					//Left
					g.DrawLine(borderPen, rect.Location, ControlDpiScalingHelper.NewScaledPoint(rect.X, rect.Y + rect.Height));
					//Right
					g.DrawLine(borderPen, ControlDpiScalingHelper.NewScaledPoint(rect.X + rect.Width, rect.Y),
						ControlDpiScalingHelper.NewScaledPoint(rect.X + rect.Width, rect.Y + rect.Height));
					//Bottom
					g.DrawLine(borderPen, ControlDpiScalingHelper.NewScaledPoint(rect.X, rect.Y + rect.Height),
						ControlDpiScalingHelper.NewScaledPoint(rect.X + rect.Width, rect.Y + rect.Height));
					//Top1
					g.DrawLine(borderPen, ControlDpiScalingHelper.NewScaledPoint(rect.X, rect.Y),
						ControlDpiScalingHelper.NewScaledPoint(rect.X + box.Padding.Left, rect.Y));
					//Top2
					g.DrawLine(borderPen, ControlDpiScalingHelper.NewScaledPoint(rect.X + box.Padding.Left + (int)(strSize.Width), rect.Y),
						new Point(rect.X + rect.Width, rect.Y));
				}
			}
		}

#endif

		void SwitchButton_Click(object sender, EventArgs e)
		{
			var address = AddressMapper.Address1;
			AddressMapper.Address1 = AddressMapper.Address2;
			AddressMapper.Address2 = address;
			AddressMapper.RefreshBinding();
			if (!AddressDetailsControlWithLanguage.LanguageControlReadonly)
			{
				AddressDetailsControlWithLanguage.LanguageControlReadonly = true;
				AddressDetailsControlWithLanguage.CountryControl.ReadOnly = true;
				AddressDetailsControlWithLanguage2.LanguageControlReadonly = false;
				AddressDetailsControlWithLanguage2.CountryControl.ReadOnly = false;
			}
			else
			{
				AddressDetailsControlWithLanguage2.LanguageControlReadonly = true;
				AddressDetailsControlWithLanguage2.CountryControl.ReadOnly = true;
				AddressDetailsControlWithLanguage.LanguageControlReadonly = false;
				AddressDetailsControlWithLanguage.CountryControl.ReadOnly = false;
			}
		}
	}
}
