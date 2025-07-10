using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradeLayouts))]
	sealed class ETradeLayoutsTest : LayoutsAbstractTest
	{
		public void TestETradeMainControlCapitons()
		{
			var manifest = Factory.New<Business.AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = "TR";
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TRETrade;

			using (var form = new ETradeForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				var voyage = asycudaManifestUserControl.Controls.Find("VoyageFlightTextBox", true).FirstOrDefault();
				var dateAtCustomsOffice = asycudaManifestUserControl.Controls.Find("DateAtCustomsOfficeDateEdit", true).FirstOrDefault();
				var goodsLocationCode = asycudaManifestUserControl.Controls.Find("GoodsLocationCodeCodeFindBox", true).FirstOrDefault();
				var goodsLocation = asycudaManifestUserControl.Controls.Find("LocationInformationTextBox", true).FirstOrDefault();
				var convCountry = asycudaManifestUserControl.Controls.Find("ConveyanceCountryCodeFindBox", true).FirstOrDefault();
				var importExportCustomsOffice = asycudaManifestUserControl.Controls.Find("ImportExportCustomsOfficeDropEdit", true).FirstOrDefault();
				var dischargeLoadingCustomsOffice = asycudaManifestUserControl.Controls.Find("DischargeLoadingCustomsOfficeDropEdit", true).FirstOrDefault();
				var dateAtCustomsOfficeDateEdit = asycudaManifestUserControl.Controls.Find("DateAtCustomsOfficeDateEdit", true).FirstOrDefault();
				var stampTaxValueTextBox = asycudaManifestUserControl.Controls.Find("StampTaxValueTextBox", true).FirstOrDefault();
				var departureFlightTextBox = asycudaManifestUserControl.Controls.Find("DepartureFlightTextBox", true).FirstOrDefault();

				AssertEquals("Nature", asycudaManifestUserControl.FindSingle<ZDropEdit>(nameof(CommonManifestControlBag.NatureDropEdit)).GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Arrival Customs Office", asycudaManifestUserControl.FindSingle<ZDropEdit>(nameof(CommonManifestControlBag.CustomsOfficeDropEdit)).GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Arrival Date", dateAtCustomsOffice.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Vessel Country", convCountry.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Goods Loc. Code", goodsLocationCode.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Goods Location", goodsLocation.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Arrival Date", dateAtCustomsOfficeDateEdit.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Stamp Tax", stampTaxValueTextBox.GetExtension<LabelCaptionRenderer>().Caption);

				manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
				manifest.RefreshBinding();
				AssertEquals("Export Cus. Off", importExportCustomsOffice.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Loading Cus. Off", dischargeLoadingCustomsOffice.GetExtension<LabelCaptionRenderer>().Caption);

				manifest.AMA_Nature = ShipmentTypeList.Codes.Import23;
				manifest.RefreshBinding();
				AssertEquals("Import Cus. Off", importExportCustomsOffice.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Discharge Cus. Off", dischargeLoadingCustomsOffice.GetExtension<LabelCaptionRenderer>().Caption);

				manifest.AMA_TransportMode = TransportModes.Air;
				manifest.RefreshBinding();
				AssertEquals("Border Flight", voyage.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Departure Flight", departureFlightTextBox.GetExtension<LabelCaptionRenderer>().Caption);

				manifest.AMA_TransportMode = TransportModes.Road;
				manifest.RefreshBinding();
				AssertEquals("Border Truck Ref", voyage.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Departure Truck Ref", departureFlightTextBox.GetExtension<LabelCaptionRenderer>().Caption);

				manifest.AMA_TransportMode = TransportModes.Sea;
				manifest.RefreshBinding();
				AssertEquals("Border Voyage", voyage.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Departure Voyage", departureFlightTextBox.GetExtension<LabelCaptionRenderer>().Caption);
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ETradeLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Medium);
				yield return (ETradeControlBag.Instance.RegistrationDateLongDateEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.DepartureFlightTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.DepartureCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.DateAtCustomsOfficeDateEdit, ControlWidthClass.Auto);
				yield return (ETradeControlBag.Instance.TransshipmentCountryCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.TransshipmentLocationTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.TransshipmentReferenceTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.TransshipmentConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.PreviousContainerNoTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.NewContainerNoTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ETradeControlBag.Instance.MessageModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.ProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.PresentationCustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.ImportExportCustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.DischargeLoadingCustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.GoodsLocationCodeCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.CustomsValueCalcFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.ExchangeRateCalcEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.FreightValueCalcFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.InsuranceValueCalcFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.OtherValueCalcFindBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.GuaranteeTypeDropEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.GuaranteeRefNoTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.GuaranteeAmountCalcEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (ETradeControlBag.Instance.TempRegNoTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.TempRegNoDateEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.DischargeRecordNoTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.DischargeRecordNoDateEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.ClosureNoTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.ClosureNoDateEdit, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.InspectionClerkTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.NumberOfBillsTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.TotalBoxQtyTextBox, ControlWidthClass.Long);
				yield return (ETradeControlBag.Instance.StampTaxValueTextBox, ControlWidthClass.Auto);
			}
		}
	}
}
