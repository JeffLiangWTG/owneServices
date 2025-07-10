using System;
using System.Windows.Forms;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class CheckGuaranteeAmountForm : ZChildForm
{
	public CheckGuaranteeAmountForm(JobDeclaration declaration) : base(declaration)
	{
		this.declaration = declaration;
		InitializeComponent();
	}
	readonly JobDeclaration declaration;

	void SubmitButton_Click(object sender, EventArgs e)
	{
		if (declaration.PaymentPartyEORINumber.IsEmpty)
		{
			Globals.Message.Show(Res.GetString("20BDCEBB-F35D-40C1-AE1C-0B25F9634253", "'Payer ID' should be filled when sending the message."),
								 Res.GetString("EEA19AD0-0B7E-44C8-B35E-5D26B233381B", "Missing information"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		else
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	void CancelButton_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}
}
