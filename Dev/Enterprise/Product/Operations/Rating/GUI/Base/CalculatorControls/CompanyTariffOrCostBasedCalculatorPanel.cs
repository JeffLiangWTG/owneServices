using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CompanyTariffOrCostBasedCalculatorPanel : ZUserControl, IBindTo
	{
		protected CompanyTariffOrCostBasedCalculatorPanel()
		{
			InitializeComponent();
		}

		public RateLine.CalculatorWrapper ViewCalculatorForBinding { get; set; }

		#region Binding

		public string BindTo
		{
			get { return bindTo; }
			set
			{
				if (ViewCalculatorForBinding.Calculator == null)
				{
					var nameOfCalculator = nameof(ViewCalculatorForBinding) + "." + nameof(ViewCalculatorForBinding.Calculator);
					throw new ArgumentNullException(nameOfCalculator);
				}

				bindTo = value;
				SetBindings();
			}
		}
		string bindTo;

		protected virtual void SetBindings()
		{
		}

		protected void SetBinding(IBindTo control, string propertyName)
		{
			if (!ViewCalculatorForBinding.Calculator.ContainsMapToProperty(propertyName))
			{
				ReportIncorrectMapperPropertyAccessed(propertyName);
			}

			control.BindTo = BindTo + '+' + propertyName;
		}

		protected void SetListBinding(IBindToList control, string propertyName)
		{
			control.BindToList = BindTo + '+' + propertyName;
		}

		void ReportIncorrectMapperPropertyAccessed(ZString propertyName)
		{
			ErrorReporter.ReportOnce("IncorrectRateLineItemMapper" + propertyName, "There is no mapping for property [" + propertyName + "] in calculator " + this.GetType().FullName + ".");
		}

		#endregion
	}
}
