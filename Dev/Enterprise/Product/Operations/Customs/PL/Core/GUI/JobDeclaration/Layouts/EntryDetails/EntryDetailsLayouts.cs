using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class EntryDetailsLayouts : IPanelLayoutProvider
{
	PanelLayout EntryDetails { get; }

	public PanelLayout Layout => EntryDetails;

	public EntryDetailsLayouts()
	{
		EntryDetails = CreateInvoiceLineDetailsLayout();
	}

	PanelLayout CreateInvoiceLineDetailsLayout()
	{
		var builder = new CommonEntryDetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.TotalsLabel, ControlWidthClass.Auto);
		builder.Add(commonBag.NoPacksCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.DutyCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VatCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.EntryLinesCountCalcEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.CustomsLabel, ControlWidthClass.Auto);
		builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.SubmittedDateDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.MRNTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.AcceptanceDateDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ReleaseDateDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.EntryStatusDropEdit, ControlWidthClass.Auto);

		return builder.Build();
	}
}
