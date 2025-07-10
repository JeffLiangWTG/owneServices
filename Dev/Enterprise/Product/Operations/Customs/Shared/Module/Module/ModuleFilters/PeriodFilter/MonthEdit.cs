using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Module
{
	public sealed class MonthEdit : ZCalcEdit
	{
		public MonthEdit()
		{
			MaxLength = 2;
			IsCalculatorEnabled = false;
			ShowGroupSeparators = false;
		}

		public override string Text
		{
			get => base.Text;
			set => base.Text = value.PadLeft(2, '0');
		}
	}
}
