namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class ChargeViewModelSample : ChargeViewModel
	{
		public ChargeViewModelSample()
		{
			ChargeCode = "FRT";
			ChargeCodeErrorLevel = ErrorLevel.None;
			IsSelected = true;
		}

		public new bool DisplayPrice { get; set; }
		public override ErrorLevel ErrorLevel => ChargeCodeErrorLevel;
	}
}
