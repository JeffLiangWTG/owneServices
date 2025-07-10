using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ConsolUserControlTest : TestCaseWithFactory
	{
		public void TestUseDeniedPartyScreeningStatusDropEdit()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var consolForm = new ConsolFormForTesting(consol))
				{
					consolForm.Show();
					var control = consolForm.ConsolControl;
					AssertEquals(true, control.JK_ScreeningStatusDropEdit.Visible);
				}
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var consolForm = new ConsolFormForTesting(consol))
				{
					consolForm.Show();
					var control = consolForm.ConsolControl;
					AssertEquals(false, control.JK_ScreeningStatusDropEdit.Visible);
				}
			}
		}

		public void TestNoOverlap_CutOff_And_CO2()
		{
			// CO2 textbox only visible when the EnableGreenhouseGasEmissionCalculation is enabled
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				using (var consolForm = new ConsolFormForTesting(consol))
				{
					consolForm.Show();
					var cutoff = consolForm.Controls.Find("JK_ConsolCutOffDateEdit", true).FirstOrDefault() as ZDateEdit;
					var co2 = consolForm.Controls.Find("TotalCO2eTextBox", true).FirstOrDefault() as ZTextBox;

					var cutoffRectangle = new Rectangle(cutoff.Left, cutoff.Top, cutoff.Width, cutoff.Height);
					var co2Rectangle = new Rectangle(co2.Left, co2.Top, co2.Width, co2.Height);

					AssertEquals(false, cutoffRectangle.IntersectsWith(co2Rectangle));
				}
			}
		}

		public void TestToggleDateTimeFormat()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			shipment.JS_E_DEP = new ZDateTime(2016, 7, 29, 1, 1, 1);
			shipment.JS_E_ARV = new ZDateTime(2016, 7, 29, 4, 4, 4);

			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				var grid = consolForm.ConsolControl.ShipmentModuleButtonGrid.InnerGrid;

				AssetColumnsDateTimeFormat(grid, "JS_E_ARV", ZDateTimePickerFormat.Short);
				AssetColumnsDateTimeFormat(grid, "JS_E_DEP", ZDateTimePickerFormat.Short);
				AssetColumnsDateTimeFormat(grid, "JS_A_BKD", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "JS_A_RCV", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_EstimatedPickup", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_PickupRequiredBy", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_PickupCartageAdvised", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_PickupCartageCompleted", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_EstimatedDelivery", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_DeliveryRequiredBy", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_DeliveryCartageAdvised", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_DeliveryCartageCompleted", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "JS_HouseBillIssueDate", ZDateTimePickerFormat.Short);

				AssertEquals(ZDateTimePickerFormat.Short, consolForm.ConsolControl.MasterBillIssueDateEdit.DateTimeFormat);
			}

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				var grid = consolForm.ConsolControl.ShipmentModuleButtonGrid.InnerGrid;

				AssetColumnsDateTimeFormat(grid, "JS_E_ARV", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "JS_E_DEP", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "JS_A_BKD", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "JS_A_RCV", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_EstimatedPickup", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_PickupRequiredBy", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_PickupCartageAdvised", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_PickupCartageCompleted", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_EstimatedDelivery", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_DeliveryRequiredBy", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_DeliveryCartageAdvised", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "DocsAndCartage+JP_DeliveryCartageCompleted", ZDateTimePickerFormat.Long);
				AssetColumnsDateTimeFormat(grid, "JS_HouseBillIssueDate", ZDateTimePickerFormat.Short);

				AssertEquals(ZDateTimePickerFormat.Long, consolForm.ConsolControl.MasterBillIssueDateEdit.DateTimeFormat);
			}
		}

		void AssetColumnsDateTimeFormat(ZGrid grid, string columnName, ZDateTimePickerFormat expectedFormat)
		{
			var style = (ZDateEditColumnStyleInfo)grid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == columnName);

			AssertEquals(expectedFormat, style.DateTimeFormat);
		}

		#region Disposed

		public void TestDispose()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var testControl = new ConsolUserControl();

			testControl.Show();
			testControl.Dispose();

			AssertNoExceptionThrown(() => testControl.SetDataBinding(consol, ""));
		}

		#endregion

		#region Visible / Hidden Tests

		public void TestMasterBillIssueDateShowsOnlyWhenAirOrSea()
		{
			// Should be visible if logged in country is ZA and mode is Road.
			var consol = Factory.New<ForwardingConsol>();
			using (ConsolFormForTesting consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;
				AssertEquals("Pre: Master Bill Issue DateEdit should not be visible", false, consolForm.MasterBillIssueDateEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Master Bill Issue DateEdit should be visible", true, consolForm.MasterBillIssueDateEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("Master Bill Issue DateEdit should not be visible", true, consolForm.MasterBillIssueDateEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Road;
				AssertEquals("Master Bill Issue DateEdit should not be visible", false, consolForm.MasterBillIssueDateEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Rail;
				AssertEquals("Master Bill Issue DateEdit should not be visible", false, consolForm.MasterBillIssueDateEdit.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				using (ConsolFormForTesting consolForm = new ConsolFormForTesting(consol))
				{
					consolForm.Show();
					consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;
					AssertEquals("Pre: Master Bill Issue DateEdit should not be visible", false, consolForm.MasterBillIssueDateEdit.Visible);

					consol.JK_TransportMode = Constants.TransportModes.Air;
					AssertEquals("Master Bill Issue DateEdit should be visible", true, consolForm.MasterBillIssueDateEdit.Visible);

					consol.JK_TransportMode = Constants.TransportModes.Sea;
					AssertEquals("Master Bill Issue DateEdit should not be visible", true, consolForm.MasterBillIssueDateEdit.Visible);

					consol.JK_TransportMode = Constants.TransportModes.Road;
					AssertEquals("Master Bill Issue DateEdit should be visible", true, consolForm.MasterBillIssueDateEdit.Visible);

					consol.JK_TransportMode = Constants.TransportModes.Rail;
					AssertEquals("Master Bill Issue DateEdit should not be visible", false, consolForm.MasterBillIssueDateEdit.Visible);
				}
			}
		}

		public void TestMasterBillIssuePlaceShowsOnlyWhenSea()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (ConsolFormForTesting consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;
				AssertEquals("Pre: Master Bill Issue Place FindBox should not be visible", false, consolForm.MasterBillIssuePlaceFindBox.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("Master Bill Issue Place FindBox should be visible", true, consolForm.MasterBillIssuePlaceFindBox.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Master Bill Issue Place FindBox should not be visible", false, consolForm.MasterBillIssuePlaceFindBox.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Road;
				AssertEquals("Master Bill Issue Place FindBox should not be visible", false, consolForm.MasterBillIssuePlaceFindBox.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Rail;
				AssertEquals("Master Bill Issue Place FindBox should not be visible", false, consolForm.MasterBillIssuePlaceFindBox.Visible);
			}
		}

		public void TestAWBDimsDropEditShowsOnlyWhenAir()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;
				Assert("Pre: AWBDimsDropEdit should not be visible", !consolForm.AWBDimsDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				Assert("AWBDimsDropEdit should not be visible", !consolForm.AWBDimsDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert("AWBDimsDropEdit should be visible", consolForm.AWBDimsDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Road;
				Assert("AWBDimsDropEdit should not be visible", !consolForm.AWBDimsDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Rail;
				Assert("AWBDimsDropEdit should not be visible", !consolForm.AWBDimsDropEdit.Visible);
			}
		}

		public void TestPackageGroupingDropEditShowsOnlyWhenSea()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;
				Assert("Pre: PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert("PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				Assert("PackageGroupingDropEdit should be visible", consolForm.PackageGroupingDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Road;
				Assert("PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Rail;
				Assert("PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;
				Assert("Pre: PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert("PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				Assert("PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Road;
				Assert("PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Rail;
				Assert("PackageGroupingDropEdit should not be visible", !consolForm.PackageGroupingDropEdit.Visible);
			}
		}

		public void TestCarrierBookingAndBillOfLadingRelatedControlsVisibilityAndReadability()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ElectronicBillOfLadingReference = "AAAAAAAAAAAAA";
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;
				Assert("Pre: LatestStatusGroupBox should not be visible", !consolForm.LatestStatusGroupBox.Visible);
				Assert("Pre: BillOfLadingBillDateDateEdit should not be visible", !consolForm.BillOfLadingBillDateDateEdit.Visible);
				Assert("Pre: BillOfLadingBillStatusDropEdit should not be visible", !consolForm.BillOfLadingBillStatusDropEdit.Visible);
				Assert("Pre: CarrierBookingDateDateEdit should not be visible", !consolForm.CarrierBookingDateDateEdit.Visible);
				Assert("Pre: CarrierBookingLatestStatusDropEdit should not be visible", !consolForm.CarrierBookingLatestStatusDropEdit.Visible);

				Assert("Pre: BillOfLadingBillTermsDropEdit should not be visible", !consolForm.BillOfLadingBillTermsDropEdit.Visible);
				Assert("Pre: BillOfLadingBillTypeDropEdit should not be visible", !consolForm.BillOfLadingBillTypeDropEdit.Visible);
				Assert("Pre: CarrierBookingOfficeCodeFindBox should not be visible", !consolForm.CarrierBookingOfficeCodeFindBox.Visible);
				Assert("Pre: ElectronicBillOfLadingReferenceTextBox should not be visible", !consolForm.ElectronicBillOfLadingReferenceTextBox.Visible);
				Assert("Pre: ViewEBLButton should not be visible", !consolForm.ViewEBLButton.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				Assert("LatestStatusGroupBox should be visible", consolForm.LatestStatusGroupBox.Visible);
				Assert("BillOfLadingBillDateDateEdit should be visible", consolForm.BillOfLadingBillDateDateEdit.Visible);
				Assert("BillOfLadingBillStatusDropEdit should be visible", consolForm.BillOfLadingBillStatusDropEdit.Visible);
				Assert("CarrierBookingDateDateEdit should be visible", consolForm.CarrierBookingDateDateEdit.Visible);
				Assert("CarrierBookingLatestStatusDropEdit should be visible", consolForm.CarrierBookingLatestStatusDropEdit.Visible);

				Assert("BillOfLadingBillTermsDropEdit should be visible", consolForm.BillOfLadingBillTermsDropEdit.Visible);
				Assert("BillOfLadingBillTypeDropEdit should be visible", consolForm.BillOfLadingBillTypeDropEdit.Visible);
				Assert("CarrierBookingOfficeCodeFindBox should be visible", consolForm.CarrierBookingOfficeCodeFindBox.Visible);
				Assert("ElectronicBillOfLadingReferenceTextBox should be visible", consolForm.ElectronicBillOfLadingReferenceTextBox.Visible);
				Assert("ViewEBLButton should be visible", consolForm.ViewEBLButton.Visible);

				Assert("BillOfLadingBillDateDateEdit should be readonly", consolForm.BillOfLadingBillDateDateEdit.ReadOnly);
				Assert("BillOfLadingBillStatusDropEdit should be readonly", consolForm.BillOfLadingBillStatusDropEdit.ReadOnly);
				Assert("CarrierBookingDateDateEdit should be readonly", consolForm.CarrierBookingDateDateEdit.ReadOnly);
				Assert("CarrierBookingLatestStatusDropEdit should be readonly", consolForm.CarrierBookingLatestStatusDropEdit.ReadOnly);

				Assert("BillOfLadingBillTermsDropEdit should be readonly", consolForm.BillOfLadingBillTermsDropEdit.ReadOnly);
				Assert("BillOfLadingBillTypeDropEdit should be readonly", consolForm.BillOfLadingBillTypeDropEdit.ReadOnly);
				Assert("ElectronicBillOfLadingReferenceTextBox should be readonly", consolForm.ElectronicBillOfLadingReferenceTextBox.ReadOnly);

				AssertEquals("CarrierBookingDateDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, consolForm.CarrierBookingDateDateEdit.DateTimeFormat);
				AssertEquals("BillOfLadingBillDateDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, consolForm.BillOfLadingBillDateDateEdit.DateTimeFormat);

				AssertEquals("LatestStatusGroupBox Caption", "Latest Status", consolForm.LatestStatusGroupBox.CaptionResourceString.Caption);
				AssertEquals("BillOfLadingBillDateDateEdit Caption", "Date", consolForm.BillOfLadingBillDateDateEdit.CaptionResourceString.Caption);
				AssertEquals("BillOfLadingBillStatusDropEdit Caption", "Bill Status", consolForm.BillOfLadingBillStatusDropEdit.CaptionResourceString.Caption);
				AssertEquals("CarrierBookingDateDateEdit Caption", "Date", consolForm.CarrierBookingDateDateEdit.CaptionResourceString.Caption);
				AssertEquals("CarrierBookingLatestStatusDropEdit Caption", "Booking Status", consolForm.CarrierBookingLatestStatusDropEdit.CaptionResourceString.Caption);

				AssertEquals("CarrierBookingLatestStatusDropEdit Caption", "Bill Terms", consolForm.BillOfLadingBillTermsDropEdit.CaptionResourceString.Caption);
				AssertEquals("CarrierBookingLatestStatusDropEdit Caption", "Bill Type", consolForm.BillOfLadingBillTypeDropEdit.CaptionResourceString.Caption);
				AssertEquals("CarrierBookingLatestStatusDropEdit Caption", "Carrier Booking Office", consolForm.CarrierBookingOfficeCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("ElectronicBillOfLadingReferenceTextBox Caption", "eBL Identifier", consolForm.ElectronicBillOfLadingReferenceTextBox.CaptionResourceString.Caption);
				AssertEquals("ViewEBLButton Caption", "View/Transact eBL", consolForm.ViewEBLButton.CaptionResourceString.Caption);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert("LatestStatusGroupBox should not be visible", !consolForm.LatestStatusGroupBox.Visible);
				Assert("CarrierBookingDateDateEdit should not be visible", !consolForm.CarrierBookingDateDateEdit.Visible);
				Assert("CarrierBookingLatestStatusDropEdit should not be visible", !consolForm.CarrierBookingLatestStatusDropEdit.Visible);
				Assert("BillOfLadingBillDateDateEdit should not be visible", !consolForm.BillOfLadingBillDateDateEdit.Visible);
				Assert("BillOfLadingBillStatusDropEdit should not be visible", !consolForm.BillOfLadingBillStatusDropEdit.Visible);

				Assert("BillOfLadingBillTermsDropEdit should not be visible", !consolForm.BillOfLadingBillTermsDropEdit.Visible);
				Assert("BllOfLadingBillTypeDropEdit should not be visible", !consolForm.BillOfLadingBillTypeDropEdit.Visible);
				Assert("CarrierBookingOfficeCodeFindBox should not be visible", !consolForm.CarrierBookingOfficeCodeFindBox.Visible);
				Assert("ElectronicBillOfLadingReferenceTextBox should not be visible", !consolForm.ElectronicBillOfLadingReferenceTextBox.Visible);
				Assert("ViewEBLButton should not be visible", !consolForm.ViewEBLButton.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Road;
				Assert("LatestStatusGroupBox should not be visible", !consolForm.LatestStatusGroupBox.Visible);
				Assert("BillOfLadingBillDateDateEdit should not be visible", !consolForm.BillOfLadingBillDateDateEdit.Visible);
				Assert("BillOfLadingBillStatusDropEdit should not be visible", !consolForm.BillOfLadingBillStatusDropEdit.Visible);
				Assert("CarrierBookingDateDateEdit should not be visible", !consolForm.CarrierBookingDateDateEdit.Visible);
				Assert("CarrierBookingLatestStatusDropEdit should not be visible", !consolForm.CarrierBookingLatestStatusDropEdit.Visible);

				Assert("BillOfLadingBillTermsDropEdit should not be visible", !consolForm.BillOfLadingBillTermsDropEdit.Visible);
				Assert("BllOfLadingBillTypeDropEdit should not be visible", !consolForm.BillOfLadingBillTypeDropEdit.Visible);
				Assert("CarrierBookingOfficeCodeFindBox should not be visible", !consolForm.CarrierBookingOfficeCodeFindBox.Visible);
				Assert("ElectronicBillOfLadingReferenceTextBox should not be visible", !consolForm.ElectronicBillOfLadingReferenceTextBox.Visible);
				Assert("ViewEBLButton should not be visible", !consolForm.ViewEBLButton.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Rail;
				Assert("LatestStatusGroupBox should not be visible", !consolForm.LatestStatusGroupBox.Visible);
				Assert("BillOfLadingBillDateDateEdit should not be visible", !consolForm.BillOfLadingBillDateDateEdit.Visible);
				Assert("BillOfLadingBillStatusDropEdit should not be visible", !consolForm.BillOfLadingBillStatusDropEdit.Visible);
				Assert("CarrierBookingDateDateEdit should not be visible", !consolForm.CarrierBookingDateDateEdit.Visible);
				Assert("CarrierBookingLatestStatusDropEdit should not be visible", !consolForm.CarrierBookingLatestStatusDropEdit.Visible);

				Assert("BillOfLadingBillTermsDropEdit should not be visible", !consolForm.BillOfLadingBillTermsDropEdit.Visible);
				Assert("BllOfLadingBillTypeDropEdit should not be visible", !consolForm.BillOfLadingBillTypeDropEdit.Visible);
				Assert("CarrierBookingOfficeCodeFindBox should not be visible", !consolForm.CarrierBookingOfficeCodeFindBox.Visible);
				Assert("ElectronicBillOfLadingReferenceTextBox should not be visible", !consolForm.ElectronicBillOfLadingReferenceTextBox.Visible);
				Assert("ViewEBLButton should not be visible", !consolForm.ViewEBLButton.Visible);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = false, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				Assert("LatestStatusGroupBox should be visible", consolForm.LatestStatusGroupBox.Visible);
				Assert("BillOfLadingBillDateDateEdit should not be visible", !consolForm.BillOfLadingBillDateDateEdit.Visible);
				Assert("BillOfLadingBillStatusDropEdit should not be visible", !consolForm.BillOfLadingBillStatusDropEdit.Visible);
				Assert("CarrierBookingDateDateEdit should be visible", consolForm.CarrierBookingDateDateEdit.Visible);
				Assert("CarrierBookingLatestStatusDropEdit should be visible", consolForm.CarrierBookingLatestStatusDropEdit.Visible);

				Assert("BillOfLadingBillTermsDropEdit should not be visible", !consolForm.BillOfLadingBillTermsDropEdit.Visible);
				Assert("BillOfLadingBillTypeDropEdit should not be visible", !consolForm.BillOfLadingBillTypeDropEdit.Visible);
				Assert("CarrierBookingOfficeCodeFindBox should be visible", consolForm.CarrierBookingOfficeCodeFindBox.Visible);
				Assert("ElectronicBillOfLadingReferenceTextBox should not be visible", !consolForm.ElectronicBillOfLadingReferenceTextBox.Visible);
				Assert("ViewEBLButton should not be visible", !consolForm.ViewEBLButton.Visible);

				Assert("CarrierBookingDateDateEdit should be readonly", consolForm.CarrierBookingDateDateEdit.ReadOnly);
				Assert("CarrierBookingLatestStatusDropEdit should be readonly", consolForm.CarrierBookingLatestStatusDropEdit.ReadOnly);

				AssertEquals("CarrierBookingDateDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, consolForm.CarrierBookingDateDateEdit.DateTimeFormat);
				AssertEquals("BillOfLadingBillDateDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, consolForm.BillOfLadingBillDateDateEdit.DateTimeFormat);

				AssertEquals("LatestStatusGroupBox Caption", "Latest Status", consolForm.LatestStatusGroupBox.CaptionResourceString.Caption);
				AssertEquals("BillOfLadingBillDateDateEdit Caption", "Date", consolForm.BillOfLadingBillDateDateEdit.CaptionResourceString.Caption);
				AssertEquals("BillOfLadingBillStatusDropEdit Caption", "Bill Status", consolForm.BillOfLadingBillStatusDropEdit.CaptionResourceString.Caption);
				AssertEquals("CarrierBookingDateDateEdit Caption", "Date", consolForm.CarrierBookingDateDateEdit.CaptionResourceString.Caption);
				AssertEquals("CarrierBookingLatestStatusDropEdit Caption", "Booking Status", consolForm.CarrierBookingLatestStatusDropEdit.CaptionResourceString.Caption);

				AssertEquals("CarrierBookingLatestStatusDropEdit Caption", "Bill Terms", consolForm.BillOfLadingBillTermsDropEdit.CaptionResourceString.Caption);
				AssertEquals("CarrierBookingLatestStatusDropEdit Caption", "Bill Type", consolForm.BillOfLadingBillTypeDropEdit.CaptionResourceString.Caption);
				AssertEquals("CarrierBookingLatestStatusDropEdit Caption", "Carrier Booking Office", consolForm.CarrierBookingOfficeCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("ElectronicBillOfLadingReferenceTextBox Caption", "eBL Identifier", consolForm.ElectronicBillOfLadingReferenceTextBox.CaptionResourceString.Caption);
				AssertEquals("ViewEBLButton Caption", "View/Transact eBL", consolForm.ViewEBLButton.CaptionResourceString.Caption);
			}
		}

		public void TestCAACIPlugInVisibilityWhenTransportModeDischargePortChanged()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			var consol = Factory.New<ForwardingConsol>();

			try
			{
				CountrySpecificForwardingShipmentSupportTest.DeleteAnyCarrierCode(Factory);
				currentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "CC", Constants.CountryCodes.Canada);
				Factory.Save();

				using (var consolForm = new ConsolForm(consol))
				{
					consolForm.Show();
					var plugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
					AssertEquals("PlugIn should exists, but should be disabled, because not all conditions followed", false, plugIn.Enabled);

					consol.JK_TransportMode = Constants.TransportModes.Sea;
					plugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
					AssertEquals("PlugIn should exists, but should be disabled, because not all conditions followed", false, plugIn.Enabled);

					consol.JK_RL_NKDischargePort = "CAYVR";
					plugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
					AssertEquals("PlugIn should be disabled now, because port of discharge changed", true, plugIn.Enabled);
				}
			}
			finally
			{
				CountrySpecificForwardingShipmentSupportTest.DeleteAnyCarrierCode(Factory);
			}
		}

		public void TestJK_TotalShipmentLoadingMetersVisibility()
		{
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Precondition", true, consol.IsRoadLoadingMetersEnabled);

			using (var form = new ConsolFormForTesting(consol))
			{
				form.Show();
				AssertEquals(true, form.JK_TotalShipmentLoadingMetersCalcEdit.Visible);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(405), form.BottomInnerRightPanel.Location.X);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Precondition", false, consol.IsRoadLoadingMetersEnabled);

				form.Show();
				AssertEquals(false, form.JK_TotalShipmentLoadingMetersCalcEdit.Visible);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(356), form.BottomInnerRightPanel.Location.X);
			}
		}

		public void TestMultiAWBMaster_ControlsVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.AWBMaster;

			using (var form = new ConsolFormForTesting(consol))
			{
				form.Show();

				var mawbControls = new Control[]
				{
					form.GetChildControl("AirlinePrefixTextBox"),
					form.GetChildControl("MAWBNumberTextBox"),
					form.GetChildControl("MAWBHyphenLabel"),
					form.GetChildControl("IsNeutralCheckBox")
				};

				var shipmentDetailsControls = new Control[]
				{
					form.GetChildControl("ShipmentDetailsBottomPanel"),
					form.GetChildControl("ShipmentModuleButtonGrid")
				};

				var colodConsolsControls = new Control[]
				{
					form.GetChildControl("ColoadConsolModuleButtonGrid"),
					form.GetChildControl("ConsolDetailsBottomPanel")
				};

				Assert("MAWB controls are hidden for multi AWB master", mawbControls.All(control => !control.Visible));
				Assert("Shipment details controls are hidden for multi AWB master", shipmentDetailsControls.All(control => !control.Visible));
				Assert("Coload consols are visible for multi AWB master", colodConsolsControls.All(control => control.Visible));

				consol.JK_AgentType = Constants.AgentType.Agent;
				Assert("MAWB controls are visible", mawbControls.All(control => control.Visible));
				Assert("Shipment details controls are visible", shipmentDetailsControls.All(control => control.Visible));
				Assert("Coload consols are hidden", colodConsolsControls.All(control => !control.Visible));
			}
		}

		public void TestAirControlsVisibilityWhenConsolIsCourier()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Courier;

			using (var form = new ConsolFormForTesting(consol))
			{
				form.Show();

				var airControls = new[]
				{
					form.GetChildControl("CharterPanel"),
					form.GetChildControl("AirConsolPanel"),
				};

				Assert(airControls.All(control => !control.Visible));

				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert(!consol.IsCharter);
				Assert(!airControls[0].Visible);
				Assert(airControls[1].Visible);
			}
		}

		public void TestSpecialHandlingControlsVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consolForm.DepartureArrivalTabControl.SelectedTab = consolForm.DocsTabPage;

				var specialHandlingUserControl = consolForm.Controls.Find("SpecialHandlingUserControl", true)[0];
				var securityStatusDropEdit = consolForm.Controls.Find("SecurityStatusDropEdit", true)[0];

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				Assert("Only AIR Consols should have the special handling grid visible.", !specialHandlingUserControl.Visible);
				Assert("Only AIR Consols should have the security status visible.", !securityStatusDropEdit.Visible);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert("AIR Consols should have the special handling grid visible.", specialHandlingUserControl.Visible);
				Assert("AIR Consols should have the security status visible.", securityStatusDropEdit.Visible);
			}
		}

		public void TestCRNPanelVisibleForAirWhenConsolIsCourier()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Courier;

			using (var form = new ConsolFormForTesting(consol))
			{
				form.Show();

				var controls = new[]
				{
					form.GetChildControl("CRNPanelSea")
				};

				Assert(controls[0].Visible);

				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert(!controls[0].Visible);
			}
		}

		public void TestCoLoadMBLAndBkgRefVisibleWhenConsolIsCoLoad()
		{
			AssertCoLoadMBLAndBkgRefVisible(Constants.AgentType.CoLoad);
		}

		void AssertCoLoadMBLAndBkgRefVisible(ZString agentType)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = agentType;

			using (var form = new ConsolFormForTesting(consol))
			{
				form.Show();

				var controls = new[]
				{
					form.GetChildControl("JK_CoLoadMasterBillBoundTextEdit"),
					form.GetChildControl("JK_CoLoadBookingReferenceBoundTextEdit")
				};

				AssertNotNull(controls[0]);
				AssertNotNull(controls[1]);
				Assert(controls[0].Visible);
				Assert(controls[1].Visible);

				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				Assert(!controls[0].Visible);
				Assert(!controls[1].Visible);
			}
		}

		public void TestConsolPanelVisibilityWhenTypeIsChanged()
		{
			CombineAssertions(() =>
			{
				AssertPanelVisibility(TransportModes.Air, AgentType.Agent, AgentType.Courier,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: true, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: false, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Air, AgentType.Agent, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CRNPanelSea", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Air, AgentType.Agent, AgentType.Charter,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: true, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: true)
					});

				AssertPanelVisibility(TransportModes.Air, AgentType.Courier, AgentType.Agent,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: true),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: false),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Air, AgentType.Courier, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: true),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: false),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Air, AgentType.Courier, AgentType.Charter,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: false),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: true)
					});

				AssertPanelVisibility(TransportModes.Air, AgentType.Charter, AgentType.Courier,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: false, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: true, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Air, AgentType.Charter, AgentType.Agent,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: true),
						("CRNPanelSea", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CharterPanel", shouldBeVisibleBefore: true, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Air, AgentType.Charter, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: true),
						("CRNPanelSea", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CharterPanel", shouldBeVisibleBefore: true, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Agent, AgentType.Courier,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Agent, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Agent, AgentType.Charter,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Courier, AgentType.Agent,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Courier, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Courier, AgentType.Charter,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Charter, AgentType.Courier,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Charter, AgentType.Agent,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Sea, AgentType.Charter, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Agent, AgentType.Courier,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Agent, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Agent, AgentType.Charter,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Courier, AgentType.Agent,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Courier, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Courier, AgentType.Charter,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Charter, AgentType.Courier,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Charter, AgentType.Agent,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});

				AssertPanelVisibility(TransportModes.Road, AgentType.Charter, AgentType.Direct,
					new List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)>
					{
						("AirConsolPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false),
						("CRNPanelSea", shouldBeVisibleBefore: true, shouldBeVisibleAfter: true),
						("CharterPanel", shouldBeVisibleBefore: false, shouldBeVisibleAfter: false)
					});
			});
		}

		void AssertPanelVisibility(
				string transportMode,
				string agentTypeFrom,
				string agentTypeTo,
				List<(string testedControlName, bool shouldBeVisibleBefore, bool shouldBeVisibleAfter)> testedControlsAndResults)
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_AgentType = agentTypeFrom;

			using (var form = new ConsolFormForTesting(consol))
			{
				form.Show();

				var agentTypeDropEdit = (ZDropEdit)form.GetChildControl("JK_AgentTypeBoundDropDownEdit");

				foreach (var (testedControlName, shouldBeVisibleBefore, shouldBeVisibleAfter) in testedControlsAndResults)
				{
					agentTypeDropEdit.SelectItem(agentTypeFrom);

					var controlToTest = form.GetChildControl(testedControlName);

					AssertEquals($"{testedControlName} visibility is incorrect for {transportMode} transport before Agent Type changed from {agentTypeFrom} to {agentTypeTo}", shouldBeVisibleBefore, controlToTest.Visible);

					// Act
					agentTypeDropEdit.SelectItem(agentTypeTo);

					// Assert
					AssertEquals($"{testedControlName} visibility is incorrect for {transportMode} transport after Agent Type changed from {agentTypeFrom} to {agentTypeTo}", shouldBeVisibleAfter, controlToTest.Visible);
				}
			}
		}

		#endregion

		#region Consignor Terminology

		public void TestConsignorTerminologyOnConsolGrid()
		{
			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			AssertDataGridText(FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestTestTest");
			AssertDataGridText("TestTestTest");
		}

		void AssertDataGridText(string expectedValue)
		{
			using (ConsolUserControl consolForm = new ConsolUserControl())
			{
				consolForm.Show();
				foreach (ZGridColumnInfo columnInfo in consolForm.ShipmentModuleButtonGrid.InnerGrid.ColumnStyles)
				{
					if (columnInfo is ZMultiControlColumnStyleInfo)
					{
						if (((ZMultiControlColumnStyleInfo)columnInfo).ColumnName == "ConsignorNameOrPK")
						{
							AssertEquals("Caption should be " + expectedValue, expectedValue, ((ZMultiControlColumnStyleInfo)columnInfo).Caption);
						}
					}
				}
			}
		}

		#endregion

		#region Order Management

		public void TestOrderManagement_NoSelectedShipment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			using (ConsolForm consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				consolForm.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Order Management").PerformClick();
				AssertEquals("Please select a shipment before attempting to enter Order References.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestOrderManagement_SelectedShipment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();

			using (ConsolFormForTesting consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();

				consolForm.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Order Management").PerformClick();
				AssertEquals(typeof(ShipmentOrderManagementForm), ZFormModaliser.ActiveForm.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				((IZForm)ZFormModaliser.ActiveForm).Dispose();
			}
		}

		#endregion

		#region View Measurements

		public void TestViewMeasurementMenuItemExists()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var form = new ConsolForm(consol))
			{
				AssertNotNull(form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("View Measurements"));
			}
		}

		public void TestViewMeasurementForm()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			Func<ForwardingShipment> createShipmentWithPackLine = () =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.ConsigneePK = organization.PK;
				shipment.ConsignorPK = organization.PK;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.Consols.Add(consol);
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 1;
				packline.JL_F3_NKPackType = "CTN";
				packline.JL_ActualWeight = 500.1;
				packline.JL_ActualWeightUQ = "KG";
				packline.JL_ActualVolume = 6;
				packline.JL_ActualVolumeUQ = "M3";
				packline.JL_Length = 1;
				packline.JL_Width = 2;
				packline.JL_Height = 3;
				packline.JL_UnitOfDimension = "M";
				return shipment;
			};

			var shipment1 = createShipmentWithPackLine();
			shipment1.CoLoadShipments.AddNew();

			var shipment2 = createShipmentWithPackLine();
			shipment2.JS_RL_NKDestination = "NZAKL";

			using (ConsolForm form = new ConsolForm(consol))
			using (ZFormModaliser.SuspendDispose())
			{
				form.Show();
				var viewMeasurementMenuItem = form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("View Measurements");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				viewMeasurementMenuItem.PerformClick();
				AssertEquals("Please select a shipment.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(0);
				viewMeasurementMenuItem.PerformClick();
				AssertEquals("Please save before viewing measurements.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				viewMeasurementMenuItem.PerformClick();

				var lastForm = ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(typeof(ShipmentViewMeasurementsForm), lastForm?.GetType());

				var shipmentPopup = lastForm.GetField("TextBox") as RichTextBox;
				var expectedMessage = @"S00001000
     1 CTN                       1.00 x 2.00 x 3.00 M     500.10 KG       6.000 M3

Totals: 1 CTN                                              500.10 KG       6.000 M3";
				AssertMultilineASCIIEquals(expectedMessage, shipmentPopup?.Text ?? string.Empty);
			}
		}

		#endregion

		#region View Shipment Tracking

		public void TestViewShipmentTrackingMenuItemExists()
		{
			TestCase(true);
			TestCase(false);

			void TestCase(bool isActive)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.Consols.Add(consol);

				using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = isActive }))
				using (var form = new ConsolForm(consol))
				using (ZFormModaliser.SuspendDispose())
				{
					form.Show();
					var viewShipmentTrackingMenuItem = form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Shipment Visibility");
					if (isActive)
					{
						AssertNotNull(viewShipmentTrackingMenuItem);
					}
					else
					{
						AssertNull(viewShipmentTrackingMenuItem);
					}
				}
			}
		}

		public void TestViewShipmentTracking()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/portals");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S01";
			shipment.Consols.Add(consol);

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			using (var form = new ConsolForm(consol))
			using (ZFormModaliser.SuspendDispose())
			{
				form.Show();
				var viewShipmentTrackingMenuItem = form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Shipment Visibility");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				viewShipmentTrackingMenuItem.PerformClick();
				AssertEquals("Please select a shipment.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(0);
				viewShipmentTrackingMenuItem.PerformClick();
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var expectedUrlPattern = @"https://address/portals/NST/Desktop\?noHeader=true#/tracker\?trackingNumber=S01&sso_otp=\w+";
				Assert(Regex.IsMatch(launchedUrl, expectedUrlPattern));
			}
		}

		#endregion

		#region Packing Details

		public void TestShowPackingMenuItemExists()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (ConsolForm form = new ConsolForm(consol))
			{
				AssertNotNull(form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing"));
			}
		}

		public void TestShowPackingDetailForms()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipmentCLD = consol.Shipments.AddNew();
			shipmentCLD.JS_UniqueConsignRef = "CLD";
			shipmentCLD.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			var shipmentSTD = shipmentCLD.CoLoadShipments.AddNew();
			shipmentSTD.JS_UniqueConsignRef = "STD";
			shipmentSTD.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipmentHLS = consol.Shipments.AddNew();
			shipmentHLS.JS_UniqueConsignRef = "HLS";
			shipmentHLS.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipmentHVL = consol.Shipments.AddNew();
			shipmentHVL.JS_UniqueConsignRef = "HVL";
			shipmentHVL.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			using (ConsolForm form = new ConsolForm(consol))
			{
				Action<ForwardingShipment> selectShipment = (shipment) =>
				{
					for (int i = 0; i < form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.Count; i++)
					{
						form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.Position = i;

						if (shipment == form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.GetCurrent())
						{
							return;
						}
					}

					throw new Exception(shipment.HumanReadableName + "not found in ShipmentModuleButtonGrid");
				};

				form.Show();
				form.ConsolControl.ShowSubHouseBillsCheckBox.Checked = true;

				selectShipment(shipmentCLD);
				AssertEquals("CLD shipment is selected", shipmentCLD,
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.GetCurrent());
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing").PerformClick();
				AssertEquals("You cannot edit pack lines of master shipments.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				selectShipment(shipmentSTD);
				AssertEquals("STD shipment is selected", shipmentSTD,
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.GetCurrent());
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing").PerformClick();
				AssertEquals(typeof(ShipmentPackingDetailForm), ZFormModaliser.ActiveForm.GetType());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				selectShipment(shipmentHLS);
				AssertEquals("HLS shipment is selected", shipmentHLS,
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.GetCurrent());
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing").PerformClick();
				AssertEquals("HLS supports packlines", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(typeof(ShipmentPackingDetailForm), ZFormModaliser.ActiveForm.GetType());

				selectShipment(shipmentHVL);
				AssertEquals("HVL shipment is selected", shipmentHVL,
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.GetCurrent());
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing").PerformClick();
				AssertEquals("HVL supports packlines", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(typeof(ShipmentPackingDetailForm), ZFormModaliser.ActiveForm.GetType());
			}
		}

		public void TestPackingDetailsFormShowsCorrectContainersOnSubsequentConsolLegs()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "CS00001";
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "CS00002";
			var consol3 = shipment.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "CS00003";

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "SDFG09876543";

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "LKJH09876543";

			var container3 = consol3.Containers.AddNew();
			container3.JC_ContainerNum = "MKLO09876543";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = container3.PK;

			Factory.Save();

			using (var form = new ConsolForm(consol2))
			{
				form.Show();
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.Position = 0;
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing").PerformClick();

				AssertEquals(typeof(ShipmentPackingDetailForm), ZFormModaliser.ActiveForm.GetType());
				AssertEquals("CS00002", shipment.OuterPackLines.CurrentConsol.JK_UniqueConsignRef);
				AssertEquals("LKJH09876543", shipment.OuterPackLines.CurrentConsol.Containers[0].JC_ContainerNum);
			}

			using (var form = new ConsolForm(consol3))
			{
				form.Show();
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.Position = 0;
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing").PerformClick();

				AssertEquals(typeof(ShipmentPackingDetailForm), ZFormModaliser.ActiveForm.GetType());
				AssertEquals("CS00003", shipment.OuterPackLines.CurrentConsol.JK_UniqueConsignRef);
				AssertEquals("MKLO09876543", shipment.OuterPackLines.CurrentConsol.Containers[0].JC_ContainerNum);
			}
		}

		#endregion

		#region Combine Shipment

		public void TestCombineShipmentMenuItemExists()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			using (ConsolForm form = new ConsolForm(consol))
			{
				AssertNotNull(form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Combine Shipment"));
			}
		}

		public void TestLabelAndCaptionResourceString()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			using (ConsolFormForTesting form = new ConsolFormForTesting(consol))
			{
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				var expectedDescription = "Enter the Coload Agent if the Consol Type is a Co-Load. A co loader is a forwarder that you use if your organization has chosen not to cut their own direct bill or consol.";
				AssertEquals(expectedDescription, form.JK_OA_CreditorAddressControl.CaptionResourceString.FullDescription);
				AssertEquals("Co-Load With", form.JK_OA_CreditorAddressControl.CaptionResourceString.Caption);

				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				expectedDescription = "The party who is the Creditor for the transportation costs associated with this consolidation. Usually this is the Carrier, their Agent or a Gateway.";
				AssertEquals(expectedDescription, form.JK_OA_CreditorAddressControl.CaptionResourceString.FullDescription);
				AssertEquals("Creditor", form.JK_OA_CreditorAddressControl.CaptionResourceString.Caption);

				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				expectedDescription = "Enter the Coload Agent if the Consol Type is a Gateway Co-Load. A co loader is a forwarder that you use if your organization has chosen not to cut their own direct bill or consol.";
				AssertEquals(expectedDescription, form.JK_OA_CreditorAddressControl.CaptionResourceString.FullDescription);
				AssertEquals("Co-Load With", form.JK_OA_CreditorAddressControl.CaptionResourceString.Caption);
			}
		}

		public void TestVolumeWeightHintAndCaption()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			using (FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(100, Weight.Kilograms, Volume.CubicMetres),
				new ConversionFactor(100, Weight.Pounds, Volume.CubicInches))))
			{
				using (FreightDataRegistry.Instance.InternationalChargeableFactorSea.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ChargeableFactor(
					new ConversionFactor(200, Weight.Kilograms, Volume.CubicMetres),
					new ConversionFactor(100, Weight.Pounds, Volume.CubicInches))))
				{
					using (ConsolFormForTesting form = new ConsolFormForTesting(consol))
					{
						form.Show();
						AssertEquals("Vol. Weight", form.JK_Calc_VolumeWeightEdit.GetExtension<ILabelCaptionRenderer>().Caption);
						AssertEquals("Weight converted to volume, Considering 1 M3 = 100 KG", form.JK_Calc_VolumeWeightEdit.GetExtension<IHintExtension>().Description);

						consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
						AssertEquals("Weight Vol.", form.JK_Calc_VolumeWeightEdit.GetExtension<ILabelCaptionRenderer>().Caption);
						AssertEquals("Weight converted to volume, Considering 1 M3 = 200 KG", form.JK_Calc_VolumeWeightEdit.GetExtension<IHintExtension>().Description);
					}
				}
			}
		}

		public void TestCombineShipmentForm()
		{
			OrgHeader organization = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();

			Func<ForwardingShipment> createShipment = () =>
			{
				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.ConsigneePK = organization.PK;
				shipment.ConsignorPK = organization.PK;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.Consols.Add(consol);
				return shipment;
			};

			ForwardingShipment shipment1 = createShipment();
			shipment1.CoLoadShipments.AddNew();

			ForwardingShipment shipment2 = createShipment();
			shipment2.JS_RL_NKDestination = "NZAKL";

			using (ConsolForm form = new ConsolForm(consol))
			{
				form.Show();
				MenuItem combineMenuItem = form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Combine Shipment");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				combineMenuItem.PerformClick();
				AssertEquals("Please select a shipment.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(0);
				combineMenuItem.PerformClick();
				AssertEquals("Please save before combining shipments.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				combineMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Shipment cannot be combined with other shipments."));

				shipment1.CoLoadShipments.RemoveAll();
				Factory.Save();

				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(0);
				combineMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Shipment does not have any matching shipments to combine with."));

				shipment2.JS_RL_NKDestination = "USLAX";
				Factory.Save();

				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(0);
				combineMenuItem.PerformClick();
				AssertEquals(typeof(CombineShipmentsForm), ZFormModaliser.ActiveForm.GetType());
			}
		}

		#endregion

		#region Transport Booking

		public void TestTransportBookingActionAdded()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			using (ConsolForm form = new ConsolForm(consol))
			{
				AssertNotNull(form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Transport Booking"));
			}
		}

		public void TestTransportBookingPlugIns()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (ConsolFormForTesting consolForm = new ConsolFormForTesting(consol))
			{
				var departureTabControl = (ZTabControl)consolForm.GetChildControl("DepartureTabControl");
				AssertNotNull(departureTabControl);
				AssertEquals(1, departureTabControl.PlugIns.Instances.Length);
				AssertEquals(ControllerIDs.DtbBookingTabPlugIn, departureTabControl.PlugIns.Instances[0].ControllerID);

				var arrivalTabControl = (ZTabControl)consolForm.GetChildControl("ArrivalTabControl");
				AssertNotNull(arrivalTabControl);
				AssertEquals(1, arrivalTabControl.PlugIns.Instances.Length);
				AssertEquals(ControllerIDs.DtbBookingTabPlugIn, arrivalTabControl.PlugIns.Instances[0].ControllerID);
			}
		}

		#endregion

		#region Create Job Header

		public void TestCreateShipmentJobHeaders()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "IGOR1";
			ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(shipment1.PK);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "IGOR2";
			new JobHeader.Loader(shipment2).TryCreate();

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "IGOR3";
			shipment3.SetReadOnlyIncludingChildren(true);

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_UniqueConsignRef = "IGOR4";

			var shipment5 = consol.Shipments.AddNew();
			shipment5.JS_UniqueConsignRef = "IGOR5";

			var shipment6 = consol.Shipments.AddNew();
			shipment6.JS_UniqueConsignRef = "IGOR6";

			var shipment7 = consol.Shipments.AddNew();
			shipment7.JS_UniqueConsignRef = "ALEX7";
			var job7 = new JobHeader.Loader(shipment7).TryCreate();
			job7.IsManuallyCreated = true;

			var shipment8 = consol.Shipments.AddNew();
			shipment8.JS_UniqueConsignRef = "ALEX8";
			var job8 = new JobHeader.Loader(shipment8).TryCreate();

			var shipment9 = consol.Shipments.AddNew();
			shipment9.JS_UniqueConsignRef = "ALEX9";
			var job9 = new JobHeader.Loader(shipment9).TryCreate();

			try
			{
				mutex.Lock();
				using (ConsolFormForTesting form = new ConsolFormForTesting(consol))
				{
					form.Show();

					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Create Job Invoicing Record").PerformClick();

					AssertEquals("Please select shipments to create Job Invoicing Record.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.UnSelectAll();
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(1);
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Create Job Invoicing Record").PerformClick();
					AssertEquals(@"Existing unsaved Job Invoicing Record has been marked as manually created for the following shipment(s):
- Shipment IGOR2
", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.UnSelectAll();
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(0);
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(1);
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(2);
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(3);
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(4);
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(6);
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(7);
					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(8);

					form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Create Job Invoicing Record").PerformClick();

					AssertEquals(@"Job Invoicing Record has been created for the following shipment(s):
- Shipment IGOR4
- Shipment IGOR5

Existing unsaved Job Invoicing Record has been marked as manually created for the following shipment(s):
- Shipment ALEX8
- Shipment ALEX9

For other shipment(s) it can't be done due to the following errors:
You have created the job IGOR1 on another form, but haven't saved it yet.
Please close or save other forms that use job IGOR1 to continue.
Shipment IGOR3 is read only
", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNull("shipment1 job header", shipment1.ShipmentJobHeader);
					AssertNotNull("shipment2 job header", shipment2.ShipmentJobHeader);
					Assert("IsManuallyCreated", shipment2.ShipmentJobHeader.IsManuallyCreated);
					AssertNull("shipment3 job header", shipment3.ShipmentJobHeader);
					AssertNotNull("shipment4 job header", shipment4.ShipmentJobHeader);
					Assert("IsManuallyCreated", !shipment4.ShipmentJobHeader.IsManuallyCreated);
					AssertNotNull("shipment5 job header", shipment5.ShipmentJobHeader);
					Assert("IsManuallyCreated", !shipment5.ShipmentJobHeader.IsManuallyCreated);
					AssertNull("shipment6 job header", shipment6.ShipmentJobHeader);
					Assert("IsManuallyCreated", job8.IsManuallyCreated);
					Assert("IsManuallyCreated", job9.IsManuallyCreated);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}

			mutex = JobHeader.GetMutex_ForTestOnly(shipment4.PK);
			AssertNotNull("Job Header Mutex", mutex);
			AssertEquals(false, mutex.IsLocked);

			mutex = JobHeader.GetMutex_ForTestOnly(shipment5.PK);
			AssertNotNull("Job Header Mutex", mutex);
			AssertEquals(false, mutex.IsLocked);
		}

		#endregion

		#region Opening Existing Consols

		public void TestOpeningExistingGatewayConsolIsInReadOnlyState()
		{
			var sendingOrganisation = Factory.NewWithValidTestData<OrgHeader>();

			var appointedPortSender = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			appointedPortSender.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedPortSender.O5_SeaAirCarrierOrForwarderType = "GTW";
			appointedPortSender.O5_SeaAgentStatus = "GTA";
			appointedPortSender.O5_OH = sendingOrganisation.PK;
			appointedPortSender.O5_OA_AgentOfficeAddress = sendingOrganisation.MainAddress.PK;
			appointedPortSender.O5_PortOrCountry = "AUSYD";

			sendingOrganisation.AppointedAgentPorts.Add(appointedPortSender);

			var receivingOrganisation = Factory.NewWithValidTestData<OrgHeader>();

			var appointedAgentReceiving = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			appointedAgentReceiving.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentReceiving.O5_SeaAirCarrierOrForwarderType = "GTW";
			appointedAgentReceiving.O5_SeaAgentStatus = "GTA";
			appointedAgentReceiving.O5_OH = receivingOrganisation.PK;
			appointedAgentReceiving.O5_OA_AgentOfficeAddress = receivingOrganisation.MainAddress.PK;
			appointedAgentReceiving.O5_PortOrCountry = "USNYC";

			receivingOrganisation.AppointedAgentPorts.Add(appointedAgentReceiving);

			var gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			gatewayConsol.JK_AgentType = AgentType.Agent;
			gatewayConsol.JK_TransportMode = TransportCodes.Sea;
			gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			gatewayConsol.JK_RL_NKDischargePort = "USNYC";
			gatewayConsol.JK_OA_SendingForwarderAddress = sendingOrganisation.MainAddress.PK;
			gatewayConsol.JK_OA_ReceivingForwarderAddress = receivingOrganisation.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = "GTA";
			gatewayConsol.JK_ReceivingForwarderHandlingType = "GTA";
			gatewayConsol.Shipments.Add(Factory.NewWithValidTestData<ForwardingShipment>());

			Factory.Save();

			using (var consolForm = new ConsolFormForTesting(new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.PK, gatewayConsol.PK))))
			{
				consolForm.Show();

				var consol = (ForwardingConsol)consolForm.BusinessEntity;

				Assert("Newly loaded consols should not have changes.", !consolForm.BusinessEntity.HasChanges);
			}
		}

		#endregion

		#region Sending Agent / Recieving Agent

		public void TestOrgDefaultAddressesPopulate()
		{
			var sendingOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var receivingOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (ConsolForm form = new ConsolForm(consol))
			{
				consol.SendingForwarderWithContact.OrgPK = sendingOrganisation.PK;
				consol.ReceivingForwarderWithContact.OrgPK = receivingOrganisation.PK;

				AssertEquals("Sending Agent Address defaults to the main address of the organisation when OrgPK is entered into sending agent field", sendingOrganisation.MainAddress.PK, consol.SendingForwarderWithContact.AddressFK);
				AssertEquals("Recieving Agent Address defaults to the main address of the organisation when OrgPK is entered into recieving agent field", receivingOrganisation.MainAddress.PK, consol.ReceivingForwarderWithContact.AddressFK);
			}
		}
		#endregion

		#region JK_BookingReference

		public void TestJK_BookingReferenceBoundTextEdit_LeaveShowsMessageBoxAndSplitsBookingReference()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_BookingReference = "123";

			var message = @"Multiple Carrier Booking References have been detected.
We will automatically split them and add them to the Reference Numbers grid under the Numbers tab.";

			using (var consolForm = new ConsolFormForTesting(consol))
			{
				consolForm.Show();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				consolForm.JK_BookingReferenceBoundTextEdit.Focus();
				consolForm.SelectNextControl(consolForm.JK_BookingReferenceBoundTextEdit, true, true, true, true);

				Assert("No message box since only one Booking Reference Number present", UnitTestUserNotification.Instance.LastMessage.WasNone);

				consol.JK_BookingReference = "1, 2:3,4; 5";
				var preexistingNumber = consol.Numbers.AddNew();
				preexistingNumber.CE_EntryType = "BKG";
				preexistingNumber.CE_EntryNum = "2";

				consolForm.JK_BookingReferenceBoundTextEdit.Focus();
				consolForm.SelectNextControl(consolForm.JK_BookingReferenceBoundTextEdit, true, true, true, true);

				AssertEquals("Shows message box because Booking Reference needs splitting", message, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertEquals("Booking Reference has been changed", "1", consol.JK_BookingReference);
				AssertContainsExactElementsInAnyOrder("Extra booking references added to Numbers", new string[] { "2", "3", "4", "5" }, consol.Numbers.GetAllReferenceNumbersByType("BKG"));
			}
		}

		#endregion

		#region Set Inspection Status

		public void TestSetInspectionStatus_MenuItemAdded()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			var packLine3 = shipment.OuterPackLines.AddNew();

			Factory.Save();

			using (var form = new ConsolForm(consol1))
			{
				form.Show();
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.Position = 0;
				var menuItem = form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Set Inspection Status");
				AssertNotNull(menuItem);
			}

			using (var form = new ConsolForm(consol2))
			{
				form.Show();
				form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.Position = 0;
				var menuItem = form.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Set Inspection Status");
				AssertNotNull(menuItem);
			}
		}

		#endregion

		#region DepartureDetailsPanelAndArrivalDetailsPanel

		public void TestDepartureDetailsPanelAndArrivalDetailsPanel()
		{
			using (var consolControl = new ConsolUserControl())
			{
				var departureDetailsTabPage = consolControl.Controls.Find("DepartureDetailsTabPage", true).FirstOrDefault();
				AssertNotNull(departureDetailsTabPage);

				var departureDetailsPanel = departureDetailsTabPage.Controls.Find("DepartureDetailsPanel", true).FirstOrDefault() as ZPanel;
				AssertNotNull(departureDetailsPanel);
				Assert(departureDetailsPanel.AutoScroll);

				var arrivalDetailsTabPage = consolControl.Controls.Find("ArrivalDetailsTabPage", true).FirstOrDefault();
				AssertNotNull(arrivalDetailsTabPage);

				var arrivalDetailsPanel = arrivalDetailsTabPage.Controls.Find("ArrivalDetailsPanel", true).FirstOrDefault() as ZPanel;
				AssertNotNull(arrivalDetailsPanel);
				Assert(arrivalDetailsPanel.AutoScroll);
			}
		}

		#endregion

		#region Carrier Contracts

		public void TestCarrierContractLookupsConditionalVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ConsolFormForTesting consolForm = new ConsolFormForTesting(consol))
			{
				ShowPreAllocationTabPage(consolForm);
				AssertEquals(false, consolForm.CarrierContractImportButton.Visible);
				AssertEquals(true, consolForm.JK_CarrierContractNumberFindBox.Visible);
				AssertEquals(true, consolForm.JK_RCA_AllocationRouteCodeFindBox.Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ConsolFormForTesting consolForm = new ConsolFormForTesting(consol))
			{
				ShowPreAllocationTabPage(consolForm);
				AssertEquals(true, consolForm.CarrierContractImportButton.Visible);
				AssertEquals(false, consolForm.JK_CarrierContractNumberFindBox.Visible);
				AssertEquals(false, consolForm.JK_RCA_AllocationRouteCodeFindBox.Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ConsolFormForTesting consolForm = new ConsolFormForTesting(consol))
			{
				ShowPreAllocationTabPage(consolForm);
				AssertEquals(false, consolForm.CarrierContractImportButton.Visible);
				AssertEquals(false, consolForm.JK_CarrierContractNumberFindBox.Visible);
				AssertEquals(false, consolForm.JK_RCA_AllocationRouteCodeFindBox.Visible);
			}
		}

		void ShowPreAllocationTabPage(ConsolFormForTesting consolForm)
		{
			consolForm.Show();
			var preallocationTabPage = consolForm.Controls.Find("PreAllocationTabPage", true).FirstOrDefault();
			preallocationTabPage.Show();
		}

		#endregion

		#region Rate Preferences

		public void TestRatesTabPageExists()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var consolForm = new ConsolFormForTesting(consol))
			{
				var ratesTabPage = consolForm.Controls.Find("RatesTabPage", true).FirstOrDefault();
				AssertNotNull(ratesTabPage);
			}
		}

		public void TestAutoratingDateOverriddenDateEdit()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var consolForm = new ConsolFormForTesting(consol))
			{
				Assert("A new consol should have empty Autorating Date Override", consol.AutoratingDate.IsEmpty);
				ShowRatesTabPage(consolForm);
				var control = consolForm.Controls.Find("AutoratingDateOverriddenDateEdit", true).FirstOrDefault() as ZDateEdit;
				Assert("Date should be empty", control.DateTimeValue.IsEmpty);

				var testDate = ZDate.Today;
				consol.AutoratingDate = testDate;
				consol.AutoratingDateInfo.RefreshBinding();
				AssertEquals("Changing Autorating Date Override should be reflected on the control", testDate, control.DateTimeValue);
			}
		}

		static void ShowRatesTabPage(ConsolFormForTesting consolForm)
		{
			consolForm.Show();
			var ratesTabPage = consolForm.Controls.Find("RatesTabPage", true).FirstOrDefault();
			ratesTabPage.Show();
		}

		#endregion

		#region Master Bill Updated
		public void TestMasterBillUpdateConfirmMessageBox_MasterBillUpdated_Visible()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.AWBMaster;
			consol.JK_MasterBillNum = "12312345678";
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Canada);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CATOR";
			var confirmMessage = "Press OK to update the CCN";
			using (var form = new ConsolFormForTesting(consol))
			{
				form.Show();
				var notCcnNumber = consol.Numbers.AddNew();
				notCcnNumber.CE_EntryType = "BKG";
				notCcnNumber.CE_EntryNum = "2";
				consol.MasterBillMAWB = "12345699";
				Factory.Save();
				Assert("No confirm message cuz no ccn numbers", !UnitTestUserNotification.Instance.PreviousMessages.Any(a => a.Text?.Equals(confirmMessage) ?? false));

				FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConsolidationCCNCustomizationTypes.Code.Non);
				var ccnNumber = consol.Numbers.AddNew();
				ccnNumber.CE_EntryType = "CCN";
				ccnNumber.CE_EntryNum = "2";
				consol.MasterBillMAWB = "12345600";
				Factory.Save();
				Assert("No confirm message cuz registry non", !UnitTestUserNotification.Instance.PreviousMessages.Any(a => a.Text?.Equals(confirmMessage) ?? false));

				FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConsolidationCCNCustomizationTypes.Code.MasterBill);
				consol.MasterBillMAWB = "12345611";
				Factory.Save();
				Assert("Confirm message should popup", UnitTestUserNotification.Instance.PreviousMessages.Any(a => a.Text?.Contains(confirmMessage) ?? false));
			}
		}
		#endregion

		#region Implementation

		class ConsolFormForTesting : ConsolForm
		{
			public ConsolFormForTesting(ForwardingConsol businessEntity)
				: base(businessEntity)
			{
			}

			public ZDateEdit MasterBillIssueDateEdit
			{
				get { return (ZDateEdit)GetChildControl("MasterBillIssueDateEdit"); }
			}

			public ZCodeFindBox MasterBillIssuePlaceFindBox
			{
				get { return (ZCodeFindBox)GetChildControl("JK_RL_NKMasterBillIssuePlaceFindBox"); }
			}

			public ZDropEdit AWBDimsDropEdit
			{
				get { return (ZDropEdit)GetChildControl("AWBDimsDropEdit"); }
			}

			public ZDropEdit PackageGroupingDropEdit
			{
				get { return (ZDropEdit)GetChildControl("PackageGroupingDropEdit"); }
			}

			public ZGroupBox LatestStatusGroupBox
			{
				get { return (ZGroupBox)GetChildControl("LatestStatusGroupBox"); }
			}

			public ZCodeFindBox CarrierBookingOfficeCodeFindBox
			{
				get { return (ZCodeFindBox)GetChildControl("CarrierBookingOfficeCodeFindBox"); }
			}

			public ZDateEdit BillOfLadingBillDateDateEdit
			{
				get { return (ZDateEdit)GetChildControl("BillOfLadingBillDateDateEdit"); }
			}

			public ZDropEdit BillOfLadingBillStatusDropEdit
			{
				get { return (ZDropEdit)GetChildControl("BillOfLadingBillStatusDropEdit"); }
			}

			public ZDropEdit BillOfLadingBillTypeDropEdit
			{
				get { return (ZDropEdit)GetChildControl("BillOfLadingBillTypeDropEdit"); }
			}

			public ZDropEdit BillOfLadingBillTermsDropEdit
			{
				get { return (ZDropEdit)GetChildControl("BillOfLadingBillTermsDropEdit"); }
			}

			public ZDateEdit CarrierBookingDateDateEdit
			{
				get { return (ZDateEdit)GetChildControl("CarrierBookingDateDateEdit"); }
			}

			public ZDropEdit CarrierBookingLatestStatusDropEdit
			{
				get { return (ZDropEdit)GetChildControl("CarrierBookingLatestStatusDropEdit"); }
			}

			public ZTemplateTabControl DepartureArrivalTabControl
			{
				get { return (ZTemplateTabControl)GetChildControl("DepartureArrivalTabControl"); }
			}

			public ZTabPage DocsTabPage
			{
				get { return (ZTabPage)GetChildControl("DocsTabPage"); }
			}

			public ZCalcEdit JK_TotalShipmentLoadingMetersCalcEdit
			{
				get { return (ZCalcEdit)GetChildControl("JK_TotalShipmentLoadingMetersCalcEdit"); }
			}

			public ZPanel BottomInnerRightPanel
			{
				get { return (ZPanel)GetChildControl("BottomInnerRightPanel"); }
			}

			public ZAddressControl JK_OA_CreditorAddressControl
			{
				get { return (ZAddressControl)GetChildControl("JK_OA_CreditorAddressControl"); }
			}

			public ZTextBox JK_BookingReferenceBoundTextEdit
			{
				get { return (ZTextBox)GetChildControl("JK_BookingReferenceBoundTextEdit"); }
			}

			public ZOrgAddressWithContactInfoControl SendingForwarderAddressControl
			{
				get { return (ZOrgAddressWithContactInfoControl)GetChildControl("SendingForwarderAddressControl"); }
			}

			public ZOrgAddressWithContactInfoControl ReceivingForwarderAddressControl
			{
				get { return (ZOrgAddressWithContactInfoControl)GetChildControl("ReceivingForwarderAddressControl"); }
			}

			public ZCalcEdit JK_Calc_VolumeWeightEdit
			{
				get { return (ZCalcEdit)GetChildControl("JK_Calc_ActualVolumeWeightEdit"); }
			}

			public Control GetChildControl(string controlName)
			{
				return (Control)base.ConsolControl.GetType().GetField(controlName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ConsolControl);
			}

			public ContractAllocationGuidFindBox JK_RCA_AllocationRouteCodeFindBox => (ContractAllocationGuidFindBox)GetChildControl(nameof(JK_RCA_AllocationRouteCodeFindBox));
			public ZButton.Bare CarrierContractImportButton => (ZButton.Bare)GetChildControl(nameof(CarrierContractImportButton));
			public ConsolContractAllocationCodeFindBox JK_CarrierContractNumberFindBox => (ConsolContractAllocationCodeFindBox)GetChildControl(nameof(JK_CarrierContractNumberFindBox));

			public ZTextBox ElectronicBillOfLadingReferenceTextBox => (ZTextBox)GetChildControl("ElectronicBillOfLadingReferenceTextBox");
			public ZButton ViewEBLButton => (ZButton)GetChildControl("ViewEBLButton");
		}

		#endregion
	}
}
