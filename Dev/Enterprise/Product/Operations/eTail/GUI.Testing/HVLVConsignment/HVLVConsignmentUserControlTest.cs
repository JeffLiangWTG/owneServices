using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	class HVLVConsignmentUserControlTest : TestCaseWithFactory
	{
		public void TestHVC_DeniedPartyScreeningStatusColumn()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				var consignmentsGrid = form.ConsignmentsGrid;
				var consignmentsGridColumnStyles = consignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var deniedPartyScreeningStatusColumn = consignmentsGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == nameof(HVLVConsignment.HVC_DeniedPartyScreeningStatus)) as ZTextBoxColumnStyleInfo;

				AssertNotNull(deniedPartyScreeningStatusColumn);
			}
		}

		public void TestPriceProperties_DecimalsAlwaysBeTwo_ConsignmentGrid()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				var consignmentsGrid = form.ConsignmentsGrid;
				var consignmentsGridColumnStyles = consignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var goodsValueColumn = consignmentsGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == nameof(HVLVConsignment.HVC_GoodsValue)) as ZCalcEditColumnStyleInfo;
				var totalLineValueColumn = consignmentsGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == nameof(HVLVConsignment.TotalLineCustomsValues)) as ZCalcEditColumnStyleInfo;
				var transportValueColumn = consignmentsGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == nameof(HVLVConsignment.HVC_TransportValue)) as ZCalcEditColumnStyleInfo;
				var insuranceValueColumn = consignmentsGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == nameof(HVLVConsignment.HVC_InsuranceValue)) as ZCalcEditColumnStyleInfo;

				var goodsValueTextBox = form.Controls.Find("zTextBoxGoodsValue", true)[0] as ZCalcEdit;
				var totalLineValuesTestBox = form.Controls.Find("zTextBoxTotalLineValues", true)[0] as ZCalcEdit;
				var transportValueTextBox = form.Controls.Find("zTextBoxTransportValue", true)[0] as ZCalcEdit;
				var insuranceValueTextBox = form.Controls.Find("zTextBoxInsuranceValue", true)[0] as ZCalcEdit;

				CombineAssertions("Decimals of consignment's price properties always be two", () =>
				{
					AssertEquals("goods value column in grid", 2, goodsValueColumn.Decimals);
					AssertEquals("total line value column in grid", 2, totalLineValueColumn.Decimals);
					AssertEquals("transport value column in grid", 2, transportValueColumn.Decimals);
					AssertEquals("insurance value column in grid", 2, insuranceValueColumn.Decimals);

					AssertEquals("goods value text box in details tab", 2, goodsValueTextBox.Decimals);
					AssertEquals("total line value text box in details tab", 2, totalLineValuesTestBox.Decimals);
					AssertEquals("transport value text box in details tab", 2, transportValueTextBox.Decimals);
					AssertEquals("insurance value text box in details tab", 2, insuranceValueTextBox.Decimals);
				});
			}
		}

		public void TestCharacterCasing_ForConsignmentGridDropEditColumnsAndUnitColumns_ShouldAlwaysBeUpper()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				form.Show();

				var consignmentsGrid = form.ConsignmentsGrid;
				var consignmentsGridColumnStyles = consignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var columnNames = new string[] { "HVC_WeightUQ", "HVC_VolumeUQ", "HVC_UndgClass", "HVC_INCO" };

				CombineAssertions("Consignment grid columns should have correct casing", () =>
				{
					foreach (var columnName in columnNames)
					{
						var columnCasing = consignmentsGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == columnName).CharacterCasing;
						AssertEquals($"${columnName} should always be Upper Case", CharacterCasing.Upper, columnCasing);
					}
				});
			}
		}

		public void TestChargeableMenuItemClick()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_WeightUQ = Weight.Kilograms;
			consignment1.HVC_VolumeUQ = Volume.CubicCentimeters;
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertEquals("Chargeable for display not returning correct value for null input", "Not Calculated", consignment1.ChargeableForDisplay);
			AssertEquals("Pre-condition", ConversionFactor.Standard.Metric.Air, FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value.MetricFactor);

			var item1 = consignment1.Items.AddNew();
			var item2 = consignment1.Items.AddNew();

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_WeightUQ = Weight.Kilograms;
			consignment2.HVC_VolumeUQ = Volume.CubicCentimeters;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertEquals("Chargeable for display not returning correct value for null input", "Not Calculated", consignment2.ChargeableForDisplay);
			AssertEquals("Pre-condition", ConversionFactor.Standard.Metric.Air, FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value.MetricFactor);

			var item3 = consignment2.Items.AddNew();
			var item4 = consignment2.Items.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			using (HVLVDataRegistry.Instance.CalculateHVLVChargeablePerItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item1.HVI_ManifestedWeight = 8;
				item1.HVI_ActualWeight = 5.3;
				item1.HVI_ActualVolume = 36000; // 6 KG chargeable
				item2.HVI_ManifestedWeight = 7.4;
				item2.HVI_ManifestedVolume = 27000; // 4.5 KG chargeable

				item3.HVI_ManifestedWeight = 8;
				item3.HVI_ActualWeight = 5.3;
				item3.HVI_ActualVolume = 36000; // 6 KG chargeable
				item4.HVI_ManifestedWeight = 7.4;
				item4.HVI_ManifestedVolume = 27000; // 4.5 KG chargeable

				form.Show();

				var grid = form.ConsignmentsGrid;
				var menuItem = grid.ContextMenu.MenuItems.OfType<MenuItem>().Single(item => item.Text == HVLVMenuItemHelper.Captions.CalculateChargeable);

				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Chargeable calculated from item1 actual volume and item2 manifested weight", "13.4 KG", consignment1.ChargeableForDisplay);
					AssertEquals("Chargeable calculated from item3 actual volume and item4 manifested weight", "13.4 KG", consignment2.ChargeableForDisplay);
				});
			}
		}

		public void TestCalculateLMCDepotDetailsContextMenuItemVisible()
		{
			var bookingHeaderWithoutConsignment = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (var form = new ConsignmentUserControlTestForm(bookingHeaderWithoutConsignment))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.CalculateLMCDepotDetails);
				AssertEquals("Should be hidden if no consignments", false, menuItem.Visible);
			}

			var bookingHeaderWithConsignments = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeaderWithConsignments.Consignments.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeaderWithConsignments))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.CalculateLMCDepotDetails);
				AssertEquals("Should be visible if there any consignments", true, menuItem.Visible);
			}
		}

		public void TestCalculateLMCDepotDetailsContextMenuItemClick()
		{
			var creator = new PortHubSelectionTestDataCreator(Factory);

			var billToParty = creator.GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress = creator.GenerateAddress("AD2", "XY2", "XY 2");
			var depotAddress = creator.GenerateAddress("AD3", "XY3", "XY 3");
			var carrier = creator.GenerateOrganisation("XY4", "XY 4");
			var agent = creator.GenerateOrganisation("ZZ5", "ZZ 5");

			var portHubSelectionPK = creator.CreatePortAndDepotSelectionWithUndgClass(depotAddress.PK, dispatchDepotAddress.PK, "EXP", "DLV", "ALL", "AAA", "ALL");
			var portHubSelection = Factory.Load<PortHubSelection>(portHubSelectionPK);
			portHubSelection.TY_OH_CarrierBookingAgent = agent.PK;
			var zonePK = creator.AddZone(portHubSelectionPK, "Z2", carrier.PK, "EXP");
			creator.AddZoneItem(zonePK, "Melbourne Metro", "VIC", "AU");

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00000050";
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader.HVH_OA_DispatchAddress = dispatchDepotAddress.PK;

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN100";
			consignment.HVC_UndgClass = "6";
			consignment.HVC_ConsigneeAddress1 = "Test Address 11";
			consignment.HVC_ConsigneeCity = "Melbourne Metro";
			consignment.HVC_ConsigneeState = "VIC";
			consignment.HVC_ConsigneePostcode = "3560";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			Factory.Save();

			CombineAssertions("Precondition: LMC Depot Details should be empty", () =>
			{
				AssertEquals("HVC_OA_DestinationDepot", ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
				AssertEquals("HVC_OH_LastMileCarrier", ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
				AssertEquals("HVC_PL_NKLastMileCarrierServiceLevel", ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
				AssertEquals("HVC_OH_LastMileCarrierBookingAgent", ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
			});

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;

				var menuItem = grid.ContextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.CalculateLMCDepotDetails);
				AssertNotNull(menuItem);

				grid.ListManager.Position = 0;
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(depotAddress.PK, consignment.HVC_OA_DestinationDepot);
					AssertEquals(carrier.PK, consignment.HVC_OH_LastMileCarrier);
					AssertEquals("EXP", consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(agent.PK, consignment.HVC_OH_LastMileCarrierBookingAgent);
				});

				PortHubSelectionTestDataCreator.ClearUpAllZoneItems(Factory);
				menuItem.PerformClick();

				CombineAssertions("LMC Depot Details should be set back to empty", () =>
				{
					AssertEquals("HVC_OA_DestinationDepot", ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
					AssertEquals("HVC_OH_LastMileCarrier", ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
					AssertEquals("HVC_PL_NKLastMileCarrierServiceLevel", ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals("HVC_OH_LastMileCarrierBookingAgent", ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
				});
			}
		}

		public void TestPreventNewDeclaration()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				form.Show();

				var consignmentsGrid = form.ConsignmentsGrid;
				var columnStyles = consignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var importDeclarationColumn = columnStyles.Single(columnStyle => columnStyle.ColumnName == "HVC_JE_ImportDeclaration") as ZGuidFindBoxColumnStyleInfo;
				var exportDeclarationColumn = columnStyles.Single(columnStyle => columnStyle.ColumnName == "HVC_JE_ExportDeclaration") as ZGuidFindBoxColumnStyleInfo;

				CombineAssertions("Should not allow new declaration to be created", () =>
				{
					AssertEquals(false, importDeclarationColumn.AllowNewForm);
					AssertEquals(false, exportDeclarationColumn.AllowNewForm);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_MenuItem_NotVisible_NoShipment()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN100";
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);

				AssertEquals("The 'Convert to Stand Alone Declaration' menu item should not be visible on a consignment that is not attached to a shipment", false, menuItem.Visible);
			}
		}

		public void TestConvertToStandAloneDeclaration_MenuItem_NotVisible_DeclarationAlreadyCreated()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_DeclarationReference = "Test Ref";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_DeclarationReference = "Test Ref";

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
				{
					form.Show();
					var grid = form.ConsignmentsGrid;
					var contextMenu = grid.ContextMenu;
					contextMenu.ShowPopupMenu();

					var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);

					Assert("The 'Convert to Stand Alone Declaration' menu item should be visible on a consignment that stand alone declaration is not created", menuItem.Visible);
				}

				consignment.HVC_JE_ImportDeclaration = declaration1.PK;

				using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
				{
					form.Show();
					var grid = form.ConsignmentsGrid;
					var contextMenu = grid.ContextMenu;
					contextMenu.ShowPopupMenu();

					var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);

					AssertEquals("The 'Convert to Stand Alone Declaration' menu item should not be visible on a consignment that already has a stand alone declaration created", false, menuItem.Visible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
				{
					form.Show();
					var grid = form.ConsignmentsGrid;
					var contextMenu = grid.ContextMenu;
					contextMenu.ShowPopupMenu();

					var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);

					Assert("The 'Convert to Stand Alone Declaration' menu item should be visible on a consignment that stand alone declaration is not created", menuItem.Visible);
				}

				consignment.HVC_JE_ExportDeclaration = declaration2.PK;

				using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
				{
					form.Show();
					var grid = form.ConsignmentsGrid;
					var contextMenu = grid.ContextMenu;
					contextMenu.ShowPopupMenu();

					var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);

					AssertEquals("The 'Convert to Stand Alone Declaration' menu item should not be visible on a consignment that already has a stand alone declaration created", false, menuItem.Visible);
				}
			}
		}

		public void TestTransportBookingOptionAdded()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm())
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Transport Booking");

				AssertEquals("The 'Transport Booking' option should be visible on right click of consignment", true, menuItem.Visible);
			}
		}

		public void TestClickTransportBookingMenuItem_WhenConsignmentIsSelfBooked_ShowDialog()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_IsSelfBooked = true;
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var transportBookingMenuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Transport Booking");
				transportBookingMenuItem.OnPopup(EventArgs.Empty);

				var menuItem = transportBookingMenuItem.MenuItems.Cast<ZMenuItem>().FirstOrDefault();
				menuItem.PerformClick();

				var expectMessage = "Consignment has been marked as Self-Booked. Do you wish to proceed with Transport Booking?";
				AssertEquals(expectMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				var expectCaption = "Warning";
				AssertEquals(expectCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestStandAloneDeclarationHotKeyF3_OpensLinkedStandAloneDeclaration()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			consignment.HVC_JE_ImportDeclaration = declaration.PK;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				form.Show();
				var consignmentsGrid = form.ConsignmentsGrid;
				var standAloneDeclarationTextBox = form.Controls.Find("zTextBoxStandAloneDeclaration", true)[0] as ZTextBox;

				consignmentsGrid.SelectSingleElement(consignment);
				KeySender.PostKeyDown(standAloneDeclarationTextBox, Keys.F3);
				Application.DoEvents();

				using (var formDeclaration = Application.OpenForms.Cast<Form>().SingleOrDefault(x => x.Name.Contains("JobDeclarationForm")))
				{
					AssertNotNull(formDeclaration);
				}
			}
		}

		public void TestStandAloneDeclarationHotKeyF3_WhenConsignmentDoesNotHaveLinkedDeclaration_DoesNotOpenForm()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				form.Show();
				var consignmentsGrid = form.ConsignmentsGrid;
				var standAloneDeclarationTextBox = form.Controls.Find("zTextBoxStandAloneDeclaration", true)[0] as ZTextBox;

				consignmentsGrid.SelectSingleElement(consignment);
				KeySender.PostKeyDown(standAloneDeclarationTextBox, Keys.F3);
				Application.DoEvents();

				using (var formDeclaration = Application.OpenForms.Cast<Form>().SingleOrDefault(x => x.Name.Contains("JobDeclarationForm")))
				{
					AssertNull(formDeclaration);
				}
			}
		}

		public void TestStandAloneDeclarationHotKeyF3_WhenLinkedDeclarationIsInOtherCompany_DisplaysMessageAndDoesNotOpenForm()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			var consignment = header.Consignments.AddNew();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			consignment.HVC_JE_ImportDeclaration = declaration.PK;

			Factory.Save();

			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_RN_NKCountryCode = CountryCodes.Australia;
			var testBranch = testCompany.Branches.AddNew();
			testBranch.GB_RN_NKCountryCode = CountryCodes.Australia;
			declaration.JE_GB = testBranch.PK;

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				form.Show();
				var consignmentsGrid = form.ConsignmentsGrid;
				var standAloneDeclarationTextBox = form.Controls.Find("zTextBoxStandAloneDeclaration", true)[0] as ZTextBox;

				consignmentsGrid.SelectSingleElement(consignment);
				KeySender.PostKeyDown(standAloneDeclarationTextBox, Keys.F3);
				Application.DoEvents();

				using (var formDeclaration = Application.OpenForms.Cast<Form>().SingleOrDefault(x => x.Name.Contains("JobDeclarationForm")))
				{
					AssertNull(formDeclaration);
					AssertContains("You are trying to view a declaration that belongs to a different company. Please log into the company", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestConvertToStandAloneDeclarationButtonClick_FormHasUnsavedChanges_ShowsError()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
			{
				form.Show();
				consignment.HVC_GoodsDescription = "Some other goods";
				AssertEquals("Precondition: Consignment has changes", true, consignment.HasChanges);

				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				newButton.PerformClick();
				AssertEquals("Please save any changes made before creating Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Stand Alone Declaration not created", consignment.StandAloneDeclarationForCurrentCompany);
			}
		}

		public void TestConvertToStandAloneDeclarationButtonClick_WhenConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
			var consignment_WithWaybillLength13 = consignmentHeader.Consignments.AddNew();
			consignment_WithWaybillLength13.HVC_WaybillNumber = "1234567890123";

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				newButton.PerformClick();
				AssertNull("Stand Alone Declaration not created", consignment_WithWaybillLength13.StandAloneDeclarationForCurrentCompany);
				AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on Stand Alone Declaration.\r\nWould you like to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertToStandAloneDeclarationButtonClick_WhenConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment_WithWaybillLength17 = consignmentHeader.Consignments.AddNew();
			consignment_WithWaybillLength17.HVC_WaybillNumber = "ABCD5678901234567";

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(consignmentHeader))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var groupBox = form.Controls.Find("groupDetails", true)[0] as ZGroupBox;

				var newButton = groupBox.Controls.Find("zButtonConvertToStandAloneDeclaration", true)[0] as ZButton;

				newButton.PerformClick();
				AssertNull("Stand Alone Declaration not created", consignment_WithWaybillLength17.StandAloneDeclarationForCurrentCompany);
				AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on Stand Alone Declaration.\r\nWould you like to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenNoConsigneeOrg_PromptsToCreateConsigneeOrg()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipperAddress.OA_OH = shipperOrg.PK;
			shipperAddress.OA_Code = "SHIPPER";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				var orgPrompt = UnitTestUserNotification.Instance.PreviousMessages.SingleOrDefault(message => message.Caption == "Organization not found");
				AssertNotNull("Not found prompt should have been shown", orgPrompt);
				AssertEquals("Prompt should have correct text to create consignee", "Consignee organization not found, would you like to convert to organization with details defaulted from 'Consignee' fields?", orgPrompt.Text);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenNoShipperOrg_PromptsToCreateShipperOrg()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_OH = consigneeOrg.PK;
			consigneeAddress.OA_Code = "CONSIGNEE";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				var orgPrompt = UnitTestUserNotification.Instance.PreviousMessages.SingleOrDefault(message => message.Caption == "Organization not found");
				AssertNotNull("Not-found prompt should have been shown", orgPrompt);
				AssertEquals("Prompt should have correct text to create shipper", "Shipper organization not found, would you like to convert to organization with details defaulted from 'Shipper' fields?", orgPrompt.Text);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenNoConsigneeOrShipperOrg_PromptsToCreateBothOrgs()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				var orgPrompt = UnitTestUserNotification.Instance.PreviousMessages.SingleOrDefault(message => message.Caption == "Organization not found");
				AssertNotNull("Not found prompt should have been shown", orgPrompt);
				AssertEquals("Prompt should have correct text to create both shipper and consignee", "Consignee and Shipper organizations not found, would you like to convert to organizations with details defaulted from 'Consignee' and 'Shipper' fields?", orgPrompt.Text);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenNoConsigneeOrg_ShowsSimilarAddressFormForConsignee()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipperAddress.OA_OH = shipperOrg.PK;
			shipperAddress.OA_Code = "SHIPPER";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var shownSimilarOrgFormAddressTypes = new List<Enterprise.Integration.Customs.ConsignmentAddressType>();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						shownSimilarOrgFormAddressTypes.Add(similarOrgForm.AddressType);
						AssertEquals("Form should be shown for current consignment", consignment.PK, similarOrgForm.Consignment.PK);
					}
				});

				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should have shown form for new consignee", new[] { Enterprise.Integration.Customs.ConsignmentAddressType.Consignee }, shownSimilarOrgFormAddressTypes);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenNoShipperOrg_ShowsSimilarAddressFormForShipper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_OH = consigneeOrg.PK;
			consigneeAddress.OA_Code = "CONSIGNEE";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var shownSimilarOrgFormAddressTypes = new List<Enterprise.Integration.Customs.ConsignmentAddressType>();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						shownSimilarOrgFormAddressTypes.Add(similarOrgForm.AddressType);
						AssertEquals("Form should be shown for current consignment", consignment.PK, similarOrgForm.Consignment.PK);
					}
				});

				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should have shown form for new shipper", [Enterprise.Integration.Customs.ConsignmentAddressType.Shipper], shownSimilarOrgFormAddressTypes);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenNoConsigneeOrShipperOrg_ShowsSimilarAddressFormForConsigneeThenShipper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var shownSimilarOrgFormAddressTypes = new List<Enterprise.Integration.Customs.ConsignmentAddressType>();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						shownSimilarOrgFormAddressTypes.Add(similarOrgForm.AddressType);
						AssertEquals("Form should be shown for current consignment", consignment.PK, similarOrgForm.Consignment.PK);
					}
				});

				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should have shown form for new consignee then new shipper",
					[Enterprise.Integration.Customs.ConsignmentAddressType.Consignee, Enterprise.Integration.Customs.ConsignmentAddressType.Shipper], shownSimilarOrgFormAddressTypes);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenSelectSimilarConsigneeAddress_PopulatesConsignee_AndPromptsToRetry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipperOrg.OH_Code = "TESSHIP";
			shipperOrg.OH_FullName = "Shipper Company That Should Not Match";
			var shipperAddress = shipperOrg.Addresses.AddNew();
			shipperAddress.OA_Code = "TESSHIPADD";
			shipperAddress.OA_Address1 = "1234 Shipper Street";
			shipperAddress.OA_City = "Shipper City";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ConsigneeName = "Test Consignee Company";
			consignment.HVC_ConsigneeAddress1 = "4444 Consignee Avenue";
			consignment.HVC_ConsigneeCity = "Consigneeland";

			var selectedConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			selectedConsigneeOrg.OH_Code = "TESCONS";
			selectedConsigneeOrg.OH_FullName = "Test Consignee Company";
			var selectedConsigneeAddress = selectedConsigneeOrg.Addresses.AddNew();
			selectedConsigneeAddress.OA_Code = "TESCONSADD";
			selectedConsigneeAddress.OA_Address1 = "4444 Consignee Avenue";
			selectedConsigneeAddress.OA_City = "Consigneeland";

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var similarOrgsGrid = similarOrgForm.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
						similarOrgsGrid.Select(0);

						AssertEquals("Precondition", selectedConsigneeAddress.PK, ((OrgPatternMatch)similarOrgsGrid.GetFirstSelectedRow()).Address.PK);
						var selectAddressButton = similarOrgForm.Controls.Find("SaveNewOrgButton", true).Single() as ZButton;
						selectAddressButton.PerformClick();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						Fail("Should not create stand alone and show form");
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Consignee org linked and declaration not created", () =>
				{
					AssertEquals("Consignment has unsaved changes", true, consignment.HasChanges);
					AssertEquals("Consignee has been converted to an org", true, consignment.ConsigneeIsOrganisation);
					AssertEquals("Consignee org address matched selected address", selectedConsigneeAddress.PK, consignment.HVC_OA_ConsigneeAddress);
					AssertEquals("Message was shown to prompt a save and retry", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenSelectSimilarShipperAddress_PopulatesShipper_AndPromptsToRetry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Code = "TESCONS";
			consigneeOrg.OH_FullName = "Consignee Company That Should Not Match";
			var consigneeAddress = consigneeOrg.Addresses.AddNew();
			consigneeAddress.OA_Code = "TESCONSADD";
			consigneeAddress.OA_Address1 = "1234 Consignee Street";
			consigneeAddress.OA_City = "Consignee City";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ShipperName = "Test Shipper Company";
			consignment.HVC_ShipperAddress1 = "4444 Shipper Avenue";
			consignment.HVC_ShipperCity = "Shipperland";

			var selectedShipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			selectedShipperOrg.OH_Code = "TESSHIP";
			selectedShipperOrg.OH_FullName = "Test Shipper Company";
			var selectedShipperAddress = selectedShipperOrg.Addresses.AddNew();
			selectedShipperAddress.OA_Code = "TESSHIPADD";
			selectedShipperAddress.OA_Address1 = "4444 Shipper Avenue";
			selectedShipperAddress.OA_City = "Shipperland";

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var similarOrgsGrid = similarOrgForm.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
						similarOrgsGrid.Select(0);

						AssertEquals("Precondition", selectedShipperAddress.PK, ((OrgPatternMatch)similarOrgsGrid.GetFirstSelectedRow()).Address.PK);
						var selectAddressButton = similarOrgForm.Controls.Find("SaveNewOrgButton", true).Single() as ZButton;
						selectAddressButton.PerformClick();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						Fail("Should not create stand alone and show form");
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Shipper org linked and declaration not created", () =>
				{
					AssertEquals("Consignment has unsaved changes", true, consignment.HasChanges);
					AssertEquals("Shipper has been converted to an org", true, consignment.ShipperIsOrganisation);
					AssertEquals("Shipper org address matched selected address", selectedShipperAddress.PK, consignment.HVC_OA_ShipperAddress);
					AssertEquals("Message was shown to prompt a save and retry", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenSelectSimilarConsigneeAndShipperAddress_PopulatesConsigneeAndShipper_AndPromptsToRetry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ConsigneeName = "Consignee Testing Company";
			consignment.HVC_ConsigneeAddress1 = "6666 Consignee Avenue";
			consignment.HVC_ConsigneeCity = "Consigneeland";
			consignment.HVC_ShipperName = "Test Shipper Company";
			consignment.HVC_ShipperAddress1 = "4444 Shipper Avenue";
			consignment.HVC_ShipperCity = "Shipperland";

			var selectedShipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			selectedShipperOrg.OH_Code = "TESSHIP";
			selectedShipperOrg.OH_FullName = "Test Shipper Company";
			var selectedShipperAddress = selectedShipperOrg.Addresses.AddNew();
			selectedShipperAddress.OA_Code = "TESSHIPADD";
			selectedShipperAddress.OA_Address1 = "4444 Shipper Avenue";
			selectedShipperAddress.OA_City = "Shipperland";

			var selectedConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			selectedConsigneeOrg.OH_Code = "TESCONS";
			selectedConsigneeOrg.OH_FullName = "Consignee Testing Company";
			var selectedConsigneeAddress = selectedConsigneeOrg.Addresses.AddNew();
			selectedConsigneeAddress.OA_Code = "TESCONSADD";
			selectedConsigneeAddress.OA_Address1 = "6666 Consignee Avenue";
			selectedConsigneeAddress.OA_City = "Consigneeland";

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var similarOrgsGrid = similarOrgForm.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
						similarOrgsGrid.Select(0);

						var expectedPK = similarOrgForm.AddressType == Enterprise.Integration.Customs.ConsignmentAddressType.Consignee ? selectedConsigneeAddress.PK : selectedShipperAddress.PK;
						AssertEquals("Precondition: similar org form populated with correct orgs", expectedPK, ((OrgPatternMatch)similarOrgsGrid.GetFirstSelectedRow()).Address.PK);
						var selectAddressButton = similarOrgForm.Controls.Find("SaveNewOrgButton", true).Single() as ZButton;
						selectAddressButton.PerformClick();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						Fail("Should not create stand alone and show form");
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Consignee and Shipper orgs linked and declaration not created", () =>
				{
					AssertEquals("Consignment has unsaved changes", true, consignment.HasChanges);
					AssertEquals("Consignee has been converted to an org", true, consignment.ConsigneeIsOrganisation);
					AssertEquals("Shipper has been converted to an org", true, consignment.ShipperIsOrganisation);
					AssertEquals("Consignee org address matched selected address", selectedConsigneeAddress.PK, consignment.HVC_OA_ConsigneeAddress);
					AssertEquals("Shipper org address matched selected address", selectedShipperAddress.PK, consignment.HVC_OA_ShipperAddress);
					AssertEquals("Message was shown to prompt a save and retry", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenSkipSelectedSimilarConsignee_CreatesStandAloneDeclarationWithoutConsignee()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipperOrg.OH_Code = "TESSHIP";
			shipperOrg.OH_FullName = "Shipper Company That Should Not Match";
			var shipperAddress = shipperOrg.Addresses.AddNew();
			shipperAddress.OA_RN_NKCountryCode = CountryCodes.NewZealand;
			shipperAddress.OA_Code = "TESSHIPADD";
			shipperAddress.OA_Address1 = "1234 Shipper Street";
			shipperAddress.OA_City = "Shipper City";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Converting consignee to org was skipped and declaration was created", () =>
				{
					AssertEquals("Consignment has no unsaved changes", false, consignment.HasChanges);
					AssertEquals("Consignee has not been converted to an org", false, consignment.ConsigneeIsOrganisation);
					AssertEquals("Declaration was created and form shown", true, ZFormModaliser.LastFormShownDialogForTest is BaseJobDeclarationForm);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenSkipSelectedSimilarShipper_CreatesStandAloneDeclarationWithoutShipper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Code = "TESCONS";
			consigneeOrg.OH_FullName = "Consignee Company That Should Not Match";
			var consigneeAddress = consigneeOrg.Addresses.AddNew();
			consigneeAddress.OA_Code = "TESCONSADD";
			consigneeAddress.OA_Address1 = "1234 Consignee Street";
			consigneeAddress.OA_City = "Consignee City";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Converting shipper to org was skipped and declaration was created", () =>
				{
					AssertEquals("Consignment has no unsaved changes", false, consignment.HasChanges);
					AssertEquals("Shipper has not been converted to an org", false, consignment.ShipperIsOrganisation);
					AssertEquals("Declaration was created and form shown", true, ZFormModaliser.LastFormShownDialogForTest is BaseJobDeclarationForm);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenSkipSelectedSimilarConsigneeAndShipper_CreatesStandAloneDeclarationWithoutConsigneeOrShipper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Converting consignee and shipper to org was skipped and declaration was created", () =>
				{
					AssertEquals("Consignment has no unsaved changes", false, consignment.HasChanges);
					AssertEquals("Consignee has not been converted to an org", false, consignment.ConsigneeIsOrganisation);
					AssertEquals("Shipper has not been converted to an org", false, consignment.ShipperIsOrganisation);
					AssertEquals("Declaration was created and form shown", true, ZFormModaliser.LastFormShownDialogForTest is BaseJobDeclarationForm);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenCancelSelectSimilarConsignee_CreatesStandAloneDeclarationWithoutConsignee()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipperOrg.OH_Code = "TESSHIP";
			shipperOrg.OH_FullName = "Shipper Company That Should Not Match";
			var shipperAddress = shipperOrg.Addresses.AddNew();
			shipperAddress.OA_RN_NKCountryCode = CountryCodes.NewZealand;
			shipperAddress.OA_Code = "TESSHIPADD";
			shipperAddress.OA_Address1 = "1234 Shipper Street";
			shipperAddress.OA_City = "Shipper City";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var standAloneDeclarationCreated = false;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var cancelButton = similarOrgForm.Controls.Find("CancelSaveButton", true).Single() as ZButton;
						cancelButton.PerformClick();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						standAloneDeclarationCreated = true;
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Converting consignee to org was cancelled and declaration was created", () =>
				{
					AssertEquals("Consignment has no unsaved changes", false, consignment.HasChanges);
					AssertEquals("Consignee has not been converted to an org", false, consignment.ConsigneeIsOrganisation);
					AssertEquals("Stand alone delclaration was created and form shown", true, standAloneDeclarationCreated);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenCancelSelectSimilarShipper_CreatesStandAloneDeclarationWithoutShipper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Code = "TESCONS";
			consigneeOrg.OH_FullName = "Consignee Company That Should Not Match";
			var consigneeAddress = consigneeOrg.Addresses.AddNew();
			consigneeAddress.OA_Code = "TESCONSADD";
			consigneeAddress.OA_Address1 = "1234 Consignee Street";
			consigneeAddress.OA_City = "Consignee City";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var standAloneDeclarationCreated = false;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var cancelButton = similarOrgForm.Controls.Find("CancelSaveButton", true).Single() as ZButton;
						cancelButton.PerformClick();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						standAloneDeclarationCreated = true;
					}
				});
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Converting shipper to org was cancelled and declaration was created", () =>
				{
					AssertEquals("Consignment has no unsaved changes", false, consignment.HasChanges);
					AssertEquals("Shipper has not been converted to an org", false, consignment.ShipperIsOrganisation);
					AssertEquals("Stand alone delclaration was created and form shown", true, standAloneDeclarationCreated);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenCancelSelectSimilarConsigneeAndShipper_CreatesStandAloneDeclarationWithoutConsigneeOrShipper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var standAloneDeclarationCreated = false;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var cancelButton = similarOrgForm.Controls.Find("CancelSaveButton", true).Single() as ZButton;
						cancelButton.PerformClick();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						standAloneDeclarationCreated = true;
					}
				});
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Converting consignee and shipper to org was cancelled and declaration was created", () =>
				{
					AssertEquals("Consignment has no unsaved changes", false, consignment.HasChanges);
					AssertEquals("Consignee has not been converted to an org", false, consignment.ConsigneeIsOrganisation);
					AssertEquals("Shipper has not been converted to an org", false, consignment.ShipperIsOrganisation);
					AssertEquals("Stand alone delclaration was created and form shown", true, standAloneDeclarationCreated);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenSelectSimilarConsigneeButCancelShipper_PopulatesConsignee_AndPromptsToRetry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ConsigneeName = "Test Consignee Company";
			consignment.HVC_ConsigneeAddress1 = "4444 Consignee Avenue";
			consignment.HVC_ConsigneeCity = "Consigneeland";

			var selectedConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			selectedConsigneeOrg.OH_Code = "TESCONS";
			selectedConsigneeOrg.OH_FullName = "Test Consignee Company";
			var selectedConsigneeAddress = selectedConsigneeOrg.Addresses.AddNew();
			selectedConsigneeAddress.OA_Code = "TESCONSADD";
			selectedConsigneeAddress.OA_Address1 = "4444 Consignee Avenue";
			selectedConsigneeAddress.OA_City = "Consigneeland";

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						if (similarOrgForm.AddressType == Enterprise.Integration.Customs.ConsignmentAddressType.Consignee)
						{
							var similarOrgsGrid = similarOrgForm.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
							similarOrgsGrid.Select(0);

							AssertEquals("Precondition - Similar org form is populated with correct consignee org", selectedConsigneeAddress.PK, ((OrgPatternMatch)similarOrgsGrid.GetFirstSelectedRow()).Address.PK);
							var selectAddressButton = similarOrgForm.Controls.Find("SaveNewOrgButton", true).Single() as ZButton;
							selectAddressButton.PerformClick();
						}
						else if (similarOrgForm.AddressType == Enterprise.Integration.Customs.ConsignmentAddressType.Shipper)
						{
							var cancelButton = similarOrgForm.Controls.Find("CancelSaveButton", true).Single() as ZButton;
							cancelButton.PerformClick();
						}
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						Fail("Should not create stand alone and show form");
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Converting shipper to org was cancelled but consignee was linked to an org, so declaration was not created and user is prompted to retry", () =>
				{
					AssertEquals("Consignment has unsaved changes", true, consignment.HasChanges);
					AssertEquals("Consignee has been converted to an org", true, consignment.ConsigneeIsOrganisation);
					AssertEquals("Shipper has not been converted to an org", false, consignment.ShipperIsOrganisation);
					AssertEquals("Consignee org address matched selected address", selectedConsigneeAddress.PK, consignment.HVC_OA_ConsigneeAddress);
					AssertEquals("Message was shown to prompt a save and retry", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenSelectSimilarShipperButCancelConsignee_PopulatesShipper_AndPromptsToRetry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ShipperName = "Test Shipper Company";
			consignment.HVC_ShipperAddress1 = "4444 Shipper Avenue";
			consignment.HVC_ShipperCity = "Shipperland";

			var selectedShipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			selectedShipperOrg.OH_Code = "TESSHIP";
			selectedShipperOrg.OH_FullName = "Test Shipper Company";
			var selectedShipperAddress = selectedShipperOrg.Addresses.AddNew();
			selectedShipperAddress.OA_Code = "TESSHIPADD";
			selectedShipperAddress.OA_Address1 = "4444 Shipper Avenue";
			selectedShipperAddress.OA_City = "Shipperland";

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						if (similarOrgForm.AddressType == Enterprise.Integration.Customs.ConsignmentAddressType.Consignee)
						{
							var cancelButton = similarOrgForm.Controls.Find("CancelSaveButton", true).Single() as ZButton;
							cancelButton.PerformClick();
						}
						else if (similarOrgForm.AddressType == Enterprise.Integration.Customs.ConsignmentAddressType.Shipper)
						{
							var similarOrgsGrid = similarOrgForm.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
							similarOrgsGrid.Select(0);

							AssertEquals("Precondition - Similar org form is populated with correct shipper org", selectedShipperAddress.PK, ((OrgPatternMatch)similarOrgsGrid.GetFirstSelectedRow()).Address.PK);
							var selectAddressButton = similarOrgForm.Controls.Find("SaveNewOrgButton", true).Single() as ZButton;
							selectAddressButton.PerformClick();
						}
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						Fail("Should not create stand alone and show form");
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("Converting consignee to org was cancelled but shipper was linked to an org, so declaration was not created and user is prompted to retry", () =>
				{
					AssertEquals("Consignment has unsaved changes", true, consignment.HasChanges);
					AssertEquals("Consignee has not been converted to an org", false, consignment.ConsigneeIsOrganisation);
					AssertEquals("Shipper has been converted to an org", true, consignment.ShipperIsOrganisation);
					AssertEquals("Shipper org address matched selected address", selectedShipperAddress.PK, consignment.HVC_OA_ShipperAddress);
					AssertEquals("Message was shown to prompt a save and retry", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenCreateNewConsigneeAddress_PopulatesConsignee_AndPromptsToRetry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipperOrg.OH_Code = "TESSHIP";
			shipperOrg.OH_FullName = "Shipper Company That Should Not Match";
			var shipperAddress = shipperOrg.Addresses.AddNew();
			shipperAddress.OA_Code = "TESSHIPADD";
			shipperAddress.OA_Address1 = "1234 Shipper Street";
			shipperAddress.OA_City = "Shipper City";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ConsigneeName = "Test Consignee Company";
			consignment.HVC_ConsigneeAddress1 = "74 O'Riordan St";
			consignment.HVC_ConsigneeCity = "Alexandria";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_ConsigneePostcode = "2015";
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var newOrg = default(OrgHeader);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var newOrgButton = similarOrgForm.Controls.Find("newOrganizationButton", true).Single() as ZButton;
						newOrgButton.PerformClick();
					}
					else if (obj is ZOrganisationsForm newOrgForm)
					{
						newOrg = newOrgForm.BusinessEntity as OrgHeader;
						newOrg.OH_RL_NKClosestPort = "AUSYD";
						newOrgForm.FireSaveButton();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						Fail("Should not create stand alone and show form");
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("A new org was created and linked to consignee, so declaration was not created and user is prompted to retry", () =>
				{
					AssertEquals("Consignment has unsaved changes", true, consignment.HasChanges);
					AssertEquals("Consignee has been converted to an org", true, consignment.ConsigneeIsOrganisation);
					AssertEquals("A new consignee organisation was created with matching details", consignment.HVC_ConsigneeName, newOrg.OH_FullName);
					AssertEquals("Consignee org address matches newly created org", newOrg.MainAddress.PK, consignment.HVC_OA_ConsigneeAddress);
					AssertEquals("Message was shown to prompt a save and retry", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenCreateNewShipperAddress_PopulatesShipper_AndPromptsToRetry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Code = "TESCONS";
			consigneeOrg.OH_FullName = "Consignee Company That Should Not Match";
			var consigneeAddress = consigneeOrg.Addresses.AddNew();
			consigneeAddress.OA_Code = "TESCONSADD";
			consigneeAddress.OA_Address1 = "1234 Consignee Street";
			consigneeAddress.OA_City = "Consignee City";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ShipperName = "Test Shipper Company";
			consignment.HVC_ShipperAddress1 = "74 O'Riordan St";
			consignment.HVC_ShipperCity = "Alexandria";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_ShipperPostcode = "2015";
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var newOrg = default(OrgHeader);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var newOrgButton = similarOrgForm.Controls.Find("newOrganizationButton", true).Single() as ZButton;
						newOrgButton.PerformClick();
					}
					else if (obj is ZOrganisationsForm newOrgForm)
					{
						newOrg = newOrgForm.BusinessEntity as OrgHeader;
						newOrg.OH_RL_NKClosestPort = "AUSYD";
						newOrgForm.FireSaveButton();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						Fail("Should not create stand alone and show form");
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				CombineAssertions("A new org was created and linked to consignee, so declaration was not created and user is prompted to retry", () =>
				{
					AssertEquals("Consignment has unsaved changes", true, consignment.HasChanges);
					AssertEquals("Shipper has been converted to an org", true, consignment.ShipperIsOrganisation);
					AssertEquals("A new shipper organisation was created with matching details", consignment.HVC_ShipperName, newOrg.OH_FullName);
					AssertEquals("Shipper org address matches newly created org", newOrg.MainAddress.PK, consignment.HVC_OA_ShipperAddress);
					AssertEquals("Message was shown to prompt a save and retry", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenCreateNewConsigneeAndShipperAddress_PopulatesConsigneeAndShipper_AndPromptsToRetry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ConsigneeName = "Test Consignee Company";
			consignment.HVC_ConsigneeAddress1 = "74 O'Riordan St";
			consignment.HVC_ConsigneeCity = "Alexandria";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_ConsigneePostcode = "2015";
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;
			consignment.HVC_ShipperName = "Test Shipper Company";
			consignment.HVC_ShipperAddress1 = "74 O'Riordan St";
			consignment.HVC_ShipperCity = "Alexandria";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_ShipperPostcode = "2015";
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;

			Factory.Save();

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("No unsaved changes", false, consignment.HasChanges);
				AssertEquals("Consignee is not an organisation", false, consignment.ConsigneeIsOrganisation);
				AssertEquals("Shipper is not an organisation", false, consignment.ShipperIsOrganisation);
			});

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var newOrgs = new List<OrgHeader>();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVSimilarAddressSelectionForm similarOrgForm)
					{
						var newOrgButton = similarOrgForm.Controls.Find("newOrganizationButton", true).Single() as ZButton;
						newOrgButton.PerformClick();
					}
					else if (obj is ZOrganisationsForm newOrgForm)
					{
						var newOrg = newOrgForm.BusinessEntity as OrgHeader;
						newOrgs.Add(newOrg);
						newOrg.OH_RL_NKClosestPort = "AUSYD";
						newOrgForm.FireSaveButton();
					}
					else if (obj is BaseJobDeclarationForm jobDecForm)
					{
						Fail("Should not create stand alone and show form");
					}
				});

				UnitTestUserNotification.Instance.AddYesAnswer();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				var newConsigneeOrg = newOrgs.SingleOrDefault(org => org.OH_IsConsignee);
				var newShipperOrg = newOrgs.SingleOrDefault(org => org.OH_IsConsignor);

				CombineAssertions("A new org was created and linked to consignee and shipper, so declaration was not created and user is prompted to retry", () =>
				{
					AssertEquals("Consignment has unsaved changes", true, consignment.HasChanges);
					AssertEquals("Consignee has been converted to an org", true, consignment.ConsigneeIsOrganisation);
					AssertEquals("Shipper has been converted to an org", true, consignment.ShipperIsOrganisation);
					AssertEquals("A new consignee organisation was created with matching details", consignment.HVC_ConsigneeName, newConsigneeOrg.OH_FullName);
					AssertEquals("A new shipper organisation was created with matching details", consignment.HVC_ShipperName, newShipperOrg.OH_FullName);
					AssertEquals("Consignee org address matches newly created org", newConsigneeOrg.MainAddress.PK, consignment.HVC_OA_ConsigneeAddress);
					AssertEquals("Shipper org address matches newly created org", newShipperOrg.MainAddress.PK, consignment.HVC_OA_ShipperAddress);
					AssertEquals("Message was shown to prompt a save and retry", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenMergeConsignmentsIsClicked_AndNoConsignmentsAreSelected_ThenDisplayAnError()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment1, HVLVReleaseStatus.Held);

			var consignment2 = header.Consignments.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment2, HVLVReleaseStatus.Held);

			var consignment3 = header.Consignments.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment3, HVLVReleaseStatus.Held);

			Factory.Save();

			AssertEquals("precondition", 2, consignment1.ConsignmentsBelongToSameConsigneeExcludingParent.Count);

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				var selectedElementsCount = 0;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(obj =>
				{
					if (obj is HVLVSelectConsignmentForm mergeDeclarationForm)
					{
						var consignmentGrid = mergeDeclarationForm.Controls.Find("gridConsignments", true).Single() as ZGrid;
						mergeDeclarationForm.Show();
						consignmentGrid.Show();
						selectedElementsCount = consignmentGrid.SelectedElements.Length;

						var mergeButton = mergeDeclarationForm.Controls.Find("btnMerge", true).Single() as ZButton;
						mergeButton.Enabled = true;
						mergeButton.PerformClick();
					}
				});

				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment1);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				AssertEquals("Precondition - no elements selected", 0, selectedElementsCount);
				Assert("Error message shown", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Please select a consignment to merge first."));
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenConsigneeIsNotAnOrg_AndImportCustomsClearanceStatusIsCLR_ThenDisplayCLRErrorMessage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment, HVLVReleaseStatus.Cleared);

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				AssertEquals("Error message shown", "A Standalone Declaration cannot be created for a Consignment with Release Status as 'Cleared'", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenConsignmentHasCommonConsigneesWithOtherConsignemnts_ThenDisplayMergeConsignmentDeclarationForm()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment1, HVLVReleaseStatus.Held);

			var consignment2 = header.Consignments.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment2, HVLVReleaseStatus.Held);

			var consignment3 = header.Consignments.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment3, HVLVReleaseStatus.Held);

			Factory.Save();

			AssertEquals("precondition", 2, consignment1.ConsignmentsBelongToSameConsigneeExcludingParent.Count);
			AssertNull("precondition consignment1 declaration should not be created", consignment1.StandAloneDeclarationForCurrentCompany);
			AssertNull("precondition consignment2 declaration should not be created", consignment2.StandAloneDeclarationForCurrentCompany);
			AssertNull("precondition consignment3 declaration should not be created", consignment3.StandAloneDeclarationForCurrentCompany);

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				List<HVLVConsignment> consignmentsInGrid = null;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(obj =>
				{
					if (obj is HVLVSelectConsignmentForm mergeDeclarationForm)
					{
						var consignmentGrid = mergeDeclarationForm.Controls.Find("gridConsignments", true).Single() as ZGrid;

						mergeDeclarationForm.Show();
						consignmentGrid.Show();
						consignmentsInGrid = consignmentGrid.List.ToList<HVLVConsignment>();
					}
				});

				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment1);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				AssertNotNull(consignmentsInGrid);
				AssertContainsExactElementsInAnyOrder("Merge declaration form should only show consignments with same consignee excluding parent to have be shown in the grid", new[] { consignment2, consignment3 }, consignmentsInGrid);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenMergeConsignmentsWithSameConsigneeForDeclaration_ThenOnlyMergeNonCLRConsignments()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = header.Consignments.AddNew();
			consignment1.Items.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment1, HVLVReleaseStatus.Held);

			var consignment2 = header.Consignments.AddNew();
			consignment2.Items.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment2, HVLVReleaseStatus.Held);

			var consignment3 = header.Consignments.AddNew();
			consignment3.Items.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment3, HVLVReleaseStatus.Cleared);

			Factory.Save();

			AssertEquals("precondition", 2, consignment1.ConsignmentsBelongToSameConsigneeExcludingParent.Count);
			AssertNull("precondition consignment1 declaration should not be created", consignment1.StandAloneDeclarationForCurrentCompany);
			AssertNull("precondition consignment2 declaration should not be created", consignment2.StandAloneDeclarationForCurrentCompany);
			AssertNull("precondition consignment3 declaration should not be created", consignment3.StandAloneDeclarationForCurrentCompany);

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				var mergeDeclarationFormShown = false;
				List<HVLVConsignment> consignmentsInGrid = null;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(obj =>
				{
					if (obj is HVLVSelectConsignmentForm mergeDeclarationForm)
					{
						mergeDeclarationFormShown = true;
						var consignmentGrid = mergeDeclarationForm.Controls.Find("gridConsignments", true).Single() as ZGrid;

						mergeDeclarationForm.Show();
						consignmentGrid.Show();
						consignmentGrid.SelectAllElements();
						mergeDeclarationForm.DialogResult = DialogResult.OK;
						mergeDeclarationFormShown = consignmentGrid != null;
						consignmentsInGrid = consignmentGrid.List.ToList<HVLVConsignment>();
					}

					if (obj is BaseJobDeclarationForm jobDeclarationForm)
					{
						Factory.Save();
					}
				});

				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment1);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				Assert(mergeDeclarationFormShown);
				AssertNotNull("post condition consignment1 declaration should be created", consignment1.StandAloneDeclarationForCurrentCompany);
				AssertNotNull("post condition consignment2 declaration should be created", consignment2.StandAloneDeclarationForCurrentCompany);

				AssertContainsExactElementsInAnyOrder("Merge declaration form should only show non-CLR consignments that have a common consignee with consignment1", new[] { consignment2 }, consignmentsInGrid);

				AssertEquals("consignments should have the same declaration after merging", consignment1.StandAloneDeclarationForCurrentCompany, consignment2.StandAloneDeclarationForCurrentCompany);
				AssertNotEquals("consignments that were merged should not have same declaration as consignment with CLR release status as it was supposed to be ignored", consignment3.StandAloneDeclarationForCurrentCompany, consignment1.StandAloneDeclarationForCurrentCompany);
				AssertNull("consignment with CLR status should not have their declarations changed", consignment3.StandAloneDeclarationForCurrentCompany);
			}
		}

		public void TestConvertToStandAloneDeclaration_WhenNoConsolAttached_ThenShowError()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			SetupImportConsignmentWithReleaseStatus(consignment, HVLVReleaseStatus.Held);

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);
				menuItem.PerformClick();

				AssertEquals("Error message shown", "A Stand Alone Declaration cannot be created for a Consignment without transport details. Attach a Shipment to a Consolidation to create the Stand Alone Declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsignmentDeleteMenuItem_WhenConsignmentIsSaved_IsInvisible()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(consignment);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				AssertEquals(false, grid.DeleteMenuItem.Visible);
			}
		}

		public void TestConsignmentDeleteMenuItem_WhenConsignmentIsNotYetSaved_IsActive()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			var unsavedConsignment = bookingHeader.ConsignmentsFilteredView.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ConsignmentsGrid;
				grid.SelectSingleElement(unsavedConsignment);
				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				AssertEquals("In DB", false, unsavedConsignment.IsInDatabase);
				AssertEquals(true, grid.DeleteMenuItem.Enabled);
			}
		}

		public void TestConsignmentActiveStatusMenuItem_WhenSelectedConsignmentsHaveAnySavedRecords_EnableMenuItem()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.AddNew();
			Factory.Save();
			bookingHeader.Consignments.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ConsignmentsGrid;
				grid.SelectAllElements();

				CombineAssertions("pre condition", () =>
				{
					AssertEquals("First consignment is saved", true, grid.SelectedElements[0].IsInDatabase);
					AssertEquals("Second consignment is not saved", false, grid.SelectedElements[1].IsInDatabase);
				});

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				AssertEquals("Menu item should be enabled", true, menuItem.Enabled);
			}
		}

		public void TestConsignmentActiveStatusMenuItem_WhenSelectedConsignmentsAreAllNotSaved_DisableMenuItem()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.AddNew();
			bookingHeader.Consignments.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ConsignmentsGrid;
				grid.SelectAllElements();

				CombineAssertions("pre condition", () =>
				{
					AssertEquals("First consignment is not saved", false, grid.SelectedElements[0].IsInDatabase);
					AssertEquals("Second consignment is not saved", false, grid.SelectedElements[1].IsInDatabase);
				});

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				AssertEquals("Menu item should be disabled", false, menuItem.Enabled);
			}
		}

		public void TestConsignmentActiveStatusMenuItemCaption_WhenSelectedConsignmentsAreAllActive_ShowsDeactivateCaption()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_IsActive = true;
			consignment2.HVC_IsActive = true;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var filterStripControl = form.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				var activeStatusFilter = filterStripControl.FilterBusinessObject.ModuleFilters["Active Status"] as ModuleTextFilter;
				activeStatusFilter.Property = "Active";
				filterStripControl.FirePerformSearch();

				var grid = form.ConsignmentsGrid;
				grid.SelectAllElements();

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItemCaptions = contextMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text).ToList();
				CombineAssertions("Menu Item Captions should contain correct option for deactivating consignments", () =>
				{
					AssertEquals("Deactivate Caption: Shown", true, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Deactivate));
					AssertEquals("Activate Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Activate));
					AssertEquals("Toggle Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.ToggleActiveStatus));
				});
			}
		}

		public void TestConsignmentActiveStatusMenuItemCaption_WhenSelectedConsignmentsAreAllInactive_ShowsActivateCaption()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_IsActive = false;
			consignment2.HVC_IsActive = false;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var filterStripControl = form.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				var activeStatusFilter = filterStripControl.FilterBusinessObject.ModuleFilters["Active Status"] as ModuleTextFilter;
				activeStatusFilter.Property = "Inactive";
				filterStripControl.FirePerformSearch();

				var grid = form.ConsignmentsGrid;
				grid.SelectAllElements();

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItemCaptions = contextMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text).ToList();
				CombineAssertions("Menu Item Captions should contain correct option for activating consignments", () =>
				{
					AssertEquals("Activate Caption: Shown", true, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Activate));
					AssertEquals("Deactivate Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Deactivate));
					AssertEquals("Toggle Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.ToggleActiveStatus));
				});
			}
		}

		public void TestConsignmentActiveStatusMenuItemCaption_WhenSelectedConsignmentsAreMixOfActiveAndInactive_ShowsToggleCaption()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_IsActive = false;
			consignment2.HVC_IsActive = true;
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var filterStripControl = form.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				var activeStatusFilter = filterStripControl.FilterBusinessObject.ModuleFilters["Active Status"] as ModuleTextFilter;
				activeStatusFilter.Property = "All";
				filterStripControl.FirePerformSearch();

				var grid = form.ConsignmentsGrid;
				grid.SelectAllElements();

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItemCaptions = contextMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text).ToList();
				CombineAssertions("Menu Item Captions should contain correct option for toggling consignment active status", () =>
				{
					AssertEquals("Toggle Caption: Shown", true, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.ToggleActiveStatus));
					AssertEquals("Activate Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Activate));
					AssertEquals("Deactivate Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Deactivate));
				});
			}
		}

		public void TestItemActiveStatusMenuItemCaption_WhenSelectedItemsAreAllActive_ShowsDeactivateCaption()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.HVI_IsActive = true;
			item2.HVI_IsActive = true;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItemCaptions = contextMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text).ToList();
				CombineAssertions("Menu Item Captions should contain correct option for deactivating items", () =>
				{
					AssertEquals("Deactivate Caption: Shown", true, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Deactivate));
					AssertEquals("Activate Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Activate));
					AssertEquals("Toggle Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.ToggleActiveStatus));
				});
			}
		}

		public void TestConsignmentActiveStatusMenuItemClick_WhenSelectedConsignmentsAreMixOfSavedAndNotSaved_NotSavedRecordsAreSkipped()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			Factory.Save();
			var consignment2 = bookingHeader.Consignments.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ConsignmentsGrid;
				grid.SelectAllElements();

				CombineAssertions("pre condition", () =>
				{
					AssertEquals("First consignment is saved", true, grid.SelectedElements[0].IsInDatabase);
					AssertEquals("Second consignment is not saved", false, grid.SelectedElements[1].IsInDatabase);
				});

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				menuItem.PerformClick();

				CombineAssertions("Menu action should only be applied to saved consignment, not saved consignment is skipped", () =>
				{
					AssertEquals("First consignment is deactivated", false, consignment1.HVC_IsActive);
					AssertEquals("Second consignment is skipped", true, consignment2.HVC_IsActive);
				});
				AssertEquals("Inactive consignments are hidden", 1, grid.List.Count);
			}
		}

		public void TestConsignmentActiveStatusMenuItemClick_WhenSelectedConsignmentsAreAllActive_Deactivates()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_IsActive = true;
			consignment2.HVC_IsActive = true;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var filterStripControl = form.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				var activeStatusFilter = filterStripControl.FilterBusinessObject.ModuleFilters["Active Status"] as ModuleTextFilter;
				activeStatusFilter.Property = "Active";
				filterStripControl.FirePerformSearch();

				var grid = form.ConsignmentsGrid;
				AssertEquals("Precondition: 2 Consignments Visible", 2, grid.List.Count);

				grid.SelectAllElements();

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				menuItem.PerformClick();

				CombineAssertions("Consignments should be deactivated", () =>
				{
					AssertEquals("Consignment 1", false, consignment1.HVC_IsActive);
					AssertEquals("Consignment 2", false, consignment2.HVC_IsActive);
				});
				AssertEquals("Inactive consignments are hidden", 0, grid.List.Count);
			}
		}

		public void TestConsignmentActiveStatusMenuItemClick_WhenSelectedConsignmentsAreAllInactive_Activates()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_IsActive = false;
			consignment2.HVC_IsActive = false;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var filterStripControl = form.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				var activeStatusFilter = filterStripControl.FilterBusinessObject.ModuleFilters["Active Status"] as ModuleTextFilter;
				activeStatusFilter.Property = "Inactive";
				filterStripControl.FirePerformSearch();

				var grid = form.ConsignmentsGrid;
				AssertEquals("Precondition: 2 Consignments Visible", 2, grid.List.Count);

				grid.SelectAllElements();

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Activate);
				menuItem.PerformClick();

				CombineAssertions("Consignments should be activated", () =>
				{
					AssertEquals("Consignment 1", true, consignment1.HVC_IsActive);
					AssertEquals("Consignment 2", true, consignment2.HVC_IsActive);
				});
				AssertEquals("Active consignments are hidden", 0, grid.List.Count);
			}
		}

		public void TestConsignmentActiveStatusMenuItemClick_WhenSelectedConsignmentsAreMixOfActiveAndInactive_TogglesActiveStatus()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_IsActive = false;
			consignment2.HVC_IsActive = true;
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var filterStripControl = form.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				var activeStatusFilter = filterStripControl.FilterBusinessObject.ModuleFilters["Active Status"] as ModuleTextFilter;
				activeStatusFilter.Property = "All";
				filterStripControl.FirePerformSearch();

				var grid = form.ConsignmentsGrid;
				AssertEquals("Precondition: 2 Consignments Visible", 2, grid.List.Count);

				grid.SelectAllElements();

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ToggleActiveStatus);
				menuItem.PerformClick();

				CombineAssertions("Consignments' Active Status should have been toggled", () =>
				{
					AssertEquals("Consignment 1, Inactive -> Active", true, consignment1.HVC_IsActive);
					AssertEquals("Consignment 2, Active -> Inactive", false, consignment2.HVC_IsActive);
				});
				AssertEquals("2 Consignments still Visible", 2, grid.List.Count);
			}
		}

		public void TestConsignmentActiveStatusMenuItem_WhenSelectedConsignmentHasCustomsStatus_ShowsMessage()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "LTTSTORE01";
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;

			Assert("Precondition: consignment has customs status", consignment.HasCustomsStatus);

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ConsignmentsGrid;
				grid.SelectAllElements();

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Cannot deactivate Consignment LTTSTORE01 as it has a Customs release status.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Consignment is still active", consignment.HVC_IsActive);
				});
			}
		}

		public void TestConsignmentActiveStatusMenuItem_WhenSomeSelectedConsignmentsHaveCustomsStatus()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignmentWithoutCustomsStatus = bookingHeader.Consignments.AddNew();
			consignmentWithoutCustomsStatus.HVC_ConsignmentId = "LTTSTORE01";
			consignmentWithoutCustomsStatus.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;

			var consignmentWithCustomsStatus = bookingHeader.Consignments.AddNew();
			consignmentWithCustomsStatus.HVC_ConsignmentId = "LTTSTORE02";
			consignmentWithCustomsStatus.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;

			CombineAssertions(() =>
			{
				Assert("Precondition: consignment does not have customs status", !consignmentWithoutCustomsStatus.HasCustomsStatus);
				Assert("Precondition: consignment has customs status", consignmentWithCustomsStatus.HasCustomsStatus);
			});

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ConsignmentsGrid;
				grid.SelectAllElements();

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					Assert("Consignment without Customs status is still deactivated", !consignmentWithoutCustomsStatus.HVC_IsActive);
					AssertEquals("Cannot deactivate Consignment LTTSTORE02 as it has a Customs release status.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestConsignmentScreeningStatus_FiltersMatchedConsignmentsStatus()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ConsignmentsGrid;
				AssertEquals("Precondition: 3 Consignments Visible", 3, grid.List.Count);

				var filterStripControl = form.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				var filter = filterStripControl.FilterBusinessObject.ModuleFilters["Screening Status" + FilterModuleStrategy.UniqueSuffix] as ModuleTextFilter;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = ScreeningStatusesList.Codes.Matched;
				filter.IsActive = true;

				filterStripControl.FirePerformSearch();

				var consignmentsInGrid = grid.List.ToList<HVLVConsignment>();
				AssertNotNull(consignmentsInGrid);
				AssertContainsExactElementsInAnyOrder("Consignment grids should show Consignments with Matched status", new[] { consignment1, consignment2 }, consignmentsInGrid);

				filter.Property = ScreeningStatusesList.Codes.NotScreened;
				filterStripControl.FirePerformSearch();

				consignmentsInGrid = grid.List.ToList<HVLVConsignment>();
				AssertNotNull(consignmentsInGrid);
				AssertContainsExactElementsInAnyOrder("Consignment grids should show Consignments with Not Screened status", new[] { consignment3 }, consignmentsInGrid);
			}
		}

		public void TestApplyFilterUsesPerformanceStatistics()
		{
			var stats = new PerformanceStatisticsCollectorForTest();

			AssertEquals("Should have no record for ApplyFilter", 0, stats.CollectedStats.Count(x => x.Contains("ApplyFilter")));

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (ObjectFactory.Substitute<IPerformanceStatisticsCollector>(stats))
			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				PerformanceStatisticsCollector.ResetInstance();
				form.Show();
			}

			AssertNotNull("Precondition: CollectedStats has been made", stats.CollectedStats);
			AssertEquals("Should have 1 record added for ApplyFilter", 1, stats.CollectedStats.Count(x => x.Contains("ApplyFilter")));
		}

		#region TestShipmentCountsFiltering

		void SetUpHVLVConsignments(ForwardingShipment shipment)
		{
			var clearedConsignment1 = Factory.New<HVLVConsignment>();
			clearedConsignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			clearedConsignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
			clearedConsignment1.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;
			var clearedItem1 = clearedConsignment1.Items.AddNew();
			clearedItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			clearedItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
			clearedItem1.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;

			var clearedConsignment2 = Factory.New<HVLVConsignment>();
			clearedConsignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			clearedConsignment2.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
			clearedConsignment2.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;
			var clearedItem2 = clearedConsignment2.Items.AddNew();
			clearedItem2.HVI_JS_LoadedOnShipment = shipment.PK;
			clearedItem2.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
			clearedItem2.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var clearedConsignment3 = Factory.New<HVLVConsignment>();
			clearedConsignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			clearedConsignment3.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
			clearedConsignment3.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;
			var clearedItem3 = clearedConsignment3.Items.AddNew();
			clearedItem3.HVI_JS_LoadedOnShipment = shipment.PK;
			clearedItem3.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
			clearedItem3.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;

			var heldConsignment1 = Factory.New<HVLVConsignment>();
			heldConsignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			heldConsignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
			heldConsignment1.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;
			var heldItem1 = heldConsignment1.Items.AddNew();
			heldItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			heldItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;
			heldItem1.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var noneConsignment1 = Factory.New<HVLVConsignment>();
			noneConsignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			noneConsignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			noneConsignment1.HVC_ExportReleaseStatus = HVLVReleaseStatus.None;
			var noneItem1 = noneConsignment1.Items.AddNew();
			noneItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			noneItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			noneItem1.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;

			var noneConsignment2 = Factory.New<HVLVConsignment>();
			noneConsignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			noneConsignment2.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			noneConsignment2.HVC_ExportReleaseStatus = HVLVReleaseStatus.None;
			var noneItem2 = noneConsignment2.Items.AddNew();
			noneItem2.HVI_JS_LoadedOnShipment = shipment.PK;
			noneItem2.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			noneItem2.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;

			var surplusItem1 = noneConsignment1.Items.AddNew();
			surplusItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			surplusItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			surplusItem1.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			surplusItem1.HVI_Status = HVLVItemStatus.Codes.SurplusAtDestinationDepot;

			var shortItem1 = noneConsignment1.Items.AddNew();
			shortItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			shortItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			shortItem1.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			shortItem1.HVI_Status = HVLVItemStatus.Codes.ShortShippedAtDestinationDepot;

			var deliveredItem1 = noneConsignment1.Items.AddNew();
			deliveredItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			deliveredItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			deliveredItem1.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			deliveredItem1.HVI_Status = HVLVItemStatus.Codes.Delivered;

			var shortItem2 = noneConsignment2.Items.AddNew();
			shortItem2.HVI_JS_LoadedOnShipment = shipment.PK;
			shortItem2.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			shortItem2.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			shortItem2.HVI_Status = HVLVItemStatus.Codes.ShortShippedAtDestinationDepot;

			var scannedClearedItem1 = clearedConsignment1.Items.AddNew();
			scannedClearedItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			scannedClearedItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
			scannedClearedItem1.HVI_IsScannedAtDestination = true;

			var scannedHeldItem1 = heldConsignment1.Items.AddNew();
			scannedHeldItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			scannedHeldItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;
			scannedHeldItem1.HVI_IsScannedAtDestination = true;

			var scannedHeldItem2 = heldConsignment1.Items.AddNew();
			scannedHeldItem2.HVI_JS_LoadedOnShipment = shipment.PK;
			scannedHeldItem2.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;
			scannedHeldItem2.HVI_IsScannedAtDestination = true;

			var scannedNoneItem1 = noneConsignment1.Items.AddNew();
			scannedNoneItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			scannedNoneItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			scannedNoneItem1.HVI_IsScannedAtDestination = true;
		}

		ForwardingShipment GetHVLVShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			return shipment;
		}

		HVLVBookingHeader GetHVLVBookingHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			return bookingHeader;
		}

		public void TestWhenClickngShipmentCounter_ThenApplyFilterOnConsignmentGrid()
		{
			var shipment = GetHVLVShipment();
			SetUpHVLVConsignments(shipment);
			Factory.Save();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var form = new ShipmentUserControlTestForm(consignmentHeader))
			{
				form.Show();

				AssertEquals("Expected all consignments to be in the consignments grid.", shipment.HVLVConsignments.Count(), form.ConsignmentsGrid.List.Count);

				CombineAssertions("Clicking on shipment counts filters the consignments grid.", () =>
				{
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ItemCount", 6);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportClearedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportHeldCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportNoneReportedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ExportClearedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ExportHeldCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ExportNoneReportedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "SurplusCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ShortCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "DeliveredCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedClearedCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedHeldCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedNoneReportedCount", 1);
				});
			}
		}

		public void TestGivenFilteredConsignmentGrid_WhenClickingOnItemCount_ThenShouldClearActiveFiltersOnConsignmentGrid()
		{
			CombineAssertions("Clicking on 'Item Count' should clear any active filters on the consignments grid.", () =>
			{
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ImportHeldCount", 1);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ImportNoneReportedCount", 2);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ImportClearedCount", 3);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("SurplusCount", 1);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ShortCount", 2);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("DeliveredCount", 1);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ScannedCount", 3);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ScannedClearedCount", 1);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ScannedHeldCount", 1);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ScannedNoneReportedCount", 1);

				GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ExportHeldCount", 2);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ExportNoneReportedCount", 2);
				AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared("ExportClearedCount", 2);
			});
		}

		void AssertFilteredConsignmentsGridOnShipmentCountsCanBeCleared(string shipmentCountButton, int expectedNumberOfConsignments)
		{
			var shipment = GetHVLVShipment();
			SetUpHVLVConsignments(shipment);
			Factory.Save();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var form = new ShipmentUserControlTestForm(consignmentHeader))
			{
				form.Show();

				AssertEquals("Expected all consignments to be in the consignments grid.", shipment.HVLVConsignments.Count(), form.ConsignmentsGrid.List.Count);
				AssertClickingShipmentCountsFiltersConsignmentGrid(form, shipmentCountButton, expectedNumberOfConsignments);
				AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ItemCount", shipment.HVLVConsignments.Count());
			}
		}

		public void TestWhenClickingShipmentCounterWithZeroShipments_ThenConsignmentsGridIsEmpty()
		{
			var bookingHeader = GetHVLVBookingHeader();
			Factory.Save();

			var shipment = GetHVLVShipment();

			var heldConsignment1 = bookingHeader.Consignments.AddNew();
			heldConsignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			heldConsignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
			var heldItem1 = heldConsignment1.Items.AddNew();
			heldItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			heldItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var noneConsignment1 = bookingHeader.Consignments.AddNew();
			noneConsignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			noneConsignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			var noneItem1 = noneConsignment1.Items.AddNew();
			noneItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			noneItem1.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;

			Factory.Save();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var form = new ShipmentUserControlTestForm(consignmentHeader))
			{
				form.Show();

				AssertEquals("Expected all consignments to be in the consignments grid.", shipment.HVLVConsignments.Count(), form.ConsignmentsGrid.List.Count);

				var btn = form.Controls.Find("ImportClearedCount", true).Single() as ToggleRadioButton;

				btn.PerformClick();
				Application.DoEvents();

				AssertEquals($"Expected consignments grid to be empty when clicking on filter with zero consignments.", 0, form.ConsignmentsGrid.List.Count);
			}
		}

		public void TestWhenClickingMultipleShipmentCountersSuccessively_ThenShouldUpdateActiveFiltersOnConsignmentGrid()
		{
			var shipment = GetHVLVShipment();
			SetUpHVLVConsignments(shipment);
			Factory.Save();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var form = new ShipmentUserControlTestForm(consignmentHeader))
			{
				form.Show();

				AssertEquals("Expected all consignments to be in the consignments grid.", shipment.HVLVConsignments.Count(), form.ConsignmentsGrid.List.Count);

				CombineAssertions(() =>
				{
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportClearedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ExportClearedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "SurplusCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ShortCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "DeliveredCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedHeldCount", 1);
				});
			}
		}

		public void TestGivenClickedShipmentCount_WhenClickingOnShipmentCountAgain_ThenClearActiveFilterOnConsignmentGrid()
		{
			var shipment = GetHVLVShipment();
			SetUpHVLVConsignments(shipment);
			Factory.Save();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var totalConsignments = shipment.HVLVConsignments.Count();

			using (var form = new ShipmentUserControlTestForm(consignmentHeader))
			{
				form.Show();

				AssertEquals("Expected all consignments to be in the consignments grid.", totalConsignments, form.ConsignmentsGrid.List.Count);

				CombineAssertions(() =>
				{
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ItemCount", 6);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ItemCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportHeldCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportHeldCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportNoneReportedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportNoneReportedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportClearedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportClearedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "SurplusCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "SurplusCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ShortCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ShortCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "DeliveredCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "DeliveredCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedClearedCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedClearedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedHeldCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedHeldCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedNoneReportedCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedNoneReportedCount", totalConsignments);
				});
			}

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
			using (var form1 = new ShipmentUserControlTestForm(consignmentHeader))
			{
				form1.Show();
				AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportHeldCount", 2);
				AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportHeldCount", totalConsignments);

				AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportNoneReportedCount", 2);
				AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportNoneReportedCount", totalConsignments);

				AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportClearedCount", 2);
				AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportClearedCount", totalConsignments);
			}
		}

		public void TestClickingSameShipmentCountConsecutively_ThenShouldToggleFilterOnConsignmentGrid()
		{
			var shipment = GetHVLVShipment();
			SetUpHVLVConsignments(shipment);
			Factory.Save();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var form = new ShipmentUserControlTestForm(consignmentHeader))
			{
				form.Show();
				var totalConsignments = shipment.HVLVConsignments.Count();

				AssertEquals("Expected all consignments to be in the consignments grid.", totalConsignments, form.ConsignmentsGrid.List.Count);

				CombineAssertions(() =>
				{
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ItemCount", 6);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ItemCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ItemCount", 6);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ItemCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportHeldCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportHeldCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportHeldCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportHeldCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportNoneReportedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportNoneReportedCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportNoneReportedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportNoneReportedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportClearedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportClearedCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportClearedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ImportClearedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "SurplusCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "SurplusCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "SurplusCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "SurplusCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ShortCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ShortCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ShortCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ShortCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "DeliveredCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "DeliveredCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "DeliveredCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "DeliveredCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedCount", 3);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedClearedCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedClearedCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedClearedCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedClearedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedHeldCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedHeldCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedHeldCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedHeldCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedNoneReportedCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedNoneReportedCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedNoneReportedCount", 1);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form, "ScannedNoneReportedCount", totalConsignments);
				});

				GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
				using (var form1 = new ShipmentUserControlTestForm(consignmentHeader))
				{
					form1.Show();
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportHeldCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportHeldCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportHeldCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportHeldCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportNoneReportedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportNoneReportedCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportNoneReportedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportNoneReportedCount", totalConsignments);

					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportClearedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportClearedCount", totalConsignments);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportClearedCount", 2);
					AssertClickingShipmentCountsFiltersConsignmentGrid(form1, "ExportClearedCount", totalConsignments);
				}
			}
		}

		void AssertClickingShipmentCountsFiltersConsignmentGrid(ShipmentUserControlTestForm form, string shipmentCountButton, int expectedNumberOfConsignments)
		{
			var button = form.Controls.Find(shipmentCountButton, true).Single() as ToggleRadioButton;

			button.PerformClick();
			Application.DoEvents();

			AssertEquals($"Expected consignments grid to have been filtered when clicking: {shipmentCountButton}.", expectedNumberOfConsignments, form.ConsignmentsGrid.List.Count);
		}

		#endregion

		#region Implementation

		void SetupImportConsignmentWithReleaseStatus(HVLVConsignment consignment, string releaseStatus)
		{
			consignment.HVC_ConsigneeName = "Test Consignee";
			consignment.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment.HVC_ConsigneePostcode = "1234";
			consignment.HVC_ConsigneeCity = "Sydney";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_ImportReleaseStatus = releaseStatus;
		}

		#endregion
	}
}
