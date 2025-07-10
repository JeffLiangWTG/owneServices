using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class ShipmentDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

		public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

		[ThreadStatic]
		static ShipmentDetailsControlBag instance;

		ShipmentDetailsControlBag()
		{
			HouseBillParcelPostTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.HouseBillParcelPostTextBox));
			GoodsOriginCodeFindBox = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsOriginCodeFindBox));
			ShipmentDetailsOriginUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsOriginUserControl));
			ShipmentDetailsFinalDestinationUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsFinalDestinationUserControl));
			GoodsDescriptionTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsDescriptionTextBox));
			OwnersReferenceTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.OwnersReferenceTextBox));
			WeightCalcDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.WeightCalcDropEdit));
			VolumeCalcDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.VolumeCalcDropEdit));
			TotalNoOfPiecesCalcEdit = RegisterControl(nameof(ShipmentDetailsUserControl.TotalNoOfPiecesCalcEdit));
			ContainerCountCalcEdit = RegisterControl(nameof(ShipmentDetailsUserControl.ContainerCountCalcEdit));
			TotalNoOfPacksCalcDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.TotalNoOfPacksCalcDropEdit));
			ShipmentDetailsIncoTermsUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsIncoTermsUserControl));
			ShipmentDetailsScreeningUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsScreeningUserControl));
			DeclarationLanguageDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.DeclarationLanguageDropEdit));
			UCRTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.UCRTextBox));
			MarksAndNumbersNotePopupEdit = RegisterControl(nameof(ShipmentDetailsUserControl.MarksAndNumbersNotePopupEdit));
		}

		public ControlReference HouseBillParcelPostTextBox;
		public ControlReference GoodsOriginCodeFindBox;
		public ControlReference ShipmentDetailsOriginUserControl;
		public ControlReference ShipmentDetailsFinalDestinationUserControl;
		public ControlReference GoodsDescriptionTextBox;
		public ControlReference OwnersReferenceTextBox;
		public ControlReference WeightCalcDropEdit;
		public ControlReference VolumeCalcDropEdit;
		public ControlReference TotalNoOfPiecesCalcEdit;
		public ControlReference ContainerCountCalcEdit;
		public ControlReference TotalNoOfPacksCalcDropEdit;
		public ControlReference ShipmentDetailsIncoTermsUserControl;
		public ControlReference ShipmentDetailsScreeningUserControl;
		public ControlReference DeclarationLanguageDropEdit;
		public ControlReference UCRTextBox;
		public ControlReference MarksAndNumbersNotePopupEdit;
	}
}
