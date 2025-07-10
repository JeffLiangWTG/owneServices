using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class CompanyTariffOrCostBasedCalculatorPanel1 : CompanyTariffOrCostBasedCalculatorPanel
	{
		public CompanyTariffOrCostBasedCalculatorPanel1()
		{
			InitializeComponent();
		}

		protected override void SetBindings()
		{
			var decimalPlaces = ViewCalculatorForBinding.Calculator.DecimalPlaces;

			BasePriceCalcEdit.DecimalPlaces = decimalPlaces;
			BasePriceCalcEdit.Decimals = decimalPlaces;
			MinimumCalcEdit.DecimalPlaces = decimalPlaces;
			MinimumCalcEdit.Decimals = decimalPlaces;
			UnitPriceCalcEdit.DecimalPlaces = decimalPlaces;
			UnitPriceCalcEdit.Decimals = decimalPlaces;
			PercentageCalcEdit.DecimalPlaces = decimalPlaces;
			PercentageCalcEdit.Decimals = decimalPlaces;
			UnitPercentageChangeCalcEdit.DecimalPlaces = decimalPlaces;
			UnitPercentageChangeCalcEdit.Decimals = decimalPlaces;

			base.SetBindings();

			SetBinding(BasePriceCalcEdit, "Decimal1");
			SetBinding(PercentageCalcEdit, "Decimal2");
			SetBinding(UnitPriceCalcEdit, "Decimal3");
			SetBinding(MinimumCalcEdit, "Decimal4");
			SetBinding(UnitPercentageChangeCalcEdit, "Decimal5");

			SetBinding(CalculationOrderDropDown, "String1");
			SetListBinding(CalculationOrderDropDown, "List1");

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

