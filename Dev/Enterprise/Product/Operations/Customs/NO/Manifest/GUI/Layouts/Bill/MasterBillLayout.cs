using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class MasterBillLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ??= CreateMasterBillLayout();
	PanelLayout layout;

	static PanelLayout CreateMasterBillLayout()
	{
		var builder = new BillLayoutBuilder<AsycudaBill>();
		var commonBag = builder.CommonBag;
		var billControlBag = BillControlBag.Instance;
		builder.AddControlBag(billControlBag);

		builder.AddColumn();
		builder.Add(commonBag.BillNumberTextBox, ControlWidthClass.Long);
		builder.Add(billControlBag.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ShipperAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ConsigneeAddressControl, ControlWidthClass.Long);
		builder.Add(billControlBag.PlaceOfLoadingPanel, ControlWidthClass.Long);
		builder.Add(billControlBag.PlaceOfUnloadingPanel, ControlWidthClass.Long);
		builder.Add(billControlBag.PlaceOfDeliveryPanel, ControlWidthClass.Long);
		builder.Add(billControlBag.EmailAddressControl, ControlWidthClass.LongNoCaption);

		return builder.Build();
	}
}
