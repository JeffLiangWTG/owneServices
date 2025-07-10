using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class StampDutyLedgerDocumentEntryForm : ZChildForm
	{
		public StampDutyLedgerDocumentEntryForm(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
			InitializeComponent();
			this.statementHeader = statementHeader;
			GetDefaultValue();
		}
		readonly CusStatementHeader statementHeader;

		void GetDefaultValue()
		{
			PrintDateDateEdit.DateTimeValue = ZDateTime.Today;
		}

		void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		void btnOk_Click(object sender, EventArgs e)
		{
			if (statementHeader != null)
			{
				var regDate = PrintDateDateEdit.DateTimeValue;
				if (regDate.IsEmpty && !regDate.IsValid)
				{
					MessageBox.Show(
						Res.GetString("1A239C6E-0F50-44A4-8D11-6A33FEF315C8", "Print Date is invalid, please correct it."),
						Res.GetString("CDD74B34-5A8C-45CB-A9E2-B4E339EC3286", "Manual Print Date Entry"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
						);
					return;
				}

				statementHeader.B2_PrintDate = PrintDateDateEdit.DateTimeValue;

				if (statementHeader.HasChanges)
				{
					FireSaveButton();
				}

				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
