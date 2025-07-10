using System;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class CarInfoUserControl : ZUserControl
{
	public CarInfoUserControl()
	{
		InitializeComponent();
	}

	void CarMakeModelLookUpButton_Click(object sender, EventArgs e)
	{
		if (CurrentDataItem is JobComInvoiceLine invoiceLine)
		{
			var helper = new CarModelFindBox(invoiceLine, (ZForm)ParentForm);
			helper.ShowModule();
		}
	}
}
