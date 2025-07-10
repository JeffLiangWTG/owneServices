using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public abstract partial class BaseCombinedCalculatorControl : RateCalculatorUserControl
	{
		protected BaseCombinedCalculatorControl()
		{
			InitializeComponent();

			UseAccumulatedCheckbox.CheckedChanged += UseAccumulatedCheckbox_CheckedChanged;
			BreaksPerDropDown.AllowOutsideOfParent();
		}

		void UseAccumulatedCheckbox_CheckedChanged(object sender, System.EventArgs e)
		{
			UseInclusiveBreaksCheckBox.Visible = !UseAccumulatedCheckbox.Checked;
		}

		#region Binding

		protected override void SetBindings()
		{
			base.SetBindings();

			SetBinding(UseAccumulatedCheckbox, "Bool1");
			SetBinding(HigherChargeableLowerRateCheckBox, "Bool2");
			SetBinding(UseInclusiveBreaksCheckBox, "Bool3");
			SetBinding(BreaksPerDropDown, "String3");
			SetListBinding(BreaksPerDropDown, "List3");
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

