using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
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
			builder.Add(twBillControlBag.SequenceNumberCalcEdit, ControlWidthClass.Long);
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(twBillControlBag.RemarksLongTextControl, ControlWidthClass.Long);
			builder.Add(twBillControlBag.ProcedureDropEdit, ControlWidthClass.Long);
			builder.Add(common.IncotermDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(twBillControlBag.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(twBillControlBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(twBillControlBag.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(twBillControlBag.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(twBillControlBag.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.LocationInformationTextBox, ControlWidthClass.Long);

			builder.SetVisibility(twBillControlBag.PortOfLoadingCodeFindBox, b => b.Header?.IsImport ?? false, b => b.Header?.AMA_NatureInfo);
			builder.SetVisibility(twBillControlBag.PortOfDischargeCodeFindBox, b => b.Header?.IsExport ?? false, b => b.Header?.AMA_NatureInfo);
			builder.SetVisibility(common.LocationInformationTextBox, b => b.Header?.IsExport ?? false, b => b.Header?.AMA_NatureInfo);
			return builder.Build();
		}
	}
}
