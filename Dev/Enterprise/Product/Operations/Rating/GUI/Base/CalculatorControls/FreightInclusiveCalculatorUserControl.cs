using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class FreightInclusiveCalculatorUserControl : RateCalculatorUserControl
	{
		public FreightInclusiveCalculatorUserControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			base.SetBindings();

			SetBinding(FreightInclusiveCalculatorTypeDropDown, "String1");
			SetListBinding(FreightInclusiveCalculatorTypeDropDown, "List1");
			SetBinding(PreCarriageOnCarriageChargeFindBox, CalculatorConstants.MapTo.ChargeCode);
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		ZArchitecture.GUI.ZDropEditWithFixedWidth FreightInclusiveCalculatorTypeDropDown;
		ZArchitecture.GUI.ZGuidFindBox PreCarriageOnCarriageChargeFindBox;
		ZArchitecture.ZLabel descriptionLabel;
	}
}
