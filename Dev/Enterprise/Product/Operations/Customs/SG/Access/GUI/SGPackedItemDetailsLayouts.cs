using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class SGPackedItemDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout PackedItemDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => PackedItemDetails;

		public SGPackedItemDetailsLayouts()
		{
			PackedItemDetails = CreatePackedItemDetailsLayout();
		}

		PanelLayout CreatePackedItemDetailsLayout()
		{
			var builder = new PackedItemDetailsLayoutBuilder<AsycudaPack>();
			var common = builder.CommonBag;
			var sgBag = SGPackedItemDetailsControlBag.Instance;
			builder.AddControlBag(sgBag);

			builder.AddColumn();
			builder.Add(common.TariffFindBox, ControlWidthClass.Medium);
			builder.Add(sgBag.SGEdiTariffFindBox, ControlWidthClass.Auto);
			builder.Add(common.CustomsQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(sgBag.GoodsTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(sgBag.GSTPaidDropEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.CustomsValueCalcEdit, ControlWidthClass.Medium);
			builder.Add(common.TaxAmountCalcEdit, ControlWidthClass.Medium);
			builder.Add(common.DutyAmountCalcEdit, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Auto);
			builder.Add(common.PackStatusTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.CustomEntriesSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.CustomEntriesGrid, ControlWidthClass.Long);

			var useDefaultTariffFindBox = Env.Registry.ExternalBorderComplianceTool == ExternalBorderComplianceToolList.Codes.None;
			builder.SetVisibility(common.TariffFindBox, (p) => useDefaultTariffFindBox);
			builder.SetVisibility(sgBag.SGEdiTariffFindBox, (p) => !useDefaultTariffFindBox);
			builder.SetVisibility(sgBag.GSTPaidDropEdit, (p) => p.Bill?.Header?.IsOVRApplicable ?? false, (p) => p.Bill?.Header?.AMA_ManifestTypeInfo);

			return builder.Build();
		}
	}
}
