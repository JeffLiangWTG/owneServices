using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.DataTransfer;
using Enterprise.eTail.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;
using TransportModes = Enterprise.Core.Constants.TransportModes;

namespace Enterprise.eTail.Module.Testing
{
	public class ETailShipmentPluginTest : ZPlugInGenericTest
	{
		public void TestNoExceptionThrown_SaveEmptyShipmentFailedFirst_ThenSaveAgain()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				AssertEquals("precondition : shipment has no consignment", 0, shipment.HVLVConsignments.Count());

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				shipment.JS_Phase = "WWW";
				form.FireSaveButton();

				AssertHasErrors("save failed", shipment.JS_PhaseInfo);

				AssertNoExceptionThrown(() =>
				{
					form.FireSaveButton();
				});
			}
		}

		public void TestJobComInvoiceLinePartSynchronisationManager()
		{
			var factory = new BusinessObjectFactory();
			var importer = factory.NewWithValidTestData<OrgHeader>();
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			var part = factory.New<OrgSupplierPart>();
			part.RelatedOrganisations.AddOwner(importer);
			part.RelatedOrganisations.AddSupplier(supplier);
			part.OP_PartNum = "P001";
			part.OP_Desc = "DESC";
			factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			shipment.ConsignorPK = supplier.PK;
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_ProductCode = part.OP_PartNum;
			AssertEquals(part.OP_Desc, itemLine.HVS_GoodsDescription);

			part.OP_Desc = "DESC1";
			factory.Save();
			AssertNotEquals("DESC1", itemLine.HVS_GoodsDescription);

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.ControllerID = ControllerIDs.JobShipment;
				shipmentForm.Show();
				shipmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.ETailShipment);

				part.OP_Desc = "DESC2";
				factory.Save();
				AssertEquals("DESC2", itemLine.HVS_GoodsDescription);
			}

			part.OP_Desc = "DESC3";
			factory.Save();
			AssertNotEquals("DESC3", itemLine.HVS_GoodsDescription);
		}

		public void TestOpenMultipleCargoReportsUserConfirm()
		{
			var shipment = Factory.New<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var converter = new DummyConverterWithPreDefinedResults(shipment);
			var dummy1 = Factory.New<DummyEnterpriseBusinessObject>();
			var dummy2 = Factory.New<DummyEnterpriseBusinessObject>();
			Factory.Save();
			converter.SetResults(dummy1, dummy2);

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				CommandJobTypeMenuGroup<DummyHVLVRelatedJobCommand>.OpenCustomsRelatedBusinessForms(converter.CustomsRelatedBusinessCollection, shipmentForm, true);

				var message = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertEquals("Confirming message", "2 Cargo Reports have been generated. Do you wish to open all?", message.Text);
					AssertNullOrEmpty("Caption is null", message.Caption);
					Assert("Should be Yes/No question", message.WasQuestion);
				});
			}
		}

		public void TestOpenCustomsRelatedBusinessFormUsesPerformanceStatistics()
		{
			var stats = new PerformanceStatisticsCollectorForTest();

			AssertEquals("Should have no record for OpenCustomsRelatedBusinessForm", 0, stats.CollectedStats.Count(x => x.Contains("OpenCustomsRelatedBusinessForm")));

			var converter = new DummyConverterWithPreDefinedResults(Factory.New<ForwardingShipment>());
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			Factory.Save();
			converter.SetResults(dummy);

			var shipment = Factory.New<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (ObjectFactory.Substitute<IPerformanceStatisticsCollector>(stats))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.ControllerID = ControllerIDs.JobShipment;
				shipmentForm.Show();
				shipmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.ETailShipment);

				PerformanceStatisticsCollector.ResetInstance();
				CommandJobTypeMenuGroup<DummyHVLVRelatedJobCommand>.OpenCustomsRelatedBusinessForms(converter.CustomsRelatedBusinessCollection, shipmentForm, true);
			}

			AssertNotNull("Precondition: CollectedStats has been made", stats.CollectedStats);
			AssertEquals("Should have 1 record added for OpenCustomsRelatedBusinessForm", 1, stats.CollectedStats.Count(x => x.Contains("OpenCustomsRelatedBusinessForm")));
		}

		public void TestClickProgressFormCancelButton_ShouldInvokeCancelAction()
		{
			var cancelActionInvoked = false;

			var shipment = Factory.New<ForwardingShipment>();
			using (var shipmentForm = new ZForm(shipment))
			{
				shipmentForm.PlugIns.Add(ControllerIDs.ETailShipment);
				shipmentForm.Show();
				var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;

				using (var progressForm = new HVLVCancelableMinimisableProgressForm(shipmentForm, CancelAction))
				{
					progressForm.ShowModalTo(shipmentForm);
					progressForm.CancelButton.PerformClick();

					Assert("Should request cancellation", cancelActionInvoked);
				}
			}

			void CancelAction()
			{
				cancelActionInvoked = true;
			}
		}

		public void TestMinimizeProgressForm_ShipmentFormIsMinimized()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var shipmentForm = new ZForm(shipment))
			{
				shipmentForm.PlugIns.Add(ControllerIDs.ETailShipment);
				shipmentForm.Show();
				var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;

				using (var progressForm = new HVLVCancelableMinimisableProgressForm(shipmentForm, null))
				{
					progressForm.ShowModalTo(shipmentForm);
					progressForm.WindowState = FormWindowState.Minimized;

					AssertEquals("Shipment form should be minimized", FormWindowState.Minimized, shipmentForm.WindowState);
				}
			}
		}

		public void TestMinimizeProgressForm_ShipmentFormIsRestoredWhenProgressFormClosed()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var shipmentForm = new ZForm(shipment))
			{
				shipmentForm.PlugIns.Add(ControllerIDs.ETailShipment);
				shipmentForm.Show();
				shipmentForm.WindowState = FormWindowState.Maximized;

				var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;

				using (var progressForm = new HVLVCancelableMinimisableProgressForm(shipmentForm, null))
				{
					progressForm.ShowModalTo(shipmentForm);
					progressForm.WindowState = FormWindowState.Minimized;

					AssertEquals("precondition", FormWindowState.Minimized, shipmentForm.WindowState);
				}

				AssertEquals("Shipment form should be restored", FormWindowState.Maximized, shipmentForm.WindowState);
			}
		}

		public void TestNoExceptionThrownWhenShipmentHasBeenDeleted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertNoExceptionThrown(() =>
			{
				using (var shipmentForm = new ZForm(shipment))
				{
					shipmentForm.PlugIns.Add(ControllerIDs.ETailShipment);
					shipmentForm.Show();
					var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;

					shipment.Delete();
					Factory.Save();
					Assert(shipment.IsDeleted);
				}
			});
		}

		public void TestAfterSavingShipmentWithPrimaryFieldChanges_WhenThereIsNoCargoReport_DoNotDisplayPromptToCreateNewCargoReport()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_HouseBill = "HouseBill001";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "USLAX";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "MAWB1234";
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
			consol.Shipments.Add(shipment);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

				Factory.Save();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				shipment.JS_HouseBill = "HB001";

				plugin.OnSaving();
				plugin.OnSaveCompletedOrAborted(true);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		void AssertHLRLogs(ForwardingShipment shipment, string[] expectedReasons)
		{
			var hlrReasons = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode)
				.OrderBy(x => x.SL_EventTime)
				.Select(x => x.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason])
				.ToArray();

			AssertEquals(expectedReasons.Length, hlrReasons.Length);

			for (var i = 0; i < expectedReasons.Length; i++)
			{
				AssertEquals(expectedReasons[i], hlrReasons[i]);
			}
		}

		public void TestCargoReportingHLREventNotAddedWithMessageErrorsOnSave()
		{
			var shipment = CreateShipmentValidForNewETailData();
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				Factory.Save();

				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Amendment Processing"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var consignment = plugin.ConsignmentHeader.Consignments.AddNew();
				consignment.HVC_ConsigneeAddress1 = "";

				var item = consignment.Items.AddNew();
				item.HVI_IsActive = true;
				item.HVI_IsUnmanifestedAtDestination = true;

				plugin.OnSaveCompletedOrAborted(true);

				AssertHLRLogs(shipment, new[] { "Amendment Processing" });
			}
		}

		public void TestTopLevelMenu()
		{
			var shipment = CreateShipmentValidForNewETailData();
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				var topLevelMenu = plugin.TopLevelMenu;
				AssertNotNull(topLevelMenu);
				AssertEquals("HVLV", topLevelMenu.Text);
			}
		}

		public void TestShouldHideTopLevelMenuWithTab()
		{
			var shipment = CreateShipmentValidForNewETailData();
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				AssertEquals(true, plugin.ShouldHideTopLevelMenuWithTab);
			}
		}

		public void TestConsignorChangeResetConsignmentsPreScreeningStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ABCDEF";
			var address1 = org1.Addresses.AddNew();
			var address2 = org1.Addresses.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "FEDCBA";
			var address3 = org2.Addresses.AddNew();
			var address4 = org2.Addresses.AddNew();

			shipment.ConsignorPK = org1.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = address1.PK;

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			preScreeningConfiguration.IsEnabled = true;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				shipment.ConsignorPK = org2.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var consignment1 = header.Consignments.AddNew();
				var consignment2 = header.Consignments.AddNew();
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;
				shipment.ConsignorPK = org1.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address2.PK;
				Application.DoEvents();
				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Message shall be same as messageContent in method", "HVLV Pre-Screening is enabled, changing the eTailer will set the Pre-Screening Status on all HVLV Consignments to Unknown.", message.Text);
				AssertEquals("Caption shall be same as messageContent in method", "eTailer", message.Caption);
				AssertEquals("Pre-Screening status of consignments shall be Unknown.", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment1.HVC_PreScreeningStatus);
				AssertEquals("Pre-Screening status of consignments shall be Unknown.", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment2.HVC_PreScreeningStatus);

				UnitTestUserNotification.Instance.ClearMessages();
				shipment.ConsignorPK = org2.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address4.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				consignment1.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			preScreeningConfiguration.IsEnabled = false;
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment3 = bookingHeader2.Consignments.AddNew();
			consignment3.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
			shipment2.ConsignorPK = org1.PK;
			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = address1.PK;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				shipment2.ConsignorPK = org2.PK;
				shipment2.ConsignorDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCoveringLabelForShipment_ShownWhenNotInactive_HideWhenArchived()
		{
			var shipment = CreateShipmentValidForNewETailData();
			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;

				var tabControl = new ZTemplateTabControl();
				tabControl.TabPages.Add(plugin.TabPage);
				form.Controls.Add(tabControl);
				((IPlugInInternals)plugin).InitializePlugin(tabControl, form);
				form.Show();

				var coveringLabel = ((IPlugInInternals)plugin).CoveringLabel;
				Assert("Precondition: covering label is not visible.", !coveringLabel.Visible);
			}

			consignmentHeader.HCH_IsArchived = true;
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;

				var tabControl = new ZTemplateTabControl();
				tabControl.TabPages.Add(plugin.TabPage);
				form.Controls.Add(tabControl);
				((IPlugInInternals)plugin).InitializePlugin(tabControl, form);
				form.Show();

				var label = plugin.TabPage.Controls[0];
				CombineAssertions("There is only one control on the page, and it is the covering label.", () =>
				{
					AssertEquals(typeof(ZLabel), label.GetType());
					AssertEquals(1, plugin.TabPage.Controls.Count);
					Assert("Covering label is visible.", label.Visible);
					AssertEquals("HVLV Consignments and Item details for this Shipment are not available as they have exceeded the archive period.", label.Text);
				});
			}
		}

		public void TestVolumeWeightTextBoxCaptionChanges_WhenShipmentTransportModeChanges()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				var shipmentUserControl = plugin.UserControl as HVLVShipmentUserControl;
				var itemsGridColumnHeaders = shipmentUserControl.Controls.Find("itemsGrid", true).Single() as ZGrid;

				var textBox = shipmentUserControl.Controls.Find("zTextBoxVolumeWeight", true).Single() as ZTextBox;
				var textBoxCaption = textBox.GetExtension<ILabelCaptionRenderer>().Caption;
				var columnHeaderCaption = itemsGridColumnHeaders.GetColumnCaption("VolumeWeightForDisplay");
				AssertEquals("Weight Volume", textBoxCaption);
				AssertEquals("Weight Volume", columnHeaderCaption);

				shipment.JS_TransportMode = TransportModes.Air;
				textBoxCaption = textBox.GetExtension<ILabelCaptionRenderer>().Caption;
				columnHeaderCaption = itemsGridColumnHeaders.GetColumnCaption("VolumeWeightForDisplay");
				AssertEquals("Volume Weight", textBoxCaption);
				AssertEquals("Volume Weight", columnHeaderCaption);
			}
		}

		void PreparedDataForTestSavingShipmentWithPrimaryFieldChanged(string transportMode, out ForwardingShipment shipment)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TEST001";

			shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = transportMode;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			shipment.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
			shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));

			var genPivot = Factory.New<IGenPivot>();
			genPivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
			genPivot.XX_Relation1ID = consignmentHeader.PK;
			genPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;

			BusinessObject customJob1 = null;
			if (transportMode == TransportModes.Air)
			{
				customJob1 = Factory.NewWithValidTestData<CusMAWB>();
				genPivot.XX_Relation2TableCode = CusMAWBSchema.Constants.Prefix;
			}
			else if (transportMode == TransportModes.Sea)
			{
				customJob1 = Factory.NewWithValidTestData<BaseCusSCAOceanBill>();
				genPivot.XX_Relation2TableCode = CusSCAOceanBillSchema.Constants.Prefix;
			}
			genPivot.XX_Relation2ID = customJob1.PK;
			Factory.Save();
		}

		void TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ShipmentNotSaved(string transportMode)
		{
			PreparedDataForTestSavingShipmentWithPrimaryFieldChanged(transportMode, out var shipment);

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				shipment.JS_HouseBill = "HB0012";
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.ConsignmentHeader.Consignments.AddNew();
				plugin.OnSaving();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var continueWithSaveResult = plugin.ShowPreSaveDialogsCore();
				AssertEquals(continueWithSaveResult, ContinueWithSave.No);

				var message = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("The dialog about existing Customs jobs would be displayed",
					"Customs job(s) have already been created, saving the shipment may negatively affect existing Customs job(s) due to primary field(s) update.", message);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestSavingShipmentWithPrimaryFieldChanged_ClickCancel_ShipmentNotSaved()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ShipmentNotSaved(TransportModes.Air);
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ShipmentNotSaved(TransportModes.Sea);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ShipmentNotSaved(TransportModes.Air);
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ShipmentNotSaved(TransportModes.Sea);
			}
		}

		void TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(string transportMode)
		{
			PreparedDataForTestSavingShipmentWithPrimaryFieldChanged(transportMode, out var shipment);

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				shipment.JS_HouseBill = "HB0012";
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.ConsignmentHeader.Consignments.AddNew();
				plugin.OnSaving();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var continueWithSaveResult = plugin.ShowPreSaveDialogsCore();
				var customsJobs = plugin.ConsignmentHeader.GenPivotCollection.CustomsJobs.OfType<ICancellable>().ToList();

				CombineAssertions(() =>
				{
					AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
					AssertEquals(1, customsJobs.Count);
					Assert(!(customsJobs[0].IsCancelled));
					AssertEquals("There exists only one log with TRF mark",
						shipment.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode), 1);
					Assert("This log should not be cancelled",
						!(shipment.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode).IsCancelled));
				});
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestSavingShipmentWithPrimaryFieldChanged_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(TransportModes.Air);
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(TransportModes.Sea);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(TransportModes.Air);
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(TransportModes.Sea);
			}
		}

		void TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanNotBeCancelled_DetailedErrorMessageDisplay()
		{
			PreparedDataForTestSavingShipmentWithPrimaryFieldChanged(TransportModes.Air, out var shipment);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.JK_IsCancelled = true;

			var jobs = Factory.Load<CusMAWB>(new ZQuery());
			jobs.ForEach(job => job.CM_JK = consol.PK);
			jobs.ForEach(job => job.IsCancelled = false);

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				shipment.JS_HouseBill = "HB0012";
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;

				plugin.OnSaving();
				plugin.ConsignmentHeader.Consignments.AddNew();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var continueWithSaveResult = plugin.ShowPreSaveDialogsCore();
				var message = UnitTestUserNotification.Instance.LastMessage.Text;

				CombineAssertions(() =>
				{
					AssertEquals(continueWithSaveResult, ContinueWithSave.No);
					AssertEquals("Shipment cannot be cancelled.",
					$"Shipment can’t be saved for primary field(s) update because Customs job(s) have already been created. This record cannot be deactivated as its parent host record cannot be deactivated due to the following reason.\r\nConsol {consol.JK_UniqueConsignRef}: You cannot deactivate Consol {consol.JK_UniqueConsignRef} since it has shipments attached. Please create a new shipment instead.", message);
					AssertEquals("There exists an log with TRF mark", shipment.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode), 1);
					Assert("The log with TRF mark is not cancelled", !shipment.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode).IsCancelled);
					AssertEquals("A related CustomJob is linked to the shipment", plugin.ConsignmentHeader.GenPivotCollection.CustomsJobs.OfType<ICancellable>().Count(), 1);
					Assert("The related CustomsJob are not cancelled", !plugin.ConsignmentHeader.GenPivotCollection.CustomsJobs.OfType<ICancellable>().Any(cancellable => cancellable.IsCancelled));
				});
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestSavingShipmentWithPrimaryFieldChanged_RelatedJobsCanNotBeCancelled_DetailedErrorMessageDisplay()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanNotBeCancelled_DetailedErrorMessageDisplay();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				TestSavingShipmentWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanNotBeCancelled_DetailedErrorMessageDisplay();
			}
		}

		public void TestSavingShipmentWithPrimaryFieldChanged_NoRelatedJobsCreate_NoneMessageDisplay()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TEST001";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;

			var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				shipment.JS_HouseBill = "HB0012";
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.ConsignmentHeader.Consignments.AddNew();
				plugin.OnSaving();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var continueWithSaveResult = plugin.ShowPreSaveDialogsCore();

				CombineAssertions(() =>
				{
					AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				});
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestShowPreSaveDialogs_WhenNoConsignmentsOnShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			using (var shipmentForm = new ZForm(shipment))
			{
				shipmentForm.PlugIns.Add(ControllerIDs.ETailShipment);
				shipmentForm.Show();

				var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;

				Assert("Precondition: no consignments on consignment header", !plugin.ConsignmentHeader.Consignments.Any());

				var preSaveMessage = "There are no HVLV Consignments on this HVL Shipment. Do you want to continue saving? Shipment type cannot be changed after shipment is saved.";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				plugin.ShowPreSaveDialogs();

				CombineAssertions(() =>
				{
					AssertNotNull(plugin.ConsignmentHeader);
					Assert(!plugin.ConsignmentHeader.IsInDatabase);
					AssertEquals(preSaveMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestShowPreSaveDialogs_WhenConsignmentsOnShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			using (var shipmentForm = new ZForm(shipment))
			{
				shipmentForm.PlugIns.Add(ControllerIDs.ETailShipment);
				shipmentForm.Show();

				var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.ConsignmentHeader.Consignments.AddNew();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugin.ShowPreSaveDialogs();

				CombineAssertions(() =>
				{
					AssertNotNull(plugin.ConsignmentHeader);
					Assert(!plugin.ConsignmentHeader.IsInDatabase);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGetNameOfCargoReportDependOnCountryTransportModeAndDirection_US()
		{
			var usBranch = Factory.NewWithValidTestData<GlbBranch>();
			usBranch.GB_RL_NKHomePort = "USCHI";

			Factory.Save();
			
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, usBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				AssertNameOfCargoReport(shipment, "HVLV Low Value Entries");
			}
		}

		public void TestGetNameOfCargoReportDependOnCountryTransportModeAndDirection_AU()
		{
			var auBranch = Factory.NewWithValidTestData<GlbBranch>();
			auBranch.GB_RL_NKHomePort = "AUSYD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, auBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var airShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var seaShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				airShipment.JS_TransportMode = TransportModes.Air;
				seaShipment.JS_TransportMode = TransportModes.Sea;

				CombineAssertions(() =>
				{
					AssertNameOfCargoReport(airShipment, "HVLV AirCargo Report");
					AssertNameOfCargoReport(seaShipment, "HVLV SeaCargo Report");
				});
			}
		}

		public void TestGetNameOfCargoReportDependOnCountryTransportModeAndDirection_NZ()
		{
			var nzBranch = Factory.NewWithValidTestData<GlbBranch>();
			nzBranch.GB_RL_NKHomePort = "NZAKl";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var airImportShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var airExportShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var seaImportShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var seaExportShipment = Factory.NewWithValidTestData<ForwardingShipment>();

				airImportShipment.JS_TransportMode = TransportModes.Air;
				airExportShipment.JS_TransportMode = TransportModes.Air;
				seaImportShipment.JS_TransportMode = TransportModes.Sea;
				seaExportShipment.JS_TransportMode = TransportModes.Sea;

				airImportShipment.JS_RL_NKOrigin = "AUSYD";
				airImportShipment.JS_RL_NKDestination = "NZAKL";
				airExportShipment.JS_RL_NKOrigin = "NZAKL";
				airExportShipment.JS_RL_NKDestination = "AUSYD";
				seaImportShipment.JS_RL_NKOrigin = "AUSYD";
				seaImportShipment.JS_RL_NKDestination = "NZAKL";
				seaExportShipment.JS_RL_NKOrigin = "NZAKL";
				seaExportShipment.JS_RL_NKDestination = "AUSYD";

				CombineAssertions(() =>
				{
					AssertNameOfCargoReport(airImportShipment, "HVLV AirCargo ICR");
					AssertNameOfCargoReport(seaImportShipment, "HVLV SeaCargo ICR");
					AssertNameOfCargoReport(airExportShipment, "HVLV AirCargo CRE");
					AssertNameOfCargoReport(seaExportShipment, "HVLV SeaCargo CRE");
				});
			}
		}

		public void TestGetNameOfCargoReportDependOnCountryTransportModeAndDirection_SG()
		{
			var sgBranch = Factory.NewWithValidTestData<GlbBranch>();
			sgBranch.GB_RL_NKHomePort = "SGSIN";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, sgBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var importShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var exportShipment = Factory.NewWithValidTestData<ForwardingShipment>();

				importShipment.JS_RL_NKOrigin = "AUSYD";
				importShipment.JS_RL_NKDestination = "SGSIN";
				exportShipment.JS_RL_NKOrigin = "SGSIN";
				exportShipment.JS_RL_NKDestination = "AUSYD";

				CombineAssertions(() =>
				{
					AssertNameOfCargoReport(importShipment, "SG ACCESS Import Manifest");
					AssertNameOfCargoReport(exportShipment, "SG ACCESS Export Manifest");
				});
			}
		}

		void AssertNameOfCargoReport(ForwardingShipment shipment, String message)
		{
			using (var shipmentForm = new ZForm(shipment))
			{
				var method = typeof(ETailShipmentPlugin).GetMethod(
					"GetNameOfCargoReportDependOnCountryTransportModeAndDirection",
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[] { typeof(ForwardingShipment) },
					null);
				shipmentForm.PlugIns.Add(ControllerIDs.ETailShipment);
				var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				var result = method.Invoke(plugin, new object[] { shipment });
				AssertEquals(message, result);
			}
		}

		#region Implementation

		protected override ZPlugIn GetPlugInToTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var result = new ETailShipmentPlugin(shipment);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			return result;
		}

		ForwardingShipment CreateShipmentValidForNewETailData(bool setConsignorAddresses = true)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "STD";

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			if (setConsignorAddresses)
			{
				var consignorDocumentaryAddress = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
				consignorDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;

				var consignorPickupAddress = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
				consignorPickupAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			}

			Factory.Save();

			return shipment;
		}

		public class DummyConverterWithPreDefinedResults : CustomsRelatedBusinessObjectConverter
		{
			public DummyConverterWithPreDefinedResults(ForwardingShipment shipment) : base(new DummyHVLVRelatedJobCommand(shipment))
			{
			}

			public void SetResults(params BusinessObject[] masterBills)
			{
				typeof(CustomsRelatedBusinessObjectConverter).
					GetProperty(nameof(CustomsRelatedBusinessCollection)).
					GetSetMethod(true).Invoke(this, new[] { new ReadOnlyCollection<BusinessObject>(masterBills.ToList()) });
			}
		}

		public class DummyHVLVRelatedJobCommand : BaseHVLVRelatedJobCommand
		{
			public DummyHVLVRelatedJobCommand(ForwardingShipment shipment) : base(shipment)
			{
			}

			public override MultilingualString RelatedJobName => (NoResString)"Dummy Cargo Report";

			protected override Type RelatedCustomsJobType => throw new NotImplementedException();

			public override CustomsRelatedBusinessObjectConverter Converter => new DummyConverterWithPreDefinedResults(Shipment);

			public override string UsageCode => throw new NotImplementedException();
		}

		#endregion
	}
}
