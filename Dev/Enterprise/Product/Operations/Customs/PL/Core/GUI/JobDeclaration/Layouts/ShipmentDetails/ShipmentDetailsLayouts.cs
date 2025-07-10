using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class ShipmentDetailsLayouts : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ShipmentDetailsLayouts()
	{
		Layout = CreateShipmentDetailsLayout();
	}

	static PanelLayout CreateShipmentDetailsLayout()
	{
		var builder = new EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.ShipmentDetailsControlBag.Instance;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentIncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.AgentsReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.UCRTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

		return builder.Build();
	}
}
