using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(TWManifestLayouts))]
	sealed class TWManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = TWManifestTypes.Codes.MAN;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				CombineAssertions(() =>
				{
					var commonBag = new TWManifestLayoutBuilder().CommonBag;
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					Assert("ShortEstArrivalDateEdit should be Visible only when transportmode is AIR", asycudaManifestUserControl.FindSingle<Control>(commonBag.ShortEstArrivalDateEdit.Name).Visible);
					Assert("VesselCodeFindBox should be hidden when transportmode is not SEA", !asycudaManifestUserControl.FindSingle<Control>(commonBag.VesselCodeFindBox.Name).Visible);
					Assert("LloydsNumberTextBox should be hidden when transportmode is not SEA", !asycudaManifestUserControl.FindSingle<Control>(commonBag.LloydsNumberTextBox.Name).Visible);
					Assert("VehicleRegistrationTextBox should be hidden when transportmode is not SEA", !asycudaManifestUserControl.FindSingle<Control>(commonBag.VehicleRegistrationTextBox.Name).Visible);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					Assert("ShortEstArrivalDateEdit should be hidden when transportmode is not AIR", !asycudaManifestUserControl.FindSingle<Control>(commonBag.ShortEstArrivalDateEdit.Name).Visible);
					Assert("VesselCodeFindBox should be Visible only when transportmode is SEA", asycudaManifestUserControl.FindSingle<Control>(commonBag.VesselCodeFindBox.Name).Visible);
					Assert("LloydsNumberTextBox should be Visible only when transportmode is SEA", asycudaManifestUserControl.FindSingle<Control>(commonBag.LloydsNumberTextBox.Name).Visible);
					Assert("VehicleRegistrationTextBox should be Visible only when transportmode is SEA", asycudaManifestUserControl.FindSingle<Control>(commonBag.VehicleRegistrationTextBox.Name).Visible);
				});
			}
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TWManifestLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (TWManifestControlBag.Instance.GoodsLocationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ShortEstArrivalDateEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.DeconsolidateAddressControl, ControlWidthClass.Long);
				yield return (TWManifestControlBag.Instance.DeconsolidateVATTextBox, ControlWidthClass.Medium);
				yield return (TWManifestControlBag.Instance.LoginCompanyGuidFindBox, ControlWidthClass.Long);
				yield return (TWManifestControlBag.Instance.MailBoxTextBox, ControlWidthClass.Medium);
				yield return (TWManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
