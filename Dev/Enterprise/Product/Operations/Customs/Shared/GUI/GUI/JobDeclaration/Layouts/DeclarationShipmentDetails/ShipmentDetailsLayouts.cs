using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class ShipmentDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout ShipmentDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ShipmentDetails;

		public ShipmentDetailsLayouts()
		{
			ShipmentDetails = CreateShipmentDetailsLayout();
		}

		PanelLayout CreateShipmentDetailsLayout()
		{
			var builder = new ShipmentDetailsLayoutBuilder<BaseJobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalNoOfPiecesCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ContainerCountCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.MarksAndNumbersNotePopupEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
