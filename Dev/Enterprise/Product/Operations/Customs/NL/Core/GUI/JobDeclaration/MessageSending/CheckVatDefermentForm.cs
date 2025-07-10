using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class CheckVatDefermentForm : ZChildForm
{
	public CheckVatDefermentForm(JobDeclaration declaration) : base(declaration)
	{
		InitializeComponent();
	}

	void SubmitButton_Click(object sender, EventArgs e)
	{
		if (ZString.Empty.Equals(VATPartyTaxNumberTextBox.Text) ||
			ZString.Empty.Equals(PaymentPartyEoriNumberTextBox.Text))
		{
			Globals.Message.Show(Res.GetString("96C4BFC1-A057-43B2-BD45-7EAAC765D35B", "'Payment Party EORI Number' AND 'VAT Party Tax Number' should be filled when sending the message."),
								 Res.GetString("94B9D28D-7B40-41A1-BF9A-51E3EC597F4A", "Missing information"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
