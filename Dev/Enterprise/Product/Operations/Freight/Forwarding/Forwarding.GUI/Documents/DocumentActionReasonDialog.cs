using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	[SuppressFormsLocalizedTest]
	partial class DocumentActionReasonDialog : ZChildForm
	{
		// for the designer only
		DocumentActionReasonDialog()
		{
			InitializeComponent();
		}

		public DocumentActionReasonDialog(string message, string caption, ICodeDescriptionPairList optionsList)
			: base(new DocumentActionReasonModel(optionsList))
		{
			InitializeComponent();

			model = (DocumentActionReasonModel)DataSource;

			FormHeading = caption;
			messageLabel.Text = message;
		}

		public readonly DocumentActionReasonModel model;

		public override string FormHeading { get; }

		public ICodeDescription ReasonCode
		{
			get
			{
				if (model == null)
				{
					return null;
				}

				var reasonCode = model.ReasonCode;

				foreach (var reason in model.ReasonCodesList.OfType<ICodeDescription>())
				{
					if (string.Compare(reason.Code, reasonCode, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return reason;
					}
				}

				return null;
			}
		}

		void OnSendButtonClick(object sender, EventArgs e)
		{
			model.RunPreSaveValidation();

			if (model.HasErrors)
			{
				using (var errorMessageBox = new ZErrorMessageBox(model))
				{
					ZFormModaliser.ShowDialogAndDispose(errorMessageBox, this);
				}
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void OnCancelButtonClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
