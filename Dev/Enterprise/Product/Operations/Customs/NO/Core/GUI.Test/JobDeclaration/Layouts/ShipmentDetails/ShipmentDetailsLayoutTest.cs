using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsLayouts))]
	sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsGoodsLocationUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsWeightAndVolumeUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ContainerCountCalcEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentDetailsLayoutBuilder<JobDeclaration>();

		public void TestShipmentDetailsFieldsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Total number of pieces - Import", true,
					LayoutForTesting.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit,
						declaration));
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Total number of pieces - Export", true,
					LayoutForTesting.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit,
						declaration));
			});
		}
	}
}
