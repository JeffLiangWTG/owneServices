using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

internal sealed class DV1DetailsLayout : IPanelLayoutProvider
{
	PanelLayout DV1Details { get; }

	PanelLayout IPanelLayoutProvider.Layout => DV1Details;

	public DV1DetailsLayout()
	{
		DV1Details = CreateDV1DetailsLayout();
	}

	PanelLayout CreateDV1DetailsLayout()
	{
		var builder = new DV1DetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.RelationshipDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.PriceInfluenceDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CloseApproximationDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.RestrictionsDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.ConsiderationDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.RoyalitiesLicenceDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.ResaleDropEdit, ControlWidthClass.Medium);

		builder.AddColumn();
		builder.Add(commonBag.RelationDetailsTextBox, ControlWidthClass.Long, commonBag.CloseApproximationDropEdit);
		builder.Add(commonBag.RestrictionsConsiderationTextBox, ControlWidthClass.Long, commonBag.ConsiderationDropEdit);
		builder.Add(commonBag.RoyalitiesLicenceDetailsTextBox, ControlWidthClass.Long, commonBag.RoyalitiesLicenceDropEdit);
		builder.Add(commonBag.ResaleDetailsTextBox, ControlWidthClass.Long, commonBag.ResaleDropEdit);
		builder.Add(commonBag.CustomsDecisionNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.ContractNumberTextBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.ContractDateDateEdit, ControlWidthClass.Auto, commonBag.ContractNumberTextBox);

		return builder.Build();
	}
}
