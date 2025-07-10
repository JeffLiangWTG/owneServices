using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class AdditionalSupTariffsUserControl : ZUserControl
	{
		public AdditionalSupTariffsUserControl()
		{
			InitializeComponent();
			OnSupTariffFormattedFieldTypeChanged(false);

			SupAdditionalTariff1GoodsValueCalcDropEdit.Visible = false;
			SupAdditionalTariff2GoodsValueCalcDropEdit.Visible = false;
			SupAdditionalTariff3GoodsValueCalcDropEdit.Visible = false;
			SupAdditionalTariff4GoodsValueCalcDropEdit.Visible = false;
			SupAdditionalTariff5GoodsValueCalcDropEdit.Visible = false;
		}

		public void OnSupTariffFormattedFieldTypeChanged(bool shouldUseDropEdit)
		{
			SupAdditionalTariff1DropEdit.Visible = shouldUseDropEdit;
			SupAdditionalTariff2DropEdit.Visible = shouldUseDropEdit;
			SupAdditionalTariff3DropEdit.Visible = shouldUseDropEdit;
			SupAdditionalTariff4DropEdit.Visible = shouldUseDropEdit;
			SupAdditionalTariff5DropEdit.Visible = shouldUseDropEdit;
			SupAdditionalTariff1FindBox.Visible = !shouldUseDropEdit;
			SupAdditionalTariff2FindBox.Visible = !shouldUseDropEdit;
			SupAdditionalTariff3FindBox.Visible = !shouldUseDropEdit;
			SupAdditionalTariff4FindBox.Visible = !shouldUseDropEdit;
			SupAdditionalTariff5FindBox.Visible = !shouldUseDropEdit;
		}
	}
}
