using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsControlBag))]
	sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentDetailsControlBag.HouseBillParcelPostTextBox);
				yield return nameof(ShipmentDetailsControlBag.GoodsOriginCodeFindBox);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsOriginUserControl);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsFinalDestinationUserControl);
				yield return nameof(ShipmentDetailsControlBag.GoodsDescriptionTextBox);
				yield return nameof(ShipmentDetailsControlBag.OwnersReferenceTextBox);
				yield return nameof(ShipmentDetailsControlBag.WeightCalcDropEdit);
				yield return nameof(ShipmentDetailsControlBag.VolumeCalcDropEdit);
				yield return nameof(ShipmentDetailsControlBag.TotalNoOfPiecesCalcEdit);
				yield return nameof(ShipmentDetailsControlBag.ContainerCountCalcEdit);
				yield return nameof(ShipmentDetailsControlBag.TotalNoOfPacksCalcDropEdit);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsIncoTermsUserControl);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsScreeningUserControl);
				yield return nameof(ShipmentDetailsControlBag.DeclarationLanguageDropEdit);
				yield return nameof(ShipmentDetailsControlBag.UCRTextBox);
				yield return nameof(ShipmentDetailsControlBag.MarksAndNumbersNotePopupEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
	}
}
