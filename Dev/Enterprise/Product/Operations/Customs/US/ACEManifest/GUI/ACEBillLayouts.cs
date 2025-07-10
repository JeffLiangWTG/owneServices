using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public class ACEBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public ACEBillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var aceBag = ACEBillControlBag.Instance;
			builder.AddControlBag(aceBag);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(aceBag.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(aceBag.GoodsOriginCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(aceBag.FDAIndicatorCheckBox, ControlWidthClass.Auto);
			builder.Add(aceBag.EntryNumberTypeDropEdit, ControlWidthClass.Long);
			builder.Add(aceBag.TariffCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Long);
			builder.Add(aceBag.BillStatusTextBox, ControlWidthClass.Auto);
			builder.Add(aceBag.BillStatusDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(aceBag.EntryNumberTextBox, ControlWidthClass.Auto);

			builder.SetCaption(common.OriginCodeFindBox, _ => Res.GetData("31BCE433-9D25-48AB-BC2C-E400757F2D18", "Flight Origin"));

			return builder.Build();
		}
	}
}
