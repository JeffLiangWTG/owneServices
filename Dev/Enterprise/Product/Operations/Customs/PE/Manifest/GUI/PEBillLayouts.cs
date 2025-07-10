using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.PE.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PE.Manifest.GUI
{
	public class PEBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public PEBillLayouts()
		{
			BillDetails = CreateBillLayout();
		}

		PanelLayout CreateBillLayout()
		{
			AddControls();
			SetVisibilities();

			return Builder.Build();
		}

		void AddControls()
		{
			var common = Builder.CommonBag;
			var pe = PEBillControlBag.Instance;
			Builder.AddControlBag(pe);

			Builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
			builder.Add(pe.BillIssueDateEdit, ControlWidthClass.Auto);

			Builder.AddColumn();
			Builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			Builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			Builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			Builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			Builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			Builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			Builder.Add(common.AgentAddressControl, ControlWidthClass.Long);
			Builder.Add(common.IncotermDropEdit, ControlWidthClass.Long);

			Builder.AddColumn();
			Builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			Builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(common.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(common.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(pe.CargoNatureDropEdit, ControlWidthClass.Long);
			Builder.Add(pe.CargoConditionDropEdit, ControlWidthClass.Long);
		}

		void SetVisibilities()
		{
			var common = Builder.CommonBag;
			var pe = PEBillControlBag.Instance;

			Builder.SetVisibility(pe.CargoNatureDropEdit, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo);
			Builder.SetVisibility(pe.CargoConditionDropEdit, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo);
		}

		BillLayoutBuilder<AsycudaBill> Builder => builder ?? (builder = new BillLayoutBuilder<AsycudaBill>());
		BillLayoutBuilder<AsycudaBill> builder;
	}
}
