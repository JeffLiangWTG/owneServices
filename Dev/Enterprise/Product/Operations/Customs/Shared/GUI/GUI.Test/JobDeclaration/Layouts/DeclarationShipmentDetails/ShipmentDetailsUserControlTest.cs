using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ShipmentDetailsUserControlTest : TestCase
	{
		public void TestDeclarationLanguageDropEdit()
		{
			AssertType<ZDropEdit>(control.DeclarationLanguageDropEdit);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals(true, control.CaptionRenderingEnabled);
		}

		public void TestTotalNoOfPiecesCalcEdit()
		{
			AssertType<ZCalcEdit>(control.TotalNoOfPiecesCalcEdit);
		}

		public void TestHouseBillParcelPostTextBox()
		{
			AssertType<ZTextBox>(control.HouseBillParcelPostTextBox);
		}

		public void TestVolumeCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.VolumeCalcDropEdit);
		}

		public void TestWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.WeightCalcDropEdit);
		}

		public void TestContainerCountCalcEdit()
		{
			AssertType<ZCalcEdit>(control.ContainerCountCalcEdit);
		}

		public void TestOwnersReferenceTextBox()
		{
			AssertType<ZTextBox>(control.OwnersReferenceTextBox);
		}

		public void TestTotalNoOfPacksCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.TotalNoOfPacksCalcDropEdit);
		}

		public void TestGoodsDescriptionTextBox()
		{
			AssertType<ZTextBox>(control.GoodsDescriptionTextBox);
		}

		public void TestShipmentDetailsOriginUserControl()
		{
			AssertType<ShipmentDetailsOriginUserControl>(control.ShipmentDetailsOriginUserControl);
		}

		public void TestShipmentDetailsFinalDestinationUserControl()
		{
			AssertType<ShipmentDetailsFinalDestinationUserControl>(control.ShipmentDetailsFinalDestinationUserControl);
		}

		public void TestShipmentDetailsIncoTermsUserControl()
		{
			AssertType<ShipmentDetailsIncoTermsUserControl>(control.ShipmentDetailsIncoTermsUserControl);
		}

		public void TestShipmentDetailsScreeningUserControl()
		{
			AssertType<ShipmentDetailsScreeningUserControl>(control.ShipmentDetailsScreeningUserControl);
		}

		public void TestGoodsOriginCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.GoodsOriginCodeFindBox);
		}

		public void TestUCRTextBox()
		{
			AssertType<ZTextBox>(control.UCRTextBox);
		}

		public void TestMarksAndNumbersPopupEdit() => CombineAssertions(() =>
		{
			var marksAndNumbersNotePopupEdit = control.MarksAndNumbersNotePopupEdit;
			AssertType<ZStmNotePopupEditWithBindableText>("Expected type to be ZStmNotePopupEditWithBindableText", marksAndNumbersNotePopupEdit);
			AssertEquals("Expected button text to be More...", marksAndNumbersNotePopupEdit.ButtonText, "More...");
			AssertEquals("Expected maximum length to be null", marksAndNumbersNotePopupEdit.MaximumNoteLength, null);
		});

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentDetailsUserControl control;
	}
}
