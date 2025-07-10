using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class TranCircumstancesUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
{
	public TranCircumstancesUserControl()
	{
		InitializeComponent();
		Extensions = new DefaultControlExtensionCollection(this);
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		Extensions.SetDataBinding(dataSource, dataMember);
	}

	public Control Host => this;

	public IControlExtensionCollection Extensions { get; }

	public string ResourceStringBindingMember => nameof(JobComInvoiceHeader.TranCircumstanceCode1);

	protected void AdditionalTranCircumstancesEditButton_Click(object sender, EventArgs e)
	{
		if (Invoice is JobComInvoiceHeader invoice)
		{
			AdditionalTranCircumstancesForm.ShowDialog(invoice.AdditionalTranCircumstanceCodes);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Extensions.Dispose();
		}
		base.Dispose(disposing);
	}

	JobComInvoiceHeader Invoice => CurrentDataItem as JobComInvoiceHeader;
}
