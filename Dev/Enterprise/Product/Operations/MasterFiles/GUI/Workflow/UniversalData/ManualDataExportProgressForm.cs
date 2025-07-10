using System;
using CargoWise.BrandManager;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ManualDataExportProgressForm : ZChildForm, IManualDataExportProgressForm
	{
		public ManualDataExportProgressForm(string universalXMLTypeName = null)
		{
			this.universalXMLTypeName = universalXMLTypeName;
			InitializeComponent();
			EnterpriseLogo.Image = BrandingFactory.Instance.ProductIcon.ToBitmap();
		}

		#region IChildManualDataExportForm members

		ZForm IManualDataExportProgressForm.Form
		{
			get { return this; }
		}

		ZLabel IManualDataExportProgressForm.TitleLabel
		{
			get { return zChildTitle; }
		}

		ZTextBox IManualDataExportProgressForm.NotificationsTextBox
		{
			get { return notificationsTextBox; }
		}

		ZButton IManualDataExportProgressForm.SendButton
		{
			get { return sendButton; }
		}

		ZButton IManualDataExportProgressForm.CloseButton
		{
			get { return closeButton; }
		}

		#endregion

		public string universalXMLTypeName;

		#region Implementation

		public override string FormHeading
		{
			get { return Res.GetString("fc1ce1d0-3e35-48c5-8b8c-82259bebbe2d", "This process can take some time..."); }
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
