using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class BillControlBag : ControlBag
{
	BillControlBag()
	{
		ImportProcedureDropEdit = RegisterControl(nameof(BillUserControl.ImportProcedureDropEdit));
		ExportProcedureDropEdit = RegisterControl(nameof(BillUserControl.ExportProcedureDropEdit));
		EmailAddressControl = RegisterControl(nameof(BillUserControl.EmailAddressControl));
		PlaceOfAcceptancePanel = RegisterControl(nameof(BillUserControl.PlaceOfAcceptancePanel));
		PlaceOfLoadingPanel = RegisterControl(nameof(BillUserControl.PlaceOfLoadingPanel));
		PlaceOfUnloadingPanel = RegisterControl(nameof(BillUserControl.PlaceOfUnloadingPanel));
		PlaceOfDeliveryPanel = RegisterControl(nameof(BillUserControl.PlaceOfDeliveryPanel));
		TransportDocumentTypeDropEdit = RegisterControl(nameof(BillUserControl.TransportDocumentTypeDropEdit));
	}

	public static BillControlBag Instance => instance ??= new BillControlBag();

	[ThreadStatic]
	static BillControlBag instance;

	protected override Control CreateTemplate() => new BillUserControl();

	public ControlReference ImportProcedureDropEdit { get; }
	public ControlReference ExportProcedureDropEdit { get; }
	public ControlReference EmailAddressControl { get; }
	public ControlReference PlaceOfAcceptancePanel { get; }
	public ControlReference PlaceOfLoadingPanel { get; }
	public ControlReference PlaceOfUnloadingPanel { get; }
	public ControlReference PlaceOfDeliveryPanel { get; }
	public ControlReference TransportDocumentTypeDropEdit { get; }
}
