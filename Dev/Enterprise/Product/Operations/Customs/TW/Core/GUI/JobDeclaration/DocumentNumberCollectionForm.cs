using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class DocumentNumberCollectionForm : ZChildForm
	{
		public DocumentNumberCollectionForm(CusEntryInstructionDocumentCollection documentNumbers)
		: base(documentNumbers)
		{
			this.DocumentNumbers = documentNumbers;
		}

		public readonly CusEntryInstructionDocumentCollection DocumentNumbers;

		public static void ShowDialog(CusEntryInstructionDocumentCollection documentNumbers)
		{
			ZFormModaliser.ShowDialogAndDispose(new DocumentNumberCollectionForm(documentNumbers));
		}

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Loading

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			fOldItems = DocumentNumbers.DocumentNumbersAsString;
		}

		ZString fOldItems;

		#endregion

		#region Closing

		protected override void OnClosing(CancelEventArgs e)
		{
			CloseButton.Focus();

			if (DialogResult == DialogResult.Cancel)
			{
				DocumentNumbers.DocumentNumbersAsString = fOldItems;
			}
			else
			{
				this.BusinessEntity.RunPreSaveValidation();
				foreach (var item in DocumentNumbers)
				{
					if (item.NotificationsIncludingChildren.GetErrors().Count() > 0)
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("668D2F9E-02C0-4ACE-9651-E7EDAA5510A9", "The form has errors. Please fix them before continuing."));
						e.Cancel = true;
						break;
					}
				}
			}

			base.OnClosing(e);
		}

		#endregion

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Buttons

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
