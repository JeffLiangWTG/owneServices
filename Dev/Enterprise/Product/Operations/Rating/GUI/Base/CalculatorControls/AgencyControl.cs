namespace Enterprise.Rating.GUI
{
	public partial class AgencyControl : RateCalculatorUserControl
	{
		public AgencyControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			MaximumCalcEdit.DecimalPlaces = DecimalPlaces;
			MaximumCalcEdit.Decimals = DecimalPlaces;
			AgencyRateCalcEdit.DecimalPlaces = DecimalPlaces;
			AgencyRateCalcEdit.Decimals = DecimalPlaces;
			AdditionalCostCalcEdit.DecimalPlaces = DecimalPlaces;
			AdditionalCostCalcEdit.Decimals = DecimalPlaces;
			AgencyCostPerAddlLineCalcEdit.DecimalPlaces = DecimalPlaces;
			AgencyCostPerAddlLineCalcEdit.Decimals = DecimalPlaces;

			base.SetBindings();

			SetBinding(AgencyFeeTypeDropDown, "String1");
			SetListBinding(AgencyFeeTypeDropDown, "List1");
			SetBinding(AgencyLineTypeDropDownEdit, "String2");
			SetListBinding(AgencyLineTypeDropDownEdit, "List2");

			SetBinding(MessageTypeDropDown, "String3");
			SetListBinding(MessageTypeDropDown, "List3");
			SetBinding(MessageSubTypeDropDown, "String4");
			SetListBinding(MessageSubTypeDropDown, "List4");

			SetBinding(AgencyRateCalcEdit, "Decimal1");
			SetBinding(AgencyCostPerAddlLineCalcEdit, "Decimal2");
			SetBinding(AdditionalCostCalcEdit, "Decimal5");
			SetBinding(MaximumCalcEdit, "Decimal6");

			SetBinding(AgencyFreeLinesCalcEdit, "Int1");
			SetBinding(AgencyMaxLineCalcEdit, "Int2");
			SetBinding(IncludedCalcEdit, "Int3");

			SetBinding(HideFeeLineTypeOnQuoteCheckBox, "Bool1");
			SetBinding(HideMessageTypeOnQuoteCheckBox, "Bool2");
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

	}
}

