using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public sealed class ShipmentDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ShipmentDetailsLayout()
	{
		Layout = CreateShipmentDetailsLayout();
	}

	static PanelLayout CreateShipmentDetailsLayout()
	{
		var builder = new EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.ShipmentDetailsControlBag.Instance;
		var nlBag = ShipmentDetailsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(nlBag);
		builder.AddColumn();

		builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
		builder.Add(nlBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(nlBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.AgentsReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.UCRTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

		return builder.Build();
	}
}
