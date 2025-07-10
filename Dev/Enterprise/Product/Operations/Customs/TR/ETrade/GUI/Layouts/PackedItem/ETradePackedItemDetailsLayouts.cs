using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public sealed class ETradePackedItemDetailsLayouts : IPanelLayoutProvider
	{
		public PanelLayout PackedItemDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => PackedItemDetails;

		public ETradePackedItemDetailsLayouts()
		{
			PackedItemDetails = CreatePackedItemDetailsLayout();
		}

		PanelLayout CreatePackedItemDetailsLayout()
		{
			var builder = new PackedItemDetailsLayoutBuilder<AsycudaPack>();
			var common = builder.CommonBag;
			var etr = ETradePackedItemDetailsControlBag.Instance;
			builder.AddControlBag(etr);

			builder.AddColumn();
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(etr.SerialNoTextBox, ControlWidthClass.Long);
			builder.Add(common.CustomsQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.BrandTextBox, ControlWidthClass.Long);
			builder.Add(common.ModelTextBox, ControlWidthClass.Long);
			builder.Add(common.TariffFindBox, ControlWidthClass.Long);
			builder.Add(etr.TariffAdditionalCodeDropEdit, ControlWidthClass.Long);
			builder.Add(etr.BanderolTariffFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(etr.UsedGoodsCodeTextBox, ControlWidthClass.Long);
			builder.Add(common.GoodsOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.CustomsQty2CalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsQty3CalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsValueLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(etr.StatisticalValueCalcFindBox, ControlWidthClass.Long);
			builder.Add(etr.AgriculturePolicyTextBox, ControlWidthClass.Long);
			builder.Add(etr.ValueDeclarationFormTextBox, ControlWidthClass.Long);
			builder.Add(etr.CalculationMethodTextBox, ControlWidthClass.Long);
			builder.Add(etr.QuotaCheckBox, ControlWidthClass.Long);

			builder.AddControlBehaviour<TariffFindBox>(common.TariffFindBox,
				(control, pack) => control.ShowDescriptionBox = true);

			return builder.Build();
		}
	}
}
