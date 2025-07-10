using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters;
using Enterprise.Freight.Forwarding.Documents.GUI.Actions;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.GUI.DocSending
{
	public partial class SupportingDocForm : ZChildForm, ISupportingDocForm
	{
		readonly ISupportingDocDataObject supportingDocDataObject;

		public SupportingDocForm(ISupportingDocDataObject supportingDocDataObject)
			: base(supportingDocDataObject.DocSendingCollection)
		{
			this.supportingDocDataObject = supportingDocDataObject;

			InitializeComponent();
		}

		ZDialogResult ISupportingDocForm.ShowDialogAndGetResult()
		{
			ZFormModaliser.ShowDialogWithoutDispose(this);
			return (ZDialogResult)DialogResult;
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		async void OKButton_Click(object sender, EventArgs e)
		{
			if (!DocumentsDataRegistry.Instance.EnableCertOfOriginIndemnity.Value)
			{
				DialogResult = DialogResult.OK;
				Close();
				return;
			}

			var term = await ObjectFactory.Get<ICertificateOfOriginIndemnityTermsAgreementChecker>().CheckTermAcknowledged(this);

			supportingDocDataObject.AgreementInfo.HasBeenAcknowledged = term.HasBeenAcknowledged;
			supportingDocDataObject.AgreementInfo.VersionNo = term.VersionNo;

			if (term.HasBeenAcknowledged)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
