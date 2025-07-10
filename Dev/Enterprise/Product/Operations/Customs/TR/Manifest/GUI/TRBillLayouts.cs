using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public class TRBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public TRBillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var tr = TRBillControlBag.Instance;
			builder.AddControlBag(tr);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.LocationInformationTextBox, ControlWidthClass.Long);
			builder.Add(tr.PaymentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(tr.TransshipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(tr.RoRoCheckBox, ControlWidthClass.Auto);
			builder.Add(common.SpecialCargoCodesDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.AgentAddressControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.CustomsEntryNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.CustomsEntryNumberTypeDropEdit, ControlWidthClass.Long);
			builder.Add(tr.BillStampDutyValueCalcEdit, ControlWidthClass.Long);
			builder.Add(tr.AirBillStampDutyABSValueCalcEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsNumbersGroupBox, ControlWidthClass.LongNoCaption);

			builder.SetVisibility(tr.BillStampDutyValueCalcEdit, b => (b.Header.IsSea || b.Header.IsAir), b => b.Header.AMA_TransportModeInfo);
			builder.SetVisibility(tr.AirBillStampDutyABSValueCalcEdit, b => (b.Header.IsAir), b => b.Header.AMA_TransportModeInfo);
			builder.SetVisibility(common.CustomsNumbersGroupBox, b => b.Header.NeedPreviousDeclarationNumbers, b => b.Header.AMA_ApplicationCodeInfo, b => b.Header.AMA_TransportModeInfo);

			builder.SetCaption(common.CustomsNumbersGroupBox, b => b?.PreviousDeclarationNoCaption);
			builder.SetCaption(tr.BillStampDutyValueCalcEdit, b => b.BillStampDutyValueCaption, b => b.Header.AMA_TransportModeInfo);

			return builder.Build();
		}
	}
}
