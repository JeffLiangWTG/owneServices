using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class BillLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateBillLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateBillLayout()
	{
		var builder = new BillLayoutBuilder<AsycudaBill>();
		var billControlBag = BillControlBag.Instance;
		builder.AddControlBag(billControlBag);
		var common = builder.CommonBag;

		builder.AddColumn();
		builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
		builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(billControlBag.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
		builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
		builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
		builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
		builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
		builder.Add(billControlBag.PlaceOfAcceptancePanel, ControlWidthClass.Long);
		builder.Add(billControlBag.PlaceOfLoadingPanel, ControlWidthClass.Long);
		builder.Add(billControlBag.PlaceOfUnloadingPanel, ControlWidthClass.Long);
		builder.Add(billControlBag.PlaceOfDeliveryPanel, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(billControlBag.ImportProcedureDropEdit, ControlWidthClass.Long);
		builder.Add(billControlBag.ExportProcedureDropEdit, ControlWidthClass.Long);
		builder.Add(common.ForwarderAddressControl, ControlWidthClass.Long);
		builder.Add(billControlBag.EmailAddressControl, ControlWidthClass.LongNoCaption);

		builder.SetCaption(common.ForwarderAddressControl, h => Res.GetData("0FB90F8F-E292-4FE0-8671-2910FA15FE27", "Representative"));

		return builder.Build();
	}
}
