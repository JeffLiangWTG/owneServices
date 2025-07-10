using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class CompanyTariffOrCostBasedCalculatorPanel2 : CompanyTariffOrCostBasedCalculatorPanel
	{
		public CompanyTariffOrCostBasedCalculatorPanel2()
		{
			InitializeComponent();
		}

		protected override void SetBindings()
		{
			MinimumCalcEdit.Decimals = ViewCalculatorForBinding.Calculator.DecimalPlaces;
			BasePriceCalcEdit.Decimals = ViewCalculatorForBinding.Calculator.DecimalPlaces;

			base.SetBindings();

			SetBinding(BasePriceCalcEdit, "Decimal1");
			SetBinding(MinimumCalcEdit, "Decimal4");

			SetBinding(EquipmentDropEdit, "String2");
			SetListBinding(EquipmentDropEdit, "List2");

			SetBinding(MessageTypeDropEdit, "String3");
			SetListBinding(MessageTypeDropEdit, "List3");

			SetBinding(MessageSubtypeDropEdit, "String4");
			SetListBinding(MessageSubtypeDropEdit, "List4");
		}

		internal void OnSwitched(Calculator viewCalculator)
		{
			EquipmentDropEdit.Enabled = viewCalculator.ShowEquipmentType;
			EquipmentDropEdit.Visible = viewCalculator.ShowEquipmentType;
			MessageTypeSubtypeGroupBox.Enabled = viewCalculator.ShowMessageTypeSubType;
			MessageTypeSubtypeGroupBox.Visible = viewCalculator.ShowMessageTypeSubType;
		}
	}
}
