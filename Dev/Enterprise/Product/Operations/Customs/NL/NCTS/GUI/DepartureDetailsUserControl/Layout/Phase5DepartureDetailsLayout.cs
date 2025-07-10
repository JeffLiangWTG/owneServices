using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI;

public sealed class Phase5DepartureDetailsLayout : IPanelLayoutProvider
{
	public Phase5DepartureDetailsLayout()
	{
		Layout = CreateLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new Phase5DepartureDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;

		var nlBag = Phase5DepartureDetailsControlBag.Instance;
		builder.AddControlBag(nlBag);

		builder.AddColumn();
		builder.Add(commonBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.SimplifiedProcedureAndReducedDataSetUserControl, ControlWidthClass.Long);
		builder.Add(nlBag.DateLimitAndCalculationUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(nlBag.CalCalculationMethodDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.SecurityDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TirCarnetNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
		builder.Add(commonBag.CommunicationLanguageDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
