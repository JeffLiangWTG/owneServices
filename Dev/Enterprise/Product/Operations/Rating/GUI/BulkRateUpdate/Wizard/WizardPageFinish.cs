using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Enterprise.Rating.GUI
{
	public partial class WizardPageFinish : WizardPage
	{
		#region Properties

		public string Title
		{
			get { return titleLabel.Text; }
			set { titleLabel.Text = value; }
		}

		public string Description
		{
			get { return descriptionLabel.Text; }
			set { descriptionLabel.Text = value; }
		}

		public Image Image
		{
			get;
			set;
		}

		#endregion

		#region Ctor

		public WizardPageFinish()
			: base()
		{
			InitializeComponent();

#if !WINZOR
			imageBox.Paint += new PaintEventHandler(OnPbImagePaint);
#endif
		}

		#endregion

		#region Overrides

		public override void NotifyActivated(WizardForm wizard)
		{
			wizard.PageHeaderVisible = false;
			wizard.PageHeaderTitle = wizard.PageHeaderDescription = string.Empty;
		}

		public override void NotifyLeaving(WizardSteppingEventArgs args)
		{
		}

		#endregion

		#region Event Handlers

#if !WINZOR

		void OnPbImagePaint(object sender, PaintEventArgs e)
		{
			LinearGradientBrush backgroundBrush = new LinearGradientBrush(imageBox.Bounds, SystemColors.ActiveCaption, SystemColors.ControlLight, 90.0f, false);
			e.Graphics.FillRectangle(backgroundBrush, imageBox.Bounds);
			e.Graphics.DrawRectangle(SystemPens.WindowText, imageBox.Bounds);

			if (this.Image != null)
			{
				e.Graphics.DrawImage(this.Image, 14, (imageBox.Bottom / 2) - (this.Image.Height / 2));
			}
		}

#endif

		#endregion
	}
}

