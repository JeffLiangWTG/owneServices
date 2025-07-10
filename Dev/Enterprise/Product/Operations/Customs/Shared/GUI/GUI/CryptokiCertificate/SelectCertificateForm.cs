using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Certificates
{
	public partial class SelectCertificateForm : ZChildForm
	{
		public SelectCertificateForm(IReadOnlyList<CryptokiCertificate> certificates)
			: base(CryptokiCertificateCollection.Wrap(certificates))
		{
			InitializeComponent();
		}

		public CryptokiCertificate SelectedCertificate { get; protected set; }

		void CertificatesGrid_DoubleClick(object sender, EventArgs e)
		{
			AcceptSelection();
		}

		void ChooseButton_Click(object sender, EventArgs e)
		{
			AcceptSelection();
		}

		void AcceptSelection()
		{
			var currentRow = CertificatesGrid.ListManager.GetCurrent();
			if (CertificatesGrid.SelectedElements.Length <= 1 && currentRow != null)
			{
				SelectedCertificate = (CryptokiCertificate)currentRow;
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CanceledButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
