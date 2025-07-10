using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class EntryInstructionDetailsLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new EntryInstructionDetailsLayoutBuilder();
		var plBag = builder.PLBag;
		var euBag = builder.EUBag;
		var commonBag = builder.CommonBag;
		builder.AddControlBag(euBag);
		builder.AddControlBag(plBag);

		builder.AddColumn();
		builder.Add(plBag.EADPrintOutDropEdit, ControlWidthClass.Long);
		builder.Add(plBag.DeclarationDateDateEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CPCDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(plBag.ExportManifestCheckBox, ControlWidthClass.Auto);
		builder.Add(plBag.PostExportTransitCheckBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(euBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
		builder.Add(plBag.TemporaryLocationDropEdit, ControlWidthClass.Auto);
		builder.Add(plBag.TemporaryLocationTextBox, ControlWidthClass.Auto);
		builder.Add(plBag.OfficeOfExitArrivalTimeLimitDateEdit, ControlWidthClass.Auto);

		return builder.Build();
	}
}
