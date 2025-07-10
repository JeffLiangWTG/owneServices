using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public sealed class ShipmentDetailsLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout => ShipmentDetails;
		public PanelLayout ShipmentDetails { get; }

		public ShipmentDetailsLayouts()
		{
			ShipmentDetails = CreateShipmentDetailsLayout();
		}
		PanelLayout CreateShipmentDetailsLayout()
		{
			var builder = new ShipmentDetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			var noBag = ShipmentDetailsControlBag.Instance;
			builder.AddControlBag(noBag);

			builder.AddColumn();

			builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			builder.Add(noBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			builder.Add(noBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(noBag.ShipmentDetailsGoodsLocationUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
			builder.Add(noBag.ShipmentDetailsWeightAndVolumeUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalNoOfPiecesCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ContainerCountCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.TotalNoOfPiecesCalcEdit, h => true, h => h.JE_MessageTypeInfo);
			return builder.Build();
		}
	}
}
