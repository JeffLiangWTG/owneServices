using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI;

public sealed class UnloadingDetailsLayout : IPanelLayoutProvider
{
	public UnloadingDetailsLayout()
	{
		Layout = CreateLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new EU.NCTS.GUI.UnloadingDetailsLayoutBuilder<Business.NctsArrivalMovementHeader>();
		var commonBag = builder.CommonBag;
		var euBag = EU.NCTS.GUI.UnloadingDetailsControlBag.Instance;
		var nlBag = UnloadingDetailsControlBag.Instance;
		builder.AddControlBag(euBag);
		builder.AddControlBag(nlBag);

		builder.AddColumn();
		builder.Add(commonBag.UnloadingDateDateEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.UnloadingConformCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.StateOfSealsCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.UnloadingCompletedCheckBox, ControlWidthClass.Long);
		builder.Add(nlBag.UnloadingRemarksFreeTextTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.OtherThingsToReportTextBox, ControlWidthClass.Long);
		builder.Add(nlBag.UnloadingRemarksGrid, ControlWidthClass.LongControl);
		return builder.Build();
	}
}
