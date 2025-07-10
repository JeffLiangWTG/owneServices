using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.Rating.GUI
{
	public partial class WizardPageStart : WizardPage
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

		public WizardPageStart()
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
			e.Graphics.FillRectangle(Brushes.White, imageBox.Bounds);
			e.Graphics.DrawRectangle(SystemPens.WindowText, imageBox.Bounds);

			if (Image != null)
			{
				e.Graphics.DrawImage(Image, 14, (imageBox.Bottom / 2) - (Image.Height / 2));
			}
		}

#endif

		#endregion
	}
}

