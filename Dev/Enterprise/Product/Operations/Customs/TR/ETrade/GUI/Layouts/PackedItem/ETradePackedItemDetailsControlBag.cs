using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public sealed class ETradePackedItemDetailsControlBag : ControlBag
	{
		public static ETradePackedItemDetailsControlBag Instance => packedItemDetailsControlBag.Value;

		public ETradePackedItemDetailsControlBag()
		{
			BanderolTariffFindBox = RegisterControl(nameof(ETradePackedItemDetailsUserControl.BanderolTariffFindBox));
			SerialNoTextBox = RegisterControl(nameof(ETradePackedItemDetailsUserControl.SerialNoTextBox));
			UsedGoodsCodeTextBox = RegisterControl(nameof(ETradePackedItemDetailsUserControl.UsedGoodsCodeTextBox));
			StatisticalValueCalcFindBox = RegisterControl(nameof(ETradePackedItemDetailsUserControl.StatisticalValueCalcFindBox));
			AgriculturePolicyTextBox = RegisterControl(nameof(ETradePackedItemDetailsUserControl.AgriculturePolicyTextBox));
			ValueDeclarationFormTextBox = RegisterControl(nameof(ETradePackedItemDetailsUserControl.ValueDeclarationFormTextBox));
			CalculationMethodTextBox = RegisterControl(nameof(ETradePackedItemDetailsUserControl.CalculationMethodTextBox));
			QuotaCheckBox = RegisterControl(nameof(ETradePackedItemDetailsUserControl.QuotaCheckBox));
			TariffAdditionalCodeDropEdit = RegisterControl(nameof(ETradePackedItemDetailsUserControl.TariffAdditionalCodeDropEdit));
		}

		public ControlReference BanderolTariffFindBox { get; }
		public ControlReference SerialNoTextBox { get; }
		public ControlReference UsedGoodsCodeTextBox { get; }
		public ControlReference AgriculturePolicyTextBox { get; }
		public ControlReference ValueDeclarationFormTextBox { get; }
		public ControlReference CalculationMethodTextBox { get; }
		public ControlReference StatisticalValueCalcFindBox { get; }
		public ControlReference QuotaCheckBox { get; }
		public ControlReference TariffAdditionalCodeDropEdit { get; }

		protected override Control CreateTemplate() => new ETradePackedItemDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<ETradePackedItemDetailsControlBag> packedItemDetailsControlBag = new Lazy<ETradePackedItemDetailsControlBag>(() => new ETradePackedItemDetailsControlBag());
	}
}
