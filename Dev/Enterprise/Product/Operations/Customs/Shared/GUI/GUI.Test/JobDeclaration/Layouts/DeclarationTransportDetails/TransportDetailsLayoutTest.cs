using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(TransportDetailsLayouts))]
	sealed class TransportDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestMasterBillTextBoxVisibility_ExpressRegistry()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			CombineAssertions(() =>
			{
				Env.Registry.IsExpress = true;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.MasterBillTextBox, Declaration));

				Env.Registry.IsExpress = false;
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.MasterBillTextBox, Declaration));
			});
		}

		public void TestMasterBillTextBoxVisibility_TransportMode()
		{
			Env.Registry.IsExpress = false;
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = Declaration.TransportModeFixedCodeForTesting;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.MasterBillTextBox, Declaration));

				Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.MasterBillTextBox, Declaration));
			});
		}

		public void TestOceanBillTextBoxVisibility_ExpressRegistry()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeRoadCodeForTesting;
			CombineAssertions(() =>
			{
				Env.Registry.IsExpress = false;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.OceanBillTextBox, Declaration));

				Env.Registry.IsExpress = true;
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.OceanBillTextBox, Declaration));
			});
		}

		public void TestOceanBillTextBoxVisibility_TransportMode()
		{
			Env.Registry.IsExpress = false;
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = Declaration.TransportModeMailCodeForTesting;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.OceanBillTextBox, Declaration));

				Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.OceanBillTextBox, Declaration));
			});
		}

		public void TestFlightUserControlVisibility()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = Declaration.TransportModeRailCodeForTesting;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.FlightUserControl, Declaration));

				Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.FlightUserControl, Declaration));
			});
		}

		public void TestVehicleRegistrationNumberTextBoxVisibility()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = Declaration.TransportModeFixedCodeForTesting;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, Declaration));

				Declaration.JE_TransportMode = Declaration.TransportModeRoadCodeForTesting;
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, Declaration));
			});
		}

		public void TestVoyageNumberTextBoxVisibility()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));

				Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));
			});
		}

		public void TestVesselCodeFindBoxVisibility()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselCodeFindBox, Declaration));

				Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselCodeFindBox, Declaration));
			});
		}

		public void TestOverrideValuesCheckBoxVisibility()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_JS = ZGuid.Empty;
				AssertEquals("Not Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.OverrideValuesCheckBox, Declaration));

				Declaration.JE_JS = ZGuid.NewZGuid();
				AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.OverrideValuesCheckBox, Declaration));
			});
		}

		public void TestUCRTextBox()
		{
			AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.UCRTextBox, Declaration));
		}

		public void TestSubLocationOfGoodsTextBox()
		{
			AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.SubLocationOfGoodsTextBox, Declaration));
		}

		public void TestGoodsDestinationCountryCodeFindBox()
		{
			AssertEquals("Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.GoodsDestinationCountryCodeFindBox, Declaration));
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
				yield return (TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Long);
				yield return (TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Medium);
				yield return (TransportDetailsControlBag.Instance.OceanBillTextBox, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.FlightUserControl, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.VoyageNumberTextBox, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
			}
		}

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new TransportDetailsLayouts()).Layout);
		PanelLayout layout;

		public BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
				}
				return declaration;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder<BaseJobDeclaration>();

		BaseJobDeclaration declaration;
	}
}
