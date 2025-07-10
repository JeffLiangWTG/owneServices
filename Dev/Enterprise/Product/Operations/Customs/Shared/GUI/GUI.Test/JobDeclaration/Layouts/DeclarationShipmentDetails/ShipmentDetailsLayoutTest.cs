using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsLayouts))]
	sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestShipmentDetailsScreeningUserControlVisibility()
		{
			CombineAssertions(() =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;
				AssertEquals("Not Visible", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, declaration));

				declaration.JE_JS = ZGuid.Empty;
				AssertEquals("Visible", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, declaration));

				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				{
					AssertEquals("Not Visible", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, declaration));
				}
			});
		}

		public void TestTotalNoOfPiecesCalcEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Not Visible", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, declaration));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Visible", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, declaration));
			});
		}

		public void TestContainerCountCalcEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				AssertEquals("Not Export", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ContainerCountCalcEdit, declaration));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
				AssertEquals("Not Sea Transport", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ContainerCountCalcEdit, declaration));

				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				AssertEquals("Export and Sea", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ContainerCountCalcEdit, declaration));
			});
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}
		protected override int ControlBagCount => 1;

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ContainerCountCalcEdit, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
				yield return (ShipmentDetailsControlBag.Instance.MarksAndNumbersNotePopupEdit, ControlWidthClass.Auto);
			}
		}

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ShipmentDetailsLayouts()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentDetailsLayoutBuilder<BaseJobDeclaration>();

		PanelLayout layout;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
		}
		BaseJobDeclaration declaration;
	}
}
