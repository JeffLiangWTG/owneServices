using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class DV1DetailsLayout : IPanelLayoutProvider
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

			var trBag = DV1DetailsControlBag.Instance;
			builder.AddControlBag(trBag);

			builder.AddColumn();
			builder.Add(commonBag.ContractNumberTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.ContractDateDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.RelationshipDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.PriceInfluenceDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CloseApproximationDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.RestrictionsDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.ConsiderationDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.RoyalitiesLicenceDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.ResaleDropEdit, ControlWidthClass.Medium);
			builder.Add(trBag.PlaceTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsDecisionNumberTextBox, ControlWidthClass.Medium);
			builder.Add(trBag.CustomsDecisionDateDateEdit, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.RelationDetailsTextBox, ControlWidthClass.LongControl, commonBag.CloseApproximationDropEdit);
			builder.Add(commonBag.RestrictionsConsiderationTextBox, ControlWidthClass.LongControl, commonBag.ConsiderationDropEdit);
			builder.Add(commonBag.RoyalitiesLicenceDetailsTextBox, ControlWidthClass.LongControl, commonBag.RoyalitiesLicenceDropEdit);
			builder.Add(commonBag.ResaleDetailsTextBox, ControlWidthClass.LongControl, commonBag.ResaleDropEdit);

			return builder.Build();
		}
	}
}
