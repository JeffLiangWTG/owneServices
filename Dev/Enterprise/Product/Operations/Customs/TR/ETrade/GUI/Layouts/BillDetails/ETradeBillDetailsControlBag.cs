using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public class ETradeBillDetailsControlBag : ControlBag
	{
		public static ETradeBillDetailsControlBag Instance => billDetailsControlBag.Value;

		public ETradeBillDetailsControlBag()
		{
			PrecedentFreightCostLocalCurrencyControl = RegisterControl(nameof(ETradeBillDetailsUserControl.PrecedentFreightCostLocalCurrencyControl));
			ShipmentTypeDropEdit = RegisterControl(nameof(ETradeBillDetailsUserControl.ShipmentTypeDropEdit));
			NatureOfBusinessDropEdit = RegisterControl(nameof(ETradeBillDetailsUserControl.NatureOfBusinessDropEdit));
			ExemptionCode1DropEdit = RegisterControl(nameof(ETradeBillDetailsUserControl.ExemptionCode1DropEdit));
			ExemptionCode2DropEdit = RegisterControl(nameof(ETradeBillDetailsUserControl.ExemptionCode2DropEdit));
			GuaranteeTypeDropEdit = RegisterControl(nameof(ETradeBillDetailsUserControl.GuaranteeTypeDropEdit));
			GuaranteeRefNoTextBox = RegisterControl(nameof(ETradeBillDetailsUserControl.GuaranteeRefNoTextBox));
			GuaranteeAmountCalcEdit = RegisterControl(nameof(ETradeBillDetailsUserControl.GuaranteeAmountCalcEdit));
			DepartureCountryCodeFindBox = RegisterControl(nameof(ETradeBillDetailsUserControl.DepartureCountryCodeFindBox));
			TradeCountryCodeFindBox = RegisterControl(nameof(ETradeBillDetailsUserControl.TradeCountryCodeFindBox));
			ExportCountryCodeFindBox = RegisterControl(nameof(ETradeBillDetailsUserControl.ExportCountryCodeFindBox));
			ArrivalCountryCodeFindBox = RegisterControl(nameof(ETradeBillDetailsUserControl.ArrivalCountryCodeFindBox));
			PaymentMethodDropEdit = RegisterControl(nameof(ETradeBillDetailsUserControl.PaymentMethodDropEdit));
			AccountantTextBox = RegisterControl(nameof(ETradeBillDetailsUserControl.AccountantTextBox));
			AccountantVATTextBox = RegisterControl(nameof(ETradeBillDetailsUserControl.AccountantVATTextBox));
			OtherValueCalcFindBox = RegisterControl(nameof(ETradeBillDetailsUserControl.OtherValueCalcFindBox));
			ContainerNumberDropEdit = RegisterControl(nameof(ETradeBillDetailsUserControl.ContainerNumberDropEdit));
			GoodsValueConvertToLocalCurrencyControl = RegisterControl(nameof(ETradeBillDetailsUserControl.GoodsValueConvertToLocalCurrencyControl));
			SeparatedCheckBox = RegisterControl(nameof(ETradeBillDetailsUserControl.SeparatedCheckBox));
		}

		public ControlReference PrecedentFreightCostLocalCurrencyControl { get; }
		public ControlReference ShipmentTypeDropEdit { get; }
		public ControlReference NatureOfBusinessDropEdit { get; }
		public ControlReference ExemptionCode1DropEdit { get; }
		public ControlReference ExemptionCode2DropEdit { get; }
		public ControlReference GuaranteeTypeDropEdit { get; }
		public ControlReference GuaranteeRefNoTextBox { get; }
		public ControlReference GuaranteeAmountCalcEdit { get; }
		public ControlReference DepartureCountryCodeFindBox { get; }
		public ControlReference TradeCountryCodeFindBox { get; }
		public ControlReference ExportCountryCodeFindBox { get; }
		public ControlReference ArrivalCountryCodeFindBox { get; }
		public ControlReference ABL_IncotermDropEdit { get; }
		public ControlReference PaymentMethodDropEdit { get; }
		public ControlReference AccountantTextBox { get; }
		public ControlReference AccountantVATTextBox { get; }
		public ControlReference OtherValueCalcFindBox { get; }
		public ControlReference ContainerNumberDropEdit { get; }
		public ControlReference GoodsValueConvertToLocalCurrencyControl { get; }
		public ControlReference SeparatedCheckBox { get; }

		protected override Control CreateTemplate() => new ETradeBillDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<ETradeBillDetailsControlBag> billDetailsControlBag = new Lazy<ETradeBillDetailsControlBag>(() => new ETradeBillDetailsControlBag());
	}
}
