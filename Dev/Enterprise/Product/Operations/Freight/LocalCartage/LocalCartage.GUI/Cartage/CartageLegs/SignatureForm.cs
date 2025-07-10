using System;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class SignatureForm : ZChildForm
	{
		public SignatureForm(CommonCartageLeg leg)
			: base()
		{
			InitializeComponent();
			var drawer = new SignatureDrawer(signaturePictureBox, false);
			drawer.Leg = leg;
			Hook();
		}

		void Hook()
		{
			signaturePictureBox.Click += new EventHandler(Close_Click);
			this.Click += new EventHandler(Close_Click);
		}

		void UnHook()
		{
			signaturePictureBox.Click -= new EventHandler(Close_Click);
			this.Click -= new EventHandler(Close_Click);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHook();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void Close_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
