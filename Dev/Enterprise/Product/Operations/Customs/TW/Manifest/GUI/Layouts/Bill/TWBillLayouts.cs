using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public sealed class TWBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }
		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public TWBillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new ASYCUDA.GUI.BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var twBillControlBag = TWBillControlBag.Instance;
			builder.AddControlBag(twBillControlBag);

			builder.AddColumn();
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.UCRNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(twBillControlBag.GoodsDescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(twBillControlBag.BagNumberDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(twBillControlBag.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(twBillControlBag.SplitQuantityCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(twBillControlBag.MarksAndNumbersLongTextControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(twBillControlBag.TariffFindBox, ControlWidthClass.Long);
			builder.Add(twBillControlBag.DGUNNOCodeFindBox, ControlWidthClass.Long);
			builder.Add(twBillControlBag.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.LocationInformationTextBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationCodeFindBox, ControlWidthClass.Long);
			builder.Add(twBillControlBag.IsEscortRequiredCheckBox, ControlWidthClass.Long);

			builder.SetVisibility(twBillControlBag.BagNumberDropEdit, b => b.IsAir, b => b.Header?.AMA_TransportModeInfo);

			return builder.Build();
		}
	}
}
