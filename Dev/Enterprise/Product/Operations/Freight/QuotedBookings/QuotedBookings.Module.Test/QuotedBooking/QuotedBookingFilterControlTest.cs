using System;
using System.Linq;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	public class QuotedBookingFilterControlTest : BaseFreightTest
	{
		public void TestHoldReasonColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var holdReason = nameof(ViewQuotedBooking.QuotedBooking) + "+" + nameof(ViewQuotedBooking.QuotedBooking.Job) + "+" + nameof(ViewQuotedBooking.QuotedBooking.Job.JH_HoldReason);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == holdReason && !c.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+Job+JH_ProfitLossReasonCode" && !c.IsVisible);

				Assert("Profit/Loss Reason column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+Job+JH_TotalProfitRevenueMargin" && !c.IsVisible);

				Assert("Margin% column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestFMCTariffIDColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "QuotedBooking+FMCTariffID" && !c.IsVisible);

				Assert("FMC Tariff ID column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestCommoidityCodeColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "QuotedBooking+Commodity" && !c.IsVisible);

				Assert("Commodity column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestCompanyTariffLevelOverrideColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "QuotedBooking+CompanyTariffLevel" && !c.IsVisible);

				Assert("Company Tariff Level Override column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestQuoteCO2TotalColumn()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
				{
					var columnInfo = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "QuotedBooking+TotalCO2eForSorting");

					AssertNotNull("TotalCO2e column should exist", columnInfo);
					AssertEquals("TotalCO2e column caption", "CO2e (kg)", columnInfo.CaptionResourceString.Caption);
					AssertEquals("TotalCO2e column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("TotalCO2e column is Upper case", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				}
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
				{
					var columnExists = filterControl.FilteredGrid.ColumnStyles
						.OfType<ZGridColumnInfo>()
						.Any(c => c.ColumnName == "QuotedBooking+TotalCO2eForSorting");

					Assert("Quote Total CO2e column should not exist", !columnExists);
				}
			}
		}

		public void TestTransportModeAndContainerModeColumnInOneOffQuoteModuleShouldExistAndBeAvailable()
		{
			using (var filterControl = new OneOffQuoteFilterControlForTest(Factory))
			{
				var columnExistsAndAvailable = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+TransportMode" && !c.IsUnavailable);

				Assert("Transport Mode column should exist and available in OneOffQuote Module", columnExistsAndAvailable);

				columnExistsAndAvailable = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+ContainerMode" && !c.IsUnavailable);

				Assert("Container Mode column should exist and available in OneOffQuote Module", columnExistsAndAvailable);
			}
		}

		public void TestTransportModeAndContainerModeColumnInBookingModuleShouldExistAndBeAvailable()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExistsAndUnAvailable = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+TransportMode" && !c.IsUnavailable);

				Assert("Transport Mode column should exist and available in Booking Module", columnExistsAndUnAvailable);

				columnExistsAndUnAvailable = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+ContainerMode" && !c.IsUnavailable);

				Assert("Container Mode column should exist and available in Booking Module", columnExistsAndUnAvailable);
			}
		}

		public void TestCreditorColumnInQuotedBookingFilterControlShouldExistAndBeAvailable()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExistsAndIsAvailable = filterControl.FilteredGrid.ColumnStyles
						.OfType<ZGridColumnInfo>()
						.Any(c => c.ColumnName == "QuotedBooking+Creditor" && !c.IsUnavailable);

				Assert("Quote Creditor column should exist and visible", columnExistsAndIsAvailable);
			}
		}

		public void TestCreditorColumnInOneOffQuoteFilterControlShouldExistAndBeAvailable()
		{
			using (var filterControl = new OneOffQuoteFilterControlForTest(Factory))
			{
				var columnExistsAndIsAvailable = filterControl.FilteredGrid.ColumnStyles
						.OfType<ZGridColumnInfo>()
						.Any(c => c.ColumnName == "QuotedBooking+Creditor" && !c.IsUnavailable);

				Assert("Quote Creditor column should exist and visible", columnExistsAndIsAvailable);
			}
		}

		public void TestStartDateColumn_InOneOffQuoteFilterControl_ShouldBeAvailableAndNotVisible()
		{
			using (var filterControl = new OneOffQuoteFilterControlForTest(Factory))
			{
				var columnIsAvailableAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+StartDate" && !c.IsUnavailable && !c.IsVisible);

				Assert("Start Date column should be available and not visible in OneOffQuoteFilterControl", columnIsAvailableAndNotVisible);
			}
		}

		public void TestStartDateColumn_InQuotedBookingFilterControl_ShouldNotExist()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExists = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+StartDate");

				Assert("Start Date column should not exist in QuotedBookingFilterControl", !columnExists);
			}
		}

		public void TestEndDateColumn_InOneOffQuoteFilterControl_ShouldBeAvailableAndNotVisible()
		{
			using (var filterControl = new OneOffQuoteFilterControlForTest(Factory))
			{
				var columnIsAvailableAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+EndDate" && !c.IsUnavailable && !c.IsVisible);

				Assert("End Date column should be available and not visible in OneOffQuoteFilterControl", columnIsAvailableAndNotVisible);
			}
		}

		public void TestEndDateColumn_InQuotedBookingFilterControl_ShouldNotExist()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExists = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+EndDate");

				Assert("End Date column should not exist in QuotedBookingFilterControl", !columnExists);
			}
		}

		public void TestQuoteCO2StatusColumn()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
				{
					var columnInfo = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "QuotedBooking+CO2eStatus");

					AssertNotNull("CO2e Status column should exist", columnInfo);
					AssertEquals("CO2e Status column caption", "CO2e Status", columnInfo.CaptionResourceString.Caption);
					AssertEquals("CO2e Status column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("CO2e Status column is Upper case", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				}
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
				{
					var columnExists = filterControl.FilteredGrid.ColumnStyles
						.OfType<ZGridColumnInfo>()
						.Any(c => c.ColumnName == "QuotedBooking+CO2eStatus");

					Assert("Quote CO2e Status column should not exist", !columnExists);
				}
			}
		}

		public void TestQuoteKPIColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				bool columnExistsAndVisible = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+OneOffQuoteStatistics+OneOffQuoteKPI" && c.IsVisible);

				Assert("Quote KPI column should exist and visible", columnExistsAndVisible);
			}
		}

		public void TestQuoteSourceColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				bool columnExistsAndVisible = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+OneOffQuoteStatistics+OneOffQuoteSource" && c.IsVisible);

				Assert("Quote Source column should exist and visible", columnExistsAndVisible);
			}
		}

		public void TestQuoteRevisionReasonColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				bool columnExistsAndVisible = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+OneOffQuoteStatistics+OneOffQuoteRevisionReason" && c.IsVisible);

				Assert("Revision Reason column should exist and visible", columnExistsAndVisible);
			}
		}

		public void TestStatusColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+Quote+QuoteStatus" && !c.IsVisible);

				Assert("Status column should exist and not visible", columnExistsAndNotVisible);
			}
		}

		public void TestNewZFilterStrip()
		{
			using (QuotedBookingFilterControlForTest control = new QuotedBookingFilterControlForTest(Factory))
			using (ZFilterStrip filterStrip = control.NewZFilterStripForTest())
			{
				AssertType(typeof(QuotedBookingFilterStrip), filterStrip);
			}
		}

		public void TestReleaseTypeAndChargesApply()
		{
			using (QuotedBookingFilterControlForTest control = new QuotedBookingFilterControlForTest(Factory))
			{
				bool releaseTypeFoundAndInvisible = control.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+Booking+JS_ReleaseType" && !c.IsVisible);

				bool chargesApplyFoundAndInvisible = control.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+HBLAWBChargesDisplay" && !c.IsVisible);

				Assert("Release Type column should exist and should NOT be visible", releaseTypeFoundAndInvisible);
				Assert("Charges Apply column should exist and should NOT be visible", chargesApplyFoundAndInvisible);
			}
		}

		public void TestWorkflowCustomFieldColums()
		{
			CreateQuotedBookingWorkflowWithCustomFields();

			using (QuotedBookingFilterControlForTest control = new QuotedBookingFilterControlForTest(Factory))
			{
				string[] workflowColumns = control.FilteredGrid.ColumnStyles.OfType<ZGridColumnInfo>()
					.Where(col => !string.IsNullOrEmpty(col.Caption) && col.Caption.StartsWith("UDF: ", StringComparison.Ordinal))
					.Select(col => col.Caption).ToArray();

				AssertContainsExactElementsInAnyOrder(new[] { "UDF: custom text", "UDF: custom int", "UDF: custom decimal", "UDF: custom datetime" }, workflowColumns);
			}
		}

		public void TestScreeningStatusColumn()
		{
			TestScreeningStatusColumnCore(enableComplianceRisk: false);
			TestScreeningStatusColumnCore(enableComplianceRisk: true);

			void TestScreeningStatusColumnCore(bool enableComplianceRisk)
			{
				var quotedBookingHasImplementedIComplianceRiskStatusProvider = typeof(IComplianceItemRiskStatusProvider).IsAssignableFrom(typeof(QuotedBooking));

				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
				using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
				{
					var screeningStatusColumnStyle = filterControl.FilteredGrid.GetColumnStyle("QuotedBooking+Booking+JS_ScreeningStatus");

					Assert("ScreeningStatus column should exist and should NOT be visible", !(screeningStatusColumnStyle?.IsVisible ?? true));
				}
			}
		}

		public void TestMilestoneColums()
		{
			using (var control = new QuotedBookingFilterControlForTest(Factory))
			{
				var milestoneColumns = control.FilteredGrid.ColumnStyles.OfType<ZGridColumnInfo>()
					.Where(col => col.CaptionResourceString != null && !string.IsNullOrEmpty(col.CaptionResourceString.Caption)
						&& (col.CaptionResourceString.Caption.StartsWith("Last Milestone", StringComparison.Ordinal) || col.CaptionResourceString.Caption.StartsWith("Next Milestone", StringComparison.Ordinal)))
					.Select(col => col.CaptionResourceString.Caption).ToArray();

				AssertContainsExactElementsInAnyOrder(new[] {
					"Last Milestone",
					"Last Milestone (incl Rltd.)",
					"Last Milestone ATD",
					"Last Milestone ATD (incl Rltd.)",
					"Last Milestone ATD Current Company",
					"Last Milestone ATD Current Company (incl Related).",
					"Last Milestone Current Company",
					"Last Milestone Current Company (incl Related).",
					"Last Milestone Desc.",
					"Last Milestone Desc. (incl Rltd.)",
					"Last Milestone Description Current Company",
					"Last Milestone Description Current Company (incl Related).",
					"Next Milestone",
					"Next Milestone (incl Rltd.)",
					"Next Milestone Current Company",
					"Next Milestone Current Company (incl Related).",
					"Next Milestone Desc.",
					"Next Milestone Desc. (incl Rltd.)",
					"Next Milestone Description Current Company",
					"Next Milestone Description Current Company (incl Related).",
					"Next Milestone ETD",
					"Next Milestone ETD (incl Rltd.)",
					"Next Milestone ETD Current Company",
					"Next Milestone ETD Current Company (incl Related)."
				}, milestoneColumns);
			}
		}

		public void TestAdditionalReferenceColumn()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.OfType<ZGridColumnInfo>()
						.Any(c => c.ColumnName == "QuotedBooking+Booking+NumbersAsString" && !c.IsVisible);

				Assert("Additional Reference Numbers column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestAuditColumns()
		{
			using (var control = new QuotedBookingFilterControlForTest(Factory))
			{
				var auditColumns = control.Grid.ColumnStyles.OfType<ZGridColumnInfo>()
					.Where(col => col.GroupName.Caption == "Audit Details")
					.Select(col => col.Caption).ToArray();

				AssertContainsExactElementsInAnyOrder(new[] {
					"Created By",
					"Created Time",
					"Last Edit",
					"Last Edited Time"
				}, auditColumns);
			}
		}

		public void TestDeliveryDueDateColumnAvailability()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertDeliveryDueDateColumnAvailability("Allow override of Delivery Due Date set to true, Delivery Due Date column should exist", true);
			}
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertDeliveryDueDateColumnAvailability("Allow override of Delivery Due Date set to false, Delivery Due Date column should not exist", false);
			}
		}

		void AssertDeliveryDueDateColumnAvailability(string message, bool expectedVisibility)
		{
			using (QuotedBookingFilterControlForTest control = new QuotedBookingFilterControlForTest(Factory))
			{
				var deliveryDueDateVisibility = control.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+DeliveryDueDate");

				AssertEquals(message, expectedVisibility, deliveryDueDateVisibility);
			}
		}

		void CreateQuotedBookingWorkflowWithCustomFields()
		{
			ProcessTaskTemplate processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QBK";

			GenCustomColumnDefinition customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "UDF: custom text";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "UDF: custom int";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			GenCustomColumnDefinition customField3 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "UDF: custom decimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;

			GenCustomColumnDefinition customField4 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "UDF: custom datetime";
			customField4.XC_Type = AddOnColumnDataType.Codes.Datetime;

			ProcessTaskTemplate processTaskTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate2.P0_ProcessType = "SHP";

			GenCustomColumnDefinition customField5 = processTaskTemplate2.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "UDF: custom shipment string";
			customField5.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		public void TestHBLDeliveryModeColumnShouldBeAvailableAndNotVisible()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "QuotedBooking+ContainerPackModeOverride" && !c.IsVisible);

				Assert("HBL Delivery Mode column should exist and not visible", columnExistsAndNotVisible);
			}
		}

		public void TestConsignorContactColumn_InOneOffQuoteFilterControl_ShouldBeAvailableAndNotVisible()
		{
			using (var filterControl = new OneOffQuoteFilterControlForTest(Factory))
			{
				var columnIsAvailableAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "QuotedBooking+ConsignorContact" && !c.IsUnavailable && !c.IsVisible);

				Assert("Consignor Contact column should be available and not visible in OneOffQuoteFilterControl", columnIsAvailableAndNotVisible);
			}
		}

		public void TestConsignorContactColumn_InQuotedBookingFilterControl_ShouldBeAvailableAndNotVisible()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnIsAvailableAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "QuotedBooking+ConsignorContact" && !c.IsUnavailable && !c.IsVisible);

				Assert("Consignor Contact column should be available and not visible in QuotedBookingFilterControl", columnIsAvailableAndNotVisible);
			}
		}

		public void TestConsigneeContactColumn_InOneOffQuoteFilterControl_ShouldBeAvailableAndNotVisible()
		{
			using (var filterControl = new OneOffQuoteFilterControlForTest(Factory))
			{
				var columnIsAvailableAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "QuotedBooking+ConsigneeContact" && !c.IsUnavailable && !c.IsVisible);

				Assert("Consignee Contact column should be available and not visible in OneOffQuoteFilterControl", columnIsAvailableAndNotVisible);
			}
		}

		public void TestConsigneeContactColumn_InQuotedBookingFilterControl_ShouldNotExist()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExists = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+ConsigneeContact");

				Assert("Consignee Contact column should not exist in QuotedBookingFilterControl", !columnExists);
			}
		}

		public void TestClientContactColumn_InOneOffQuoteFilterControl_ShouldBeAvailableAndNotVisible()
		{
			using (var filterControl = new OneOffQuoteFilterControlForTest(Factory))
			{
				var columnIsAvailableAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "QuotedBooking+ClientContact" && !c.IsUnavailable && !c.IsVisible);

				Assert("Client Contact column should be available and not visible in OneOffQuoteFilterControl", columnIsAvailableAndNotVisible);
			}
		}

		public void TestClientContactColumn_InQuotedBookingFilterControl_ShouldNotExist()
		{
			using (var filterControl = new QuotedBookingFilterControlForTest(Factory))
			{
				var columnExists = filterControl.FilteredGrid.ColumnStyles
					.OfType<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "QuotedBooking+ClientContact");

				Assert("Client Contact column should not exist in QuotedBookingFilterControl", !columnExists);
			}
		}
	}
}
