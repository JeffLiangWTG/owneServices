using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Rating.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class CompanyTariffOrCostBasedCalculatorControl : RateCalculatorUserControl
	{
		public CompanyTariffOrCostBasedCalculatorControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			base.SetBindings();

			Panel1.ViewCalculatorForBinding = ViewCalculatorForBinding;
			Panel2.ViewCalculatorForBinding = ViewCalculatorForBinding;
			Panel1.BindTo = BindTo;
			Panel2.BindTo = BindTo;

			if (DataSource != null)
			{
				SetDataSourceBinding("IsSliding", BindTo.Replace("ViewCalculator", "ViewCalculator+IsSlidingForBinding"));
			}
		}

		internal override ZGrid GetRateLineItemsGrid()
		{
			return Panel2.RateLineItemsGrid;
		}

		internal override void OnSwitched(Calculator viewCalculator)
		{
			base.OnSwitched(viewCalculator);

			Panel1.OnSwitched(viewCalculator);
			Panel2.OnSwitched(viewCalculator);
		}

		public bool IsSliding
		{
			get { return isSliding ?? false; }
			set
			{
				isSliding = value;

				if (Panel1.Enabled != !value)
				{
					Panel1.Enabled = !value;
					Panel1.Visible = !value;
				}

				if (Panel2.Enabled != value)
				{
					Panel2.Enabled = value;
					Panel2.Visible = value;
				}

				if (ByWeightBreakButton.Checked != value)
				{
					ByWeightBreakButton.Checked = value;
				}
			}
		}

		bool? isSliding;

		void ByWeightBreakButton_CheckedChanged(object sender, EventArgs e)
		{
			IsSliding = ByWeightBreakButton.Checked;

			KBinding binding = DataBindings[nameof(IsSliding)] as KBinding;
			if (binding != null)
			{
				binding.WriteValue();
			}
		}

		#endregion

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<CompanyTariffOrCostBasedCalculatorControl>()
			.Property("IsSliding", false, false)
			.Result;
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
