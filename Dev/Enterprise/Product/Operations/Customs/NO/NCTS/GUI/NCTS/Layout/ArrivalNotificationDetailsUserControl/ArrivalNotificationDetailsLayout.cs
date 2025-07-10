using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.NCTS.GUI;

sealed class ArrivalNotificationDetailsLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new EU.NCTS.GUI.ArrivalNotificationDetailsLayoutBuilder<EU.NCTS.Business.NctsHeader>();
		var commonBag = builder.CommonBag;

		var noBag = ArrivalNotificationDetailsControlBag.Instance;
		builder.AddControlBag(noBag);

		builder.AddColumn();
		builder.Add(commonBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.MrnTextBox, ControlWidthClass.Long);
		builder.Add(noBag.GoodsRegistrationNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.AuthorizationCodeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.NumberCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.IncidentFlagDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
		return builder.Build();
	}
}
