using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

partial class SupplementaryCodesUserControl : ZUserControl
{
	public SupplementaryCodesUserControl()
	{
		InitializeComponent();
	}

	void AdditionalSupplementaryCodesEditButton_Click(object sender, EventArgs e)
	{
		if (CurrentDataItem is ISupplementaryCodeSupporter supplementaryCodeSupporter)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdditionalSupplementaryCodesForm(supplementaryCodeSupporter));
		}
	}
}
