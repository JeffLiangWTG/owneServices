using System;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class AdditionalTranCircumstancesForm : ZChildForm
{
	public AdditionalTranCircumstancesForm(TranCircumstanceCollection tranCircumstances)
		: base(tranCircumstances)
	{
		SetTranCircumstanceColumnStyleInfo();
	}

	public override string FormVerb => "";

	public static void ShowDialog(TranCircumstanceCollection tranCircumstances)
	{
		ZFormModaliser.ShowDialogAndDispose(new AdditionalTranCircumstancesForm(tranCircumstances));
	}

	void SetTranCircumstanceColumnStyleInfo()
	{
		var tranCircumstance = new ZMultiControlColumnStyleInfo();
		tranCircumstance.CaptionResourceString = Res.GetData("5C08CD82-DCA7-4474-AC81-E738D020A817", "Tran. Circumstance");
		tranCircumstance.ColumnName = TranCircumstance.Schema.CY_Code;
		tranCircumstance.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
		tranCircumstance.FieldTypeColumnName = "TranCircumstanceFieldType";

		AdditionalTranCircumstanceGrid.ColumnStyles.Add(tranCircumstance);
	}

	void OnCloseButton_Click(object sender, EventArgs e)
	{
		Close();
	}
}
