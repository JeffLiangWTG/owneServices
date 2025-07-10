using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class LicensingFoodAndDrugUserControl : ZUserControl
	{
		public LicensingFoodAndDrugUserControl()
		{
			InitializeComponent();
			AddNumericEvents(TW_SterilizationValueCalcEdit);
			AddNumericEvents(TW_PHValueCalcEdit);
		}

		void AddNumericEvents(ZArchitecture.ZTextBox textBox)
		{
			textBox.TextChanged -= Numeric_TextChanged;
			textBox.TextChanged += Numeric_TextChanged;
		}

		void Numeric_TextChanged(object sender, System.EventArgs e)
		{
			var calcEdit = sender as ZArchitecture.ZTextBox;
			if (ZDecimal.TryParse(calcEdit.Text, out var number) && number < -9.9m)
			{
				calcEdit.Text = ZString.Empty;
			}
		}
	}
}
