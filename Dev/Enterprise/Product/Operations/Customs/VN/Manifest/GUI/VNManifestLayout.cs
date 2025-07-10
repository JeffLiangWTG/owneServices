using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.VN.Manifest.GUI
{
	public class VNManifestLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public VNManifestLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			builder.Add(common.RegistrationNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.RegistrationYearEdit, ControlWidthClass.Medium);
			builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);

			builder.SetVisibility(common.MessageStatusDropEdit, _ => true);

			Layout = builder.Build();
		}
	}
}
