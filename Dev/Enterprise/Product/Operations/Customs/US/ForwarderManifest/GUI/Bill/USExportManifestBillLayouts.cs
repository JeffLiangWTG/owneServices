using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public class USExportManifestBillLayouts : IPanelLayoutProvider
	{
		public USExportManifestBillLayouts()
		{
			ManifestBillDetails = CreateBillLayout();
		}

		PanelLayout IPanelLayoutProvider.Layout => ManifestBillDetails;

		PanelLayout ManifestBillDetails { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		PanelLayout CreateBillLayout()
		{
			var builder = new USExportManifestBillLayoutBuilder<USExportAsycudaBill>();
			var common = builder.CommonBag;
			var usExportManifestBillControlBag = USExportManifestBillControlBag.Instance;
			builder.AddControlBag(usExportManifestBillControlBag);

			builder.AddColumn();
			builder.Add(common.BillIssuerCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.SpecialCargoCodesDropEdit, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.AESITNNumbersUserControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.AESExemptionCodeTextBox, ControlWidthClass.Medium);
			builder.Add(usExportManifestBillControlBag.InBondNumbersUserControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.OriginPortUserControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.FinalDestinationPortUserControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.LadingPortUserControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.UnladingPortUserControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.DeparturePortUserControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.ArrivalPortUserControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.PlaceOfReceiptTextBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.PriorTransportationModeDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(usExportManifestBillControlBag.BoardedQuantityCalcEdit, ControlWidthClass.Medium, common.ManifestQtyCalcDropEdit);
			builder.Add(usExportManifestBillControlBag.BoardedWeightCalcDropEdit, ControlWidthClass.Medium, common.GrossWeightCalcDropEdit);

			return builder.Build();
		}
	}
}
